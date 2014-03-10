using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ILPathways.Utilities
{
    public class BadWordChecker
    {
        public static string badWords = @"anus|arse|arsehole|ass|a$$|ass-hat|ass-pirate|ass-pirating|assbag|assbagging|assbandit|assbanger|assbanging|assbite|assbiting|assclown|assclowning|asscock|asscracker|asscracking|asses|assface|assfuck|assfucker|assfucking|assgoblin|asshat|asshead|asshole|asshopper|asshopping|assjacker|assjacking|asslick|asslicker|asslicking|assmonkey|assmunch|assmuncher|assmunching|assnigger|asspirate|asspirating|assshit|assshitting|assshole|asssucker|asssucking|asswad|asswipe|asswiping|bampot|bastard|beaner|bitch|bitchass|bitches|bitching|bitchtits|bitchy|blowjob|bollocks|bollox|boner|boning|brotherfucker|brotherfucking|bullshit|bullshitting|bumblefuck|bumblefucking|buttplug|buttplugging|butt-pirate|butt-pirating|buttfucka|buttfucker|buttfucking|cameltoe|carpetmuncher|carpetmunching|chinc|chink|choad|chode|clit|clitface|clitfuck|clitfucking|clusterfuck|clusterfucking|cock|cockass|cockbite|cockbiting|cockburger|cockface|cockfucker|cockfucking|cockhead|cockjockey|cockknoker|kockknoking|cockmaster|cockmongler|cockmongruel|cockmonkey|cockmuncher|cockmunching|cocknose|cocknugget|cockshit|cocksmith|cocksmoker|cocksmoking|cocksucker|cocksucking|coochie|coochy|coon|cooter|cracker|cum|cumbubble|cumdumpster|cumguzzler|cumguzzling|cumjockey|cumslut|cumtart|cunnie|cunnilingus|cunt|cuntface|cunthole|cuntlicker|cuntlicking|cuntrag|cuntslut|dago|damn|deggo|dickbag|dickbagging|dickbeaters|dickbeating|dickface|dickfuck|dickfucker|dickfucking|dickhead|dickhole|dickjuice|dickmilk|dickmonger|dickmongering|dicks|dickslap|dickslapping|dicksucker|dicksucking|dickwad|dickweasel|dickweed|dickwod|dike|dildo|dipshit|doochbag|dookie|douche|douche-fag|douchebag|douchewaffle|dumass|dumbass|dumbass|dumbfuck|dumbshit|dumshit|dyke|fag|fagbag|fagbagging|fagfucker|fagfucking|faggit|faggot|faggotcock|fagtard|fatass|fellatio|feltch|flamer|f*ck|fuck|f*ckass|fuckass|f*ckbag|fuckbag|f*ckboy|fuckboy|f*ckbrain|fuckbrain|f*ckbutt|fuckbutt|f*cked|fucked|f*cker|fucker|f*cking|fucking|f*ckersucker|fuckersucker|f*ckersucking|f*ckface|fuckface|f*ckhead|fuckhead|f*ckhole|fuckhole|f*ckin|fuckin|f*cking|fucking|f*cknut|fucknut|f*cknutt|fucknutt|f*ckoff|fuckoff|f*cks|fucks|f*ckstick|fuckstick|f*cksticking|fucksticking|f*cktard|fucktard|f*ckup|fuckup|f*ckwad|fuckwad|f*ckwit|fuckwit|f*ckwitt|fuckwitt|fudgepacker|fudgepacking|gayass|gaya$$|gaybob|gaydo|gayf*ck|gayfuck|gayf*cker|gayfucker|gayf*cking|gayfucking|gayf*ckist|gayfuckist|gaylord|gaytard|gaywad|goddamn|goddammit|goddamnit|gooch|gook|gringo|guido|handjob|hardon|heeb|homodumbshit|honkey|humping|jackass|jap|jerkoff|jigaboo|jizz|junglebunny|junglebunny|kike|kooch|kootch|kunt|kyke|lesbian|lesbo|lezzie|mcfagget|mick|minge|mothaf*cka|mothafucka|motherf*cker|motherfucker|motherf*cking|motherfucking|muff|muffdiver|munging|negro|nigga|nigger|niggers|niglet|nutsack|nutsack|paki|panooch|pecker|peckerhead|penis|penisfucker|penispuffer|piss|pissed|pissedoff|pissflaps|polesmoker|polesmoking|poon|poonani|poonany|poontang|porchmonkey|prick|punanny|punta|pussies|pussy|pussylicker|pussylicking|puto|queef|queer|queerbait|queerhole|renob|rimjob|ruski|sandnigger|schlong|scrote|sh*t|shit|sh*tass|sh*ta$$|shitass|sh*tbag|shitbag|sh*tbagger|shitbagger|sh*tbrains|shitbrains|sh*tbreath|shitbreath|sh*tcunt|shitcunt|sh*tdick|shitdick|sh*tface|shitface|sh*tfaced|shitfaced|sh*thead|shithead|sh*thole|shithole|sh*thouse|shithouse|sh*tspitter|shitspitter|sh*tspitting|shitspitting|sh*tstain|shitstain|sh*tter|shitter|sh*ttiest|shittiest|sh*tting|shitting|sh*tty|shitty|shiz|shiznit|skank|skeet|skullfuck|slut|slutbag|slutbagging|smeg|smegma|snatch|spic|spick|splooge|tard|testicle|thundercunt|tit|titf*ck|titfuck|tits|tittyf*ck|tittyfuck|twat|twatlips|twats|twatwaffle|unclefucker|unclefucking|va-j-j|vagina|viagara|vjayjay|wank|wetback|whore|whorebag|whorebagging|whoreface";

        public static bool CheckForBadWords(string txtInput)
        {
            // Certain characters, such as ^,*,$ have special meaning in a regular expression.  Escape these characters to fix up the badWord string.
            // Use a StringBuilder for memory efficiency
            StringBuilder badWordStringBuilder = new StringBuilder(badWords);
            badWordStringBuilder.Replace("*","\\*");
            badWordStringBuilder.Replace("$","\\$");
            badWordStringBuilder.Replace("^", "\\^");
            bool FoundMatch = false;
            try
            {
                FoundMatch = Regex.IsMatch(txtInput, "\\b(?:" + badWordStringBuilder.ToString() + ")\\b",
                  RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

            return FoundMatch;


        }

        public static string Highlight(string InputTxt)
        {
            string s = InputTxt;
            if (s != null)
            {
                //string Search_Str = txtGrid.Text.ToString();                   
                // Setup the regular expression         
                Regex regex = new Regex(badWords.Trim(), RegexOptions.IgnoreCase);
                //Highlight keywords by calling the delegate                         
                //each time a keyword is found.                        
                return
                  regex.Replace(s, new MatchEvaluator(ReplaceKeyWords));
                // Set the RegExp to null.                            
                //Regex = null;  
            }
            else
            {
                return s;
            }
        }


        public static string ReplaceKeyWords(Match m)
        {
            // may need span method for other browsers - left as an example tcw
            //return "<span class=highlight>" + m.Value + "</span>";
            return "<font style='background:yellow; color:red'>" + m.Value + "</font>";
        }

    }
}
