using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using DebuggingExample;

namespace DebuggingExample.Tests
{
    public class FunctionTest
    {
        public FunctionTest() { }

        [Fact]
        public void TetGetMethod()
        {
            APIGatewayProxyResponse response;

            var functions = new Functions();
            var request = new APIGatewayProxyRequest();
            var context = new TestLambdaContext();

            response = functions.Get(request, context);
            Assert.Equal(200, response.StatusCode);
        }
    }
}
