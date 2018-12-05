using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiXeduleApiGrabber.Models
{
    public class ScheduleResponse
    {
        public string Subject { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string[] ClassRooms { get; set; }
        public string[] Teachers { get; set; }
    }
}
