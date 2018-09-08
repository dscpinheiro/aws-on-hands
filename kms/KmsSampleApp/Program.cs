using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace KmsSampleApp
{
    class Program
    {
        static AmazonKeyManagementServiceClient _client;
        static string _masterKeyId;

        static void Main(string[] args)
        {
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var region = Environment.GetEnvironmentVariable("AWS_REGION");

            _client = new AmazonKeyManagementServiceClient(accessKey, secretKey, RegionEndpoint.GetBySystemName(region));

            Create().Wait();
            GenerateDataKey().Wait();
            Disable().Wait();
        }

        static async Task Create()
        {
            var createResponse = await _client.CreateKeyAsync(new CreateKeyRequest
            {
                Description = $"My sample master key - {DateTime.UtcNow.Ticks}"
            });

            _masterKeyId = createResponse.KeyMetadata.Arn;

            await _client.CreateAliasAsync(new CreateAliasRequest
            {
                AliasName = "alias/mysamplekey1",
                TargetKeyId = _masterKeyId
            });

            var describeKeyResponse = await _client.DescribeKeyAsync(new DescribeKeyRequest
            {
                KeyId = _masterKeyId
            });

            Console.WriteLine(describeKeyResponse.KeyMetadata);
        }

        static async Task GenerateDataKey()
        {
            var generateKeyResponse = await _client.GenerateDataKeyAsync(new GenerateDataKeyRequest
            {
                KeyId = _masterKeyId,
                KeySpec = DataKeySpec.AES_256
            });

            var decryptResponse = await _client.DecryptAsync(new DecryptRequest
            {
                CiphertextBlob = generateKeyResponse.CiphertextBlob
            });

            Console.WriteLine(Encoding.ASCII.GetString(decryptResponse.Plaintext.ToArray()));
        }

        static async Task Disable()
        {
            await _client.DisableKeyAsync(new DisableKeyRequest
            {
                KeyId = _masterKeyId
            });

            await _client.ScheduleKeyDeletionAsync(new ScheduleKeyDeletionRequest
            {
                KeyId = _masterKeyId,
                PendingWindowInDays = 7
            });
        }
    }
}
