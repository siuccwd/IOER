using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkChecker2
{
    public class ContentCheck
    {
        static private bool blackHatDetected = false;
        static private Regex LeadingWhitespace = new Regex("^\\s*");
        static private Regex TrailingWhitespace = new Regex("\\s*$");

        static public bool UsesBlackHatTechniques(string pageContent)
        {
            blackHatDetected = false;
            CheckForNoscriptTagOnly(pageContent);
            
            return blackHatDetected;
        }

        /// <summary>
        /// Checks for noscript tag in the body of a document
        /// </summary>
        /// <param name="pageContent"></param>
        /// <returns></returns>
        static private bool CheckForNoscriptTagOnly(string pageContent) 
        {
            if (blackHatDetected)
                return blackHatDetected;
            if (pageContent == null || pageContent == string.Empty)
                return blackHatDetected;
            //pageContent = "<body someAttribute=someValue>\n<noscript anotherAttribute=anotherValue>Some content</noscript>\n</body>";
            int start = 0;
            int end = 0;
            int length = 0;

            if (pageContent.IndexOf("<noscript", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                // Noscript tag not found, so no need to check for this.
                blackHatDetected = false;
                return blackHatDetected;
            }

            // Extract the body tag
            start = pageContent.IndexOf("<body", StringComparison.InvariantCultureIgnoreCase);
            end = pageContent.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase) + "</body>".Length;
            if (start > -1 && end > -1)
            {
                // Extract the inner contents of the body tag
                pageContent = pageContent.Substring(start, end - start);
                start = pageContent.IndexOf(">", StringComparison.InvariantCultureIgnoreCase) + 1;
                end = pageContent.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);
                pageContent = pageContent.Substring(start, end - start);
                // trim leading and trailing whitespace
                pageContent = LeadingWhitespace.Replace(pageContent, "");
                pageContent = TrailingWhitespace.Replace(pageContent, "");
                start = pageContent.IndexOf("<noscript", StringComparison.InvariantCultureIgnoreCase);
                end = pageContent.IndexOf("</noscript>", StringComparison.InvariantCultureIgnoreCase) + "</noscript>".Length;
                length = end - start;
                if (length == pageContent.Length)
                {
                    // All contents of the body tag, ignoring leading and trailing white space, are part of the noscript tag.
                    // This is a black hat technique, return true
                    blackHatDetected = true;
                }
            }

            return blackHatDetected;
        }

    }
}
