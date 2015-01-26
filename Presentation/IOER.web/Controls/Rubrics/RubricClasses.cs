using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LRWarehouse.Business;

namespace ILPathways.Controls.Rubrics
{
  /* Base Rubric */
  public abstract class BaseRubricModule : System.Web.UI.UserControl
  {
    public BaseRubricModule()
    {
      dimensions = new List<BaseRubricDimensionModule>();
    }
    public List<BaseRubricDimensionModule> dimensions { get; set; }

    public abstract bool IsApplicable( Resource resource );
  }

  /* Base Dimension */
  public class BaseRubricDimensionModule
  {
    public int id { get; set; }
    public decimal score { get; set; }
    public string name { get; set; }
  }

  /* Data Transfer */
  public class RubricDTO
  {
    public RubricDTO()
    {
      dimensions = new List<BaseRubricDimensionModule>();
    }
    public List<BaseRubricDimensionModule> dimensions { get; set; }
    public int resourceIntID { get; set; }
    public int rubricID { get; set; }
    public string selectedRubric { get; set; }
    public string title { get; set; }
    public string resourceURL { get; set; }
    public string previewMode { get; set; }
  }
}