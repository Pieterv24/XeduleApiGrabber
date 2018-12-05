using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;

namespace AiXeduleApiGrabber.Models
{
    public class CacheEntry
    {
        public string GroupName { get; set; }
        public Calendar Calendar { get; set; }
        public DateTime lastUpdated { get; set; }
    }
}
