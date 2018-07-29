using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace aspnetapp
{
    public class ProductsClient
    {
        private HttpClient _client;
        private ILogger<ProductsClient> _logger;

        public ProductsClient(HttpClient client, ILogger<ProductsClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            try
            {
                var response = await _client.GetAsync("api/products");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsAsync<IEnumerable<Product>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured connecting to API {ex.ToString()}");
                
                return Enumerable.Empty<Product>();
            }
        }
    }

    public class Product
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}