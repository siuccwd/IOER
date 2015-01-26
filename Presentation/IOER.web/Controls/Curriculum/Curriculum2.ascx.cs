using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;

namespace ILPathways.Controls.Curriculum
{
  public partial class Curriculum2 : System.Web.UI.UserControl
  {
    public List<Common.CodeItem> k12Subjects { get; set; }
    public List<Common.CodeItem> gradeLevels { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      LoadFormsData();
    }

    protected void LoadFormsData()
    {
      //K-12 Subjects
      k12Subjects = CodeTableBizService.Resource_CodeTableSelectList( "Codes.Subject" );
      gradeLevels = CodeTableBizService.Resource_CodeTableSelectList( "Codes.GradeLevel" );
    }
  }
}