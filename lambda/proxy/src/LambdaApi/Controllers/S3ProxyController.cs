using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Amazon.S3;
using Amazon.S3.Model;

using Newtonsoft.Json;

namespace LambdaApi.Controllers
{
    /// <summary>
    /// ASP.NET Core controller acting as a S3 Proxy.
    /// </summary>
    [Route("api/[controller]")]
    public class S3ProxyController : Controller
    {
        private IAmazonS3 _s3Client;
        private ILogger _logger;

        string BucketName { get; set; }

        public S3ProxyController(IConfiguration configuration, ILogger<S3ProxyController> logger, IAmazonS3 s3Client)
        {
            _logger = logger;
            _s3Client = s3Client;

            this.BucketName = configuration[Startup.AppS3BucketKey];
            if (string.IsNullOrWhiteSpace(this.BucketName))
            {
                var message = "Missing configuration for S3 bucket. The AppS3Bucket configuration must be set to a S3 bucket.";

                logger.LogCritical(message);
                throw new Exception(message);
            }

            logger.LogInformation($"Configured to use bucket {this.BucketName}");
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            var listResponse = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = this.BucketName
            });

            try
            {
                Response.ContentType = "text/json";

                return new JsonResult(listResponse.S3Objects, new JsonSerializerSettings { Formatting = Formatting.Indented });
            }
            catch (AmazonS3Exception e)
            {
                Response.StatusCode = (int)e.StatusCode;
                
                return new JsonResult(e.Message);
            }
        }

        [HttpGet("{key}")]
        public async Task Get(string key)
        {
            try
            {
                var getResponse = await _s3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = this.BucketName,
                    Key = key
                });

                Response.ContentType = getResponse.Headers.ContentType;
                getResponse.ResponseStream.CopyTo(Response.Body);
            }
            catch (AmazonS3Exception e)
            {
                Response.StatusCode = (int)e.StatusCode;
                var writer = new StreamWriter(Response.Body);
                writer.Write(e.Message);
            }
        }

        [HttpPut("{key}")]
        public async Task Put(string key)
        {
            // Copy the request body into a seekable stream required by the AWS SDK for .NET.
            var seekableStream = new MemoryStream();
            await this.Request.Body.CopyToAsync(seekableStream);
            seekableStream.Position = 0;

            var putRequest = new PutObjectRequest
            {
                BucketName = this.BucketName,
                Key = key,
                InputStream = seekableStream
            };

            try
            {
                var response = await _s3Client.PutObjectAsync(putRequest);
                _logger.LogInformation($"Uploaded object {key} to bucket {this.BucketName}. Request Id: {response.ResponseMetadata.RequestId}");
            }
            catch (AmazonS3Exception e)
            {
                Response.StatusCode = (int)e.StatusCode;
                var writer = new StreamWriter(Response.Body);
                writer.Write(e.Message);
            }
        }

        [HttpDelete("{key}")]
        public async Task Delete(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = this.BucketName,
                Key = key
            };

            try
            {
                var response = await _s3Client.DeleteObjectAsync(deleteRequest);
                _logger.LogInformation($"Deleted object {key} from bucket {this.BucketName}. Request Id: {response.ResponseMetadata.RequestId}");
            }
            catch (AmazonS3Exception e)
            {
                Response.StatusCode = (int)e.StatusCode;
                var writer = new StreamWriter(Response.Body);
                writer.Write(e.Message);
            }
        }
    }
}
