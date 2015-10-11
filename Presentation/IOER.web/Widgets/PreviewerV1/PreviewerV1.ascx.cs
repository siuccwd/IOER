using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;

namespace IOER.Controls.PreviewerV1
{
  public partial class PreviewerV1 : BaseUserControl
  {
    public string userGuid { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      if ( WebUser != null && WebUser.Id != 0 )
      {
        userGuid = WebUser.RowId.ToString();
      }
    }
  }
}