using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))] 

namespace DebuggingExample
{
    public class Functions
    {
        private ITimeProcessor _processor;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions() => _processor = new TimeProcessor();

        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            LogMessage(context, "Processing request started");

            APIGatewayProxyResponse response;

            try
            {
                var result = _processor.CurrentTimeUTC();
                response = CreateResponse(result);

                LogMessage(context, "Processing request succeeded.");
            }
            catch (Exception ex)
            {
                LogMessage(context, $"Processing request failed - {ex.Message}");
                response = CreateResponse(null);
            }

            return response;
        }

        private void LogMessage(ILambdaContext context, string message) => 
            context.Logger.LogLine($"{context.AwsRequestId}:{context.FunctionName} - {message}");

        private APIGatewayProxyResponse CreateResponse(DateTime? result)
        {
            var statusCode = result.HasValue ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError;
            var body = result.HasValue ? JsonConvert.SerializeObject(result) : string.Empty;

            var response = new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = body,
                Headers = new Dictionary<string, string>
                { 
                    { "Content-Type", "application/json" }, 
                    { "Access-Control-Allow-Origin", "*" } 
                }
            };

            return response;
        }
    }
}
