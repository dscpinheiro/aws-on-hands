using System.Runtime.Serialization;

namespace NetworkRangeManager
{
    [DataContract]
    public class CustomResourceRequest
    {
        [DataMember]
        public string RequestType { get; set; }

        [DataMember]
        public string ResponseURL { get; set; }

        [DataMember]
        public string StackId { get; set; }

        [DataMember]
        public string RequestId { get; set; }

        [DataMember]
        public string ResourceType { get; set; }

        [DataMember]
        public string LogicalResourceId { get; set; }

        [DataMember]
        public string PhysicalResourceId { get; set; }

        [DataMember]
        public ResourceProperties ResourceProperties { get; set; }
    }

    [DataContract]
    public class CustomResourceResponse
    {
        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string PhysicalResourceId { get; set; }

        [DataMember]
        public string StackId { get; set; }

        [DataMember]
        public string RequestId { get; set; }

        [DataMember]
        public string LogicalResourceId { get; set; }

        [DataMember]
        public ResponseData Data { get; set; }
    }

    [DataContract]
    public class ResourceProperties
    {
        [DataMember]
        public string VpcName { get; set; }

        [DataMember]
        public string VpcRange { get; set; }

        [DataMember]
        public byte Cidr { get; set; }

        [DataMember]
        public NetworkType NetworkType { get; set; }
    }

    [DataContract]
    public enum NetworkType
    {
        [DataMember]
        Vpc = 0,

        [DataMember]
        Subnet = 1
    }

    [DataContract]
    public class ResponseData
    {
        [DataMember]
        public string AddressRange { get; set; }
    }
}