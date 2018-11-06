using System.Collections.Generic;
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
        public byte VpcCidr { get; set; }

        [DataMember]
        public List<byte> SubnetCidrs { get; set; }
    }

    [DataContract]
    public class ResponseData
    {
        [DataMember]
        public string VpcAddressRange { get; set; }

        [DataMember]
        public List<string> SubnetsAddressRanges { get; set; }
    }
}
