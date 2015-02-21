using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
  public class ResourcesBox
  {
    public ResourcesBox()
    {
      resources = new List<DashboardResourceDTO>();
    }
    public string name { get; set; }
    public int total { get; set; }
    public List<DashboardResourceDTO> resources { get; set; }
  }
}
