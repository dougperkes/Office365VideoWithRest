using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace o365Sample.Models
{
    public class VideoFileInformation
    {
        public Guid ChannelID { get; set; }
        public DateTime CreateDate { get; set; }
        public string Description { get; set; }
        public string DisplayFormUrl { get; set; }
        public string FileName { get; set; }
        public string OwnerName { get; set; }
        public string ServerRelativeUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Title { get; set; }
        public Guid ID { get; set; }
        public string Url { get; set; }
        public int VideoDurationIsSeconds { get; set; }
        public int VideoProcessingStatus { get; set; }
        public int ViewCount { get; set; }
        public string YammerObjectUrl { get; set; }
    }
}