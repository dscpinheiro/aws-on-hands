using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace NetworkRangeManager
{
    public class Function
    {
        private static DynamoDBContext _context = new DynamoDBContext(new AmazonDynamoDBClient());
        private static Random _random = new Random();

        private const string PrivateABlock = "10.0.0.0/8";
        private const string VpcTableName = "VpcNetworkRanges";
        private const string SubnetTableName = "SubnetNetworkRanges";
        private const string IndexName = "VpcName-IX";

        public async Task<CustomResourceResponse> FunctionHandler(CustomResourceRequest request, ILambdaContext context)
        {
            var response = new CustomResourceResponse
            {
                Status = "FAILED",
                PhysicalResourceId = request.PhysicalResourceId,
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId
            };

            try
            {
                var vpcName = request.ResourceProperties.VpcName;
                context.Logger.LogLine($"Received '{request.RequestType}' request for vpc '{vpcName}'");

                if (request.RequestType == "Create")
                {
                    response.PhysicalResourceId = GenerateRandomString();

                    var vpcAddressRange = await InternalGetRange(PrivateABlock, request.ResourceProperties.VpcCidr, VpcTableName, vpcName);
                    if (string.IsNullOrWhiteSpace(vpcAddressRange))
                    {
                        throw new Exception("No available addresses for VPC cidr");
                    }

                    response.Data = new ResponseData 
                    { 
                        VpcAddressRange = vpcAddressRange,
                        SubnetsAddressRanges = new List<string>()
                    };

                    foreach (var subnetCidr in request.ResourceProperties.SubnetCidrs)
                    {
                        var subnetAddressRange = await InternalGetRange(vpcAddressRange, subnetCidr, SubnetTableName, vpcName);
                        if (string.IsNullOrWhiteSpace(subnetAddressRange))
                        {
                            throw new Exception("No available addresses for subnets in this VPC");
                        }

                        response.Data.SubnetsAddressRanges.Add(subnetAddressRange);
                    }
                }

                response.Status = "SUCCESS";
            }
            catch (Exception exception)
            {
                response.Reason = exception.Message;
                response.Data = null;
                context.Logger.LogLine(exception.ToString());
            }

            return response;
        }

        /// <summary>
        /// Calculates all possible subnets in (<paramref name="addressBlock"/>/<paramref name="cidr"/>) and returns one
        /// not being used at the moment (that is, not present in the <paramref name="tableName"/>).
        /// </summary>
        private async Task<string> InternalGetRange(string addressBlock, byte cidr, string tableName, string vpcName)
        {
            if (cidr < 16 || cidr > 28) throw new ArgumentOutOfRangeException(nameof(cidr), "Cidr must be between /16 and /28");

            var network = IPNetwork.Parse(addressBlock);
            var subnets = network.Subnet(cidr);
            var numberOfPossibleSubnets = (int)subnets.Count;

            IPNetwork validSubnet = null;
            NetworkRange networkRange = null;
            var count = 0;

            var search = _context.QueryAsync<NetworkRange>(vpcName, new DynamoDBOperationConfig { OverrideTableName = tableName, IndexName = IndexName });
            var storedRanges = await search.GetRemainingAsync();

            do
            {
                count++;
                var possibleSubnet = subnets[_random.Next(numberOfPossibleSubnets)];

                networkRange = new NetworkRange
                {
                    AddressRange = possibleSubnet.ToString(),
                    VpcName = vpcName
                };

                if (CanUseRange(storedRanges, possibleSubnet))
                {
                    validSubnet = possibleSubnet;
                }
            } while (validSubnet == null && count < numberOfPossibleSubnets);

            if (validSubnet == null)
            {
                return string.Empty;
            }

            await _context.SaveAsync(networkRange, new DynamoDBOperationConfig { OverrideTableName = tableName });

            return networkRange.AddressRange;
        }

        static bool CanUseRange(IEnumerable<NetworkRange> storedRanges, IPNetwork network)
        {
            return
                storedRanges.All(x => x.AddressRange != network.ToString()) &&
                !storedRanges.Any(x => IPNetwork.Parse(x.AddressRange).Overlap(network));
        }

        static string GenerateRandomString(int length = 24)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
