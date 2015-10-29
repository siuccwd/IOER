using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using LRWarehouse.DAL;

namespace IOER.Pages.Developers
{
	public partial class LinkChecker : DocumentationItem
	{
		public LinkChecker()
		{
			PageTitle = "Link Checker";
			UpdatedDate = DateTime.Parse( "2015/10/20" );
		}

		protected void Page_Load( object sender, EventArgs e )
		{

		}

        protected void HandleButton(string commandName)
        {
            DataSet ds = new DataSet();
            string status = "successful";
            List<string> columnNamesToExclude = (new string[5] {"Id","Created","CreatedBy","LastUpdated","LastUpdatedBy"}).ToList();
            switch (commandName)
            {
                case "BadContent":
                    ds = new LinkCheckerRulesManager().GetKnownBadContent(ref status);
                    if (DoesDataSetHaveRows(ds))
                    {
                        InitReport("KnownBadContent_{0}.csv");
                    }
                    break;
                case "BadTitle":
                    ds = new LinkCheckerRulesManager().GetKnownBadTitle(ref status);
                    if (DoesDataSetHaveRows(ds))
                    {
                        InitReport("KnownBadTitle_{0}.csv");
                    }
                    break;
                case "404Pages":
                    ds = new LinkCheckerRulesManager().GetKnown404Pages(ref status);
                    if (DoesDataSetHaveRows(ds))
                    {
                        InitReport("Known404Pages_{0}.csv");
                    }
                    break;
            }

            if (DoesDataSetHaveRows(ds))
            {
                bool firstTimeThru = true;
                StringBuilder headerRow = new StringBuilder();
                StringBuilder row = new StringBuilder();
                StreamWriter sw = new StreamWriter(UtilityManager.GetAppKeyValue("serverImageFilePath") + commandName + ".html", false, Encoding.UTF8);
                int columnCount = ds.Tables[0].Columns.Count;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    headerRow=new StringBuilder();
                    row = new StringBuilder();
                    for (int i = 0; i < columnCount; i++)
                    {
                        if (!columnNamesToExclude.Contains(ds.Tables[0].Columns[i].ColumnName))
                        {
                            if (!Convert.IsDBNull(dr[i]))
                            {
                                string val = dr[i].ToString().Replace("%>", "%\\>");
                                row.Append(GetWriteableValue(val, ds.Tables[0].Columns[i].ColumnName, headerRow, firstTimeThru, false));
                            }
                            else
                            {
                                row.Append(GetWriteableValue("NULL", ds.Tables[0].Columns[i].ColumnName, headerRow, firstTimeThru, false));
                            }
                        }
                    }// for
                    if (firstTimeThru)
                    {
                        Response.Write(headerRow.ToString() + "\n");
                        firstTimeThru = false;
                    }
                    Response.Write(row.ToString() + "\n");
                }// foreach
                EndReport();
            }
        }

        protected void btnBadContent_Click(object sender, EventArgs e)
        {
            HandleButton("BadContent");
        }

        protected void btnBadTitle_Click(object sender, EventArgs e)
        {
            HandleButton("BadTitle");
        }

        protected void btn404Pages_Click(object sender, EventArgs e)
        {
            HandleButton("404Pages");
        }

        
	}
}