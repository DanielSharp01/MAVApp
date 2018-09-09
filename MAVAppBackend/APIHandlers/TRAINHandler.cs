using HtmlAgilityPack;
using MAVAppBackend.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MAVAppBackend.APIHandlers
{
    public class TRAINHandler
    {
        public static readonly LoggerFactory MyLoggerFactory
            = new LoggerFactory(new[] { new ConsoleLoggerProvider((category, level)
            => category == DbLoggerCategory.Database.Command.Name
               && level == LogLevel.Information, true) });

        private HtmlDocument html;
        private Polyline polyline;
        private string elviraId;
        private int trainNumber;
        private MAVAppContext DbContext;

        public TRAINHandler(JObject apiResponse)
        {
            // TODO: DBContext should not be created here and also not disposed at the end of handle
            DbContext = new MAVAppContext(new DbContextOptionsBuilder<MAVAppContext>().UseMySql("Server=localhost;Database=mavapp_ef;Uid=root;Pwd=mysql;")
                .UseLoggerFactory(MyLoggerFactory)
                .Options);

            if (apiResponse == null) return;

            html = new HtmlDocument();
            html.LoadHtml(WebUtility.HtmlDecode(apiResponse["d"]["result"]["html"].ToString()));
            try
            {
                polyline = new Polyline(Polyline.DecodePoints(apiResponse["d"]["result"]["line"][0]["points"].ToString(), Polyline.EncodingFactor));
            }
            catch (Exception e)
            {
                throw new MAVAPIException("No polyline for train.", e);
            }

            object v = apiResponse["d"]["param"]["v"];
            if (v != null)
                elviraId = v.ToString().Trim();
        }

        public void Handle()
        {
            TrainInstance trainInstance = elviraId != null
                ? DbContext.TrainInstances.Where(t => t.ElviraId == elviraId).Include(t => t.TrainInstanceStations).FirstOrDefault() ?? new TrainInstance() { ElviraId = elviraId }
                : null;
            Train train = trainInstance?.Train;

            var table = html.DocumentNode.Descendants("table").FirstOrDefault(tb => tb.HasClass("vt"));
            if (table == null)
                throw new MAVParseException("No train table.");

            HtmlNode headerTr = table.Descendants("tr").FirstOrDefault();
            if (headerTr == null)
                throw new MAVParseException("No train table.");

            IEnumerable<HtmlNode> ths = headerTr.Descendants("th");
            var topHeader = ths.FirstOrDefault();
            if (topHeader == null)
                throw new MAVParseException("No train header.");

            HandleTrainHeader(topHeader, ref train, trainInstance);

            if (!train.IsValid)
            {
                IList<List<HtmlNode>> rows = table.Descendants("tr").Where(tr => !tr.Descendants("th").Any() && tr.Attributes.Contains("onmouseover"))
                    .Select(tr => tr.Descendants("td").ToList()).ToList();

                HandleStationsRecreate(rows, train, trainInstance);
                HandleExpiryDate(train);
                train.Polyline = polyline;

                if (trainInstance != null)
                    DbContext.Update(trainInstance);
                
                DbContext.Update(train);
            }
            else if (trainInstance?.Id == 0)
            {
                IList<List<HtmlNode>> rows = table.Descendants("tr").Where(tr => !tr.Descendants("th").Any() && tr.Attributes.Contains("onmouseover"))
                    .Select(tr => tr.Descendants("td").ToList()).ToList();

                HandleStationsFromExisting(rows, train, trainInstance);

                DbContext.Update(trainInstance);
            }

            DbContext.SaveChanges();
            DbContext.Dispose();
        }

        private void HandleTrainHeader(HtmlNode topHeader, ref Train train, TrainInstance trainInstance)
        {
            using (IEnumerator<HtmlNode> header = topHeader.ChildNodes.AsEnumerable().GetEnumerator())
            {
                if (!header.MoveNext()) throw new MAVParseException("No train header.");

                string[] nameType = header.Current.InnerText.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameType.Length < 1) throw new MAVParseException("Train header is not in the correct format.");

                if (!int.TryParse(nameType.First(), out int number)) throw new MAVParseException("Train header does not contain a number.");

                train = train ?? DbContext.Trains.Where(t => t.Number == number).Include(t => t.TrainStations).FirstOrDefault() ?? new Train() { Number = number };

                if (trainInstance?.Id == 0)
                {
                    trainInstance.Train = train;
                }

                train.Type = nameType.Length > 1 ? nameType.Last() : null;
                train.Name = nameType.Length > 2 ? string.Join(" ", nameType.Skip(1).SkipLast(1)) : null;

                header.MoveNext();

                if (header.Current.Name == "ul")
                {
                    train.Type = FigureOutTypeUl(header.Current);
                    header.MoveNext();
                }

                if (header.Current.HasClass("viszszam2"))
                    train.Name = header.Current.InnerText;
            }
        }

        private void HandleExpiryDate(Train train)
        {
            var expiryDateLink = html.DocumentNode.FirstChild.ChildNodes.SkipWhile(h => h.InnerText.Trim() != "Menetrend").SkipWhile(ul => ul.Name != "ul").FirstOrDefault()?.Descendants("li")
                    .FirstOrDefault(li => li.Attributes.Contains("style") && li.Attributes["style"].Value.Contains("bolder"))
                    ?.Descendants("a").FirstOrDefault();
            DateTime? expiryDate = expiryDateLink == null ? (DateTime?)null : DateTime.Parse(expiryDateLink.InnerText.Split('-')[1]).ToUniversalTime();
            train.ExpiryDate = expiryDate ?? new DateTime(DateTime.UtcNow.Year, 12, 31, 23, 59, 59).ToUniversalTime();
        }

        private void HandleStationsRecreate(IList<List<HtmlNode>> rows, Train train, TrainInstance trainInstance)
        {
            List<string> stationNames = new List<string>();
            train.TrainStations.Clear();
            trainInstance?.TrainInstanceStations.Clear();

            foreach (IList<HtmlNode> row in rows)
            {
                string stationName = row[1].InnerText.Trim();
                string normName = Station.NormalizeName(stationName);
                stationNames.Add(normName);

                var (arrival, actualArrival) = Parsing.GetCellTimes(row[2]);
                var (departure, actualDeparture) = Parsing.GetCellTimes(row[3]);
                var platform = row.Count == 5 ? row[4].InnerText.Trim() : null;

                var trainStation = new TrainStation
                {
                    Train = train,
                    Ordinal = train.TrainStations.Count,
                    Station = new Station() { Name = stationName, NormName = normName },
                    Arrival = arrival,
                    Departure = departure,
                    Platform = string.IsNullOrEmpty(platform) ? null : platform
                };

                train.TrainStations.Add(trainStation);

                if (trainInstance != null)
                {
                    var trainInstanceStation = new TrainInstanceStation
                    {
                        TrainInstance = trainInstance,
                        TrainStation = trainStation,
                        ActualArrival = actualArrival,
                        ActualDeparture = actualDeparture
                    };

                    trainInstance.TrainInstanceStations.Add(trainInstanceStation);
                }
            }

            IDictionary<string, Station> stations = DbContext.Stations.Where(s => stationNames.Contains(s.NormName)).ToDictionary(s => s.NormName, s => s);

            foreach (TrainStation trainStation in train.TrainStations)
            {
                trainStation.Station = stations[trainStation.Station.NormName];
            }

            var relativeDistances = GetRelativeDistances(train.TrainStations.Select(s => s.Station).ToList());

            for (int i = 0; i < train.TrainStations.Count; i++)
            {
                train.TrainStations[i].RelativeDistance = relativeDistances[i];
            }
        }

        private void HandleStationsFromExisting(IList<List<HtmlNode>> rows, Train train, TrainInstance trainInstance)
        {
            if (trainInstance.Id != 0)
                trainInstance?.TrainInstanceStations.Clear();

            DbContext.Entry(train).Collection(t => t.TrainStations).Query().Include(st => st.Station).Load();
            Dictionary<string, TrainStation> trainStations = train.TrainStations.ToDictionary(st => st.Station.NormName, st => st);

            foreach (IList<HtmlNode> row in rows)
            {
                var (_, actualArrival) = Parsing.GetCellTimes(row[2]);
                var (_, actualDeparture) = Parsing.GetCellTimes(row[3]);

                var trainInstanceStation = new TrainInstanceStation
                {
                    TrainInstance = trainInstance,
                    TrainStation = trainStations[Station.NormalizeName(row[1].InnerText.Trim())],
                    ActualArrival = actualArrival,
                    ActualDeparture = actualDeparture
                };

                trainInstance.TrainInstanceStations.Add(trainInstanceStation);
            }
        }

        public IList<double?> GetRelativeDistances(List<Station> stations)
        {
            if (polyline == null)
                return stations.Select<Station, double?>(s => null).ToList();

            double?[] relativeDistances = new double?[stations.Count];
            for (int i = 0; i < stations.Count; i++)
            {
                double? dist = null;

                if (stations[i].GPSCoord != null)
                {
                    dist = polyline.GetProjectedDistance(stations[i].GPSCoord.AsVector2, 0.25);
                }

                relativeDistances[i] = dist;
            }

            bool flip = false;
            int first = -1;
            for (int i = 0; i < relativeDistances.Length; i++)
            {
                if (first != -1 && relativeDistances[first].HasValue && relativeDistances[i].HasValue && relativeDistances[i] < relativeDistances[first])
                {
                    flip = true;
                    break;
                }

                if (first == -1 && relativeDistances[i].HasValue)
                    first = i;
            }

            double firstDistance = first != -1 ? relativeDistances[first].Value : 0;

            // We have to flip the line because we don't have positive distances
            if (flip)
            {
                relativeDistances[first] = 0;
                for (int i = first + 1; i < relativeDistances.Length; i++)
                {
                    if (relativeDistances[i] != null)
                        relativeDistances[i] = firstDistance - relativeDistances[i];
                }

                polyline = new Polyline(polyline.Points.Reverse());
            }

            return relativeDistances;
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
    }
}

