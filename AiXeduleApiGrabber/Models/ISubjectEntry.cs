using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiXeduleApiGrabber.Models
{
    public interface ISubjectEntry
    {
        string Id { get; set; }
        string Code { get; set; }
        int[] Orus { get; set; }
    }
}
