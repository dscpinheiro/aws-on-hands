using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using dotnet_ddb_api.Models;

namespace dotnet_ddb_api.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class BlogsController : ControllerBase
    {
        private const string TABLE_NAME = "Blog";
        private IDynamoDBContext _context;

        public BlogsController()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Blog)] = new Amazon.Util.TypeMapping(typeof(Blog), TABLE_NAME);

            var config = new DynamoDBContextConfig 
            { 
                Conversion = DynamoDBEntryConversion.V2 
            };
            
            _context = new DynamoDBContext(new AmazonDynamoDBClient(RegionEndpoint.USWest2), config);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Blog>>> Get()
        {
            var search = _context.ScanAsync<Blog>(null);
            var blogs = await search.GetNextSetAsync();

            return blogs;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Blog>> Get(string id)
        {
            var blog = await _context.LoadAsync<Blog>(id);
            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Blog blog)
        {
            if (blog == null)
            {
                return BadRequest();
            }

            await _context.SaveAsync<Blog>(blog);
            
            return CreatedAtAction(nameof(Get), new { id = blog.Id }, blog);
        }
    }
}
