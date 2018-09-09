using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.APIHandlers
{
    public class STATIONHandler
    {
        private readonly string stationName;
        private readonly MAVTable table;

        public STATIONHandler(JObject apiResponse)
        {;
            if (apiResponse == null) return;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(WebUtility.HtmlDecode(apiResponse["d"]["result"].ToString()));
            table = new MAVTable(html.DocumentNode.Descendants("table").FirstOrDefault(tb => tb.HasClass("af")));
            stationName = apiResponse["d"]["param"]["a"].ToString().Trim();
        }

        public void UpdateDatabase()
        {
            
        }
    }
}
