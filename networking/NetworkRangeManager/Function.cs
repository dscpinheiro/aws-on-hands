using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace NetworkRangeManager
{
    public class Function
    {
        private static AmazonDynamoDBClient _dynamoClient = new AmazonDynamoDBClient();
        private static DynamoDBContext _dynamoContext = new DynamoDBContext(_dynamoClient);
        private static Random _random = new Random();

        private const string PrivateABlock = "10.0.0.0/8";

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

            var type = request.ResourceProperties.NetworkType;
            var vpcName = request.ResourceProperties.VpcName;
            var parentRange = string.IsNullOrWhiteSpace(request.ResourceProperties.ParentRange) ? PrivateABlock : request.ResourceProperties.ParentRange;
            var cidr = request.ResourceProperties.Cidr;
            var tableName = type == NetworkType.Vpc ? "VpcNetworkRanges" : "SubnetNetworkRanges";

            try
            {
                context.Logger.LogLine($"Received '{request.RequestType}' request for vpc '{vpcName}' and block address {parentRange} - {cidr}");

                if (request.RequestType == "Create")
                {
                    var addressRange = await InternalGetRange(parentRange, cidr, tableName, vpcName);
                    if (string.IsNullOrWhiteSpace(addressRange))
                    {
                        throw new Exception("No available addresses for parameters specified");
                    }

                    response.PhysicalResourceId = GenerateRandomString();
                    response.Data = new ResponseData { AddressRange = addressRange };
                }
                else if (request.RequestType == "Delete")
                {
                    await DeleteRangeAsync(new NetworkRange { AddressRange = parentRange, VpcName = vpcName }, tableName);
                }

                response.Status = "SUCCESS";
            }
            catch (Exception exception)
            {
                response.Reason = exception.Message;
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
            if (cidr < 16 || cidr > 28) throw new ArgumentOutOfRangeException(nameof(cidr));

            var network = IPNetwork.Parse(addressBlock);
            var subnets = network.Subnet(cidr);
            var numberOfPossibleSubnets = (int)subnets.Count;

            IPNetwork validSubnet = null;
            NetworkRange networkRange = null;
            var count = 0;

            do
            {
                count++;
                var index = _random.Next(numberOfPossibleSubnets);
                var possibleSubnet = subnets[index];

                networkRange = new NetworkRange
                {
                    AddressRange = possibleSubnet.ToString(),
                    VpcName = vpcName
                };

                if (await CanUseRangeAsync(networkRange, tableName))
                {
                    validSubnet = possibleSubnet;
                }
            } while (validSubnet == null && count < numberOfPossibleSubnets);

            if (networkRange == null)
            {
                return string.Empty;
            }

            await _dynamoContext.SaveAsync(networkRange, new DynamoDBOperationConfig { OverrideTableName = tableName });

            return networkRange.AddressRange;
        }

        static async Task<bool> CanUseRangeAsync(NetworkRange range, string tableName)
        {
            var storedRange = await _dynamoContext.LoadAsync(range, new DynamoDBOperationConfig { OverrideTableName = tableName });

            return storedRange == null;
        }

        static async Task DeleteRangeAsync(NetworkRange range, string tableName)
        {
            await _dynamoContext.DeleteAsync(range, new DynamoDBOperationConfig { OverrideTableName = tableName });
        }

        static string GenerateRandomString(int length = 24)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
