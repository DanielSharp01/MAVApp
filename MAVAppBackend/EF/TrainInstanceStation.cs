using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EF
{
    public class TrainInstanceStation
    {
        public int Id { get; set; }
        public int TrainInstanceId { get; set; }
        public TrainInstance TrainInstance { get; set; }
        public int TrainStationId { get; set; }
        public TrainStation TrainStation { get; set; }
        public TimeSpan? ActualArrival { get; set; }
        public TimeSpan? ActualDeparture { get; set; }
    }
}
