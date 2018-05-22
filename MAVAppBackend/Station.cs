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
}
