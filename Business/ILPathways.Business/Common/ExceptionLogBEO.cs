using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public class ExceptionLogBEO : BaseNameBEO
    {       
        
        public string MessageId { get; set; }
        public string MessageText { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }        
    }
}
