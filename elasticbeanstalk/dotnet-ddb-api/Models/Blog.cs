using System;

namespace dotnet_ddb_api.Models
{
    public class Blog
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int CreatedTimestamp { get; set; }
    }
}