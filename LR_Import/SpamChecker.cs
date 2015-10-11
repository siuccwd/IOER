using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILPathways.Utilities;

namespace LearningRegistryCache2
{
    public class SpamChecker
    {
        public static bool IsSpam(string record)
        {
            // Assume the record is not spam.
            bool retVal = false;

            // If the spamScore is > 80, it's spam.
            if (CalculateSpamScore(record) > 80)
            {
                retVal = true;
            }

            return retVal;
        }

        /// <summary>
        /// Calculate a Spam score
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static int CalculateSpamScore(string record)
        {
            /***************************************************************************************
             * Begin with a spamScore of zero (NOT SPAM).  Add points for each thing that is       *
             * considered to be spammy - some things may be spammier than others.                  *
             ***************************************************************************************/
            int spamScore = 0;

            // If it contains bad words, it should probably be flagged as spam based on that alone.
            if (BadWordChecker.CheckForBadWords(record))
            {
                spamScore += 100;
            }

            return spamScore;
        }
    }
}
