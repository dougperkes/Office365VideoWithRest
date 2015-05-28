using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace o365Sample.Models
{
    public class Office365DiscoveryResource
    {
        public string capability { get; set; }
        public string entityKey { get; set; }
        public Guid providerId { get; set; }
        public string providerName { get; set; }
        public int serviceAccountType { get; set; }
        public string serviceApiVersion { get; set; }
        public string serviceEndpointUri { get; set; }
        public string serviceId { get; set; }
        public string serviceName { get; set; }
        public string serviceResourceId { get; set; }
    }
}