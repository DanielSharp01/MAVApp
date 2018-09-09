using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MAVAppBackend.EF
{
    public class TrainInstance
    {
        public int Id { get; set; }
        [MaxLength(16)]
        public string ElviraId { get; set; }
        public int TrainId { get; set; }
        public Train Train { get; set; }
        public IList<TrainInstanceStation> TrainInstanceStations { get; set; } = new List<TrainInstanceStation>();
        public IList<Trace> Traces { get; set; } = new List<Trace>();
    }
}
