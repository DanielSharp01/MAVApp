using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// Constructs a train from the DB
        /// </summary>
        /// <param name="name">Name of the station</param>
        /// <param name="intDistance">Integer distance supplied by MÁV</param>
        /// <param name="distance">Precise distance on the line</param>
        /// <param name="positionAccuracy">Tells how accurate the positional data is (0 - missing, 1 - integer, 2 - precise)</param>
        /// <param name="arrival">Arrival DateTime</param>
        /// <param name="departure">Departure DateTime</param>
        /// <param name="expectedArrival">Expected or actual arrival DateTime</param>
        /// <param name="expectedDeparture">Expected or actual departure DateTime</param>
        /// <param name="arrived">Did the train arrive yet?</param>
        /// <param name="platform">Platform if known, null otherwise</param>
        public StationInfo(Station station, string name, int intDistance, double distance, int positionAccuracy, DateTime arrival, DateTime departure,
            DateTime expectedArrival, DateTime expectedDeparture, bool arrived, string platform)
        {
            Station = station;
            Name = name;
            IntDistance = intDistance;
            Distance = distance;

            if (positionAccuracy == 0) PositionAccuracy = StationPositionAccuracy.Missing;
            else if (positionAccuracy == 1) PositionAccuracy = StationPositionAccuracy.IntegerPrecision;
            else /*if (positionAccuracy == 2)*/ PositionAccuracy = StationPositionAccuracy.Precise;

            Arrival = arrival;
            Departure = departure;
            ExpectedArrival = expectedArrival;
            ExpectedDeparture = expectedDeparture;
            Arrived = arrived;
            Platform = platform;
        }

        /// <summary>
        /// Constructs a train upon first recieving the data from the MÁV API
        /// </summary>
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

        /// <summary>
        /// Updates station with new information from the JSON API
        /// </summary>
        /// <param name="expectedArrival">Expected or actual arrival DateTime</param>
        /// <param name="expectedDeparture">Expected or actual departure DateTime</param>
        /// <param name="arrived">Did the train arrive yet?</param>
        /// <param name="platform">Platform if known, null otherwise</param>
        public void Update(DateTime expectedArrival, DateTime expectedDeparture, bool arrived, string platform)
        {
            ExpectedArrival = expectedArrival;
            ExpectedDeparture = expectedDeparture;
            Arrived = arrived;
            Platform = platform;
        }
    }
}
