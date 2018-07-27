using HtmlAgilityPack;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MAVAppBackend.APIHandlers
{
    public class TRAINHandler
    {
        private MAVTable table;
        public Polyline polyline; // TODO: depublicize

        public TRAINHandler(JObject apiResponse)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(WebUtility.HtmlDecode((string)apiResponse["d"]["result"]["html"]));
            table = new MAVTable(html.DocumentNode.Descendants("table").FirstOrDefault(tb => tb.HasClass("vt")));
            polyline = new Polyline(Polyline.DecodePoints(apiResponse["d"]["result"]["line"][0]["points"].ToString(), 1E5));
        }

        public Train GetTrainFromHeader()
        {
            if (table == null) throw new MAVParseException("No train table.");

            HtmlNode tableHeader = table.GetTopHeader();
            IEnumerator<HtmlNode> header = table.GetTopHeader().Descendants().Where(n => n.ParentNode == tableHeader).GetEnumerator();
            if (!header.MoveNext()) throw new MAVParseException("No train header.");

            string[] nameType = header.Current.InnerText.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameType.Length < 1) throw new MAVParseException("Train header is not in the correct format.");

            if (!int.TryParse(nameType.First(), out int number)) throw new MAVParseException("Train header does not contain a number.");

            string type = nameType.Length > 1 ? nameType.Last() : null;
            string name = nameType.Length > 2 ? string.Join(" ", nameType.Skip(1).SkipLast(1)) : null;

            if (!header.MoveNext()) throw new MAVParseException("No train relations.");

            if (header.Current.Name == "ul")
            {
                type = figureOutTypeUl(header.Current);
                if (!header.MoveNext()) throw new MAVParseException("No train relations.");
            }

            if (header.Current.HasClass("viszszam2"))
            {
                name = header.Current.InnerText;
                if (!header.MoveNext()) throw new MAVParseException("No train relations.");
            }

            bool next = true;
            while (header.Current.Name != "font" && (next = header.MoveNext())) ;
            if (!next) throw new MAVParseException("No train relations.");

            string relation = header.Current.InnerText.Substring(1, header.Current.InnerText.IndexOf(",") - 1);
            relation.Split(" - ");

            string[] relationSpl = relation.Split(" - ");
            if (relationSpl.Length != 2) throw new MAVParseException("Train relation is not in the correct format.");
            relationSpl[0] = relationSpl[0].Trim();
            relationSpl[1] = relationSpl[1].Trim();

            string from = null;
            string to = null;

            if (relationSpl[0] != "")
            {
                from = relationSpl[0];
            }

            if (relationSpl[1] != "")
            {
                to = relationSpl[1];
            }

            if (from == null || to == null) throw new MAVParseException("Train relation is not in the correct format.");

            Train train = Database.Instance.TrainMapper.GetByKey(number);
            train.APIFill(name, from, to, type);
            Database.Instance.TrainMapper.Update(train);
            return train;
        }

        public List<(string stationName, Station station, TimeSpan? schedDeparture, TimeSpan? schedArrival, string platform)> Test()
        {
            List<(string stationName, Station station, TimeSpan? schedDeparture, TimeSpan? schedArrival, string platform)> ret
                = new List<(string stationName, Station station, TimeSpan? schedDeparture, TimeSpan? schedArrival, string platform)>();

            Database.Instance.StationMapper.BeginSelectNormName(new WhereInStrategy<string, Station>());
            foreach (MAVTableRow row in table.GetRows())
            {
                string stationName = row.GetCellString(1);
                Station station = Database.Instance.StationMapper.GetByName(stationName, false);
                (TimeSpan? schedDeparture, TimeSpan? actDeparture) = row.GetCellTimes(2);
                (TimeSpan? schedArrival, TimeSpan? actArrival) = row.GetCellTimes(3);

                string platform = row.GetCellString(4);
                if (platform != null) platform = platform.Trim();
                if (platform == "") platform = null;

                ret.Add((stationName, station, schedDeparture, schedArrival, platform));
            }
            Database.Instance.StationMapper.EndSelectNormName();

            Database.Instance.StationMapper.BeginUpdate();
            foreach ((string stationName, Station station, TimeSpan? schedDeparture, TimeSpan? schedArrival, string platform) in ret)
            {
                if (!station.Filled) Database.Instance.StationMapper.Update(station);
            }
            Database.Instance.StationMapper.EndUpdate();

            return ret;
        }

        public void LineMapping()
        {
            
        }

        private string figureOutTypeUl(HtmlNode ul)
        {
            IEnumerable<string> types = ul.Descendants("li").Select(li =>
            {
                IEnumerator<HtmlNode> enumerator = li.Descendants().GetEnumerator();
                if (!enumerator.MoveNext()) throw new MAVParseException("Train type list is not in the correct format.");
                string[] spl = enumerator.Current.InnerText.Split(':');
                if (spl.Length < 1) throw new MAVParseException("Train type list is not in the correct format.");

                if (spl.Length == 1 || spl[1].Trim() == "")
                {
                    if (!enumerator.MoveNext()) throw new MAVParseException("Train type list is not in the correct format.");
                    return enumerator.Current.Attributes["alt"].Value.Trim();
                }
                else return spl[1].Trim();
            });

            return types.SkipWhile(s => s == "vonatpótló autóbusz").FirstOrDefault() ?? throw new MAVParseException("Train type list is not in the correct format.");
        }   
    }
}
