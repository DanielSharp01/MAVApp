using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Tells how accurate the positional data is
    /// </summary>
    public enum StationPositionAccuracy
    {
        /// <summary>
        /// No data could be acquired (MÁV doesn't supply an integer distance)
        /// </summary>
        NoData,
        /// <summary>
        /// No Google data could be acquired (GooglePlaces API is unavaliable)
        /// </summary>
        IntegerAccuracy,
        /// <summary>
        /// Google data could be acquired
        /// </summary>
        PreciseAccuracy
    }

    /// <summary>
    /// Represents a train station
    /// </summary>
    public class Station
    {
        /// <summary>
        /// Name of the station
        /// </summary>
        public string Name
        {
            private set;
            get;
        }

        /// <summary>
        /// Position if known
        /// See also: <seealso cref="PositionAccuracy"/>
        /// </summary>
        public Vector2 Position
        {
            private set;
            get;
        }

        /// <summary>
        /// Tells how accurate the positional data is
        /// </summary>
        public StationPositionAccuracy PositionAccuracy
        {
            private set;
            get;
        } = StationPositionAccuracy.NoData;

        /// <param name="name">Name of the station</param>
        public Station(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Updates the positional information of this station
        /// </summary>
        /// <param name="position">New position in Map.Default WebMercator projection</param>
        /// <param name="accuracy">Accuracy of the new position</param>
        public void UpdatePosition(Vector2 position, StationPositionAccuracy accuracy)
        {
            Position = position;
            PositionAccuracy = accuracy;
        }
    }

    /// <summary>
    /// Station information of a specific train journey
    /// </summary>
    public class StationInfo
    {
        /// <summary>
        /// Station object which contains name and positional information
        /// </summary>
        public Station Station
        {
            private set;
            get;
        }

        /// <summary>
        /// Integer distance supplied by MÁV
        /// </summary>
        public int IntDistance
        {
            private set;
            get;
        }

        /// <summary>
        /// Precise distance on the line
        /// </summary>
        public double Distance
        {
            private set;
            get;
        }

        /// <summary>
        /// Tells how accurate the positional data is
        /// </summary>
        public StationPositionAccuracy PositionAccuracy => Station.PositionAccuracy;

        /// <summary>
        /// Arrival DateTime
        /// </summary>
        public DateTime Arrival
        {
            private set;
            get;
        }

        /// <summary>
        /// Departure DateTime
        /// </summary>
        public DateTime Departure
        {
            private set;
            get;
        }

        /// <summary>
        /// Expected or actual arrival DateTime
        /// </summary>
        public DateTime ExpectedArrival
        {
            private set;
            get;
        }

        /// <summary>
        /// Expected or actual departure DateTime
        /// </summary>
        public DateTime ExpectedDeparture
        {
            private set;
            get;
        }

        /// <summary>
        /// Did the train arrive yet?
        /// </summary>
        public bool Arrived
        {
            private set;
            get;
        }

        /// <summary>
        /// Platform if known, null otherwise
        /// </summary>
        public string Platform
        {
            private set;
            get;
        }

        /// <param name="station">Station object which contains name and positional information</param>
        /// <param name="intDistance">Integer distance supplied by MÁV</param>
        /// <param name="arrival">Arrival DateTime</param>
        /// <param name="departure">Departure DateTime</param>
        /// <param name="expectedArrival">Expected or actual arrival DateTime</param>
        /// <param name="expectedDeparture">Expected or actual departure DateTime</param>
        /// <param name="arrived">Did the train arrive yet?</param>
        /// <param name="platform">Platform if known, null otherwise</param>
        public StationInfo(Station station, int intDistance, DateTime arrival, DateTime departure, DateTime expectedArrival, DateTime expectedDeparture, bool arrived, string platform)
        {
            Station = station;
            IntDistance = intDistance;
            Distance = intDistance;
            Arrival = arrival;
            Departure = departure;
            ExpectedArrival = expectedArrival;
            ExpectedDeparture = expectedDeparture;
            Arrived = arrived;
            Platform = platform;
        }

        /// <summary>
        /// Updates the precise distance of this station on the line of the train
        /// </summary>
        /// <param name="distance">Precise distance of this station on the line of the train</param>
        public void UpdateRealDistance(double distance)
        {
            Distance = distance;
        }
    }

    public class Train
    {
        /// <summary>
        /// Most unique ID used by MÁV
        /// </summary>
        public string ElviraID
        {
            private set;
            get;
        }

        /// <summary>
        /// Number of the train, a somewhat unique identifier
        /// </summary>
        public string Number

        {
            private set;
            get;
        }

        /// <summary>
        /// Name of the train (IC's usually have a textual name as well)
        /// </summary>
        public string Name
        {
            private set;
            get;
        }

        /// <summary>
        /// Type of the train (as a Hungarian string for now), probably get changed to an enum
        /// </summary>
        public string Type

        {
            private set;
            get;
        }

        /// <summary>
        /// Number type for trains which have it (eg. S50, G60 etc.)
        /// </summary>
        public string NumberType

        {
            private set;
            get;
        }

        /// <summary>
        /// Reason for the delay
        /// </summary>
        public string DelayReason

        {
            private set;
            get;
        }

        /// <summary>
        /// Misc info such as service changes etc.
        /// </summary>
        private List<string> miscInfo = new List<string>();

        /// <summary>
        /// Misc info such as service changes etc.
        /// </summary>
        public IEnumerable<string> MiscInfo
        {
            get
            {
                foreach (string info in miscInfo)
                {
                    yield return info;
                }
                yield break;
            }
        }

        /// <summary>
        /// Stations this train hits
        /// </summary>
        private List<StationInfo> stations = new List<StationInfo>();

        /// <summary>
        /// Stations this train hits
        /// </summary>
        public IEnumerable<StationInfo> Stations
        {
            get
            {
                foreach (StationInfo info in stations)
                {
                    yield return info;
                }
                yield break;
            }
        }

        /// <summary>
        /// Polyline representing this Train journey
        /// </summary>
        public Polyline Polyline
        {
            private set;
            get;
        }

        /// <summary>
        /// Constructs a train from a JSON API response of the TRAIN MÁV API
        /// </summary>
        /// <param name="elviraID">ID of the call to the JSON API</param>
        /// <param name="apiResponse">JSON TRAIN MÁV API response</param>
        public Train(string elviraID, JObject apiResponse)
        {
            ElviraID = elviraID;

            HtmlDocument trainHTML = new HtmlDocument();
            trainHTML.LoadHtml(WebUtility.HtmlDecode((string)apiResponse["d"]["result"]["html"]));
            HtmlNode table = trainHTML.DocumentNode.Descendants("table").Where(tb => tb.HasClass("vt")).First();

            // The header containing train type and name information
            IEnumerable<HtmlNode> tableHeader = table.Descendants("th").First().Descendants();
            string[] name = tableHeader.First().InnerHtml.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            Number = name[0];

            // Figuring out train type
            if (name.Length > 1)
            {
                if (name[1].Trim().ToLower() == "sebesvonat" || name[1].Trim().ToLower() == "gyorsvonat" || name[1].Trim().ToLower() == "személyvonat"
                    || name[1].Trim().ToLower() == "EuroRegio" || name[1].Trim().ToLower() == "InterCity" || name[1].Trim().ToLower() == "EuroCity"
                    || name[1].Trim().ToLower() == "vonatpótló autóbusz" || name[1].Trim().ToLower() == "railjet")
                {
                    Type = name[1].Trim().ToLower();
                }
                else
                {
                    Name = name[1];
                }
            }

            bool ul = false;

            foreach (HtmlNode node in tableHeader)
            {
                // This is a sort of secondary train type for specific trains
                if (node.HasClass("viszszam2"))
                {
                    NumberType = node.InnerHtml;
                }
                // Sometimes train type can only be decuded from the alt of an image
                else if (node.Name == "img" && !ul)
                {
                    Type = node.Attributes["alt"].Value;
                }
                // Sometimes trains can be broken down into multiple types
                else if (node.Name == "ul")
                {
                    ul = true;
                    Type = "";
                    foreach (HtmlNode li in node.Descendants("li"))
                    {
                        string type = "";
                        type = Regex.Replace(li.Descendants().First().InnerHtml, @"\s+", " ");
                        IEnumerable<HtmlNode> img = li.Descendants().Where(d => d.Name == "img");
                        if (img != null && img.Any())
                        {
                            type += " " + img.First().Attributes["alt"].Value;
                        }
                        Type += (Type == "" ? "" : "\n") + type;
                    }
                }
            }

            // Any information that might be helpful
            IEnumerable<HtmlNode> miscInform = table.Descendants("th").Skip(1);
            foreach (HtmlNode node in miscInform)
            {
                // The actual table headers start which are uninteresting
                if (node.InnerHtml == "Km") break;

                // Anything starting with this text is redundant with another API's information
                if (node.InnerHtml.StartsWith("A pillanatnyi késés")) continue;

                 // Anything containing the word delay is related to delays (this might fail)
                else if (node.InnerHtml.Contains("késés"))
                {
                    DelayReason = node.InnerHtml;
                }
                // The truly misc info
                else
                {
                    miscInfo.Add(node.InnerHtml);
                }
            }

            // Polyline of the train path
            List<Vector2> points = Polyline.DecodePoints(apiResponse["d"]["result"]["line"][0]["points"].ToString(), 1E5f, Map.DefaultMap);
            Polyline = new Polyline(points, Map.DefaultMap);

            // Station infos
            IEnumerable<HtmlNode> stationRows = table.Descendants("tr").Where(tr => tr.Attributes.Contains("class"));

            foreach (HtmlNode stationRow in stationRows)
            {
                bool highlighted = stationRow.Attributes["class"].Value.StartsWith("row_past");
                
                // Skip invalid rows
                if (stationRow.Descendants("td").Count() < 5) continue;

                IEnumerator<HtmlNode> tdEnumerator = stationRow.Descendants("td").GetEnumerator();
                tdEnumerator.MoveNext(); // On integer distance cell
                int intDistance = -1;
                if (tdEnumerator.Current.InnerHtml.Trim() != "")
                {
                    intDistance = int.Parse(tdEnumerator.Current.InnerHtml);
                }

                tdEnumerator.MoveNext(); // On station name cell
                Station station = new Station(tdEnumerator.Current.InnerText);
                DateTime date = DateTime.ParseExact(Regex.Match(tdEnumerator.Current.Descendants("a").First().Attributes["onclick"].Value, "d: '(?<date>[^' ]*?)'").Groups["date"].Value, "yy.MM.dd", null);
                tdEnumerator.MoveNext(); // On arrival time cell

                TimeSpan arrival = new TimeSpan(0, 0, 0), expectedArrival = new TimeSpan(0, 0, 0);
                if (stationRows.First() != stationRow)
                {
                    // Arrival time parsing block
                    {
                        IEnumerator<HtmlNode> timeEnumerator = tdEnumerator.Current.Descendants().GetEnumerator();
                        timeEnumerator.MoveNext(); // On the first text block contatining scheduled arrival
                        arrival = TimeSpan.ParseExact(timeEnumerator.Current.InnerHtml, "g", null);
                        if (timeEnumerator.MoveNext()) // On a <br>
                        {
                            timeEnumerator.MoveNext(); // On the second text block in a span containing expected arrival
                            expectedArrival = TimeSpan.ParseExact(timeEnumerator.Current.InnerHtml, "g", null);
                        }
                        else expectedArrival = arrival;
                    }
                }

                tdEnumerator.MoveNext(); // On departure time cell
                TimeSpan departure = new TimeSpan(0, 0, 0), expectedDeparture = new TimeSpan(0, 0, 0);
                if (stationRows.Last() != stationRow)
                {
                    // Departure time parsing block
                    {
                        IEnumerator<HtmlNode> timeEnumerator = tdEnumerator.Current.Descendants().GetEnumerator();
                        timeEnumerator.MoveNext(); // On the first text block contatining scheduled arrival
                        departure = TimeSpan.ParseExact(timeEnumerator.Current.InnerHtml, "g", null);
                        if (timeEnumerator.MoveNext()) // On a <br>
                        {
                            timeEnumerator.MoveNext(); // On the second text block in a span containing expected arrival
                            expectedDeparture = TimeSpan.ParseExact(timeEnumerator.Current.InnerHtml, "g", null);
                        }
                        else expectedDeparture = departure;
                    }
                }

                tdEnumerator.MoveNext(); // On platform cell
                string platform = tdEnumerator.Current.InnerHtml.Trim();

                DateTime arrivalDateTime = date + arrival, departureDateTime = date + departure, expectedArrivalDateTime = date + expectedArrival, expectedDepartureDateTime = date + expectedDeparture;

                if (arrival > departure) // we are on the border of this day
                {
                    departureDateTime = date.AddDays(1) + departure;
                }

                if (arrival > expectedArrival) // we are on the border of this day
                {
                    expectedArrivalDateTime = date.AddDays(1) + expectedArrival;
                }

                if (arrival > expectedDeparture) // we are on the border of this day
                {
                    expectedDepartureDateTime = date.AddDays(1) + expectedDeparture;
                }

                StationInfo stationInfo = new StationInfo(station, intDistance, arrivalDateTime, departureDateTime, expectedArrivalDateTime, expectedDepartureDateTime,
                    highlighted || (expectedDepartureDateTime <= DateTime.Now), platform == "" ? null : platform);
                stations.Add(stationInfo);

                // Try to find the exact position of the station
                if (intDistance != -1)
                {
                    Vector2 mavPoint = Polyline.GetPoint(intDistance);
                    try
                    {
                        double distance = backfitStationPosition(mavPoint, station.Name);
                        stationInfo.UpdateRealDistance(distance);
                        station.UpdatePosition(Polyline.GetPoint(distance), StationPositionAccuracy.PreciseAccuracy);
                    }
                    catch (Exception e)
                    {
                        station.UpdatePosition(Polyline.GetPoint(intDistance), StationPositionAccuracy.IntegerAccuracy);
                    }
                }
            }
        }

        /// <summary>
        /// Prints this train's line an stations into an SVG stream for debug purposes
        /// </summary>
        /// <param name="svg">SVG stream to print into</param>
        public void PrintIntoSVG(SVGStream svg)
        {
            svg.DrawPolyline(Polyline, new Vector2(1920.0 / 2.0, 1080.0 / 2.0), "black", 1);
            foreach (StationInfo station in stations)
            {
                if (station.Station.Position != null) svg.DrawCircle(station.Station.Position, new Vector2(1920.0 / 2.0, 1080.0 / 2.0), 3, (station.Station.PositionAccuracy == StationPositionAccuracy.PreciseAccuracy ? "green" : "red"), 1);
            }
        }

        /// <summary>
        /// Returns the real station distance on the line according Google
        /// </summary>
        /// <param name="mavPoint">Integer distance precision point given by MÁV</param>
        /// <param name="stationName">Station name given by MÁV</param>
        private double backfitStationPosition(Vector2 mavPoint, string stationName)
        {
            List<PlacesData> places = GoogleMaps.RequestPlaces(Map.DefaultMap.ToLatLon(mavPoint), 1500);
            PlacesData data = places.Find(d => string.Compare(d.Name, stationName, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0);

            if (data == null)
            {
                throw new Exception("Places API haven't found station.");
            }
            
            return Polyline.GetProjectedDistance(Map.DefaultMap.FromLatLon(data.GPSCoord));
        }
    }
}
