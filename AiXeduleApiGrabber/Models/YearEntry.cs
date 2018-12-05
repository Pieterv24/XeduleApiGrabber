using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiXeduleApiGrabber.Models
{
    public class YearEntry
    {
        public string Id { get; set; }
        public int Oru { get; set; }
        public int Year { get; set; }
        public int[] Schs { get; set; }
        public string Deps { get; set; }
        public string Avis { get; set; }
        public int PeriodCount { get; set; }
        public string Cal { get; set; }
        public DateTime IStart { get; set; }
        public DateTime IEnd { get; set; }
        public string IStartOfDay { get; set; }
        public string IEndOfDay { get; set; }
        public int FirstDayOfWeek { get; set; }
        public int LastDayOfWeek { get; set; }
    }
}
