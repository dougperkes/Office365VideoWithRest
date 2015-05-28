using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace o365Sample.Models
{
    public class Office365Context
    {
        public List<Office365DiscoveryResource> Resources { get; set; }

        public List<OAuthTokenResponse> Tokens { get; set; }
    }
}