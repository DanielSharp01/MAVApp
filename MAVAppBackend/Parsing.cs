using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend
{
    public static class Parsing
    {
        public static TimeSpan? TimeFromHoursMinutes(string hmString)
        {
            if (hmString == null) return null;

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

        public static (TimeSpan? first, TimeSpan? second) GetCellTimes(HtmlNode cell)
        {
            TimeSpan? first = null;
            TimeSpan? second = null;
            using (IEnumerator<HtmlNode> cellNodes = cell.Descendants().GetEnumerator())
            {

                if (cellNodes.MoveNext() && cellNodes.Current.InnerText.Trim() != "")
                {
                    first = TimeFromHoursMinutes(cellNodes.Current.InnerText);
                    if (first == null)
                        throw new MAVAPIException("Cell times are not in the correct format.");
                }

                if (cellNodes.MoveNext() && cellNodes.MoveNext() && cellNodes.Current.InnerText.Trim() != "")
                {
                    second = TimeFromHoursMinutes(cellNodes.Current.InnerText);
                    if (second == null)
                        throw new MAVAPIException("Cell times are not in the correct format.");
                }
            }

            return (first, second);
        }
    }
}
