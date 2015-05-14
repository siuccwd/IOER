using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using LRWarehouse.Business.ResourceV2;
using System.Drawing;

namespace ILPathways.Controls.SearchV6.Themes
{
  public partial class ioer : SearchTheme
  {
    /* --- Initialization --- */
    public ioer()
    {
      //var fields = ltlFieldList.Text.Split( ',' ).ToList();
      //Temporary
      var fields = "accessRights,accessibilityAPI,accessibilityControl,accessibilityFeature,accessibilityHazard,educationalRole,careerCluster,educationalUse,gradeLevel,groupType,inLanguage,mediaType,learningResourceType,k12Subject,assessmentType".Split( ',' ).ToList();
      SetFields( fields );

      //Fields = new ResourceV2Services().GetFieldAndTagCodeData();
      SiteId = 1;
      UseResourceUrl = false;
      MainColor = ColorTranslator.FromHtml( "#3572B8" );
    }

    protected void Page_Load( object sender, EventArgs e )
    {
      
    }

  }
}