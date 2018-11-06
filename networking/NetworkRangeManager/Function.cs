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
            var status = "FAILED";
            var addressRange = string.Empty;
            var reason = string.Empty;

            var type = request.ResourceProperties.NetworkType;
            var vpcName = request.ResourceProperties.VpcName;
            var vpcRange = request.ResourceProperties.VpcRange;
            var cidr = request.ResourceProperties.Cidr;

            var physicalResourceId = 
                string.IsNullOrWhiteSpace(request.PhysicalResourceId) ? 
                new { vpcName, addressRange }.GetHashCode().ToString() : 
                request.PhysicalResourceId;

            try
            {
                if (request.RequestType == "Create")
                {
                    context.Logger.LogLine($"Received {type} request for vpc '{vpcName}' with cidr /{cidr}");

                    addressRange = type == NetworkType.Vpc ? await GetVpcRange(vpcName, cidr) : await GetSubnetRange(vpcRange, vpcName, cidr);
                    if (string.IsNullOrWhiteSpace(addressRange))
                    {
                        throw new Exception("No available addresses for parameters specified");
                    }
                }

                status = "SUCCESS";
            }
            catch (Exception exception)
            {
                reason = exception.Message;
                context.Logger.LogLine(exception.ToString());
            }

            return new CustomResourceResponse
            {
                Status = status,
                Reason = reason,
                PhysicalResourceId = physicalResourceId,
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                Data = new ResponseData { AddressRange = addressRange }
            };
        }

        private void ValidateCidr(byte cidr)
        {
            if (cidr < 16 || cidr > 28)
            {
                throw new ArgumentOutOfRangeException(nameof(cidr));
            }
        }

        public async Task<string> GetVpcRange(string vpcName, byte cidr = 16)
        {
            ValidateCidr(cidr);

            return await InternalGetRange(PrivateABlock, cidr, "VpcNetworkRanges", vpcName);
        }

        public async Task<string> GetSubnetRange(string vpcRange, string vpcName, byte cidr = 24)
        {
            ValidateCidr(cidr);

            return await InternalGetRange(vpcRange, cidr, "SubnetNetworkRanges", vpcName);
        }

        /// <summary>
        /// Calculates all possible subnets in (<paramref name="addressBlock"/>/<paramref name="cidr"/>) and returns one
        /// not being used at the moment (that is, not present in the <paramref name="tableName"/>).
        /// </summary>
        private async Task<string> InternalGetRange(string addressBlock, byte cidr, string tableName, string vpcName)
        {
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
            var storedRange = await _dynamoContext.LoadAsync(range, new DynamoDBOperationConfig
            {
                OverrideTableName = tableName
            });

            return storedRange == null;
        }
    }
}
