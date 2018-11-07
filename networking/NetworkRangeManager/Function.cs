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

            context.Logger.LogLine($"Request id: {request.RequestId}; Stack: {request.StackId}");

            try
            {
                var vpcName = request.ResourceProperties.VpcName;
                context.Logger.LogLine($"Received '{request.RequestType}' request for VPC '{vpcName}'");

                if (request.RequestType == "Create")
                {
                    response.PhysicalResourceId = GenerateRandomString();

                    var vpcAddressRange = await InternalGetRange(PrivateABlock, request.ResourceProperties.VpcCidr, VpcTableName, vpcName);
                    context.Logger.LogLine($"Associated {vpcAddressRange} to VPC '{vpcName}'");

                    response.Data = new ResponseData 
                    { 
                        VpcAddressRange = vpcAddressRange,
                        SubnetsAddressRanges = new List<string>()
                    };

                    foreach (var subnetCidr in request.ResourceProperties.SubnetCidrs)
                    {
                        var subnetAddressRange = await InternalGetRange(vpcAddressRange, subnetCidr, SubnetTableName, vpcName);
                        response.Data.SubnetsAddressRanges.Add(subnetAddressRange);
                        context.Logger.LogLine($"Associated {subnetAddressRange} to VPC '{vpcName}'");
                    }
                }
                else if (request.RequestType == "Delete")
                {
                    await Task.WhenAll(
                        InternalDeleteRanges(vpcName, VpcTableName),
                        InternalDeleteRanges(vpcName, SubnetTableName)
                    );
                }

                response.Status = "SUCCESS";
            }
            catch (Exception exception)
            {
                response.Reason = exception.Message;
                response.Data = null;
                context.Logger.LogLine(exception.ToString());
            }

            context.Logger.LogLine($"Returning status: {response.Status}");

            return response;
        }

        /// <summary>
        /// Calculates all possible subnets in the (address block + cidr) and returns one not being used at the moment.
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

            var storedRanges = await GetRangesForVpc(vpcName, tableName);

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
                throw new Exception("No available ranges for this address block");
            }

            await _context.SaveAsync(networkRange, new DynamoDBOperationConfig { OverrideTableName = tableName });

            return networkRange.AddressRange;
        }

        /// <summary>
        /// Deletes all ranges associated with a VPC in the specified table.
        /// </summary>
        static async Task InternalDeleteRanges(string vpcName, string tableName)
        {
            var storedRanges = await GetRangesForVpc(vpcName, tableName);
            foreach (var range in storedRanges)
            {
                await _context.DeleteAsync(range, new DynamoDBOperationConfig { OverrideTableName = tableName });
            }
        }

        /// <summary>
        /// Gets all ranges associated with a VPC in the specified table.
        /// </summary>
        static async Task<IEnumerable<NetworkRange>> GetRangesForVpc(string vpcName, string tableName)
        {
            var search = _context.QueryAsync<NetworkRange>(vpcName, new DynamoDBOperationConfig { OverrideTableName = tableName, IndexName = IndexName });
            var storedRanges = await search.GetRemainingAsync();

            return storedRanges;
        }

        /// <summary>
        /// Checks whether the specified network can be used. The following validations are performed: <para />
        /// - Address block not currently used. <para />
        /// - Address block does not overlap with any existing ones.
        /// </summary>
        static bool CanUseRange(IEnumerable<NetworkRange> storedRanges, IPNetwork network)
        {
            return
                storedRanges.All(x => x.AddressRange != network.ToString()) &&
                !storedRanges.Any(x => IPNetwork.Parse(x.AddressRange).Overlap(network));
        }

        /// <summary>
        /// Generates a random string to be used as the physical resource id.
        /// </summary>
        static string GenerateRandomString(int length = 24)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
