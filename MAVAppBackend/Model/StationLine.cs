using MAVAppBackend.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.Model
{
    public class StationLine : Entity
    {
        public Station Station { private set; get; }
        public Line Line { private set; get; }
        public double Distance { private set; get; }

        public StationLine(int key)
            : base(key)
        { }

        public void Fill(Station station, Line line, double distance)
        {
            Station = station;
            Line = line;
            Distance = distance;
            Filled = true;
        }
    }
}
