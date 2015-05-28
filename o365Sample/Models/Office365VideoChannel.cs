using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace o365Sample.Models
{
    public class Office365VideoChannel
    {
        public string Description { get; set; }
        public Guid Id { get; set; }
        public String TitleHtmlColor { get; set; }
        public string Title { get; set; }
        public bool YammerEnabled { get; set; }
    }
}