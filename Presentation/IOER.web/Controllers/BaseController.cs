using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ILPathways.Controllers
{
	public class BaseController
	{

		/// <summary>
		/// Check if the passed dataset is indicated as one containing an error message (from a web service)
		/// </summary>
		/// <param name="wsDataset">DataSet for a web service method</param>
		/// <returns>True if dataset contains an error message, otherwise false</returns>
		public static bool HasErrorMessage( DataSet wsDataset )
		{

			if ( wsDataset.DataSetName == "ErrorMessage" )
				return true;
			else
				return false;

		} //
	}
}