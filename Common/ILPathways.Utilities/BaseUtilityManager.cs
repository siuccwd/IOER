using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Resources;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;


namespace ILPathways.Utilities
{
	/// <summary>
	/// Summary description for BaseUtilityManager
	/// </summary>
  public class BaseUtilityManager : IDisposable
	{
		public BaseUtilityManager()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		//Implement IDisposable.
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		} //

		protected virtual void Dispose( bool disposing )
		{
			if ( disposing )
			{
				// Free other state (managed objects).
			}
			// Free your own state (unmanaged objects).
			// Set large fields to null.
		} //

		// Use C# destructor syntax for finalization code.
        ~BaseUtilityManager()
		{
			// Simply call Dispose(false).
			Dispose( false );
		}

	}
}