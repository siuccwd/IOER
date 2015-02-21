using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
  public class DashboardResourceDTO
  {
      /// <summary>
      /// Unique id of item
      /// </summary>
    public int id { get; set; } 
      /// <summary>
      /// item title
      /// </summary>
    public string title { get; set; }

    /// <summary>
    /// Item image url
    /// </summary>
    public string imageUrl { get; set; } 
      /// <summary>
      /// Item url
      /// </summary>
    public string url { get; set; } 
      /// <summary>
      /// The container/origin of the Resource (e.g., the name of its collection or the name of the organization library it comes from )
      /// </summary>
    public string containerTitle { get; set; } 

      /// <summary>
      /// Date added to collection, or add resource added to IOER
      /// </summary>
    public DateTime DateAdded { get; set; }
  }
}
