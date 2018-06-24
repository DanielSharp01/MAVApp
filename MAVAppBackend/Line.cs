using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    public class Line
    {
        public int ID { private set; get; }
        public Polyline Polyline { private set; get; }

        public Line(int id, Polyline polyline)
        {
            ID = id;
            Polyline = polyline;
        }
    }
}
