using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;

namespace IOER.Controls
{
	public partial class UploadBoxV1 : System.Web.UI.UserControl
	{
		public string ClientBoxId { get; set; }
		public string GoogleDriveClientId { get; set; }
		public bool AllowMultiple { get; set; }
		public string Accept { get; set; } // File input's accept property value. Uses mime types. For example "image/*" for images
		public int MaxFileSizeInBytes { get; set; }
		public string RenderMode { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{
			SetupUploadDefaults();
		}
		//

		private void SetupUploadDefaults()
		{
			GoogleDriveClientId = UtilityManager.GetAppKeyValue( "googleClientId", "" );

			if ( string.IsNullOrWhiteSpace( ClientBoxId ) )
			{
				ClientBoxId = "ajaxUpload";
			}

			if ( MaxFileSizeInBytes == 0 )
			{
				MaxFileSizeInBytes = UtilityManager.GetAppKeyValue( "maxDocumentSize", 30000000 );
			}

			if ( string.IsNullOrWhiteSpace( RenderMode ) )
			{
				RenderMode = "block";
			}
		}
		//

	}
}