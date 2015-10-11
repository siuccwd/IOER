using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILPathways.Utilities;
using IOER.Library;
using Isle.BizServices;

using LRWarehouse.Business;
using LumenWorks.Framework.IO.Csv;
using MyManager = Isle.BizServices.AccountServices;
using OrgManager = Isle.BizServices.OrganizationBizService;

namespace IOER.Organizations.controls
{
	public partial class Import : BaseUserControl
	{
		const string thisClassName = "IOER.Controls.OrgMgmt_Import";
		const string fileName = "";
		MyManager myManager = new MyManager();
		OrgManager orgManager = new OrgManager();

		int insertErrors = 0;
		int insertSuccess = 0;
		int recordSkipped = 0;

		string lastRecordSummary = "";
		#region Properties
		/// <summary>
		/// The last selected, or current orgId
		/// </summary>
		public int LastOrgId
		{
			get
			{
				if (Session["LastOrgId"] == null)
					Session["LastOrgId"] = "0";

				return Int32.Parse(Session["LastOrgId"].ToString());
			}
			set { Session["LastOrgId"] = value.ToString(); }
		}
		/// <summary>
		/// Get/Set target file name
		/// </summary>
		public string FileName
		{
			get
			{
				return importFileName.Text;
			}

			set { importFileName.Text = value; }
		}

		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			if (this.WebUser == null)
			{
				//should be taken care of at a higher level
				//SetConsoleErrorMessage("You must be authenticated to use this page. Please log in and then return to this page.");
				PanelUpload.Visible = false;
				PanelView.Visible = false;
				PanelImport.Visible = false;
				importPanel.Visible = false;
				importDetailsPanel.Visible = false;
				return;
			}

		}

		/// <summary>
		/// called by parent in prep for import
		/// </summary>
		public void InitializeImport()
		{
			ResetForm();
			//should have orgId, would like title for email
			if (LastOrgId > 0)
			{
				Get(LastOrgId);
			}
		}
		public void Get(int recId)
		{
			try
			{
				//get record
				Organization entity = OrgManager.EFGet(recId);

				if (entity == null || entity.IsValid == false)
				{
					this.SetConsoleErrorMessage("Sorry the requested organization does not exist");
					return;

				}
				else
				{
					txtOrgName.Text = entity.Name;
				}

			}
			catch (System.Exception ex)
			{
				//Action??		- display message and close form??	
				LoggingHelper.LogError(ex, thisClassName + ".Get() - Unexpected error encountered");
				this.SetConsoleErrorMessage("Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString());

			}

		}	// End method
		/// <summary>
		/// This method is obsolete - only one type of import so no need for it
		/// Action ????
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnShowUpload_Click(object sender, System.EventArgs e)
		{
			ResetForm();
		}
		public void ResetForm()
		{
			PanelUpload.Visible = true;
			PanelView.Visible = false;
			PanelImport.Visible = false;
			importDetailsPanel.Visible = false;

			//btnShowUpload.Enabled = false;
			this.ViewDataButton.Enabled = true;
			this.ImportDataButton.Enabled = true;

			this.ViewDataButton.Enabled = false;

			ViewDataButton2.Enabled = false;
			ImportDataButton2.Enabled = false;

			LabelImport.Text = "";
			lblError.Text = "";
			formGrid.DataSource = null;
			formGrid.DataBind();
		}

		protected void ButtonUploadFile_Click(object sender, System.EventArgs e)
		{
			//myc.LastActivePane = MyController.SYEP_GROUP_INVOICES_PANE;
			if (FileUpload.HasFile)
			{

				try
				{
					//check file type
					string sFileName = FileUpload.FileName;
					string destFile = "";

					//get file extension
					string sFileType = "";
					sFileType = System.IO.Path.GetExtension(sFileName);
					sFileType = sFileType.ToLower();

					//setup parameters for new file name
					string newFile = "";
					DateTime dt = DateTime.Now;
					string newDate = dt.ToString("MMddyyyyss");
					int targetOrgId = LastOrgId;

					//if ( ddlOrgList.Items.Count > 0 && ddlOrgList.SelectedIndex > 0 )
					//{
					//    targetOrgId = Int32.Parse( ddlOrgList.SelectedValue );

					//}
					//else if ( Int32.TryParse( txtOrgId.Text, out targetOrgId ) )
					//{

					//}
					//else
					//{
					//    if ( WebUser.OrgId > 0 )
					//        targetOrgId = WebUser.OrgId;
					//}

					if (targetOrgId == 0)
					{
						//now what
					}

					//user may ber importing to different org than direct
					string fileId = "Org-" + targetOrgId.ToString();

					destFile = "IOER_" + fileId + "_" + newDate + sFileType;

					lblImportFile.Text = sFileName + " (server file: " + destFile + ")";


					//filter file options based upon the file type

					switch (sFileType)
					{
						case ".xls":
							SetConsoleErrorMessage("Sorry this file type is not allowed. Please use a comma separated format (CSV) file only");
							break;
						case ".csv":

							//FileUpload.SaveAs(Server.MapPath("~/ExcelImport.xls"));
							string logFile = UtilityManager.GetAppKeyValue("path.ReportsOutputPath", "C:\\VOS_LOGS.txt");
							FileUpload.SaveAs(logFile + destFile);
							//save server filename
							FileName = logFile + destFile;

							LabelUpload.Text = "Upload File Name: " +
									FileUpload.PostedFile.FileName + "<br>" +
									"Type: " + FileUpload.PostedFile.ContentType +
									" File Size: " + FileUpload.PostedFile.ContentLength +
									" kb<br>";

							btnShowUpload.Enabled = true;

							if (doAutoViewAfterLoad.Text.Equals("yes"))
							{
								PreviewData();
							}
							else
							{
								PanelUpload.Visible = false;
								fileUploadSuccessfulPanel.Visible = true;
							}
							break;
						default:
							SetConsoleErrorMessage("File Type Is Incorrect. Please use a comma separated format (CSV) file only!" + errorMessage.Text);
							FileUpload.Dispose();
							break;
					}


				}
				catch (System.NullReferenceException ex)
				{
					LabelUpload.Text = "Error: " + ex.Message + errorMessage.Text;
				}
			}
			else
			{
				SetConsoleErrorMessage("Please select a file to upload and then try again.");
			}
		}//

		/// <summary>
		/// handle preview of file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ViewDataButton_Click(object sender, System.EventArgs e)
		{
			//myc.LastActivePane = 4; // MyController.LWIA_GROUP_INVITATION_PANE;
			PreviewData();
		}//

		/// <summary>
		/// handle preview of file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void PreviewData()
		{
			PanelView.Visible = true;
			ImportDataButton2.Enabled = true;

			PanelUpload.Visible = false;
			PanelImport.Visible = false;
			bool doingValidationOnly = true;

			//lblDataViewMessage.Text = successfulViewMsg.Text;
			int expectedColumns = Int32.Parse(importColumnsCount.Text);
			string sFileType = System.IO.Path.GetExtension(FileName);
			sFileType = sFileType.ToLower();

			//filter file options based upon the file type
			try
			{
				switch (sFileType)
				{

					case ".xls":
						SetConsoleErrorMessage("This file type is not allowed. Only a comma separated file (extension of .csv) may be used for data import.");
						ResetForm();
						ImportDataButton2.Enabled = false;
						break;
					case ".csv":
						if (HandleFileUsingCsvReader(FileName, doingValidationOnly) == true)
						{
							lblDataViewMessage.Visible = true;
						}
						else
						{
							//error so disable import option
							ImportDataButton2.Enabled = false;
							lblDataViewMessage.Visible = false;
							if (showImportLinkOnErrors.Text.Equals("yes"))
							{
								ImportDataButton2.Enabled = true;
							}
						}
						importDetailsPanel.Visible = true;
						formGrid.Visible = true;
						previewGrid.Visible = false;

						break;

					default:
						SetConsoleErrorMessage("This file type is not allowed. Only a comma separated file (extension of .csv) may be used for data import.");
						ResetForm();
						ImportDataButton2.Enabled = false;
						break;
				}
			}
			catch (Exception ex)
			{
				SetConsoleErrorMessage("There was a problem reading the file. Error = " + ex + errorMessage.Text);
				ImportDataButton2.Enabled = false;
			}

		}

		/// <summary>
		/// import file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ImportDataButton_Click(object sender, System.EventArgs e)
		{
			PanelUpload.Visible = false;
			PanelView.Visible = false;
			PanelImport.Visible = true;
			LabelImport.Text = "";
			// OK to go back to view?
			ViewDataButton2.Enabled = true;


			try
			{

				//determine file type
				string sFileType = System.IO.Path.GetExtension(FileName);
				sFileType = sFileType.ToLower();

				//filter file options based upon the file type
				switch (sFileType)
				{
					case ".csv":
						HandleFileUsingCsvReader(FileName, false);
						this.ViewDataButton.Enabled = false;
						this.ImportDataButton.Enabled = false;

						ViewDataButton2.Enabled = false;
						this.ImportDataButton2.Enabled = false;
						break;
					default:
						break;
				}

			}
			catch (Exception exp)
			{
				SetConsoleErrorMessage("Could not import the file! Error = " + exp + errorMessage.Text);
				LabelImport.Text = LabelImport.Text.Replace("<span style='font-weight:bold;color:green;'>Import Succeeded</span>", "<span style='font-weight:bold;color:yellow;background-color: #000;'>Import cancelled</span>");
				LabelImport.Text = LabelImport.Text.Replace("Import Succeeded", "***Import Cancelled***");
				//
				ResetForm();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected bool HandleFileUsingCsvReader(string importFile, bool doingValidationOnly)
		{
			bool isValid = true;
			importDetailsPanel.Visible = true;

			int counter = 0;
			insertErrors = 0;
			insertSuccess = 0;
			recordSkipped = 0;
			int id = 0;
			string currentRecord = "";
			string recordMessages;
			int parentId = 0;
			int validColumnsCount = Int32.Parse(importColumnsCount.Text);

			PatronImport import = new PatronImport();
			List<PatronImport> list = new List<PatronImport>();

			string rowMessage = "";
			string statusMessage = "";
			string headerColumns = "";

			try
			{
				using (CsvReader csv = new CsvReader(new StreamReader(importFile), true))
				{
					int fieldCount = csv.FieldCount;
					if (fieldCount < validColumnsCount)
					{
						isValid = false;
						SetConsoleErrorMessage(string.Format("Error: there should be {0} columns in the import file and your file has {1} columns.", importColumnsCount.Text, fieldCount.ToString()) + "<br/>Valid column headers are:<br/>" + validColumns.Text);
						return false;
					}

					if (isValid)
					{
						string[] headers = csv.GetFieldHeaders();

						if (ValidateColumnNames(headers, validColumnsCount) == false)
							return false;
					}
					bool isRowValid;

					formGrid.Visible = true;
					bool isDuplicate = false;
					#region Read through file
					while (csv.ReadNextRecord())
					{
						counter++;
						rowMessage = "";
						statusMessage = "";
						isRowValid = true;
						isDuplicate = false;

						//get next record =====================
						import = FillImport(csv);
						import.Id = counter;
						currentRecord = "";

						//validate ============================
						if (import.IsRecordEmpty())
						{
							//this should be a warning only if in preview
							import.Message = "Main required fields are missing - skipping blank row";
							import.Status = "Skipping Blank";
							import.IsValid = false;
							recordSkipped++;
						}
						else
						{
							if (!import.IsRecordValid())
							{
								import.IsValid = false;
								import.Status = "<span style='color:red;'>Errors</span>";
								isValid = false;

								if (doingValidationOnly == false)
								{
									//should produce error and reject if not in validation stage!
									SetConsoleErrorMessage("Errors were encountered for record:<br/>&nbsp;&nbsp;&nbsp;&nbsp;"
											+ currentRecord + "<br/>Previous record was <br/>&nbsp;&nbsp;&nbsp;&nbsp;" + lastRecordSummary
											+ " <br/><br/>Please correct this record (and check all other records for similar issues). The import has been rolled back - all previous records have been removed. After making corrections, the whole file will have to be reimported! ");

									break;
									//return false;

								}
							}
							else
							{
								//ensure a the user email does not exists
								if (DuplicatesCheck(import) == false)
								{
									isDuplicate = true;
									import.IsValid = false;
								}
							}

							if (import.IsValid == false)
							{
								import.Status = "<span class='errorAction'>Errors</span>";
								isValid = false;
							}
							else if (isDuplicate)
							{
								import.Status = "<span class='skipAction'>Duplicate-SKIPPING</span>";
							}
							else if (import.UpdateAction == "update")
							{
								import.Status = "<span class='updateAction'>UPDATE</span>";
							}

							if (doingValidationOnly == false
								&& isDuplicate == false)
							{
								//perist data
								AddUser(import);
								//statusMessage = "";
								//import.CreatedById = this.WebUser.Id;

								//try
								//{
								//    Patron user = new Patron();
								//    user.FirstName = import.FirstName;
								//    user.LastName = import.LastName;
								//    user.Email = import.Email;
								//    user.Username = import.Email;
								//    string password = "ChangeMe_" + System.DateTime.Now.Millisecond.ToString();
								//    user.Password = UtilityManager.Encrypt( password );
								//    user.IsActive = false;

								//    id = myManager.Create( user, ref statusMessage );
								//    if ( id == 0 )
								//    {
								//        insertErrors = insertErrors + 1;
								//        import.Status = "<span style='color:red;'>Import FAILED</span>";
								//    }
								//    else
								//    {
								//        user.Id = id;
								//        int orgId = LastOrgId;  // Int32.Parse( txtOrgId.Text );
								//        PatronProfile prof = new PatronProfile();
								//        prof.UserId = id;
								//        prof.OrganizationId = orgId;

								//        if ( myManager.PatronProfile_Create( prof, ref statusMessage ) > 0 )
								//        {
								//            //add to org
								//            int omid = OrgManager.OrganizationMember_Create( orgId, prof.UserId, import.EmployeeTypeId, WebUser.Id, ref statusMessage );
								//            //what about libraries??
								//            if ( omid > 0 )
								//            {
								//                OrganizationMember mbr = OrgManager.OrganizationMember_Get( omid );
								//                if (sendEmailonImport.Text == "yes")
								//                    Notify( user, mbr );
								//            }

								//            insertSuccess++;
								//            import.Status = "<span style='font-weight:bold;color:green;'>Import Succeeded</span>";
								//        }
								//    }

								//}
								//catch ( Exception ex )
								//{
								//    SetConsoleErrorMessage( "Error: " + ex + errorMessage.Text );
								//    id = 0;
								//}
							}
						}


						//output columns
						list.Add(import);

						//record last record keys
						lastRecordSummary = import.CurrentRecord("Org", LastOrgId.ToString());
					}
				}//end while
					#endregion

				if (list.Count > 0)
				{
					ShowImportDetails(list, isValid, doingValidationOnly, parentId);
				}

				insertSuccess = counter - (insertErrors + recordSkipped);

				//check for rollback
				if (isValid == false)
				{
					if (doingValidationOnly == false)
					{
						SetConsoleErrorMessage(importErrorsFoundMsg.Text);

					}
					else
					{
						SetConsoleErrorMessage(previewErrorsFoundMsg.Text);
					}
				}
				else
				{

					if (doingValidationOnly == false)
					{
						string stats = "<br/><br/># of records attempted = " + counter
								+ "<br/># of records succeeded = " + insertSuccess
								+ "<br/># of records skipped = " + recordSkipped
								+ "<br /># of records failed = " + insertErrors;

						SetConsoleSuccessMessage(successfulImportMsg.Text + stats);


					}
					else
						SetConsoleInfoMessage(successfulViewMsg.Text);
				}

			}
			catch (Exception exc)
			{
				SetConsoleErrorMessage("The file format was incorrect for one of the records ( " + currentRecord + "). Please adjust your file and upload again!<br/><br/> Error = " + exc.Message + errorMessage.Text);

				isValid = false;
				//not sure if we want to display array contents now?
			}

			return isValid;
		}
		private void AddUser(PatronImport import)
		{
			//perist data
			string statusMessage = "";
			import.CreatedById = this.WebUser.Id;

			try
			{
				Patron user = new Patron();
				user.FirstName = import.FirstName;
				user.LastName = import.LastName;
				user.Email = import.Email;
				user.UserName = import.Email;
				string password = "ChangeMe_" + System.DateTime.Now.Millisecond.ToString();
				user.Password = UtilityManager.Encrypt(password);
				user.IsActive = false;

				int id = myManager.Create(user, ref statusMessage);
				if (id == 0)
				{
					insertErrors = insertErrors + 1;
					import.Status = "<span style='color:red;'>Import FAILED</span>";
				}
				else
				{
					user.Id = id;
					int orgId = LastOrgId;  // Int32.Parse( txtOrgId.Text );
					PatronProfile prof = new PatronProfile();
					prof.UserId = id;
					prof.OrganizationId = orgId;

					if (myManager.PatronProfile_Create(prof, ref statusMessage) > 0)
					{
						//add to org
						int omid = orgManager.OrganizationMember_Create(orgId, prof.UserId, import.EmployeeTypeId, WebUser.Id, ref statusMessage);
						//what about libraries??
						if (omid > 0)
						{
							OrganizationMember mbr = OrgManager.OrganizationMember_Get(omid);
							if (sendEmailonImport.Text == "yes")
								Notify(user, mbr);
						}

						insertSuccess++;
						import.Status = "<span style='font-weight:bold;color:green;'>Import Succeeded</span>";
					}
				}

			}
			catch (Exception ex)
			{
				SetConsoleErrorMessage("Error: " + ex + errorMessage.Text);

			}
		}

		private void Notify(Patron user, OrganizationMember mbr)
		{
			string eMessage = "";
			string statusMessage = "";
			string toEmail = user.Email;
			string bcc = UtilityManager.GetAppKeyValue("systemAdminEmail", "mparsons@siuccwd.com");
			string fromEmail = UtilityManager.GetAppKeyValue("contactUsMailFrom", "mparsons@siuccwd.com");

			bool isSecure = false;

			if (UtilityManager.GetAppKeyValue("SSLEnable", "0") == "1")
				isSecure = true;

			string proxyId = new AccountServices().Create_3rdPartyAddProxyLoginId(user.Id, "User Import", ref statusMessage);

			string subject = string.Format(this.noticeSubject.Text, WebUser.FullName());
			eMessage = string.Format(noticeEmail.Text, txtOrgName.Text, mbr.OrgMemberType);

			string confirmUrl = string.Format(this.activateLink.Text, proxyId.ToString());
			confirmUrl = UtilityManager.FormatAbsoluteUrl(confirmUrl, isSecure);

			string acctCreated = string.Format(this.acctCreatedMsg.Text, user.Email, "Change password on login", confirmUrl);
			eMessage += acctCreated;

			eMessage += " <p>" + WebUser.EmailSignature() + "</p>";

			EmailManager.SendEmail(toEmail, fromEmail, subject, eMessage, "", bcc);
		}
		#region validation

		/// <summary>
		/// validate participant
		/// </summary>
		/// <param name="import"></param>
		/// <param name="doingValidationOnly"></param>
		/// <returns></returns>
		protected bool ValidateImport(PatronImport import, bool doingValidationOnly)
		{
			bool isValid = true;
			string statusMessage = "";


			return isValid;
		}

		protected bool DuplicatesCheck(PatronImport import)
		{
			bool isValid = true;

			Patron user = new Patron();
			user = myManager.GetByEmail(import.Email);

			//found?
			if (user != null && user.Id > 0)
			{
				//found, produce error
				import.Message = "Error - user email already exists";
				isValid = false;
			}

			return isValid;
		}
		#endregion
		protected void ShowImportDetails(List<PatronImport> list, bool isValid, bool doingValidationOnly, int parentId)
		{
			//if there were any invalid rows and the current transaction is an import (not a validation), then need to read through list and change all status of OK to rolled back (or something like that)
			if (doingValidationOnly == false && isValid == false)
			{
				foreach (PatronImport item in list)
				{
					if (item.Status.ToLower().IndexOf("ok") > -1 || item.Status.ToLower().IndexOf("succeeded") > -1)
						item.Status = "<span style='color:black; background-color:yellow;'>Import Cancelled</span>";
				}
			}

			formGrid.DataSource = list;
			formGrid.PageIndex = 0;
			formGrid.DataBind();

		}

		#region Helper methods

		/// <summary>
		/// Fill invoice from import
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected PatronImport FillImport(CsvReader csv)
		{
			PatronImport import = new PatronImport();
			import.Status = "OK";
			import.IsValid = true;

			//not sure what will be the column header
			//or should we just do by column order?
			import.FirstName = GetValueFromReader(csv, "FirstName");
			import.LastName = GetValueFromReader(csv, "LastName");
			import.Email = GetValueFromReader(csv, "Email");
			import.EmployeeTypeId = GetIntValueFromReader(csv, "EmployeeTypeId");

			return import;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected bool ValidateColumnNames(DataSet ds)
		{
			string columns = validColumns.Text;
			string invalidColumns = "";
			string prefix = "";
			bool isValid = true;

			foreach (DataColumn col in ds.Tables[0].Columns)
			{
				if (columns.ToLower().IndexOf(col.ColumnName.ToLower()) < 0)
				{
					invalidColumns += prefix + col.ColumnName;
					prefix = ", ";
					isValid = false;
				}

			}

			if (!isValid)
			{
				SetConsoleErrorMessage("Error: The input file contains some invalid column names. Please check the instructions for the proper names(which must match exactly, including case), make the necessary corrections and try again.<br/>Invalid columns: " + invalidColumns);
			}
			return isValid;
		}


		/// <summary>
		/// Validate Column Names  - optionally skip extra columns??
		/// </summary>
		/// <param name="headers"></param>
		/// <param name="validColumnsCount"></param>
		protected bool ValidateColumnNames(string[] headers, int validColumnsCount)
		{
			string columns = validColumns.Text;
			string invalidColumns = "";
			string prefix = "";
			bool isValid = true;

			for (int i = 0; i < headers.Length; i++)
			{
				if (ignoreExtraColumns.Text.Equals("yes") && i >= validColumnsCount)
					break;

				if (columns.IndexOf(headers[i]) < 0)
				{
					invalidColumns += prefix + headers[i];
					prefix = ", ";
					isValid = false;
				}
			}

			if (!isValid)
			{
				SetConsoleErrorMessage("Error: The input file contains some invalid column names. Please check the instructions for the proper names, make the necessary corrections and try again.<br/>Invalid columns: " + invalidColumns + "<br/>Valid column headers are:<br/>" + validColumns.Text);
			}
			return isValid;
		}

		protected string GetValueFromReader(OleDbDataReader myreader, string columnName)
		{
			try
			{
				object val = myreader[columnName];
				if (val != DBNull.Value)
					return val.ToString();
				else
					return "";
			}
			catch (Exception ex)
			{
				return "Missing";
			}
		}

		protected string GetValueFromReader(CsvReader myreader, string columnName)
		{
			try
			{
				object val = myreader[columnName];
				if (val != DBNull.Value)
					return val.ToString();
				else
					return "";
			}
			catch (Exception ex)
			{
				return "Missing";
			}
		}

		protected int GetIntValueFromReader(CsvReader myreader, string columnName)
		{
			int value = 0;
			try
			{
				string val = myreader[columnName];
				if (val != null)
					Int32.TryParse(val, out value);
				else
					return 0;
			}
			catch (Exception ex)
			{
				return 0;
			}

			return value;
		}


		#endregion
	} //class

	public class PatronImport
	{

		public PatronImport()
		{
			FirstName = "";
			LastName = "";
			Email = "";
			Status = "";
			UpdateAction = "";
		}
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public int EmployeeTypeId { get; set; }
		public string Status { get; set; }
		public bool IsValid { get; set; }
		public int CreatedById { get; set; }
		public int LastUpdatedById { get; set; }
		public string Message { get; set; }
		public string UpdateAction { get; set; }

		/// <summary>
		/// Formats a display string for current record
		/// - descendants will override, add additional checks
		/// </summary>
		/// <returns></returns>
		public string CurrentRecord(string providerTitle, string providerValue)
		{
			return string.Format("{0}: {1}, Employee: {2}, Email: {3}", providerTitle, providerValue, FirstName + " " + LastName, Email);
		}
		public bool IsRecordValid()
		{
			Message = "";
			IsValid = true;

			if (this.FirstName.Trim().Length == 0)
				Message += "First Name is blank<br/>";
			if (this.LastName.Trim().Length == 0)
				Message += "Last Name is blank<br/>";
			if (this.Email.Trim().Length == 0)
				Message += "Email is blank<br/>";

			if (EmployeeTypeId == 0)
				Message += "EmployeeTypeId is missing<br/>";

			if (Message.Length > 0)
			{
				IsValid = false;
			}

			return IsValid;

		}
		public bool IsRecordEmpty()
		{
			if (this.FirstName.Length == 0
				&& this.LastName.Length == 0
				&& this.Email.Length == 0
				&& EmployeeTypeId == 0
				)
			{
				IsValid = false;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}