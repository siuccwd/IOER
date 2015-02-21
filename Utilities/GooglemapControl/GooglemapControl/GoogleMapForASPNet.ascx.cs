//   Google Maps User Control for ASP.Net version 1.0:
//   ========================
//   Copyright (C) 2008  Shabdar Ghata 
//   Email : ghata2002@gmail.com
//   URL : http://www.shabdar.org

//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.

//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.

//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.

//   This program comes with ABSOLUTELY NO WARRANTY.
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Drawing;
using System.Web.Services;

public partial class GoogleMapForASPNet : System.Web.UI.UserControl
{
    #region Properties

    GoogleObject _googlemapobject = new GoogleObject();
    public GoogleObject GoogleMapObject
    {
        get { return _googlemapobject; }
        set { _googlemapobject = value; }
    }

    #endregion


    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Session["GOOGLE_MAP_OBJECT"] = GoogleMapObject;
        }
        else
        {
            GoogleMapObject = (GoogleObject)Session["GOOGLE_MAP_OBJECT"];
        }
    }
}
