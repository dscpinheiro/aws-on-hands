using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace NetworkRangeManager
{
    class NetworkRange
    {
        [DynamoDBHashKey]
        public string AddressRange { get; set; }
        public string VpcName { get; set; }

        public override bool Equals(object obj) => Equals(obj as NetworkRange);
        public bool Equals(NetworkRange other) => other != null && AddressRange == other.AddressRange && VpcName == other.VpcName;
        public override int GetHashCode() => (AddressRange, VpcName).GetHashCode();
    }
}