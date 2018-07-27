using MAVAppBackend.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.Model
{
    public class Line : Entity
    {
        public Polyline Polyline { private set; get; }

        public Line(int key)
            : base(key)
        { }

        public void Fill(Polyline polyline)
        {
            Polyline = polyline;
            Filled = true;
        }
    }
}
