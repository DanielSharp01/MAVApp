using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EF
{
    public class Train
    {
        public int Id { get; set; }
        public int Number { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string Type { get; set; }

        private string encodedPolyline;
        public string EncodedPolyline
        {
            get => encodedPolyline;
            set
            {
                if (encodedPolyline != value)
                {
                    encodedPolyline = value;
                    polyline = new Polyline(Polyline.DecodePoints(encodedPolyline, Polyline.EncodingFactor));
                }
            }
        }

        private Polyline polyline = null;

        public IList<TrainStation> TrainStations { get; set; } = new List<TrainStation>();

        [NotMapped]
        public bool IsValid
        {
            get
            {
                return polyline != null && ExpiryDate != null && ExpiryDate >= DateTime.UtcNow;
            }
        }

        [NotMapped]
        public Polyline Polyline
        {
            get => polyline;
            set
            {
                if (polyline != value)
                {
                    polyline = value;
                    encodedPolyline = Polyline.EncodePoints(polyline.Points, Polyline.EncodingFactor);
                }
            }
        }

        public DateTime? ExpiryDate { get; set; }
    }
}
