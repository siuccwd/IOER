using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;

//using workNet.BusObj.Entity;

namespace ILPathways.Utilities
{
    public class ContentHelper : UtilityManager
    {
        const string thisClassName = "ContentHelper";

        const string mcInternalLink = " <img class=\"linkImg\" src='/vos_portal/images/link_NewInternal.gif' alt='Link opens in a new window' height='16' width='16' />";
        const string mcExternalLink = " <img class=\"linkImg\" src='/vos_portal/images/link_NewExternal.gif' alt='External link will open in a new window' height='16' width='16' />";

        public ContentHelper()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        #region === placeholder methods

        /// <summary>
        /// Format a help link including image, tool tip, and url. The web.config is used to determine if help will open in a popup
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="helpLink">HyperLink to format</param>
        /// <param name="helpUrlResource">Name of resource string holding the help url</param>
        /// <param name="helpToolTipResource">Name of the resource string holding the help tooltip</param>
        public static void FormatHelpLink( ResourceManager rm, HyperLink helpLink, string helpUrlResource, string helpToolTipResource )
        {
            bool allowPopups = false;

            string usingPopups = UtilityManager.GetAppKeyValue( "usingPopups", "no" );

            if ( usingPopups.Equals( "yes" ) )
                allowPopups = true;

            FormatHelpLink( rm, helpLink, helpUrlResource, helpToolTipResource, allowPopups );
        }

        /// <summary>
        /// Format a help link including image, tool tip, and url
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="helpLink">HyperLink to format</param>
        /// <param name="helpUrlResource">Name of resource string holding the help url</param>
        /// <param name="helpToolTipResource">Name of the resource string holding the help tooltip</param>
        /// <param name="allowingPopups">True if help can open via a popup window</param>
        public static void FormatHelpLink( ResourceManager rm, HyperLink helpLink, string helpUrlResource, string helpToolTipResource, bool allowingPopups )
        {

            //assuming same image will be used for all
            helpLink.ImageUrl = UtilityManager.GetResourceValue( rm, "context_help_icon_ImageUrl" );
            helpLink.Text = UtilityManager.GetResourceValue( rm, helpToolTipResource );

            string helpTopic = UtilityManager.GetResourceValue( rm, helpUrlResource );
            if ( allowingPopups )
            {
                helpLink.NavigateUrl = helpLink.NavigateUrl.Replace( "##LINK##", helpTopic );
            }
            else
            {
                helpLink.NavigateUrl = helpTopic;
                helpLink.Target = "_blank";
            }



        } //


        /// <summary>
        /// A method to handle custom requirements for rendering text including:
        /// - glossary snippets
        /// - YouTube snippets
        /// - external links
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HandleCustomTextRendering( ResourceManager rm, string html )
        {
            bool forceTooltipFormating = false;
            html = HandleCustomTextRendering( rm, html, forceTooltipFormating );

            return html;

        } //

        /// <summary>
        /// A method to handle custom requirements for rendering text including:
        /// - glossary snippets
        /// - YouTube snippets
        /// - external links
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HandleCustomTextRendering( ResourceManager rm, string html, bool forceTooltipFormating )
        {

            html = FormatLinks( html, "img", true );
            html = HandleAllSnippets( html, forceTooltipFormating );

            return html;

        } //

        public static string HandleAllSnippets( string html )
        {
            bool forceTooltipFormating = false;
            return HandleAllSnippets( html, forceTooltipFormating );
        }

        /// <summary>
        /// A method to handle every snippet
        /// </summary>
        /// <remarks>
        /// We handle snippets only for live pages.  The reason we do it only if the page is live is because
        /// if the user is editing a page, we want them to be able to edit the snippet, not have the snippet embedded
        /// in the text they are trying to edit, which would be very bad.  RadEditor would probably remove it,
        /// and if it didn't, then MCMS would!
        /// </remarks>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HandleAllSnippets( string html, bool forceTooltipFormating )
        {

            html = HandleViddlerSnippet( html );
           // html = HandleViddlerPlaylistSnippet( html );
            html = HandleYouTubeSnippet( html );
            html = HandleYouTubePlaylistSnippet( html );
            //html = HandleGlossarySnippet( html, forceTooltipFormating );

  
            return html;
        }

    
       
        /// <summary>
        /// Add HTML line breaks to glossary entry
        /// </summary>
        /// <param name="text"></param>
        /// <returns>
        /// HTML-formatted string with line breaks (<br /> tags) inserted at 1st opportunity
        /// following the 50th character on a line.  This is configurable by changing the lineBreakPos
        /// variable.  It also formats links using a call to the FormatLinks method.
        /// </returns>
        public static string AddLineBreaks( string text )
        {
            bool outsideHtml = true;
            StringBuilder output = new StringBuilder( 500 );
            int lineBreakPos = 50;
            int charsBeforeBreak = 0;
            for ( int i = 0 ; i < text.Length ; i++ )
            {
                if ( text.Substring( i, 1 ) == "<" )
                {
                    outsideHtml = false;
                    if ( text.Substring( i, 3 ) == "<br" || text.Substring( i, 2 ) == "<p" || text.Substring( i, 3 ) == "</p" )
                    {
                        charsBeforeBreak = 0;
                    }
                }
                if ( !outsideHtml )
                {
                    output.Append( text.Substring( i, 1 ) );
                }
                else
                {
                    charsBeforeBreak++;
                    if ( charsBeforeBreak >= lineBreakPos )
                    {
                        if ( text.Substring( i, 1 ) == " " )
                        {
                            output.Append( "<br />" );
                            charsBeforeBreak = 0;
                        }
                        else
                        {
                            output.Append( text.Substring( i, 1 ) );
                        }
                    }
                    else
                    {
                        output.Append( text.Substring( i, 1 ) );
                    }
                }
                if ( text.Substring( i, 1 ) == ">" )
                {
                    outsideHtml = true;
                }
            }
            return FormatLinks( output.ToString() );
        }

        /// <summary>
        /// Remove links from text
        /// </summary>
        /// <param name="text"></param>
        /// <returns>HTML string with links removed</returns>
        public static string StripLinks( string text )
        {
            string exprToReplace = @"<a(.|\n)*?</a>";
            return Regex.Replace( text, exprToReplace, string.Empty );
        }

        /// <summary>
        /// Handle YouTube snippets
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HandleYouTubeSnippet( string html )
        {
 
          
            return "";
        }//

        public static string HandleYouTubePlaylistSnippet( string html )
        {
            return "";
        }

        /// <summary>
        /// Handle Viddler snippets
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HandleViddlerSnippet( string html )
        {
            return "";
        }//

        /// <summary>
        /// Formats links that open in a new windows with appropriate graphic
        /// </summary>
        /// <param name="rm">Resource Manager for calling object</param>
        /// <param name="text">HTML from the placeholder</param>
        /// <returns>HTML formated with graphic indicating popup window</returns>
        public static string FormatLinks( string text )
        {
            return FormatLinks( text, "img", false );

        } //


        /// <summary>
        /// Formats links that open in a new windows with appropriate graphic
        /// </summary>
        /// <param name="rm">Resource Manager for calling object</param>
        /// <param name="text">HTML from the placeholder</param>
        /// <param name="linkFormat">IMG to format link with an image or text to just show text</param>
        /// <param name="insertingSoftBreaks">Set to true to insert softbreaks in anchor text</param>
        /// <returns>HTML formated with graphic indicating popup window</returns>
        public static string FormatLinks( string text, string linkFormat, bool insertingSoftBreaks )
        {
            bool isFormatOk = true;

            string defaultPage = GetAppKeyValue( "defaultPage", "/vos_portal/" );
            bool removeDefaultTooltip = false;  // Boolean.Parse( GetAppKeyValue( "removeDefaultTooltip", "false" ) );

            //get internal link snippet using default from web.config (or other way around??)
            string internalLink = GetInternalLink();
            string externalLink = GetExternalLink();
            string glossaryLink = GetGlossaryLink();
            string textOnlyIndicator = "opens in a new window";
            // string landingPageLink = UtilityManager.GetResourceValue( rm, "url_landing_page" );

            string anchorStart = "<a ";
            string anchorEnd = "</a>";
            string insideTag = "";
            string output = "";
            StringBuilder content = new StringBuilder();
            StringBuilder sbAnchor = new StringBuilder();

            int lastPos = 0;
            string anchorTag = "";

            string anchorText = "";
            //find first anchor tag
            int nextPos = text.ToLower().IndexOf( anchorStart.ToLower() );
            int endInsideTagPos = -1;
            int endOfAnchorPos = -1;

            if ( nextPos < 0 )
            {
                output = text;

            }
            else
            {

                while ( nextPos >= 0 )
                {
                    content.Append( text.Substring( lastPos, nextPos - lastPos ) );

                    //get next anchor close	
                    endOfAnchorPos = text.ToLower().IndexOf( anchorEnd, nextPos );
                    if ( endOfAnchorPos < 0 )
                    {
                        // error, break
                        isFormatOk = false;
                        break;
                    }
                    //verify anchor tag goes from <a to before the close
                    anchorTag = text.Substring( nextPos, endOfAnchorPos - nextPos );
                    sbAnchor = new StringBuilder( text.Substring( nextPos, endOfAnchorPos - nextPos ) );

                    //set end of this tag (from <a to </a>)
                    lastPos = endOfAnchorPos + 4;

                    //get the inside tag ending pos
                    endInsideTagPos = anchorTag.ToLower().IndexOf( ">" );
                    if ( endInsideTagPos != -1 )
                    {
                        insideTag = anchorTag.Substring( 0, endInsideTagPos + 1 );
                    }
                    else
                    {
                        // error - malformed tag??
                        isFormatOk = false;
                        break;
                    }

                    //get the text portion
                    int textLength = anchorTag.Length - endInsideTagPos - 1;
                    anchorText = anchorTag.Substring( endInsideTagPos + 1, textLength );

                    //format long string with soft breaks
                    if ( insertingSoftBreaks )
                        anchorText = InsertSoftbreaks( anchorText );

                    //using inside tag
                    if ( removeDefaultTooltip && insideTag.IndexOf( "new window" ) > 0 )
                    {
                        insideTag = insideTag.Replace( "Link will open in a new window", "" );
                        insideTag = insideTag.Replace( "Link opens in a new window", "" );
                        insideTag = insideTag.Replace( "Link opens a new window", "" );
                        insideTag = insideTag.Replace( "Link opens in new window", "" );
                        insideTag = insideTag.Replace( "Link will open a new window", "" );
                        insideTag = insideTag.Replace( "will open in a new window", "" );
                    }
                    // do checks before insert of landing link
                    bool isPopupLink = IsLinkPopup( insideTag );
                    bool isInternalLink = IsPathInternal( insideTag );
                    //bool isGlossaryLink = IsPathGlossary( insideTag );

                    //check if extenal link
                    if ( !isInternalLink )
                    {
                        //if not opening in a new window, change to do so
                        if ( !isPopupLink )
                        {
                            insideTag = InsertOpenNewPage( insideTag );
                            isPopupLink = true;
                        }

                    }

                    //put back together
                    anchorTag = insideTag + anchorText;

                    // determine if this is a link that opens in new window        
                    if ( isPopupLink )
                    {
                        // check for an existing img tag
                        if ( anchorTag.ToLower().IndexOf( "<img" ) > 0
                            || anchorTag.ToLower().IndexOf( "<asp:img" ) > 0 )
                        {
                            //ignore existing image

                        }
                        else
                        {
                            //insert snippet dependent on the href url
                            if ( linkFormat.ToLower() == "text" )
                            {
                                anchorTag = anchorTag + textOnlyIndicator;
                            }
                            else
                            {
                                // 2009-06-08 jgrimmer - GlossaryLink formatting removed until the rest of the new glossary version is ready
                                // Add this back in when the rest of the glossary is ready.
                                /*if (isGlossaryLink)
                                {
                                  //insert glossary
                                  anchorTag = anchorTag + glossaryLink;
                                } else */
                                if ( isInternalLink )
                                {
                                    //insert internal
                                    anchorTag = anchorTag + internalLink;
                                }
                                else
                                {
                                    //insert external
                                    anchorTag = anchorTag + externalLink;
                                }
                            }


                        }
                        content.Append( anchorTag ).Append( anchorEnd );

                    }
                    else
                    {
                        //doesn't open in new window
                        //Append anchor
                        content.Append( anchorTag ).Append( anchorEnd );
                    }

                    //reset the nextPos using lastPos (set previously)
                    nextPos = text.ToLower().IndexOf( anchorStart.ToLower(), lastPos );

                } //end while			

                //check if last part of text needs to be added
                if ( lastPos < text.Length )
                {
                    content.Append( text.Substring( lastPos, text.Length - lastPos ) );
                }
                output = content.ToString();
            } //end else

            if ( !isFormatOk )
            {
                //if problems encountered, just return the input
                // - may want to log error
                output = text;
                //return text;
            }
            else
            {
                output = output.Replace( "+target%3d", " target=" );
                return output;
            }

            //issue handling

            return output;
        } //

        /// <summary>
        /// Formats links that open in a new windows with appropriate graphic
        /// </summary>
        /// <returns>HTML formated with graphic indicating popup window</returns>
        public static void FormatLinks( ResourceManager rm, System.Web.UI.WebControls.HyperLink link )
        {
            //System.Web.UI.WebControls.HyperLink 
            // check if link opens in new window

            string defaultPage = GetAppKeyValue( "defaultPage", "/vos_portal/" );
            bool removeDefaultTooltip = Boolean.Parse( GetAppKeyValue( "removeDefaultTooltip", "false" ) );
            bool insertingSoftBreaks = false;

            //hard code linkFormat for now
            string linkFormat = "img";
            //get internal link snippet using default from web.config (or other way around??)
            string internalLink = GetInternalLink();
            string externalLink = GetExternalLink();
            string textOnlyIndicator = " " + GetResourceValue( rm, "url_Open_Link_text",
                "opens in a new window" ) + "";
            string landingPageLink = UtilityManager.GetResourceValue( rm, "url_landing_page" );

            bool isInternalLink = IsPathInternal( link.NavigateUrl.ToString() );

            bool isPopupLink = link.Target.ToLower() == "_blank" ? true : false;

            //if external and not opening in a new window, change to do so
            if ( !isInternalLink && !isPopupLink )
            {
                link.Target = "_blank";
                isPopupLink = true;
            }

            //format long string with soft breaks
            if ( insertingSoftBreaks )
                link.Text = InsertSoftbreaks( link.Text );

            // determine if this is a link that opens in new window        
            if ( isPopupLink )
            {
                // check for an existing img tag
                if ( link.ImageUrl.Length > 0
                    || link.Text.ToLower().IndexOf( "<img" ) > 0
                    || link.Text.ToLower().IndexOf( "<asp:img" ) > 0
                    )
                {
                    //ignore existing image
                }
                else
                {
                    //insert snippet dependent on the href url
                    if ( linkFormat.ToLower() == "text" )
                    {
                        link.Text += textOnlyIndicator;
                    }
                    else
                    {
                        if ( isInternalLink )
                        {
                            //insert internal
                            link.Text += internalLink;
                            //link.ImageUrl = internalLink;
                        }
                        else
                        {
                            //insert external
                            link.Text += externalLink;
                            //link.ImageUrl = externalLink;
                        }
                    }
                }


            }
            else
            {
                //doesn't open in new window
            }
            //return link;

        }//

        ///// <summary>
        ///// Insert soft breaks into long URLs or email addresses in text string
        ///// </summary>
        ///// <param name="anchorText">Text to insert soft breaks into</param>
        ///// <returns></returns>
        //public static string InsertSoftbreaks( string anchorText )
        //{
        //    string skippingSoftBreaks = GetAppKeyValue( "skippingSoftBreaks", "no" );

        //    StringBuilder newText = new StringBuilder( 255 );
        //    const string softbreak = "<span class=\"softbreak\"> </span>";
        //    // First make sure that the text doesn't already contain softbreak
        //    int sbPos = anchorText.ToLower().IndexOf( "softbreak" );
        //    if ( sbPos >= 0 )
        //        return anchorText;

        //    if ( skippingSoftBreaks.Equals( "yes" ) )
        //        return anchorText;

        //    //skip if an img exists
        //    int imgPos = anchorText.ToLower().IndexOf( "<img" );
        //    if ( imgPos >= 0 )
        //        return anchorText;
        //    imgPos = anchorText.ToLower().IndexOf( "<asp:img" );
        //    if ( imgPos >= 0 )
        //        return anchorText;

        //    //check for large anchor text - could be indicative of missing/misplaced ending tags, 
        //    //which could cause a problem
        //    if ( anchorText.Length > 200 )
        //        return anchorText;

        //    // We're going to look for http, img, /, and @
        //    //MP - should also try to handle https!
        //    int httpPos = anchorText.IndexOf( "http://" );
        //    int atPos = anchorText.IndexOf( "@" );
        //    int slashPos = anchorText.IndexOf( "/" );

        //    if ( ( httpPos >= 0 ) && ( atPos == -1 ) )
        //    {
        //        // We have http but not @
        //        if ( ( httpPos >= imgPos ) && ( imgPos >= 0 ) )
        //        {
        //            // The http may be inside an img tag, do nothing
        //            return anchorText;
        //        }
        //    }
        //    if ( ( httpPos == -1 ) && ( atPos >= 0 ) )
        //    {
        //        // We have @ but not http
        //        if ( atPos >= imgPos )
        //        {
        //            // the @ may be inside an img tag, do nothing
        //            return anchorText;
        //        }
        //    }

        //    if ( ( httpPos >= 0 ) && ( atPos >= 0 ) )
        //    {
        //        //We have both @ and http
        //        if ( httpPos < atPos )
        //        {
        //            if ( imgPos < httpPos )
        //            {
        //                return anchorText;
        //            }
        //        }
        //        if ( atPos < httpPos )
        //        {
        //            if ( imgPos < atPos )
        //            {
        //                return anchorText;
        //            }
        //        }
        //    }

        //    if ( ( httpPos == -1 ) && ( atPos == -1 ) )
        //    {
        //        // we have neither @ nor http
        //        return anchorText;
        //    }

        //    // First we look to see if we have an http link, and handle it.
        //    if ( httpPos >= 0 )
        //    {
        //        string imgTagToEnd = "";
        //        string priorToImgTag = "";
        //        if ( imgPos >= 0 )
        //        {
        //            priorToImgTag = anchorText.Substring( 0, imgPos );
        //            imgTagToEnd = anchorText.Substring( imgPos, anchorText.Length - imgPos );
        //        }
        //        else
        //        {
        //            priorToImgTag = anchorText;
        //        }
        //        httpPos += 7;  // 7 = length of string "http://"
        //        newText.Append( priorToImgTag.Substring( 0, httpPos ) );
        //        newText.Append( softbreak );
        //        priorToImgTag = priorToImgTag.Substring( httpPos, priorToImgTag.Length - ( httpPos ) );
        //        slashPos = priorToImgTag.IndexOf( "/" );
        //        while ( slashPos > -1 )
        //        {
        //            slashPos++;
        //            newText.Append( priorToImgTag.Substring( 0, slashPos ) );
        //            newText.Append( softbreak );
        //            priorToImgTag = priorToImgTag.Substring( slashPos, priorToImgTag.Length - slashPos );
        //            slashPos = priorToImgTag.IndexOf( "/" );
        //        }
        //        if ( newText.ToString() == "http://" + softbreak )
        //        {
        //            newText.Append( priorToImgTag );
        //        }
        //        priorToImgTag = newText.ToString();
        //        newText.Remove( 0, newText.ToString().Length );
        //        int dotPos = priorToImgTag.IndexOf( "." );
        //        while ( dotPos > -1 )
        //        {
        //            dotPos++;
        //            newText.Append( priorToImgTag.Substring( 0, dotPos ) );
        //            newText.Append( softbreak );
        //            priorToImgTag = priorToImgTag.Substring( dotPos, priorToImgTag.Length - dotPos );
        //            dotPos = priorToImgTag.IndexOf( "." );
        //        }
        //        newText.Append( priorToImgTag );
        //        newText.Append( imgTagToEnd );
        //    }
        //    else
        //    {
        //        // Now we want to know if we're looking at an email address
        //        if ( atPos >= 0 )
        //        {
        //            string imgTagToEnd = "";
        //            string priorToImgTag = "";
        //            if ( imgPos >= 0 )
        //            {
        //                priorToImgTag = anchorText.Substring( 0, imgPos );
        //                imgTagToEnd = anchorText.Substring( imgPos, anchorText.Length - imgPos );
        //            }
        //            else
        //            {
        //                priorToImgTag = anchorText;
        //            }
        //            // Insert softbreak after the '@' sign.
        //            atPos++;
        //            newText.Append( priorToImgTag.Substring( 0, atPos ) );
        //            newText.Append( softbreak );
        //            newText.Append( priorToImgTag.Substring( atPos, priorToImgTag.Length - atPos ) );
        //            // Now insert softbreak after each dot.
        //            priorToImgTag = newText.ToString();
        //            newText.Remove( 0, newText.ToString().Length );
        //            int dotPos = priorToImgTag.IndexOf( "." );
        //            while ( dotPos > -1 )
        //            {
        //                dotPos++;
        //                newText.Append( priorToImgTag.Substring( 0, dotPos ) );
        //                newText.Append( softbreak );
        //                priorToImgTag = priorToImgTag.Substring( dotPos, priorToImgTag.Length - dotPos );
        //                dotPos = priorToImgTag.IndexOf( "." );
        //            }
        //            newText.Append( priorToImgTag );
        //            newText.Append( imgTagToEnd );
        //        }
        //        else
        //        {
        //            newText.Append( anchorText );
        //        }
        //    }
        //    return newText.ToString();
        //}//


        /// <summary>
        /// Insert landing page - used to record a link to an external site before actual transfer
        /// </summary>
        /// <param name="insideTag">Destination URL</param>
        /// <returns></returns>
        private static string InsertOpenNewPage( string insideTag )
        {
            const string target = " target=\"_blank\"";
            string newTag;
            newTag = insideTag.Trim();

            if ( newTag.EndsWith( ">" ) )
                newTag = newTag.Replace( ">", target ) + ">";
            else
                newTag = insideTag + target;

            return newTag;
        }


        /// <summary>
        /// Insert landing page - used to record a link to an external site before actual transfer
        /// </summary>
        /// <param name="insideTag">Destination URL</param>
        /// <param name="landingPageLink">Landing page URL</param>
        /// <returns></returns>
        private static string InsertLandingPage( string insideTag, string landingPageLink )
        {

            int pos = insideTag.IndexOf( "href=\"" );
            if ( pos == -1 )
                pos = insideTag.IndexOf( "href='" );

            int endPos = insideTag.IndexOf( "\"", pos + 10 );
            if ( endPos == -1 )
                endPos = insideTag.IndexOf( "'", pos + 10 );

            //check for an apostrophe
            if ( pos == -1 && endPos == -1 )
            {
                pos = insideTag.IndexOf( "href='" );
                endPos = insideTag.IndexOf( "'", pos + 10 );
            }
            string updatedTag = "";

            if ( endPos > pos )
            {
                pos = pos + 6;
                updatedTag = insideTag.Substring( 0, pos ) + landingPageLink + "?pred=";
                string destUrl = insideTag.Substring( pos, endPos - pos );
                //destUrl = HttpUtility.UrlEncode( destUrl );
                //add check for url already encoded
                destUrl = EncodeUrl( destUrl );
                updatedTag += destUrl + insideTag.Substring( endPos );

            }
            else
            {
                updatedTag = insideTag;
            }

            return updatedTag;
        }//


        /// <summary>
        /// extract value of a particular named parameter from passed string (Assumes and equal sign is used)
        /// ex: for string:		
        ///			string searchString = "key1=value1;key2=value2;key3=value3;";
        /// To retrieve the value for key2 use:
        ///			value = ExtractNameValue( searchString, "key2", ";");
        /// </summary>
        /// <param name="sourceString">String to search</param>
        /// <param name="name">Name of "parameter" in string</param>
        /// <param name="endDelimiter">End Delimeter. A character used to indicate the end of value in the string (often a semi-colon)</param>
        /// <returns>The value associated with the passed name</returns>
        public static string ExtractNameValue( string sourceString, string name, string endDelimiter )
        {
            string assignDelimiter = "=";

            return ExtractNameValue( sourceString, name, assignDelimiter, endDelimiter );
        }//

        /// <summary>
        /// extract value of a particular named parameter from passed string. The assign delimiter
        /// ex: for string:		
        ///			string radioButtonId = "Radio_q_4_c_15_";
        /// To retrieve the value for question # use:
        ///			qNbr = ExtractNameValue( radioButtonId, "q", "_", "_");
        /// To retrieve the value for choiceId use:
        ///			choiceId = ExtractNameValue( radioButtonId, "c", "_", "_");
        /// </summary>
        /// <param name="sourceString">String to search</param>
        /// <param name="name">Name of "parameter" in string</param>
        /// <param name="assignDelimiter">Assigned delimiter. Typically an equal sign (=), but could be any defining character</param>
        /// <param name="endDelimiter">End Delimeter. A character used to indicate the end of value in the string (often a semi-colon)</param>
        /// <returns></returns>
        public static string ExtractNameValue( string sourceString, string name, string assignDelimiter, string endDelimiter )
        {
            int pos = sourceString.IndexOf( name + assignDelimiter );

            if ( pos == -1 )
                return "";

            string value = sourceString.Substring( pos + name.Length + 1 );
            int pos2 = value.IndexOf( endDelimiter );
            if ( pos2 > -1 )
                value = value.Substring( 0, pos2 );

            return value;
        }//

        /// <summary>
        /// Extract the value of an attribute from an Html tag
        /// </summary>
        /// <param name="htmlData"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string ExtractAttributeValue( string htmlData, string tag, string attribute )
        {
            //include leading less than sign
            //string value = ExtractAttributeValue( "test", "<img", "src")

            string value = "";

            try
            {
                //first ensure tag exists
                int tagPos = htmlData.ToLower().IndexOf( tag.ToLower() );
                if ( tagPos == -1 )
                    return "";

                int attrPos = htmlData.ToLower().IndexOf( attribute.ToLower(), tagPos );
                if ( attrPos == -1 )
                    return "";

                //add length of tag plus equals sign 
                attrPos += attribute.Length + 1;
                //make sure data will be inside tag
                int brackedEnd = htmlData.IndexOf( '>', attrPos );
                if ( brackedEnd == -1 )
                    return "";

                //next char should be delimiter - assuming must exist!
                string delim = htmlData.Substring( attrPos, 1 );

                int endPos = htmlData.ToLower().IndexOf( delim, attrPos + 1 );

                if ( endPos > -1 )
                {
                    value = htmlData.Substring( attrPos + 1, ( endPos - ( attrPos + 1 ) ) );

                }
            }
            catch
            {
                return "";

            }

            return value;
        }//

        #endregion


        #region === Formatting of Links related Methods ===


        /// <summary>
        /// Format an anchor tag - ensures destination url is properly formed, adds additional formatting if link is a popup and/or opens in a new window
        /// </summary>
        /// <param name="rm">Resource Manager</param>
        /// <param name="url">Destination web address</param>
        /// <param name="target">Specify if address is to open in a new window, etc.</param>
        /// <param name="linkText">Text to display for the anchor</param>
        /// <returns>Formatted anchor tag</returns>
        public static string FormatAnchor( ResourceManager rm, string url, string target, string linkText )
        {
            string anchor = "";
            string targetTag = "";

            anchor = url.TrimEnd();
            if ( anchor.Length > 0 )
            {
                if ( anchor.StartsWith( "/" ) )
                {
                    //leave alone
                }
                else
                {
                    anchor = FormatUrl( anchor );
                }

                if ( target.Length > 0 ) targetTag = " target='" + target + "' ";

                anchor = "<a href='" + anchor + "' " + targetTag + ">" + linkText + "</a>";

                anchor = FormatLinks( anchor );

            }
            else
            {
                anchor = "";
            }

            return anchor;
        }


        /// <summary>
        /// Format the passed address parameters as a link to a map site - location is in tooltip
        /// </summary>
        /// <param name="rm">Resource Manager</param>
        /// <param name="address">Street address (typically don't need city or state)</param>
        /// <param name="zipcode">Zipcode (5 or 9)</param>
        /// <returns>Formatted link to a map website</returns>
        public static string FormatShortMapSnippet( ResourceManager rm, string address, string zipcode, string displayText )
        {
            //string mapText1 = UtilityManager.GetResourceValue( rm, "ViewMapFor_Text" );
            string mapText = UtilityManager.GetResourceValue( rm, "clickHereToViewMap_Text" );
            string mapLink = FormatMapSnippet( rm, address, zipcode, true, mapText );
            mapLink = mapLink.Replace( "this location", displayText );
            return mapLink;

        } //

        /// <summary>
        /// Format the passed address parameters as a link to a map site
        /// </summary>
        /// <param name="rm">Resource Manager</param>
        /// <param name="address">Street address (typically don't need city or state)</param>
        /// <param name="zipcode">Zipcode (5 or 9)</param>
        /// <returns>Formatted link to a map website</returns>
        public static string FormatLongMapSnippet( ResourceManager rm, string address, string zipcode, string displayText )
        {
            string mapText1 = UtilityManager.GetResourceValue( rm, "ViewMapFor_Text" );
            //string mapText2 = UtilityManager.GetResourceValue( rm, "clickHereToViewMap_Text" );
            string mapLink = FormatMapSnippet( rm, address, zipcode, true, displayText );

            return mapText1 + mapLink;

        } //


        /// <summary>
        /// Format the passed address parameters as a link to a map site
        /// </summary>
        /// <param name="rm">Resource Manager</param>
        /// <param name="address">Street address (typically don't need city or state)</param>
        /// <param name="zipcode">Zipcode (5 or 9)</param>
        /// <returns>Formatted link to a map website</returns>
        public static string FormatMapSnippet( ResourceManager rm, string address, string zipcode )
        {

            string displayText = UtilityManager.GetResourceValue( rm, "clickHereToViewMap_Text" );

            return FormatMapSnippet( rm, address, zipcode, true, displayText );

        } //

        /// <summary>
        /// Format the passed address parameters as a link to a map site
        /// </summary>
        /// <param name="rm">Resource Manager</param>
        /// <param name="address">Street address (typically don't need city or state)</param>
        /// <param name="zipcode">Zipcode (5 or 9)</param>
        /// <param name="displayText">Text to display for link (ex. address)</param>
        /// <returns>Formatted link to a map website</returns>
        public static string FormatMapSnippet( ResourceManager rm, string address, string zipcode, string displayText )
        {
            return FormatMapSnippet( rm, address, zipcode, true, displayText );

        } //

        /// <summary>
        /// Format the passed address parameters as a link to a map site
        /// </summary>
        /// <param name="rm">Resource Manager</param>
        /// <param name="address">Street address (typically don't need city or state)</param>
        /// <param name="zipcode">Zipcode (5 or 9)</param>
        /// <param name="useText">True - use text, otherwise display a common image</param>
        /// <param name="displayText">Text to display for link (ex. address)</param>
        /// <returns>Formatted link to a map website</returns>
        /// <remarks>06-11-10 mparsons - changed to have the map provider taken parameters for address and zip. This allows a more generic approach and flexibilty in providers</remarks>
        public static string FormatMapSnippet( ResourceManager rm, string address, string zipcode, bool useText, string displayText )
        {
            string mapSnippet = "";
            string mapsite = "";
            string mapUrl = "";
            string mapUrl2 = "";
            string anchorClass = GetAppKeyValue( "defaultAnchorClass" );
            string mapToolTip = UtilityManager.GetResourceValue( rm, "clickHereToViewMap_Tooltip" );

            try
            {
                string mapProvider = UtilityManager.GetAppKeyValue( "mapProvider", "http://maps.google.com/?q=loc%3a{0}+{1}" );
                string mapProvider2 = UtilityManager.GetAppKeyValue( "mapProvider2", "n/a" );

                //				string mapAddress = Server.UrlEncode(address);
                string mapAddress = address;
                mapAddress = mapAddress.Replace( System.Environment.NewLine, "+" );
                mapAddress = mapAddress.Replace( " ", "+" );
                mapAddress = mapAddress.Replace( ",", "+" );
                mapAddress = mapAddress.Replace( "++", "+" );
                mapUrl = String.Format( mapProvider, mapAddress, zipcode );
                //NOTE DO NOT encode the map url - gets messed up - just handle the &
                //mapUrl = EncodeUrl( mapUrl );

                if ( useText )
                {
                    mapsite = "<a " + anchorClass + " href='" + mapUrl + "' target='_blank' title='" + mapToolTip + "'>" + displayText + "</a>";

                    if ( mapProvider2 != "n/a" )
                    {
                        mapUrl2 = String.Format( mapProvider2, mapAddress, zipcode );
                        mapsite += " <a " + anchorClass + " href='" + mapUrl2 + "' target='_blank'> or " + UtilityManager.GetAppKeyValue( "mapLinkImg", "" ) + "</a>";
                    }
                }
                else
                {
                    string imgLink = UtilityManager.GetAppKeyValue( "mapLinkImg", "" );
                    if ( displayText.Length > 0 )
                    {
                        imgLink = displayText + " " + imgLink;
                    }

                    mapsite = "<a " + anchorClass + " href='" + mapUrl + "' target='_blank'>" + imgLink + "</a>";
                    if ( mapProvider2 != "n/a" )
                    {
                        mapUrl2 = String.Format( mapProvider2, mapAddress, zipcode );

                        mapsite += " <a " + anchorClass + " href='" + mapUrl2 + "' target='_blank'>or " + UtilityManager.GetAppKeyValue( "mapLinkImg", "" ) + "</a>";
                    }
                }

                mapSnippet = FormatLinks( mapsite );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "UtilityManager.FormatMapSnippet" );
                mapSnippet = "";
            }
            //handle the & for w3c validation cops
            if ( mapSnippet.IndexOf( "&amp;" ) == -1 )
            {
                mapSnippet = mapSnippet.Replace( "&", "&amp;" );
            }
            return mapSnippet;
        } //

        /// <summary>
        /// Format a link that opens in a new page (via popup or new browser)
        /// Primary purpose is to override a javascript popup. Uses system setting for popups.
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="url">Destination URL</param>
        /// <param name="displayName">Visible portion of URL</param>
        /// <returns>Formatted URL</returns>
        public static string FormatPopupLink( ResourceManager rm, string url, string displayName )
        {
            string usingPopups = UtilityManager.GetAppKeyValue( "usingPopups", "no" );
            bool allowPopups = false;

            if ( usingPopups.Equals( "yes" ) )
                allowPopups = true;

            return FormatPopupLink( rm, url, displayName, allowPopups );
        }

        /// <summary>
        /// Format a link that opens in a new page (via popup or new browser)
        /// Primary purpose is to override a javascript popup
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="url">Destination URL</param>
        /// <param name="displayName">Visible portion of URL</param>
        /// <returns>Formatted URL</returns>
        public static string FormatPopupLink( ResourceManager rm, string url, string displayName, bool allowPopups )
        {
            string formattedUrl = "";
            string linkSnippet = "";
            string onclick = "";
            string href = "";
            string anchorClass = GetAppKeyValue( "defaultAnchorClass" );

            string landingPageLink = UtilityManager.GetResourceValue( rm, "url_landing_page" );

            if ( IsPathInternal( url ) )
                linkSnippet = GetInternalLink( );
            else
                linkSnippet = GetExternalLink();

            href = " href =\""
                + landingPageLink + "?pred="
                + UtilityManager.EncodeUrl( url ) + "\" target='_blank'";

            if ( allowPopups )
            {
                //using popup
                onclick = " onclick=\"javascript:popLanding('"
                            + landingPageLink + "','"
                            + UtilityManager.EncodeUrl( url ) + "');return false;\"";

                //formattedUrl = "<a " + anchorClass + " href =\"javascript:popLanding('"
                //  + landingPageLink + "','"
                //  + UtilityManager.EncodeUrl( url ) + "')\">"
                //  + displayName + linkSnippet + "</a>";
            }
            else
            {
                //using new window
                //formattedUrl = "<a " + anchorClass + " href =\""
                //  + landingPageLink + "?pred="
                //  + UtilityManager.EncodeUrl( url ) + "\" target='_blank'>"
                //  + displayName + linkSnippet + "</a>";
            }

            formattedUrl = "<a " + anchorClass + onclick + href + ">"
                + displayName + linkSnippet + "</a>";

            return formattedUrl;
        } //

        /// <summary>
        /// Similar to the above, with the exception that if there is a confirmation message, display it before redirecting to the page. 
        /// Otherwise, do the regular version.
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="url">Destination URL</param>
        /// <param name="displayName">Visible portion of URL</param>
        /// <param name="allowPopups">True if popups are allowed. otherwise false</param>
        /// <param name="confirmationMessage">Message to display in the confirmation dialog</param>
        /// <returns>Formatted URL</returns>
        public static string FormatPopupLink( ResourceManager rm, string url, string displayName, bool allowPopups, string confirmationMessage )
        {
            string formattedUrl = "";
            string linkSnippet = "";
            //string onclick = "";
            string href = "";
            string anchorClass = GetAppKeyValue( "defaultAnchorClass" );
            string landingPageLink = UtilityManager.GetResourceValue( rm, "url_landing_page" );

            //if no confirmation message found (empty string), call the "non-extended" function 
            if ( confirmationMessage == "" )
            {
                formattedUrl = FormatPopupLink( rm, url, displayName, allowPopups );
            }
            else
            {
                //Create the link 

                if ( IsPathInternal( url ) )
                    linkSnippet = GetInternalLink( );
                else
                    linkSnippet = GetExternalLink();

                //this is a slightly simplified version of the href code used in the non-extended functions
                href = landingPageLink + "?pred=" + UtilityManager.EncodeUrl( url );

                //add the A tags and a javascript confirmation box. If confirm=OK then open the link in a new window. Otherwise do nothing.
                formattedUrl = "<a href='#' target='self' onclick='{var answer = confirm(\"" + confirmationMessage + "\");if (answer){  window.open (\"" + href + "\",\"_blank\"); return false }else{return false}};'>" + displayName + linkSnippet + "</a>";
            }
            return formattedUrl;
        }


        /// <summary>
        /// Returns a properly formatted url (checks for http prefix)
        /// </summary>
        /// <param name="url">URL to format</param>
        /// <returns></returns>
        public static string FormatUrl( string url )
        {
            string formattedUrl = "";

            if ( url.Length > 0 )
            {
                if ( url.ToLower().StartsWith( "http" ) )
                    formattedUrl = url;
                else
                    formattedUrl = "http://" + url;
            }

            return formattedUrl;
        }
        #endregion


    }
}


