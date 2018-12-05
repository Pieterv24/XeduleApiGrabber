using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiXeduleApiGrabber.Models
{
    public class ScheduleEntry
    {
        public string Id { get; set; }
        public DateTime IPublicationDate { get; set; }
        public bool Concept { get; set; }
        public CourseEntry[] Apps { get; set; }
    }

    public class CourseEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Attention { get; set; }
        public DateTime IStart { get; set; }
        public DateTime IEnd { get; set; }
        public int[] Atts { get; set; }
    }
}
