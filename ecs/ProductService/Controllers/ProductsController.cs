using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace aspnetapp.Controllers
{
    [ApiController, Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private static readonly IEnumerable<Product> _products;

        static ProductsController()
        {
            _products = new List<Product>
            {
                new Product
                {
                    ID = "0000-0000-0001",
                    Title = "Fork Handles",
                    Description = "Got forks? Worn out ones? You need our all new Fork Handles",
                    Price = 6.95M
                },
                new Product
                {
                    ID = "0000-0000-0002",
                    Title = "Four Candles",
                    Description = "One candle never enough? You need our new Four Candles bundle",
                    Price = 3.75M
                },
                new Product
                {
                    ID = "0000-0000-0003",
                    Title = "Egg Basket",
                    Description = "Holds 6 unbroken eggs or 36 broken ones",
                    Price = 9.99M
                }
            };
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get() => _products.ToList();

        [HttpGet("{id}")]
        public ActionResult<Product> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var product = _products.SingleOrDefault(p => p.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return product;
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