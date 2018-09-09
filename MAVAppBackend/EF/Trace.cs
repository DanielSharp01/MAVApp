using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EF
{
    public class Trace
    {
        public int Id { get; set; }
        public int TrainInstanceId { get; set; }
        public TrainInstance TrainInstance { get; set; }
        public DbVector2 GPSCoord { get; set; }
        public int DelayMinutes { get; set; }
        public DateTime DateTime { get; set; }
    }
}
