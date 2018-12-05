using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiXeduleApiGrabber.Models
{
    public class FacilityEntry
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public int[] Orus { get; set; }
    }
}
