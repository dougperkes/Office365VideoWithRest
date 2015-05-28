using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace o365Sample.Models
{
    public class VideoPlaceholderModel
    {
        public VideoPlaceholderModel()
        {
            __metadata = new Metadata();    
        }

        public Metadata __metadata { get; set; }
        public class Metadata
        {
            public string Description { get; set; }
            public string Title { get; set; }
            public string FileName { get; set; }
            public string Type { get; set; }
        }
    }

    
}