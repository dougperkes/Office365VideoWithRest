using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace o365Sample.Models
{
    public class Office365VideoServiceDiscoveryResponse
    {
        public bool IsVideoPortalEnabled { get; set; }
        public string VideoPortalUrl { get; set; }
    }
}