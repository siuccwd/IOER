using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILPathways.Utilities;
using ReportingManager = ILPathways.DAL.ActivityOrgReportingManager;
using IOER.Library;
using Isle.BizServices;
using Isle.DTO.Reports;

namespace IOER.Organizations.controls.Reports
{
    public partial class LibraryViews : BaseUserControl
    {
        private bool _useParentControlOrgId { get; set; }
        public bool UseParentControlOrgId
        {
            get { return this._useParentControlOrgId; }
            set { this._useParentControlOrgId = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlNoResults.Visible = false;
            if (WebUser == null)
            {
                SetConsoleErrorMessage("You must be authenticated and authorized to use this report.");
                this.Visible = false;
            }
            if (!Page.IsPostBack)
            {
                InitializeForm();
            }
        }

        protected void InitializeForm()
        {
            if (UseParentControlOrgId)
            {
                txtOrgId.Text = ((Literal)Parent.FindControl("txtCurrentOrgId")).Text;
            }
            else
            {
                txtOrgId.Text = FormHelper.GetRequestKeyValue("organizationID", "0");
                if (txtOrgId.Text == "0")
                {
                    SetConsoleErrorMessage("Organization is missing.");
                    this.Visible = false;
                }
            }
        }

        protected void ReadOptionalInfoCheckboxes(ref bool includeCollection, ref bool includeResource)
        {
            foreach (ListItem item in cblOptionalInfo.Items)
            {
                switch (item.Value)
                {
                    case "Collection Views":
                        includeCollection = item.Selected;
                        break;
                    case "Resource Views":
                        includeResource = item.Selected;
                        break;
                }
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            bool isValid = AllFieldsValid();
            if (isValid)
            {
                int orgId = int.Parse(txtOrgId.Text);
                DateTime? pStartDate, pEndDate;
                pStartDate = pEndDate = null;
                DateTime startDate, endDate;
                string status = "successful";
                bool includeCollection = false;
                bool includeResource = false;
                DateTime.TryParse(BaseFilters.StartDate, out startDate);
                DateTime.TryParse(BaseFilters.EndDate, out endDate);
                if (BaseFilters.StartDate != string.Empty) pStartDate = (DateTime?)startDate;
                if (BaseFilters.EndDate != string.Empty) pEndDate = (DateTime?)endDate;

                List<OrgLibraryView> dbRows = new ReportingManager().GetLibraryViews((int?)orgId, pStartDate, pEndDate, ref status);
                if (status == "successful")
                {
                    ReadOptionalInfoCheckboxes(ref includeCollection, ref includeResource);
                    LoadGrid(dbRows, includeCollection, includeResource);
                }
            }
        }

        protected bool AllFieldsValid()
        {
            bool retVal = true;
            bool dateValid = true;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            string errorMessages = "";
            if (BaseFilters.StartDate != string.Empty)
            {
                dateValid = DateTime.TryParse(BaseFilters.StartDate, out startDate);
                if (!dateValid)
                {
                    retVal = false;
                    errorMessages+="Start Date is not a valid date.<br />";
                }
            }
            if (BaseFilters.EndDate != string.Empty)
            {
                dateValid = DateTime.TryParse(BaseFilters.EndDate, out endDate);
                if (!dateValid)
                {
                    retVal = false;
                    errorMessages+="End Date is not a valid date.<br />";
                }
            }
            if (BaseFilters.StartDate != string.Empty && BaseFilters.EndDate != string.Empty)
            {
                if (startDate > endDate)
                {
                    retVal = false;
                    errorMessages+="End Date must be greater than or equal to Start Date.<br />";
                }
            }

            errorMessages = new Regex("<br />$").Replace(errorMessages, "");

            if (errorMessages != string.Empty)
            {
                SetConsoleErrorMessage(errorMessages);
            }

            return retVal;
        }

        private void LoadGrid(List<OrgLibraryView> dbRows, bool includeCollection, bool includeResource)
        {
            List<LibraryViewGridRow> gridRows = new List<LibraryViewGridRow>();
            foreach (OrgLibraryView library in dbRows)
            {
                LibraryViewGridRow row = new LibraryViewGridRow
                {
                    Title = library.ObjectTitle,
                    Type = "Library",
                    Views = library.NbrViews
                };
                gridRows.Add(row);
                if (includeResource)
                {
                    List<int> resInts = library.OrgResourceViews.Select(x => x.ObjectId).Distinct().ToList();
                    foreach (int resourceIntId in resInts)
                    {
                        List<OrgResourceView> resView = library.OrgResourceViews.Where(x => x.ObjectId == resourceIntId).ToList();
                        row = new LibraryViewGridRow
                        {
                            Title = "<span style='margin-left:40px;'>" + resView.FirstOrDefault().ObjectTitle + "</span>",
                            Type = "Resource",
                            Views = 0
                        };
                        foreach (OrgResourceView view in resView)
                        {
                            row.Views += view.NbrViews;
                        }
                        gridRows.Add(row);
                    }
                }
                if (includeCollection)
                {
                    foreach (OrgCollectionView collection in library.OrgCollectionViews)
                    {
                        row = new LibraryViewGridRow
                        {
                            Title = "<span style='margin-left:20px;'>" + collection.ObjectTitle + "</span>",
                            Type = "Collection",
                            Views = collection.NbrViews
                        };
                        gridRows.Add(row);
                        if (includeResource)
                        {
                            foreach (OrgResourceView resource in collection.OrgResourceViews)
                            {
                                row = new LibraryViewGridRow
                                {
                                    Title = "<span style='margin-left:40px;'>" + resource.ObjectTitle + "</span>",
                                    Type = "Resource",
                                    Views = resource.NbrViews
                                };
                                gridRows.Add(row);
                            }// foreach resource
                        }

                    }// foreach collection
                }
            }// foreach library

            if (gridRows.Count() == 0)
            {
                pnlNoResults.Visible = true;
            }
            else
            {
                pnlNoResults.Visible = false;
            }
            gvResults.DataSource = gridRows;
            gvResults.DataBind();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            bool isValid = AllFieldsValid();
            if (isValid)
            {
                int orgId = int.Parse(txtOrgId.Text);
                DateTime? pStartDate, pEndDate;
                pStartDate = pEndDate = null;
                DateTime startDate, endDate;
                string status = "successful";
                bool includeCollection = false;
                bool includeResource = false;
                DateTime.TryParse(BaseFilters.StartDate, out startDate);
                DateTime.TryParse(BaseFilters.EndDate, out endDate);
                if (BaseFilters.StartDate != string.Empty) pStartDate = (DateTime?)startDate;
                if (BaseFilters.EndDate != string.Empty) pEndDate = (DateTime?)endDate;

                List<OrgLibraryView> dbRows = new ReportingManager().GetLibraryViews((int?)orgId, pStartDate, pEndDate, ref status);
                if (dbRows.Count() == 0)
                {
                    pnlNoResults.Visible = true;
                    gvResults.DataSource = null;
                    gvResults.DataBind();
                    return;
                }
                else
                {
                    pnlNoResults.Visible = false;
                }
                if (status == "successful")
                {
                    ReadOptionalInfoCheckboxes(ref includeCollection, ref includeResource);
                    ExportData(dbRows, includeCollection, includeResource);
                }
            }
        }

        protected void ExportData(List<OrgLibraryView> dbRows, bool includeCollection, bool includeResource)
        {
            InitReport("LibraryViews_{0}.csv");
            StringBuilder headerRow = null;
            StringBuilder row = null;
            bool firstTimeThru = true;
            foreach (OrgLibraryView library in dbRows)
            {
                if (!includeCollection && !includeResource)
                {
                    headerRow = new StringBuilder();
                    row = new StringBuilder();
                    BuildLibraryFields(row, headerRow, library, firstTimeThru);
                    WriteRows(row, headerRow, ref firstTimeThru);
                }// if not doing resource or collection
                else if (includeCollection && !includeResource)
                {
                    if (library.OrgCollectionViews == null || library.OrgCollectionViews.Count == 0)
                    {
                        headerRow = new StringBuilder();
                        row = new StringBuilder();
                        BuildLibraryFields(row, headerRow, library, firstTimeThru);
                        BuildCollectionFields(row, headerRow, null, firstTimeThru);
                        WriteRows(row, headerRow, ref firstTimeThru);
                    }
                    else
                    {
                        foreach (OrgCollectionView collection in library.OrgCollectionViews)
                        {
                            headerRow = new StringBuilder();
                            row = new StringBuilder();
                            BuildLibraryFields(row, headerRow, library, firstTimeThru);
                            BuildCollectionFields(row, headerRow, collection, firstTimeThru);
                            WriteRows(row, headerRow, ref firstTimeThru);
                        }
                    }
                }
                else if (!includeCollection && includeResource)
                {
                    if (library.OrgResourceViews == null || library.OrgResourceViews.Count == 0)
                    {
                        headerRow = new StringBuilder();
                        row = new StringBuilder();
                        BuildLibraryFields(row, headerRow, library, firstTimeThru);
                        row.Append(GetWriteableValue("", "Resource Title", headerRow, firstTimeThru));
                        row.Append(GetWriteableValue("", "Resource Views", headerRow, firstTimeThru));
                        WriteRows(row, headerRow, ref firstTimeThru);
                    }
                    else
                    {
                        List<int> resInts = library.OrgResourceViews.Select(x => x.ObjectId).Distinct().ToList();
                        foreach (int resourceIntId in resInts)
                        {
                            headerRow = new StringBuilder();
                            row = new StringBuilder();
                            BuildLibraryFields(row, headerRow, library, firstTimeThru);
                            List<OrgResourceView> resView = library.OrgResourceViews.Where(x => x.ObjectId == resourceIntId).ToList();
                            row.Append(GetWriteableValue(resView.FirstOrDefault().ObjectTitle, "Resource Title", headerRow, firstTimeThru));
                            int count = resView.Sum(x => x.NbrViews);
                            row.Append(GetWriteableValue(count,"Resource Views",headerRow,firstTimeThru));
                            WriteRows(row, headerRow, ref firstTimeThru);
                        }
                    }
                }
                else
                {
                    if ((library.OrgCollectionViews == null || library.OrgCollectionViews.Count() == 0) &&
                        (library.OrgResourceViews == null || library.OrgResourceViews.Count() == 0))
                    {
                        // print out library anyway
                        headerRow = new StringBuilder();
                        row = new StringBuilder();
                        BuildLibraryFields(row, headerRow, library, firstTimeThru);
                        BuildCollectionFields(row, headerRow, null, firstTimeThru);
                        row.Append(GetWriteableValue("", "Resource Title", headerRow, firstTimeThru));
                        row.Append(GetWriteableValue("", "Resource Views", headerRow, firstTimeThru));
                        WriteRows(row, headerRow, ref firstTimeThru);
                    }
                    else
                    {
                        // first do resource totals
                        List<int> resInts = library.OrgResourceViews.Select(x => x.ObjectId).Distinct().ToList();
                        foreach (int resourceIntId in resInts)
                        {
                            headerRow = new StringBuilder();
                            row = new StringBuilder();
                            BuildLibraryFields(row, headerRow, library, firstTimeThru);
                            BuildCollectionFields(row, headerRow, null, firstTimeThru);
                            List<OrgResourceView> resView = library.OrgResourceViews.Where(x => x.ObjectId == resourceIntId).ToList();
                            row.Append(GetWriteableValue(resView.FirstOrDefault().ObjectTitle, "Resource Title", headerRow, firstTimeThru));
                            int count = resView.Sum(x => x.NbrViews);
                            row.Append(GetWriteableValue(count, "Resource Views", headerRow, firstTimeThru));
                            WriteRows(row, headerRow, ref firstTimeThru);
                        }
                        // now do collections
                        foreach (OrgCollectionView collection in library.OrgCollectionViews)
                        {
                            if (collection.OrgResourceViews == null || collection.OrgResourceViews.Count() == 0)
                            {
                                // Print out collection anyway
                                headerRow = new StringBuilder();
                                row = new StringBuilder();
                                BuildLibraryFields(row, headerRow, library, firstTimeThru);
                                BuildCollectionFields(row, headerRow, collection, firstTimeThru);
                                row.Append(GetWriteableValue("", "Resource Title", headerRow, firstTimeThru));
                                row.Append(GetWriteableValue("", "Resource Views", headerRow, firstTimeThru));
                                WriteRows(row, headerRow, ref firstTimeThru);
                            }
                            else
                            {
                                foreach (OrgResourceView resource in collection.OrgResourceViews)
                                {
                                    headerRow = new StringBuilder();
                                    row = new StringBuilder();
                                    BuildLibraryFields(row, headerRow, library, firstTimeThru);
                                    BuildCollectionFields(row, headerRow, collection, firstTimeThru);
                                    row.Append(GetWriteableValue(resource.ObjectTitle, "Resource Title", headerRow, firstTimeThru));
                                    row.Append(GetWriteableValue(resource.NbrViews, "Resource Views", headerRow, firstTimeThru));
                                    WriteRows(row, headerRow, ref firstTimeThru);
                                }
                            }
                        }
                    }
                }
                /*if (includeResource)
                {
                    // if no resources viewed, you still need to print out the library
                    if (library.OrgResourceViews == null || library.OrgResourceViews.Count() == 0)
                    {
                        headerRow = new StringBuilder();
                        row = new StringBuilder();
                        BuildLibraryFields(row, headerRow, library, firstTimeThru);
                        if (includeCollection)
                        {
                            // if no collections viewed, still need collection fields to show on report
                            BuildCollectionFields(row, headerRow, null, firstTimeThru);
                        }
                        WriteRows(row, headerRow, ref firstTimeThru);

                    }
                    else
                    {
                        // Do resource totals first
                        List<int> resInts = library.OrgResourceViews.Select(x => x.ObjectId).Distinct().ToList();
                        foreach (int resourceIntId in resInts)
                        {
                            headerRow = new StringBuilder();
                            row = new StringBuilder();
                            BuildLibraryFields(row, headerRow, library, firstTimeThru);
                            if (includeCollection)
                            {
                                BuildCollectionFields(row, headerRow, null, firstTimeThru);
                            }
                            List<OrgResourceView> resView = library.OrgResourceViews.Where(x => x.ObjectId == resourceIntId).ToList();
                            row.Append(GetWriteableValue(resView.FirstOrDefault().ObjectTitle, "Resource Title", headerRow, firstTimeThru));
                            int count = resView.Sum(x => x.NbrViews);
                            row.Append(GetWriteableValue(count, "Resource Views", headerRow, firstTimeThru));
                            WriteRows(row, headerRow, ref firstTimeThru);
                        }// foreach resourceIntId
                    }
                }// if includeResource
                if (includeCollection)
                {
                    // if no collections viewed, you still need to print out the library
                    if (library.OrgCollectionViews == null || library.OrgCollectionViews.Count() == 0)
                    {
                        headerRow = new StringBuilder();
                        row = new StringBuilder();
                        BuildLibraryFields(row, headerRow, library, firstTimeThru);
                        BuildCollectionFields(row, headerRow, null, firstTimeThru);
                        WriteRows(row, headerRow, ref firstTimeThru);
                    }

                    foreach (OrgCollectionView collection in library.OrgCollectionViews)
                    {
                        if (includeResource)
                        {
                            // if no resource view in collection, still need to print out the collection
                            if (collection.OrgResourceViews == null || collection.OrgResourceViews.Count() == 0)
                            {
                                headerRow = new StringBuilder();
                                row = new StringBuilder();
                                BuildLibraryFields(row, headerRow, library, firstTimeThru);
                                BuildCollectionFields(row, headerRow, collection, firstTimeThru);
                                row.Append(GetWriteableValue("", "Resource Title", headerRow, firstTimeThru));
                                row.Append(GetWriteableValue("", "Resource Views", headerRow, firstTimeThru));
                                WriteRows(row, headerRow, ref firstTimeThru);
                            }
                            else
                            {
                                foreach (OrgResourceView resource in collection.OrgResourceViews)
                                {
                                    headerRow = new StringBuilder();
                                    row = new StringBuilder();
                                    BuildLibraryFields(row, headerRow, library, firstTimeThru);
                                    BuildCollectionFields(row, headerRow, collection, firstTimeThru);
                                    row.Append(GetWriteableValue(resource.ObjectTitle, "Resource Title", headerRow, firstTimeThru));
                                    row.Append(GetWriteableValue(resource.NbrViews, "Resource Views", headerRow, firstTimeThru));
                                    WriteRows(row, headerRow, ref firstTimeThru);
                                }
                            }
                        }
                        else
                        {
                            headerRow = new StringBuilder();
                            row = new StringBuilder();
                            BuildLibraryFields(row, headerRow, library, firstTimeThru);
                            BuildCollectionFields(row, headerRow, collection, firstTimeThru);
                            WriteRows(row, headerRow, ref firstTimeThru);
                        }
                    }// foreach collection
                }// if includeCollection
                 */

            }// foreach library

            EndReport();
        }

        protected void BuildLibraryFields(StringBuilder row, StringBuilder headerRow, OrgLibraryView library, bool isWritingHeader)
        {
            row.Append(GetWriteableValue(library.ObjectTitle, "Library Title", headerRow, isWritingHeader));
            row.Append(GetWriteableValue(library.NbrViews, "Library Views", headerRow, isWritingHeader));
        }

        protected void BuildCollectionFields(StringBuilder row, StringBuilder headerRow, OrgCollectionView collection, bool isWritingHeader)
        {
            if (collection == null)
            {
                row.Append(GetWriteableValue("", "Collection Title", headerRow, isWritingHeader));
                row.Append(GetWriteableValue("", "Collection Views", headerRow, isWritingHeader));
            }
            else
            {
                row.Append(GetWriteableValue(collection.ObjectTitle, "Collection Title", headerRow, isWritingHeader));
                row.Append(GetWriteableValue(collection.NbrViews, "Collection Views", headerRow, isWritingHeader));
            }
        }

        protected void WriteRows(StringBuilder row, StringBuilder headerRow, ref bool isWritingHeader)
        {
            if (isWritingHeader)
            {
                Response.Write(headerRow.ToString() + "\n");
                isWritingHeader = false;
            }
            Response.Write(row.ToString() + "\n");
        }
    }

    public class LibraryViewGridRow
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public int Views { get; set; }

        public LibraryViewGridRow()
        {
            this.Title = "";
            this.Type = "";
        }
    }

    public class LibraryViewExportRow
    {
        public string LibraryTitle { get; set; }
        public int LibraryViews { get; set; }
        public string CollectionTitle { get; set; }
        public int CollectionViews { get; set; }
        public string ResourceTitle { get; set; }
        public int ResourceViews { get; set; }

        public LibraryViewExportRow()
        {
            this.LibraryTitle = "";
            this.CollectionTitle = "";
            this.ResourceTitle = "";
        }
    }
}