using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.APIHandlers
{
    public class ROUTEHandler
    {
        private readonly MAVTable table;

        public ROUTEHandler(JObject apiResponse)
        {
            if (apiResponse == null) return;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(WebUtility.HtmlDecode(apiResponse["d"]["result"].ToString()));
            table = new MAVTable(html.DocumentNode.Descendants("table").FirstOrDefault(tb => tb.HasClass("uf")));
        }

        public void UpdateDatabase()
        {
        }
    }
}

