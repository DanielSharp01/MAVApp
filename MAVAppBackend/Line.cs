using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    public class Line
    {
        /// <summary>
        /// ID used in the MySQL database
        /// </summary>
        public int Id { private set; get; }

        /// <summary>
        /// Points in this line as (lat, lon) Vector
        /// </summary>
        List<Vector2> Points { get; set; }
    }
}
