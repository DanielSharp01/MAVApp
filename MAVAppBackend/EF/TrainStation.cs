using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EF
{
    public class TrainStation
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public Train Train { get; set; }
        public int Ordinal { get; set; }
        public int StationId { get; set; }
        public Station Station { get; set; }
        public TimeSpan? Arrival { get; set; }
        public TimeSpan? Departure { get; set; }
        public double? RelativeDistance { get; set; }
        [MaxLength(16)]
        public string Platform { get; set; }
    }
}
