using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using LRWarehouse.Business.ResourceV2;
using System.Drawing;

namespace IOER.Controls.SearchV6.Themes
{
  public partial class ioer_library : SearchTheme
  {
    /* --- Initialization --- */
    public ioer_library()
    {
			//Load all potentially applicable fields - they will be trimmed down later
      //Fields = new ResourceV2Services().GetFieldAndTagCodeData();
			
			var fields = "educationalRole,careerCluster,educationalUse,gradeLevel,inLanguage,mediaType,learningResourceType,k12Subject,nrsEducationalFunctioningLevel".Split( ',' ).ToList();
			SetFields( fields );

      SiteId = 1;
      UseResourceUrl = false;
      MainColor = ColorTranslator.FromHtml( "#3572B8" );
    }

		protected void Page_Load( object sender, EventArgs e )
		{
		}
	}
}