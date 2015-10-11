using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using IOER.Library;

namespace IOER.Pages.Developers
{
	public class DocumentationItem : BaseUserControl
	{
		public DocumentationItem()
		{
			PageTitle = "Page Title";
			UpdatedDate = DateTime.Now;
		}

		public string PageTitle { get; set; }
		public DateTime UpdatedDate { get; set; }
	}
}