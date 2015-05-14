using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Web.Configuration;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using ILPathways.Utilities;

namespace LearningRegistryCache2
{
    public class Program
    {

        static void Main( string[] args )
        {
            string startDate;
            string endDate;
            string filePath;
            LearningRegistry lr = new LearningRegistry();

            if (args == null || args.Length == 0 || args[0].ToLower() != "batch")
            {

                int option = 0;
                while (option != 8)
                {
                    while (option < 1)
                    {
                        Console.WriteLine("1) Get Data from LR");
                        Console.WriteLine("2) Process file");
                        Console.WriteLine("3) Process path");
                        Console.WriteLine("4) Extract paradata with a given verb");
                        Console.WriteLine("5) Send test email");
                        Console.WriteLine("6) Extract meta/paradata with a given schema");
                        Console.WriteLine("7) Undelete resources incorrectly \"deleted\" by LinkChecker");
                        Console.WriteLine("8) Quit");
                        Console.Write("Select one ===> ");
                        string strOption = Console.ReadLine();
                        if (strOption == "1" || strOption == "2" || strOption == "3" || strOption == "4" || strOption == "5" || strOption == "6" || strOption == "7" || strOption == "8")
                        {
                            option = int.Parse(strOption);
                        }
                    }
                    switch (option)
                    {
                        case 1:
                            Console.Write("Enter start date in yyyy-mm-dd format: ");
                            startDate = Console.ReadLine();
                            Console.Write("Enter end date in yyyy-mm-dd format: ");
                            endDate = Console.ReadLine();
                            lr.ExtractData(startDate, endDate, false);
                            option = 0;
                            break;
                        case 2:
                            Console.Write("Enter file path: ");
                            filePath = Console.ReadLine();
                            lr.ProcessFile(filePath);
                            //WaitHandle.WaitAll(BaseDataController.doneEvents.ToArray());
                            option = 0;
                            break;
                        case 3:
                            Console.Write("Enter file path: ");
                            filePath = Console.ReadLine();
                            lr.ProcessPath(filePath, false);
                            //WaitHandle.WaitAll(BaseDataController.doneEvents.ToArray());
                            option = 0;
                            break;
                        case 4:
                            Console.Write("Enter file path: ");
                            filePath = Console.ReadLine();
                            Console.Write("Enter verb to extract: ");
                            string verb = Console.ReadLine();
                            lr.ExtractVerb(filePath, verb);
                            option = 0;
                            break;
                        case 5:
                            string adminEmail = WebConfigurationManager.AppSettings["adminEmail"].ToString();
                            EmailManager.SendEmail(adminEmail, "LR_Import@ilsharedlearning.org", "Import Status Test", "This is a test email.  This is only a test.  Please ignore.");
                            option = 0;
                            break;
                        case 6:
                            Console.Write("Enter file path: ");
                            filePath = Console.ReadLine();
                            Console.Write("Enter schema to extract: ");
                            string schema = Console.ReadLine();
                            lr.ExtractSchema(filePath, schema);
                            option = 0;
                            break;
                        case 7:
                            Console.Write("Has Stored Proc [Resource.UndoBadLinkCheck] been properly configured (Y/N)? ");
                            string choice = Console.ReadLine();
                            if (choice.ToUpper() == "Y")
                            {
                                lr.UndeleteResources();
                            }
                            option = 0;
                            break;
                        case 8:
                            break;
                        default:
                            Console.Write("Option not implemented.  ");
                            option = 0;
                            break;
                    }
                }
                //int recordCntr = lr.DoWork(startDate, endDate);

                Console.Write(string.Format("Press <enter> key... "));
                Console.ReadLine();
            }
            else // Batch Processing
            {
                string report = "";
                try
                {
                    string status = "";
                    Console.WriteLine("Getting LR_Import System.Process record");
                    SystemProcessManager spm = new SystemProcessManager();
                    SystemProcess process = spm.GetByCode("LR_Import", ref status);
                    if (status != "successful")
                    {
                        Console.WriteLine(status);
                        Console.Write("Press <enter> key... ");
                        Console.ReadLine();
                        return;
                    }
                    if (process == null)
                    {
                        Console.WriteLine("LR_Import process not found.");
                        Console.Write("Press <enter> key... ");
                        Console.ReadLine();
                        return;
                    }

                    Console.WriteLine("Retrieved LR_Import System.Process record");
                    startDate = process.LastRunDate.ToString();
                    endDate = DateTime.Now.ToString();
                    lr.ExtractData(startDate, endDate, true);
                    process.LastRunDate = DateTime.Parse(endDate);
                    spm.Update(process);
                    filePath = lr.GetFilePath(DateTime.Parse(endDate), true) + "*.xml";
                    lr.ProcessPath(filePath, true);
                    //WaitHandle.WaitAll(BaseDataController.doneEvents.ToArray());
                }
                catch (Exception ex)
                {
                    BaseDataManager.LogError("Program.Main(): " + ex.ToString());
                    report += "An exception occurred: " + ex.ToString() + "\n\n\n";
                }
                //Dump counts to an email or something here
                report += DateTime.Now.ToShortDateString() + System.Environment.NewLine
                    + LearningRegistry.lrRecordsRead + " records read" + System.Environment.NewLine
                    + LearningRegistry.lrRecordsProcessed + " records processed" + System.Environment.NewLine
                    + LearningRegistry.lrRecordsSpam + " records were spam" + System.Environment.NewLine
                    + LearningRegistry.lrRecordsUnknownSchema + " records had unknown schema" + System.Environment.NewLine
                    + LearningRegistry.lrRecordsEmptySchema + " records had no schema" + System.Environment.NewLine
                    + LearningRegistry.lrRecordsBadDataType + " records with bad resource data type" + System.Environment.NewLine;
                    //+ LearningRegistry.lrRecordsBadPayloadPlacement + " records with bad payload placement" + System.Environment.NewLine + System.Environment.NewLine;

                if (LearningRegistry.HasEsUploadErrors)
                {
                    report += System.Environment.NewLine + "***** ATTENTION ***** ATTENTION ***** ATTENTION *****" + System.Environment.NewLine
                        + "     There were errors uploading documents to Elastic Search.  Please Review File "
                        + ConfigurationManager.AppSettings["esIdLog"].Replace("[date]", LearningRegistry.EsIdDate)
                        + " for affected Resource IntIds." + System.Environment.NewLine
                        + "***** ATTENTION ***** ATTENTION ***** ATTENTION *****" + System.Environment.NewLine;
                }
                try
                {
                    System.IO.File.WriteAllText( @"C:\IOER_Tools\ImportReports\" + DateTime.Now.ToString( "yyyy-dd-MM" ) + ".txt", report );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Error writing report file:" );
                    Console.WriteLine( ex.ToString() );
                }

                report += "Processed data from Learning Registry." + System.Environment.NewLine + "Process complete." + System.Environment.NewLine;
                string adminEmail = WebConfigurationManager.AppSettings["adminEmail"].ToString();
                EmailManager.SendEmail(adminEmail, "LR_Import@ilsharedlearning.org", "Import Status", report.Replace(System.Environment.NewLine, "<br />"));

            }
        }
    }
}
