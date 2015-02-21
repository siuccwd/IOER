/*********************************************************************************
= Author: Michael Parsons
=
= Date: Mar 08/2013
= Assembly: LRWarehouse.Business
= Description:
= Notes:
=
=
= Copyright 2013, Isle All rights reserved.
********************************************************************************/
using System; 
using System.Collections.Generic;

namespace ILPathways.Business
{
  ///<summary>
  ///Represents an object that describes a LibraryResource
  ///</summary>
  [Serializable]
  public class LibraryResource : BaseBusinessDataEntity
  {

    public static string PdfImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pdf_200x150.png";
    public static string PPTImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pptx_200x150.png";
    public static string WordImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_docx_200x150.png";
    public static string XlxImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_xlsx_200x150.png";
    public static string SwfImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_swf_200x200.png";
     //large

    public static string PdfLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pdf_400x300.png";
    public static string PPTLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pptx_400x300.png";
    public static string WordLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_docx_400x300.png";
    public static string XlxLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_xlsx_400x300.png";
    public static string SwfLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_swf_400x400.png";


    ///<summary>
    ///Initializes a new instance of the LRWarehouse.Business.LibraryResource class.
    ///</summary>
    public LibraryResource() 
    {
        ResourceLibrary = new Library();
        ResourceSection = new LibrarySection();
    }

    #region Properties created from dictionary for LibraryResource

    private int _libraryId;
    /// <summary>
    /// Gets/Sets LibraryId
    /// </summary>
    public int LibraryId
    {
        get
        {
            return this._libraryId;
        }
        set
        {
            if ( this._libraryId == value )
            {
                //Ignore set
            }
            else
            {
                this._libraryId = value;
                HasChanged = true;
            }
        }
    }

    private int _librarySectionId;
    /// <summary>
    /// Gets/Sets LibrarySectionId
    /// </summary>
    public int LibrarySectionId
    {
      get
      {
        return this._librarySectionId;
      }
      set
      {
        if (this._librarySectionId == value) {
          //Ignore set
        } else {
          this._librarySectionId = value;
          HasChanged = true;
        }
      }
    }

    private string _comment = "";
    /// <summary>
    /// Gets/Sets Comment
    /// </summary>
    public string Comment
    {
      get
      {
        return this._comment;
      }
      set
      {
        if (this._comment == value) {
          //Ignore set
        } else {
          this._comment = value.Trim();
          HasChanged = true;
        }
      }
    }

    #endregion


    #region Properties of external objects
    public string CollectionTitle { get; set; }

    public string Title { get; set; }
    public string SortTitle { get; set; }
    public string Description { get; set; }
    public string ResourceUrl { get; set; }

    private int _resourceIntId;
    /// <summary>
    /// Gets/Sets ResourceIntId
    /// </summary>
    public int ResourceIntId
    {
        get
        {
            return this._resourceIntId;
        }
        set
        {
            if ( this._resourceIntId != value )
            {
                this._resourceIntId = value;
                HasChanged = true;
            }
        }
    }
    public int ResourceVersionIntId { get; set; }
    public string DetailPageUrl { get; set; }

    string _imageUrl = "";
    /// <summary>
    /// Resource Image Url
    /// - convenience method. Should be using the ResourceBizService.GetResourceImageUrl method (at least keep in sync)
    /// </summary>
    public string ImageUrl
    {
        get
        {
            if ( ResourceIntId == 0 )
            {
                _imageUrl = "";
                return "";
            }
            else if ( _imageUrl.Length > 10 )
            {
                //already set
                return _imageUrl;
            }
            else
            {
                SetImageUrls();

                //need to handle special types
                //also should not hard-code domain - maybe should be at the controller level!
                //if ( ResourceUrl != null && ResourceUrl.Length > 5 )
                //{
                //    string url = ResourceUrl.ToLower().Trim();

                //    if ( url.EndsWith( ".pdf" ) ) _imageUrl = PdfLrgImageUrl;
                //    else if ( url.EndsWith( ".swf" ) ) _imageUrl = SwfLrgImageUrl;
                //    else if ( url.EndsWith( ".ppt" ) ) _imageUrl = PPTLrgImageUrl;
                //    else if ( url.EndsWith( ".pptx" ) ) _imageUrl = PPTLrgImageUrl;
                //    else if ( url.EndsWith( ".xls" ) ) _imageUrl = XlxLrgImageUrl;
                //    else if ( url.EndsWith( ".xlzx" ) ) _imageUrl = XlxLrgImageUrl;
                //    else if ( url.EndsWith( ".doc" ) ) _imageUrl = WordLrgImageUrl;
                //    else if ( url.EndsWith( ".docx" ) ) _imageUrl = WordLrgImageUrl;
                //    else
                //        _imageUrl = string.Format( "//ioer.ilsharedlearning.org/OERThumbs/large/{0}-large.png", ResourceIntId );
                //}

                return _imageUrl;

            }
        }
        set
        {
            if ( _imageUrl != value )
            {
                this._imageUrl = value;
            }
        }

    }
    string _thumbImageUrl = "";
    /// <summary>
    /// Resource thumbnail Image Url
    /// - convenience method. Should be using the ResourceBizService.GetResourceThumbailImageUrl method (at least keep in sync)
    /// </summary>
    public string ThumbnailUrl
    {
        get
        {
            if ( ResourceIntId == 0 )
            {
                _thumbImageUrl = "";
                return "";
            }
            else if ( _thumbImageUrl.Length > 10 )
            {
                //already set
                return _thumbImageUrl;
            }
            else
            {
                SetImageUrls();
                //need to handle special types
                ////also should not hard-code domain - maybe should be at the controller level!
                //if ( ResourceUrl != null && ResourceUrl.Length > 5 )
                //{
                //    string url = ResourceUrl.ToLower().Trim();

                //    if ( url.EndsWith( ".pdf" ) ) _thumbImageUrl = PdfImageUrl;
                //    else if ( url.EndsWith( ".swf" ) ) _thumbImageUrl = SwfImageUrl;
                //    else if ( url.EndsWith( ".ppt" ) ) _thumbImageUrl = PPTImageUrl;
                //    else if ( url.EndsWith( ".pptx" ) ) _thumbImageUrl = PPTImageUrl;
                //    else if ( url.EndsWith( ".xls" ) ) _thumbImageUrl = XlxImageUrl;
                //    else if ( url.EndsWith( ".xlzx" ) ) _thumbImageUrl = XlxImageUrl;
                //    else if ( url.EndsWith( ".doc" ) ) _thumbImageUrl = WordImageUrl;
                //    else if ( url.EndsWith( ".docx" ) ) _thumbImageUrl = WordImageUrl;
                //    else
                //        _thumbImageUrl = string.Format( "//ioer.ilsharedlearning.org/OERThumbs/thumb/{0}-thumb.png", ResourceIntId );
                //}

                return _thumbImageUrl;

            }
        }
        set
        {
            if ( _thumbImageUrl != value )
            {
                this._thumbImageUrl = value;
            }
        }

    }

    public string CreatedByImageUrl { get; set; }
    public string ProfileUrl { get; set; }  

    public void SetImageUrls()
    {
        if ( ResourceIntId == 0 
          || ResourceUrl == null || ResourceUrl.Length < 10 )
        {
            ImageUrl = "";
            ThumbnailUrl = "";
            return;
        }

        string url = ResourceUrl.ToLower().Trim();

        //if ( url.EndsWith( ".pdf" ) ) _imageUrl = PdfLrgImageUrl;
        //else if ( url.EndsWith( ".swf" ) ) _imageUrl = SwfLrgImageUrl;
        //else 
        if ( url.EndsWith( ".ppt" ) ) _imageUrl = PPTLrgImageUrl;
        else if ( url.EndsWith( ".pptx" ) ) _imageUrl = PPTLrgImageUrl;
        else if ( url.EndsWith( ".xls" ) ) _imageUrl = XlxLrgImageUrl;
        else if ( url.EndsWith( ".xlsx" ) ) _imageUrl = XlxLrgImageUrl;
        else if ( url.EndsWith( ".doc" ) ) _imageUrl = WordLrgImageUrl;
        else if ( url.EndsWith( ".docx" ) ) _imageUrl = WordLrgImageUrl;
        else
            _imageUrl = string.Format( "//ioer.ilsharedlearning.org/OERThumbs/large/{0}-large.png", ResourceIntId );
      

        //if ( url.EndsWith( ".pdf" ) ) _thumbImageUrl = PdfImageUrl;
        //else if ( url.EndsWith( ".swf" ) ) _thumbImageUrl = SwfImageUrl;
        //else 
        if ( url.EndsWith( ".ppt" ) ) _thumbImageUrl = PPTImageUrl;
        else if ( url.EndsWith( ".pptx" ) ) _thumbImageUrl = PPTImageUrl;
        else if ( url.EndsWith( ".xls" ) ) _thumbImageUrl = XlxImageUrl;
        else if ( url.EndsWith( ".xlsx" ) ) _thumbImageUrl = XlxImageUrl;
        else if ( url.EndsWith( ".doc" ) ) _thumbImageUrl = WordImageUrl;
        else if ( url.EndsWith( ".docx" ) ) _thumbImageUrl = WordImageUrl;
        else
            _thumbImageUrl = string.Format( "//ioer.ilsharedlearning.org/OERThumbs/thumb/{0}-thumb.png", ResourceIntId );
        
    }// 

    private Library _resourceLibrary;
    public Library ResourceLibrary
    {
        get
        {
            if ( _resourceLibrary == null )
                _resourceLibrary = new Library();

            return this._resourceLibrary;
        }
        set
        {
            if ( this._resourceLibrary == value )
            {
                //Ignore set
            }
            else
            {
                this._resourceLibrary = value;
                HasChanged = true;
            }
        }
    } //

    private LibrarySection _resourceSection;
    public LibrarySection ResourceSection
    {
        get
        {
            if ( _resourceSection == null )
                _resourceSection = new LibrarySection();

            return this._resourceSection;
        }
        set
        {
            if ( this._resourceSection == value )
            {
                //Ignore set
            }
            else
            {
                this._resourceSection = value;
                HasChanged = true;
            }
        }
    } //

    #endregion

  } // end class 
} // end Namespace 

