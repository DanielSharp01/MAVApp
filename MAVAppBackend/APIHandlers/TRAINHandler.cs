using HtmlAgilityPack;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MAVAppBackend.APIHandlers
{
    public class TRAINHandler
    {
        private long? instanceID;
        private HtmlNode mainDiv;
        private MAVTable table;
        private Polyline polyline;

        public TRAINHandler(JObject apiResponse)
        {
            if (apiResponse == null) return;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(WebUtility.HtmlDecode((string)apiResponse["d"]["result"]["html"]));
            mainDiv = html.DocumentNode.FirstChild;
            table = new MAVTable(html.DocumentNode.Descendants("table").FirstOrDefault(tb => tb.HasClass("vt")));
            polyline = new Polyline(Polyline.DecodePoints(apiResponse["d"]["result"]["line"][0]["points"].ToString(), 1E5));

            object v = apiResponse["d"]["param"]["v"];
            if (v != null)
                instanceID = TrainInstance.GetInstanceID(v.ToString());
        }

        public void UpdateDatabase()
        {
            if (table == null) throw new MAVParseException("No train table.");

            int number;
            string type, name;
            HtmlNode tableHeader = table.GetTopHeader();
            using (IEnumerator<HtmlNode> header = table.GetTopHeader().ChildNodes.AsEnumerable().GetEnumerator())
            {
                if (!header.MoveNext()) throw new MAVParseException("No train header.");

                string[] nameType = header.Current.InnerText.Split(new char[] {' ', '\r', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (nameType.Length < 1) throw new MAVParseException("Train header is not in the correct format.");

                if (!int.TryParse(nameType.First(), out number)) throw new MAVParseException("Train header does not contain a number.");

                type = nameType.Length > 1 ? nameType.Last() : null;
                name = nameType.Length > 2 ? string.Join(" ", nameType.Skip(1).SkipLast(1)) : null;

                if (header.MoveNext() && header.Current.Name == "ul")
                    type = FigureOutTypeUl(header.Current);

                if (header.MoveNext() && header.Current.HasClass("viszszam2"))
                    name = header.Current.InnerText;
            }

            HtmlNode expiryDateLink = mainDiv.ChildNodes.SkipWhile(h => h.InnerText.Trim() != "Menetrend").SkipWhile(ul => ul.Name != "ul").FirstOrDefault()?.Descendants("li")
                .FirstOrDefault(li => li.Attributes.Contains("style") && li.Attributes["style"].Value.Contains("bolder"))
                ?.Descendants("a").FirstOrDefault();

            DateTime? expiryDate = expiryDateLink == null ? (DateTime?)null : DateTime.Parse(expiryDateLink.InnerText.Split('-')[1]);

            DateTime now = DateTime.Now;
            Train train = Database.Instance.TrainMapper.GetByKey(number);
            train.Name = name;
            train.Type = type;
            train.Polyline = polyline;
            train.ExpiryDate = expiryDate ?? new DateTime(now.Year, 12, 31, 23, 59, 59);
            Database.Instance.TrainMapper.Update(train);

            if (expiryDateLink != null)
            {
                long foundInstance = TrainInstance.GetInstanceID(Parsing.OnClickToJOBject(expiryDateLink.Attributes["onclick"].Value)["v"]?.ToString());
                if (foundInstance != instanceID)
                {
                    Database.Instance.TrainInstanceMapper.Update(new TrainInstance() {Key = foundInstance, TrainID = number});
                }
            }
            if (instanceID.HasValue)
                Database.Instance.TrainInstanceMapper.Update(new TrainInstance() {Key = instanceID.Value, TrainID = number});

            UpdateStations(number);
            
        }

        private string FigureOutTypeUl(HtmlNode ul)
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

        private void UpdateStations(int trainNumber)
        {
            List<MAVTableRow> rows = table.GetRows().ToList();

            Database.Instance.StationMapper.ByNormName.BeginSelect();
            List<Station> stations = new List<Station>();
            foreach (var row in rows)
            {
                Station station = new Station()
                {
                    Key = -1,
                    Name = row.GetCellString(1),
                    NormalizedName = Database.StationNormalizeName(row.GetCellString(1))
                };

                Database.Instance.StationMapper.ByNormName.FillByKey(station);
                stations.Add(station);
            }
            Database.Instance.StationMapper.ByNormName.EndSelect();
            Database.Instance.StationMapper.Update(stations.Where(s => !s.Filled).ToList());

            double?[] relativeDistances = new double?[stations.Count];
            for (int i = 0; i < stations.Count; i++)
            {
                double? dist = null;

                if (stations[i].GPSCoord != null)
                {
                    dist = polyline.GetProjectedDistance(stations[i].GPSCoord, 0.25); 
                }

                relativeDistances[i] = dist;
            }

            bool flip = false;
            double? lastLength = null;
            foreach (double? dist in relativeDistances)
            {
                if (lastLength.HasValue && dist.HasValue && dist < lastLength)
                {
                    flip = true;
                    break;
                }

                if (dist.HasValue)
                    lastLength = dist;
            }

            // We have to flip the line because we don't have positive distances
            if (flip)
            {
                double? first = relativeDistances.FirstOrDefault(r => r.HasValue);

                if (first.HasValue)
                {
                    for (int i = 0; i < relativeDistances.Length; i++)
                    {
                        if (relativeDistances[i] != null)
                            relativeDistances[i] = first - relativeDistances[i];
                    }
                }

                polyline = new Polyline(polyline.Points.Reverse());
            }
            
            Database.Instance.TrainStationMapper.BeginUpdate();
            
            for (int i = 0; i < rows.Count; i++)
            {
                string platform = rows[i].GetCellString(4).Trim();
                TrainStation trainStation = new TrainStation()
                {
                    TrainID = trainNumber,
                    Ordinal = i,
                    StationID = stations[i].Key,
                    Arrival = rows[i].GetCellTimes(2).first,
                    Departure = rows[i].GetCellTimes(3).first,
                    RelativeDistance = relativeDistances[i],
                    Platform = platform == "" ? null : platform
                };
                Database.Instance.TrainStationMapper.Update(trainStation);
            }
            Database.Instance.TrainStationMapper.EndUpdate();


            List<TrainStation> trainStations = Database.Instance.TrainStationMapper.ByTrainID.GetByKey(trainNumber).ToList(); 
            if (instanceID.HasValue)
            {
                Database.Instance.TrainInstanceStationMapper.BeginUpdate();
                for (int i = 0; i < rows.Count; i++)
                {
                    TrainInstanceStation trainStation = new TrainInstanceStation()
                    {
                        TrainInstanceID = instanceID.Value,
                        TrainStationID = trainStations[i].Key,
                        ActualArrival = (((TimeSpan?, TimeSpan? second))rows[i].GetCellObject(2)).second,
                        ActualDeparture = (((TimeSpan?, TimeSpan? second))rows[i].GetCellObject(3)).second
                    };
                    Database.Instance.TrainInstanceStationMapper.Update(trainStation);
                }
                Database.Instance.TrainInstanceStationMapper.EndUpdate();
            }
        }
    }
}
