﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOER.Widgets.FullSearch
{
  public partial class Default : System.Web.UI.Page
  {
    protected void Page_Load( object sender, EventArgs e )
    {
			search.LoadTheme = string.IsNullOrWhiteSpace( Request.Params[ "theme" ] ) ? defaultTheme.Text : ( string ) Request.Params[ "theme" ];
    }
  }
}