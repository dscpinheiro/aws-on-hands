using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace dotnet_core_tutorial
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
              .UseKestrel()
              .UseContentRoot(Directory.GetCurrentDirectory())
              .UseIISIntegration()
              .UseStartup<Startup>()
              .Build();

            host.Run();
        }
    }
}
