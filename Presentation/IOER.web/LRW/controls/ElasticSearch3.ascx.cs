using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ILPathways.Utilities;

namespace ILPathways.LRW.controls
{
    public partial class ElasticSearch3 : System.Web.UI.UserControl
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            string imgUrl = ContentHelper.GetAppKeyValue( "cachedImagesUrl", "//ioer.ilsharedlearning.org/OERThumbs/" );
            this.imgThumbnail.Src = imgUrl + "large/{intID}-large.png";
        }
    }
}