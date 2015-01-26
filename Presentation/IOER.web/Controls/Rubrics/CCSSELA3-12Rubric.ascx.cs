using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using LRWarehouse.Business;


namespace ILPathways.Controls.Rubrics
{
  public partial class CCSSELA3_12Rubric : BaseRubricModule
  {
    public override bool IsApplicable( Resource resource )
    {
      return true;
    }

    protected void Page_Load( object sender, EventArgs e )
    {

    }

  }
}