using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
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
        /// Could not find the station
        /// </summary>
        Missing,
        /// <summary>
        /// No Google data could be acquired
        /// </summary>
        IntegerPrecision,
        /// <summary>
        /// Google data could be acquired
        /// </summary>
        Precise
    }

    /// <summary>
    /// Station information of a specific train journey
    /// </summary>
    public class StationInfo
    {
        /// <summary>
        /// Station name as provided by MÁV
        /// </summary>
        public string Name
        {
            private set;
            get;
        }

        /// <summary>
        /// Station object with precise information
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
        public StationPositionAccuracy PositionAccuracy
        {
            private set;
            get;
        }

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

        /// <param name="name">Name of the station</param>
        /// <param name="intDistance">Integer distance supplied by MÁV</param>
        /// <param name="arrival">Arrival DateTime</param>
        /// <param name="departure">Departure DateTime</param>
        /// <param name="expectedArrival">Expected or actual arrival DateTime</param>
        /// <param name="expectedDeparture">Expected or actual departure DateTime</param>
        /// <param name="arrived">Did the train arrive yet?</param>
        /// <param name="platform">Platform if known, null otherwise</param>
        public StationInfo(string name, int intDistance, DateTime arrival, DateTime departure, DateTime expectedArrival, DateTime expectedDeparture, bool arrived, string platform)
        {
            Name = name;
            IntDistance = intDistance;
            Distance = intDistance;
            PositionAccuracy = StationPositionAccuracy.Missing;
            Arrival = arrival;
            Departure = departure;
            ExpectedArrival = expectedArrival;
            ExpectedDeparture = expectedDeparture;
            Arrived = arrived;
            Platform = platform;
        }

        /// <summary>
        /// Updates this station with a known station object (containing precise GPS information)
        /// </summary>
        /// <param name="station">Precise station data object</param>
        public void UpdateStation(Station station)
        {
            Station = station;
        }

        /// <summary>
        /// Updates this station's distance on the line of the train
        /// </summary>
        /// <param name="distance">Distance of this station on the line of the train</param>
        /// <param name="accuracy">Accuracy of the information</param>
        public void UpdateDistanceInformation(double distance, StationPositionAccuracy accuracy)
        {
            Distance = distance;
            PositionAccuracy = accuracy;
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
            HtmlNode table;
            try
            {
                table = trainHTML.DocumentNode.Descendants("table").Where(tb => tb.HasClass("vt")).First();
            }
            catch (InvalidOperationException e)
            {
                throw new MAVAPIException("Cannot parse train table.");
            }

            try
            {
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
            }
            catch (InvalidOperationException e)
            {
                throw new MAVAPIException("Cannot parse train header.");
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
                    try
                    {
                        intDistance = int.Parse(tdEnumerator.Current.InnerHtml);
                    }
                    catch (FormatException e)
                    {
                        throw new MAVAPIException("Cannot parse train station distance cell.");
                    }
                }

                tdEnumerator.MoveNext(); // On station name cell
                string stationName = tdEnumerator.Current.InnerText;
                DateTime date = new DateTime(0);
                try
                {
                    date = DateTime.ParseExact(Regex.Match(tdEnumerator.Current.Descendants("a").First().Attributes["onclick"].Value, "d: '(?<date>[^' ]*?)'").Groups["date"].Value, "yy.MM.dd", null);
                }
                catch (FormatException e)
                {
                    throw new MAVAPIException("Cannot parse train date.");
                }

                tdEnumerator.MoveNext(); // On arrival time cell

                TimeSpan arrival = new TimeSpan(0, 0, 0), expectedArrival = new TimeSpan(0, 0, 0);
                try
                {
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
                }
                catch (FormatException e)
                {
                    throw new MAVAPIException("Cannot parse train arrival time.");
                }

                tdEnumerator.MoveNext(); // On departure time cell

                TimeSpan departure = new TimeSpan(0, 0, 0), expectedDeparture = new TimeSpan(0, 0, 0);
                try
                {
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
                }
                catch (FormatException e)
                {
                    throw new MAVAPIException("Cannot parse train departure time.");
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

                StationInfo stationInfo = new StationInfo(stationName, intDistance, arrivalDateTime, departureDateTime, expectedArrivalDateTime, expectedDepartureDateTime,
                    highlighted || (expectedDepartureDateTime <= DateTime.Now), platform == "" ? null : platform);
                stations.Add(stationInfo);
            }

            int first = -1;
            int second = -1;
            Station firstPrec = null;
            bool directionDetermined = false;
            for (int i = 0; i < stations.Count; i++)
            {
                //Try to find precise data for this station
                Station s = MAVAPI.GetStation(stations[i].Name);
                if (s != null)
                {
                    // We have found the first station
                    if (firstPrec == null)
                    {
                        first = i;
                        firstPrec = s;
                    }
                    // We have found the second station
                    else if (!directionDetermined)
                    {
                        second = i;

                        //The second station is before the first in distance => flip the line because it's wrong
                        if (Polyline.GetProjectedDistance(Map.DefaultMap.FromLatLon(s.GPSCoord)) < Polyline.GetProjectedDistance(Map.DefaultMap.FromLatLon(firstPrec.GPSCoord)))
                        {
                            Polyline = new Polyline(Polyline.Points.Reverse().ToList(), Polyline.Map);
                        }

                        directionDetermined = true;

                        stations[first].UpdateStation(firstPrec);
                        stations[first].UpdateDistanceInformation(Polyline.GetProjectedDistance(Map.DefaultMap.FromLatLon(firstPrec.GPSCoord)), StationPositionAccuracy.Precise);
                        stations[i].UpdateStation(s);
                        stations[i].UpdateDistanceInformation(Polyline.GetProjectedDistance(Map.DefaultMap.FromLatLon(s.GPSCoord)), StationPositionAccuracy.Precise);
                    }
                    else
                    {
                        // Update precise info if direction was found already
                        stations[i].UpdateStation(s);
                        stations[i].UpdateDistanceInformation(Polyline.GetProjectedDistance(Map.DefaultMap.FromLatLon(s.GPSCoord)), StationPositionAccuracy.Precise);
                    }
                }
                // If the direction is determined we can at least do integer precision on all subsequent stations, provided integer distance is supplied
                else if (directionDetermined && stations[i - 1].IntDistance != -1 && stations[i].IntDistance != -1)
                {
                    stations[i].UpdateDistanceInformation(stations[i - 1].Distance + (stations[i].IntDistance - stations[i - 1].IntDistance), StationPositionAccuracy.IntegerPrecision);
                }
                // Any other case it's missing (and it's the default)
            }

            // We could not determine the direction for certain => everything is marked missing
            // But even if we did stations before the second could still be marked missing
            if (directionDetermined)
            {
                // Try to determine based on the next station and propagate backwards
                for (int i = second - 1; i >= 0; i--)
                {
                    if (stations[i].PositionAccuracy == StationPositionAccuracy.Missing && stations[i + 1].IntDistance != -1 && stations[i].IntDistance != -1)
                    {
                        stations[i].UpdateDistanceInformation(stations[i + 1].Distance - (stations[i + 1].IntDistance - stations[i].IntDistance), StationPositionAccuracy.IntegerPrecision);
                    }
                    // Any other case it's still missing
                }
            }
        }

        /// <summary>
        /// Prints this train's line an stations into an SVG stream for debug purposes
        /// </summary>
        /// <param name="svg">SVG stream to print into</param>
        public void PrintIntoSVG(SVGStream svg)
        {
            svg.DrawPolyline(Polyline, "black", 1);
            foreach (StationInfo station in stations)
            {
                if (station.PositionAccuracy != StationPositionAccuracy.Missing)
                    svg.DrawCircle(Polyline.GetPoint(station.Distance), 3, strokeColor: (station.PositionAccuracy == StationPositionAccuracy.Precise ? "green" : "red"), strokeWidth : 1);
                else
                {
                    Console.WriteLine(station.Name + " is missing positional data!");
                }
            }
        }
    }
}
