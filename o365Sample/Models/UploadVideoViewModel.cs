using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace o365Sample.Models
{
    public class UploadVideoViewModel
    {
        public Guid ChannelId { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

    }
}