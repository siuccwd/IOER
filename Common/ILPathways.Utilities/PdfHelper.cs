using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ILPathways.Utilities
{
     /// <summary>
      /// Concat,PDFMerge
      /// in memory
    /// PdfReader reader = new PdfReader(baos.toByteArray());
      /// </summary>
    public class PdfHelper
    {
        public string Concat( string destFile, string baseFile, string fileToAppend )
        {
            string message = "";
            if ( baseFile == null || baseFile.Trim().Length == 0 || fileToAppend == null || fileToAppend.Trim().Length == 0 )
            {
                message = "Please enter a valid base file and a file to append to the base" ;
                return message;
            }
          
            try
            {
                
                // we create a reader for a certain document
                PdfReader reader = new PdfReader( baseFile );
                // we retrieve the total number of pages
                int n = reader.NumberOfPages;
                Console.WriteLine( "There are " + n + " pages in the original file." );

                // step 1: creation of a document-object
                Document document = new Document( reader.GetPageSizeWithRotation( 1 ) );

                // step 2: we create a writer that listens to the document
                PdfWriter writer = PdfWriter.GetInstance( document, new FileStream( destFile, FileMode.Create ) );
                // step 3: we open the document
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                
                
                // step 4: we add content

                MergeFiles( writer, document, reader,   cb );
                reader = new PdfReader( fileToAppend );
                // we retrieve the total number of pages
                
                MergeFiles( writer, document, reader,   cb );

				//int f = 1;
				//PdfImportedPage page;
				//int rotation = 0;
                //while ( f < args.Length )
                //{
                //    int i = 0;
                //    while ( i < n )
                //    {
                //        i++;
                //        document.SetPageSize( reader.GetPageSizeWithRotation( i ) );
                //        document.NewPage();
                //        page = writer.GetImportedPage( reader, i );
                //        rotation = reader.GetPageRotation( i );
                //        if ( rotation == 90 || rotation == 270 )
                //        {
                //            cb.AddTemplate( page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation( i ).Height );
                //        }
                //        else
                //        {
                //            cb.AddTemplate( page, 1f, 0, 0, 1f, 0, 0 );
                //        }
                //        Console.WriteLine( "Processed page " + i );
                //    }
                //    f++;
                //    //if ( f < args.Length )
                //    //{
                //    //    reader = new PdfReader( args[ f ] );
                //    //    // we retrieve the total number of pages
                //    //    n = reader.NumberOfPages;
                //    //    Console.WriteLine( "There are " + n + " pages in the original file." );
                //    //}
                //}
                // step 5: we close the document
                document.Close();
            }
            catch ( Exception e )
            {
                Console.Error.WriteLine( e.Message );
                Console.Error.WriteLine( e.StackTrace );
                return e.Message;
            }
            return "OK";
        }
        public static void MergeFiles( PdfWriter writer, Document document, PdfReader reader, PdfContentByte cb )
        {
            PdfImportedPage page;
            int rotation = 0;
            int n = reader.NumberOfPages;
            int i = 0;
            while ( i < n )
            {
                i++;
                document.SetPageSize( reader.GetPageSizeWithRotation( i ) );
                document.NewPage();
                page = writer.GetImportedPage( reader, i );
                rotation = reader.GetPageRotation( i );
                if ( rotation == 90 || rotation == 270 )
                {
                    cb.AddTemplate( page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation( i ).Height );
                }
                else
                {
                    cb.AddTemplate( page, 1f, 0, 0, 1f, 0, 0 );
                }
                Console.WriteLine( "Processed page " + i );
            };
        }
        public static void MergeFiles( string destinationFile, string[] sourceFiles )
        {

            try
            {
                int f = 0;
                // we create a reader for a certain document
                PdfReader reader = new PdfReader( sourceFiles[ f ] );
                // we retrieve the total number of pages
                int n = reader.NumberOfPages;
                //Console.WriteLine("There are " + n + " pages in the original file.");
                // step 1: creation of a document-object
                Document document = new Document( reader.GetPageSizeWithRotation( 1 ) );
                // step 2: we create a writer that listens to the document
                PdfWriter writer = PdfWriter.GetInstance( document, new FileStream( destinationFile, FileMode.Create ) );
                // step 3: we open the document
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page;
                int rotation;
                // step 4: we add content
                while ( f < sourceFiles.Length )
                {
                    int i = 0;
                    while ( i < n )
                    {
                        i++;
                        document.SetPageSize( reader.GetPageSizeWithRotation( i ) );
                        document.NewPage();
                        page = writer.GetImportedPage( reader, i );
                        rotation = reader.GetPageRotation( i );
                        if ( rotation == 90 || rotation == 270 )
                        {
                            cb.AddTemplate( page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation( i ).Height );
                        }
                        else
                        {
                            cb.AddTemplate( page, 1f, 0, 0, 1f, 0, 0 );
                        }
                        //Console.WriteLine("Processed page " + i);
                    }
                    f++;
                    if ( f < sourceFiles.Length )
                    {
                        reader = new PdfReader( sourceFiles[ f ] );
                        // we retrieve the total number of pages
                        n = reader.NumberOfPages;
                        //Console.WriteLine("There are " + n + " pages in the original file.");
                    }
                }
                // step 5: we close the document
                document.Close();
            }
            catch ( Exception e )
            {
                string strOb = e.Message;
            }
        }

        public static int CountPageNo( string strFileName )
        {
            // we create a reader for a certain document
            PdfReader reader = new PdfReader( strFileName );
            // we retrieve the total number of pages
            return reader.NumberOfPages;
        }

        /// <summary>
        /// jave based example
        /// </summary>
        /// <param name="list"></param>
        /// <param name="outputStream"></param>
        //private void doMerge(List<InputStream> list, OutputStream outputStream) 
        //{
        //    //throws DocumentException, IOException 
        //    Document document = new Document();
        //    PdfCopy copy = new PdfCopy(document, outputStream);
        //    document.open();
        //    int n;
        //    for (InputStream in : list) {
        //        PdfReader reader = new PdfReader(in);
        //        for (int i = 1; i <= reader.getNumberOfPages(); i++) {
        //            n = reader.getNumberOfPages();
        //            // loop over the pages in that document
        //            for (int page = 0; page < n; ) {
        //                copy.addPage(copy.getImportedPage(reader, ++page));
        //            }
        //            copy.freeReader(reader);
        //            reader.close();
        //        }
        //    }
        //    outputStream.flush();
        //    document.close();
        //    outputStream.close();
        //}

        /// <summary>
        /// Merge constructor
        /// Destfile
        /// - at least two files
        /// </summary>
        /// <param name="args"></param>
        public void Concat( String[] args )
	          {
	              if (args.Length < 3) 
	              {
	                  Console.Error.WriteLine("This tools needs at least 3 parameters:\njava Concat destfile file1 file2 [file3 ...]");
	              }
	              else 
	              {
	                  try 
	                  {
	                      int f = 1;
	                      // we create a reader for a certain document
	                      PdfReader reader = new PdfReader(args[f]);
	                      // we retrieve the total number of pages
	                      int n = reader.NumberOfPages;
	                      Console.WriteLine("There are " + n + " pages in the original file.");
	                  
	                      // step 1: creation of a document-object
	                      Document document = new Document(reader.GetPageSizeWithRotation(1));
	                      // step 2: we create a writer that listens to the document
	                      PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(args[0], FileMode.Create));
	                      // step 3: we open the document
	                      document.Open();
	                      PdfContentByte cb = writer.DirectContent;
	                      PdfImportedPage page;
	                      int rotation;
	                      // step 4: we add content
	                      while (f < args.Length) 
	                      {
	                          int i = 0;
	                          while (i < n) 
	                          {
	                              i++;
	                              document.SetPageSize(reader.GetPageSizeWithRotation(i));
	                              document.NewPage();
	                              page = writer.GetImportedPage(reader, i);
	                              rotation = reader.GetPageRotation(i);
	                              if (rotation == 90 || rotation == 270) 
	                              {
	                                  cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
	                              }
	                              else 
	                              {
	                                  cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
	                              }
	                              Console.WriteLine("Processed page " + i);
	                          }
	                          f++;
	                          if (f < args.Length) 
	                          {
	                              reader = new PdfReader(args[f]);
	                              // we retrieve the total number of pages
	                              n = reader.NumberOfPages;
	                              Console.WriteLine("There are " + n + " pages in the original file.");
	                          }
	                      }
	                      // step 5: we close the document
	                      document.Close();
	                  }
	                  catch(Exception e) 
	                  {
	                      Console.Error.WriteLine(e.Message);
	                      Console.Error.WriteLine(e.StackTrace);
	                  }
	              }
	 
	          }
    }
}
