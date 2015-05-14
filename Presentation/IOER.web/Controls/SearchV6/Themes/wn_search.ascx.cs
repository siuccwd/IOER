using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using System.Drawing;

namespace ILPathways.Controls.SearchV6.Themes
{
  public partial class wn_search : SearchTheme
  {
    /* --- Initialization --- */
    public wn_search()
    {
      //Fields = new ResourceV2Services().GetFieldAndTagCodeData();
      //Temporary
      var fields = "accessRights,accessibilityControl,accessibilityFeature,accessibilityHazard,educationalRole,careerCluster,careerPlanning,disabilityTopic,employerProgram,jobPreparation,inLanguage,mediaType,learningResourceType,resources,wfePartner,wioaWorks,workplaceSkill,region,subject,layoffAssistance,ilPathway,workNetArea,guidanceScenario,wdqi,demandDrivenIT".Split( ',' ).ToList();
      SetFields( fields );
      SiteId = 3;
      UseResourceUrl = true;
      MainColor = ColorTranslator.FromHtml( "#B74900" );
    }

    protected void Page_Load( object sender, EventArgs e )
    {

    }
  }
}