//#define TEST
using System.Text.RegularExpressions;
//using System.Windows.Forms;


namespace Telegram_Bot
{
    public class IsRegexMatch
    {
        string Pattern { get; set; }
        string Str { get; set; }
        public MatchCollection matches;
        public IsRegexMatch(string pattern, string str = null)
        {
            if (pattern != null)
            {
                Pattern = pattern;
                Str = str;
                Regex rg = new Regex(Pattern);
                if (str != null)
                {
                    matches = rg.Matches(str);
                }
            }
        }
        public bool IsMatch(string str, string pattern = null)
        {
            if (pattern != null)
                Pattern = pattern;
            Str = str;
            Regex rg = new Regex(this.Pattern);
            matches = rg.Matches(this.Str);
            return matches.Count > 0;
            
        }
    }
}
