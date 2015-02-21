using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isle.DTO.Common
{
  public class AJAXResponse
  {
      public object data { get; set; }
      public bool valid { get; set; }
      public string status { get; set; }
      public object extra { get; set; }
  }
}
