using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<Product> Products { get; set; }

        public async Task OnGet([FromServices]ProductsClient client)
        {
            Products = await client.GetProducts();
        }
    }
}
