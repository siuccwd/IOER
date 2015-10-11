using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkChecker2.App_Code.BusObj
{
    public class KnownBadTitle
    {
        public int Id { get; set; }
        public string HostName { get; set; }
        public string Title { get; set; }
        public bool TitleIsRegex { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
