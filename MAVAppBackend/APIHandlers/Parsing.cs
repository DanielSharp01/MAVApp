using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.APIHandlers
{
    public static class Parsing
    {
        public static TimeSpan? TimeFromHoursMinutes(string hmString)
        {
            string[] spl = hmString.Split(':');
            if (spl.Length == 2 && int.TryParse(spl[0], out int hours) && int.TryParse(spl[1], out int minutes))
            {
                return new TimeSpan(hours, minutes, 0);
            }

            return null;
        }

        public static string SubstringBetween(this string str, int start, int end)
        {
            return str.Substring(start, end - start + 1);
        }

        public static string SubstringBetweenChars(this string str, char start, char end)
        {
            int s = str.IndexOf(start);
            if (s == -1) return null;
            int e = str.IndexOf(end, s);
            if (e == -1) return null;

            return str.SubstringBetween(s, e);
        }

        public static JObject OnClickToJOBject(string onclick)
        {
            return JObject.Parse(onclick.SubstringBetweenChars('{', '}'));
        }
    }
}
