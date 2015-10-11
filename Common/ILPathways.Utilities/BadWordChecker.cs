using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ILPathways.Utilities
{
    public class BadWordChecker
    {
        public static string badWords = @"anus|arse|arsehole|ass|a$$|ass-hat|ass-pirate|ass-pirating|assbag|assbagging|assbandit|assbanger|assbanging|assbite|assbiting|assclown|assclowning|asscock|asscracker|asscracking|asses|assface|assfuck|assfucker|assfucking|assgoblin|asshat|asshead|asshole|asshopper|asshopping|assjacker|assjacking|asslick|asslicker|asslicking|assmonkey|assmunch|assmuncher|assmunching|assnigger|asspirate|asspirating|assshit|assshitting|assshole|asssucker|asssucking|asswad|asswipe|asswiping|bampot|bastard|beaner|bitch|bitchass|bitches|bitching|bitchtits|bitchy|blowjob|bollocks|bollox|boner|boning|brotherfucker|brotherfucking|bullshit|bullshitting|bumblefuck|bumblefucking|buttplug|buttplugging|butt-pirate|butt-pirating|buttfucka|buttfucker|buttfucking|cameltoe|carpetmuncher|carpetmunching|chinc|chink|choad|chode|clit|clitface|clitfuck|clitfucking|clusterfuck|clusterfucking|cock|cockass|cockbite|cockbiting|cockburger|cockface|cockfucker|cockfucking|cockhead|cockjockey|cockknoker|kockknoking|cockmaster|cockmongler|cockmongruel|cockmonkey|cockmuncher|cockmunching|cocknose|cocknugget|cockshit|cocksmith|cocksmoker|cocksmoking|cocksucker|cocksucking|coochie|coochy|coon|cooter|cracker|cum|cumbubble|cumdumpster|cumguzzler|cumguzzling|cumjockey|cumslut|cumtart|cunnie|cunnilingus|cunt|cuntface|cunthole|cuntlicker|cuntlicking|cuntrag|cuntslut|dago|damn|deggo|dickbag|dickbagging|dickbeaters|dickbeating|dickface|dickfuck|dickfucker|dickfucking|dickhead|dickhole|dickjuice|dickmilk|dickmonger|dickmongering|dicks|dickslap|dickslapping|dicksucker|dicksucking|dickwad|dickweasel|dickweed|dickwod|dike|dildo|dipshit|doochbag|dookie|douche|douche-fag|douchebag|douchewaffle|dumass|dumbass|dumbass|dumbfuck|dumbshit|dumshit|dyke|fag|fagbag|fagbagging|fagfucker|fagfucking|faggit|faggot|faggotcock|fagtard|fatass|fellatio|feltch|flamer|fuck|fuckass|fuckbag|fuckboy|fuckbrain|fuckbutt|fucked|fucker|fucking|fuckersucker|fuckface|fuckhead|fuckhole|fuckin|fucking|fucknut|fucknutt|fuckoff|fucks|fuckstick|fucksticking|fucktard|fuckup|fuckwad|fuckwit|fuckwitt|fudgepacker|fudgepacking|gayass|gaya$$|gaybob|gaydo|gayfuck|gayfucker|gayfucking|gayfuckist|gaylord|gaytard|gaywad|goddamn|goddammit|goddamnit|gooch|gook|gringo|guido|handjob|hardon|heeb|homodumbshit|honkey|humping|jackass|jap|jerkoff|jigaboo|jizz|junglebunny|junglebunny|kike|kooch|kootch|kunt|kyke|lesbian|lesbo|lezzie|mcfagget|mick|minge|mothafucka|motherfucker|motherfucking|muff|muffdiver|munging|negro|nigga|nigger|niggers|niglet|nutsack|nutsack|paki|panooch|pecker|peckerhead|penis|penisfucker|penispuffer|piss|pissed|pissedoff|pissflaps|polesmoker|polesmoking|poon|poonani|poonany|poontang|porchmonkey|prick|punanny|punta|pussies|pussy|pussylicker|pussylicking|puto|queef|queer|queerbait|queerhole|renob|rimjob|ruski|sandnigger|schlong|scrote|shit|shitass|shitbag|shitbagger|shitbrains|shitbreath|shitcunt|shitdick|shitface|shitfaced|shithead|shithole|shithouse|shitspitter|shitspitting|shitstain|shitter|shittiest|shitting|shitty|shiz|shiznit|skank|skeet|skullfuck|slut|slutbag|slutbagging|smeg|smegma|snatch|spic|spick|splooge|tard|testicle|thundercunt|tit|titfuck|tits|tittyfuck|twat|twatlips|twats|twatwaffle|unclefucker|unclefucking|va-j-j|vagina|viagara|vjayjay|wank|wetback|whore|whorebag|whorebagging|whoreface";

		public static string wildBadWords = @"an*s|a*sehole|a**|a**-hat|a**-pirate|a**-pirating|a**bag|a**bagging|a**bandit|a**banger|a**banging|a**bite|a**biting|a**clown|a**clowning|a**cock|a**cracker|a**cracking|a**es|a**face|a**fuck|a**fucker|a**fucking|a**goblin|a**hat|a**head|a**hole|a**hopper|a**hopping|a**jacker|a**jacking|a**lick|a**licker|a**licking|a**monkey|a**munch|a**muncher|a**munching|a**nigger|a**pirate|a**pirating|a**shit|a**shitting|a**shole|a**sucker|a**sucking|a**wad|a**wipe|a**wiping|b*stard|b*tch|bitcha**|b*tches|b*tching|b*tchtits|b*tchy|bl*wjob|b*ner|b*ning|br*therf*cker|br*therf*cking|bullsh*t|bullsh*tting|bumblef*ck|bumblef*cking|b*ttplug|b*ttplugging|b*tt-pirate|b*tt-pirating|b*ttfucka|b*ttfucker|b*ttfucking|ch*nc|ch*nk|cl*t|cl*tface|cl*tfuck|cl*tfucking|cl*sterfuck|cl*sterfucking|c*ck|cocka**|c*ckbite|c*ckbiting|c*ckburger|c*ckface|c*ckfucker|c*ckfucking|c*ckhead|c*ckjockey|c*ckknoker|k*ckknoking|c*ckmaster|c*ckmongler|c*ckmongruel|c*ckmonkey|c*ckmuncher|c*ckmunching|c*cknose|c*cknugget|c*ckshit|c*cksmith|c*cksmoker|c*cksmoking|c*cksucker|c*cksucking|c*ochie|c*ochy|c*on|c*oter|cr*cker|c*m|c*mbubble|c*mdumpster|c*mguzzler|c*mguzzling|c*mjockey|c*msl*t|c*mtart|c*nnilingus|c*nt|c*ntface|c*nthole|c*ntlicker|c*ntlicking|c*ntrag|c*ntslut|d*go|d*mn|d*ckbag|d*ckbagging|d*ckbeaters|d*ckbeating|d*ckface|d*ckfuck|d*ckfucker|d*ckfucking|d*ckhead|d*ckhole|d*ckjuice|d*ckmilk|d*ckmonger|d*ckmongering|d*cks|d*ckslap|d*ckslapping|d*cksucker|d*cksucking|d*ckwad|d*ckweasel|d*ckweed|d*ckwod|d*ke|d*ldo|dipsh*t|d*ochbag|d*okie|d*uche|d*uche-fag|d*uchebag|d*uchewaffle|duma**|dumba**|dumba**|dumbf*ck|dumbsh*t|dumsh*t|f*g|f*gbag|f*gbagging|f*gfucker|f*gfucking|f*ggit|f*ggot|f*ggotcock|f*gtard|fata**|f*llatio|f*ck|f*cka**|f*ckbag|f*ckboy|f*ckbrain|f*ckbutt|f*cked|f*cker|f*cking|f*ckersucker|f*ckersucking|f*ckface|f*ckhead|f*ckhole|f*ckin|f*cking|f*cknut|f*cknutt|f*ckoff|f*cks|f*ckstick|f*cksticking|f*cktard|f*ckup|f*ckwad|f*ckwit|f*ckwitt|f*dgepacker|f*dgepacking|gaya**|g*ybob|g*ydo|gayf*ck|gayf*cker|gayf*cking|gayf*ckist|g*ytard|g*ywad|g*ddamn|g*ddammit|g*och|g*ok|handj*b|homodumbsh*t|jacka**|k*unt|k*ke|mothaf*cka|motherf*cker|motherf*cking|m*ffdiver|n*gga|n*gger|n*ggers|n*glet|p*cker|p*ckerhead|p*nis|p*nisfucker|p*nispuffer|p*ss|p*ssed|p*ssedoff|p*ssflaps|p*on|po*n|p*onany|p*ontang|po*ntang|pr*ck|p*ssy|p*ssylicker|p*ssylicking|qu*er|que*r|r*mjob|sandn*gger|schl*ng|sh*t|sh*ta**|sh*ta$$|shita**|sh*tbag|sh*tbagger|sh*tbrains|sh*tbreath|sh*tcunt|sh*tdick|sh*tface|sh*tfaced|sh*thead|sh*thole|sh*thouse|sh*tspitter|sh*tspitting|sh*tstain|sh*tter|sh*ttiest|sh*tting|sh*tty|sh*z|skullf*ck|sl*t|sl*tbag|sl*tbagging|sm*g|sm*gma|sn*tch|sp*c|sp*ck|thunderc*nt|t*t|titf*ck|t*tfuck|t*ts|tittyf*ck|t*ttyfuck|tw*t|tw*tlips|tw*ts|tw*twaffle|unclef*cker|unclef*cking|v*gina|vag*na|w*nk|wh*re|wh*rebag|wh*rebagging|wh*reface";

		public static string badWordsAt = @"f@ck|f@ckass|f@ckbag|f@ckboy|f@ckbrain|f@ckbutt|f@cked|f@cker|f@cking|f@ckersucker|f@ckface|f@ckhead|f@ckhole|f@ckin|f@cknut|f@cknutt|f@ckoff|f@cks|f@ckstick|f@cksticking|f@cktard|f@ckup|f@ckwad|f@ckwit|f@ckwitt|";

        public static bool CheckForBadWords(string txtInput)
        {
            // Certain characters, such as ^,@,$ have special meaning in a regular expression.  Escape these characters to fix up the badWord string.
            // Use a StringBuilder for memory efficiency
			string words = GetBadwords();
			StringBuilder badWordStringBuilder = new StringBuilder( words );
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

		public static string GetBadwords()
		{
			//or just successively replace each vowel, by each special character
			string words = badWords;
			words += wildBadWords;
			words += wildBadWords.Replace( "*", "@" );
			words += wildBadWords.Replace( "*", "$" );

			return words;
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
