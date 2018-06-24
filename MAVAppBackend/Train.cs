using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MAVAppBackend
{
    public class Train
    {
        /// <summary>
        /// MySQL ID
        /// </summary>
        public int ID
        {
            private set;
            get;
        } = -1;

        /// <summary>
        /// Unique ID used by MÁV
        /// </summary>
        public string ElviraID
        {
            private set;
            get;
        }

        /// <summary>
        /// Number of the train, a somewhat unique identifier if known, null otherwise
        /// </summary>
        public string Number

        {
            private set;
            get;
        } = null;

        /// <summary>
        /// Name of the train (IC's usually have a textual name as well) if known, null otherwise
        /// </summary>
        public string Name
        {
            private set;
            get;
        } = null;

        /// <summary>
        /// Type of the train (as a Hungarian string for now), probably get changed to an enum if known, null otherwise
        /// </summary>
        public string Type

        {
            private set;
            get;
        } = null;

        /// <summary>
        /// Number type for trains which have it (eg. S50, G60 etc.) if known, null otherwise
        /// </summary>
        public string NumberType

        {
            private set;
            get;
        } = null;

        /// <summary>
        /// Delay in minutes
        /// </summary>
        public int Delay
        {
            private set;
            get;
        } = 0; // Will be set in the Update method

        /// <summary>
        /// Reason for the delay
        /// </summary>
        public string DelayReason

        {
            private set;
            get;
        } = null;

        /// <summary>
        /// Current GPS Position of the train if known, null otherwise
        /// </summary>
        public Vector2 GPSPosition
        {
            private set;
            get;
        } = null;

        /// <summary>
        /// Last GPS Position (one TRAINS API request before) of the train if known, null otherwise
        /// </summary>
        public Vector2 LastGPSPosition
        {
            private set;
            get;
        } = null;

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
        } = null;

        /// <summary>
        /// TRAIN API was requested at least once upon creation
        /// </summary>
        public bool HasTRAINData
        {
            get
            {
                return Polyline != null;
            }
        }

        /// <summary>
        /// The last TRAINS cycle had valid data for this train
        /// </summary>
        public bool HasTRAINSData
        {
            get
            {
                return GPSPosition != null;
            }
        }

        /// <summary>
        /// The TRAINS API has been requested and returned valid data for this train
        /// </summary>
        public bool HadTRAINSData
        {
            get
            {
                return LastGPSPosition != null;
            }
        }

        /// <summary>
        /// Constructs a train from MySQL data
        /// </summary>
        /// <param name="id">MySQL ID</param>
        /// <param name="elviraID">Unique ID used by MÁV</param>
        /// <param name="number">Number of the train, a somewhat unique identifier if known, null otherwise</param>
        /// <param name="type">Type of the train (as a Hungarian string for now), probably get changed to an enum if known, null otherwise</param>
        /// <param name="numberType">Number type for trains which have it (eg. S50, G60 etc.) if known, null otherwise</param>
        /// <param name="delay">Delay in minutes</param>
        /// <param name="delayReason">Reason for the delay</param>
        /// <param name="miscInfo">Misc info such as service changes etc.</param>
        /// <param name="gpsPosition">Current GPS Position of the train if known, null otherwise</param>
        /// <param name="lastGpsPosition">Last GPS Position (one TRAINS API request before) of the train if known, null otherwise</param>
        /// <param name="encPolyline">Polyline representing this Train journey</param>
        /// <param name="stations">Stations this train hits, if null then we don't initialize this. See also: <seealso cref="LateConstructStations(List{StationInfo})"></param>
        public Train(int id, string elviraID, string number, string name, string type, string numberType, int delay, string delayReason, string miscInfo, Vector2 gpsPosition, Vector2 lastGpsPosition, string encPolyline, List<StationInfo> stations)
        {
            ID = id;
            ElviraID = elviraID;
            Number = number;
            Name = name;
            Type = type;
            NumberType = numberType;
            Delay = delay;
            DelayReason = delayReason;
            this.miscInfo.AddRange(miscInfo.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries));

            if (encPolyline == null) Polyline = null;
            else Polyline = new Polyline(Polyline.DecodePoints(encPolyline, 1E5f, WebMercator.DefaultMap));

            GPSPosition = gpsPosition;
            LastGPSPosition = lastGpsPosition;
            if (stations != null) this.stations.AddRange(stations);
        }

        /// <summary>
        /// Constructs a train without information, with no database backup yet
        /// </summary>
        /// <param name="elviraID">Unique ID used by MÁV</param>
        public Train(string elviraID)
        {
            ElviraID = elviraID;
        }

        /// <summary>
        /// Constructs a train from a JSON API response of the TRAIN MÁV API
        /// </summary>
        /// <param name="elviraID">ID of the call to the JSON API</param>
        /// <param name="apiResponse">JSON TRAIN MÁV API response</param>
        public Train(string elviraID, JObject apiResponse)
        {
            ElviraID = elviraID;
            UpdateTRAIN_API(apiResponse);
        }

        /// <summary>
        /// Acts as if the parameter of the constructor was entered later, for avoiding nested MySQL readers.
        /// </summary>
        /// <param name="stations">Stations this train hits</param>
        public void LateConstructStations(List<StationInfo> stations)
        {
            this.stations.Clear();
            this.stations.AddRange(stations);
        }

        /// <summary>
        /// Updates fields based on a TRAIN API JSON request
        /// </summary>
        /// <param name="apiResponse">JSON TRAIN MÁV API response</param>
        public void UpdateTRAIN_API(JObject apiResponse)
        {
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

            string fix2400Time(string s)
            {
                if (s.Trim() == "24:00") return "0:00";
                else return s;
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
                miscInfo.Clear();
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

            if ((apiResponse["d"]["result"]["line"] as JArray).Count == 0)
               throw new MAVAPIException("Cannot get train line.");

            bool hasTrainData = HasTRAINData; // Polyline will be defined in a second so that would skew our method
            // Polyline of the train path
            List<Vector2> points = Polyline.DecodePoints(apiResponse["d"]["result"]["line"][0]["points"].ToString(), 1E5f, WebMercator.DefaultMap);
            Polyline = new Polyline(points);

            // Station infos
            IEnumerable<HtmlNode> stationRows = table.Descendants("tr").Where(tr => tr.Attributes.Contains("class"));

            if (hasTrainData) // If we already have train data then stations will be updated
            {
                int stationCnt = 0;
                foreach (HtmlNode stationRow in stationRows)
                {
                    bool highlighted = stationRow.Attributes["class"].Value.StartsWith("row_past");

                    // Skip invalid rows
                    if (stationRow.Descendants("td").Count() < 4) continue;

                    IEnumerator<HtmlNode> tdEnumerator = stationRow.Descendants("td").GetEnumerator();
                    tdEnumerator.MoveNext(); // On integer distance cell
                    tdEnumerator.MoveNext(); // On station name cell
                    tdEnumerator.MoveNext(); // On arrival time cell

                    TimeSpan expectedArrival = stations[stationCnt].ExpectedArrival.TimeOfDay; //Don't change if there's an error parsing
                    try
                    {
                        if (stationRows.First() != stationRow)
                        {
                            // Arrival time parsing block
                            {
                                IEnumerator<HtmlNode> timeEnumerator = tdEnumerator.Current.Descendants().GetEnumerator();
                                timeEnumerator.MoveNext(); // On the first text block contatining scheduled arrival
                                if (timeEnumerator.MoveNext()) // On a <br>
                                {
                                    timeEnumerator.MoveNext(); // On the second text block in a span containing expected arrival
                                    expectedArrival = TimeSpan.ParseExact(fix2400Time(timeEnumerator.Current.InnerHtml), "g", null);
                                }
                            }
                        }
                    }
                    catch (FormatException e)
                    {
                        throw new MAVAPIException("Cannot parse train arrival time.");
                    }

                    tdEnumerator.MoveNext(); // On departure time cell

                    TimeSpan expectedDeparture = stations[stationCnt].ExpectedDeparture.TimeOfDay; //Don't change if there's an error parsing
                    try
                    {
                        if (stationRows.Last() != stationRow)
                        {
                            // Departure time parsing block
                            {
                                IEnumerator<HtmlNode> timeEnumerator = tdEnumerator.Current.Descendants().GetEnumerator();
                                timeEnumerator.MoveNext(); // On the first text block contatining scheduled departure
                                if (timeEnumerator.MoveNext()) // On a <br>
                                {
                                    timeEnumerator.MoveNext(); // On the second text block in a span containing expected departure
                                    expectedDeparture = TimeSpan.ParseExact(fix2400Time(timeEnumerator.Current.InnerHtml), "g", null);
                                }
                            }
                        }
                    }
                    catch (FormatException e)
                    {
                        throw new MAVAPIException("Cannot parse train departure time.");
                    }

                    string platform = null;
                    if (tdEnumerator.MoveNext()) // There might not even be a cell for platforms
                    {
                        // On platform cell
                        platform = tdEnumerator.Current.InnerHtml.Trim();
                    }

                    DateTime expectedArrivalDateTime = stations[stationCnt].Arrival.Date + expectedArrival, expectedDepartureDateTime = stations[stationCnt].Departure.Date + expectedDeparture;

                    if (stations[stationCnt].Arrival.TimeOfDay > expectedArrival) // we are on the border of this day
                    {
                        expectedArrivalDateTime = stations[stationCnt].Arrival.Date.AddDays(1) + expectedArrival;
                    }

                    if (stations[stationCnt].Departure.TimeOfDay > expectedDeparture) // we are on the border of this day
                    {
                        expectedDepartureDateTime = stations[stationCnt].Departure.Date.AddDays(1) + expectedDeparture;
                    }

                    stations[stationCnt++].Update(expectedArrivalDateTime, expectedDepartureDateTime, highlighted || (expectedDepartureDateTime <= DateTime.Now), platform == "" ? null : platform);
                }
            }
            else // If we don't have train data already then stations must be added, and the distance information also has to be calculated
            {
                foreach (HtmlNode stationRow in stationRows)
                {
                    bool highlighted = stationRow.Attributes["class"].Value.StartsWith("row_past");

                    // Skip invalid rows
                    if (stationRow.Descendants("td").Count() < 4) continue;

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
                                arrival = TimeSpan.ParseExact(fix2400Time(timeEnumerator.Current.InnerHtml), "g", null);
                                if (timeEnumerator.MoveNext()) // On a <br>
                                {
                                    timeEnumerator.MoveNext(); // On the second text block in a span containing expected arrival
                                    expectedArrival = TimeSpan.ParseExact(fix2400Time(timeEnumerator.Current.InnerHtml), "g", null);
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
                                timeEnumerator.MoveNext(); // On the first text block contatining scheduled departure
                                departure = TimeSpan.ParseExact(fix2400Time(timeEnumerator.Current.InnerHtml), "g", null);
                                if (timeEnumerator.MoveNext()) // On a <br>
                                {
                                    timeEnumerator.MoveNext(); // On the second text block in a span containing expected departure
                                    expectedDeparture = TimeSpan.ParseExact(fix2400Time(timeEnumerator.Current.InnerHtml), "g", null);
                                }
                                else expectedDeparture = departure;
                            }
                        }
                    }
                    catch (FormatException e)
                    {
                        throw new MAVAPIException("Cannot parse train departure time.");
                    }

                    string platform = null;
                    if (tdEnumerator.MoveNext()) // There might not even be a cell for platforms
                    {
                        // On platform cell
                        platform = tdEnumerator.Current.InnerHtml.Trim();
                    }

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


                if (determineDirection())
                {
                    int firstFound = -1;
                    for (int i = 0; i < stations.Count; i++)
                    {
                        if (stations[i].IntDistance != -1)
                        {
                            Station st = Database.GetStation(stations[i].Name);
                            if (st != null) stations[i].UpdateStation(st);

                            double dist = 0;
                            if (st != null && !double.IsNaN(dist = Polyline.GetProjectedDistance(WebMercator.DefaultMap.FromLatLon(st.GPSCoord), WebMercator.DefaultMap, 0.25)))
                            {
                                stations[i].UpdateDistanceInformation(dist, StationPositionAccuracy.Precise);
                                if (firstFound == -1) firstFound = i;
                            }
                            else if (!double.IsNaN(dist) && i > 0 && stations[i - 1].PositionAccuracy != StationPositionAccuracy.Missing)
                            {
                                stations[i].UpdateDistanceInformation(stations[i - 1].Distance + (stations[i].IntDistance - stations[i - 1].IntDistance), StationPositionAccuracy.IntegerPrecision);
                            }
                        }
                    }

                    for (int i = firstFound - 1; i >= 0; i--) // We do a reverse check as well so that we can figure out missing stations backwards from the first found
                    {
                        if (stations[i].IntDistance != -1
                            && (stations[i].Station != null && !double.IsNaN(Polyline.GetProjectedDistance(WebMercator.DefaultMap.FromLatLon(stations[i].Station.GPSCoord), WebMercator.DefaultMap, 0.05)))
                            && stations[i].PositionAccuracy == StationPositionAccuracy.Missing)
                        {
                            stations[i].UpdateDistanceInformation(stations[i + 1].Distance - (stations[i + 1].IntDistance - stations[i].IntDistance), StationPositionAccuracy.IntegerPrecision);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines the direction of the line and flips it neccesary.
        /// </summary>
        /// <returns>Whether the direction could be determined</returns>
        private bool determineDirection()
        {
            double firstDist = double.NaN;
            foreach (StationInfo station in stations)
            {
                if (station.IntDistance != -1)
                {
                    Station st = Database.GetStation(station.Name);
                    double dist;
                    if (st != null && !double.IsNaN(dist = Polyline.GetProjectedDistance(WebMercator.DefaultMap.FromLatLon(st.GPSCoord), WebMercator.DefaultMap, 0.05)))
                    {
                        if (double.IsNaN(firstDist))
                        {
                            firstDist = dist;
                        }
                        else //if (double.IsNaN(secondDist))
                        {
                            if (dist < firstDist)
                            {
                                Polyline = new Polyline(Polyline.Points.Reverse().ToList());
                            }

                            return true;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Updates fields based on a TRAINS API JSON request
        /// </summary>
        /// <param name="trainsData">Positional and delay data from the TRAINS API</param>
        public void UpdateTRAINS_API(TRAINSData trainsData)
        {
            Delay = trainsData.Delay;
            GPSPosition = trainsData.GPSCoord;
        }

        /// <summary>
        /// Clears the position field of the train and sets the last position to the cleared value
        /// </summary>
        public void ClearPosition()
        {
            LastGPSPosition = GPSPosition;
            GPSPosition = null;
        }

        /// <summary>
        /// Sets ID after inserting into the database
        /// </summary>
        /// <param name="id">MySQL ID</param>
        public void SetDBId(int id)
        {
            ID = id;
        }
    }
}
