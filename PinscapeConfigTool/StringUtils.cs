using System;
using System.Text.RegularExpressions;

namespace StringUtils
{
    public static class StringUtils
    {
        // quote HTML characters
        public static String Htmlify(this String s)
        {
            return Regex.Replace(s, @"[<>&""'/]", new MatchEvaluator(m =>
            {
                switch (m.Value)
                {
                    case "<": return "&lt;";
                    case ">": return "&gt;";
                    case "&": return "&amp;";
                    case "\"": return "&#34;";
                    case "'": return "&#39;";
                    case "/": return "&#47;";
                    default: return m.Value;
                }
            }));
        }

        // escape a string for js insertion
        public static String JSStringify(this String s)
        {
            return Regex.Replace(s, @"[\\""\n\r]", new MatchEvaluator(m =>
            {
                switch (m.Value)
                {
                    case "\n": return "\\n";
                    case "\r": return "\\r";
                    default: return "\\" + m.Value;
                }
            }));
        }
    }
}
