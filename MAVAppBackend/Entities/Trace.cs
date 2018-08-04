using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public class Trace : Entity<int>
    {
        private int trainInstanceID;
        public int TrainInstanceID
        {
            get => trainInstanceID;
            set
            {
                trainInstanceID = value;
                OnChange();
            }
        }

        private Vector2 gpsCoord;
        public Vector2 GPSCoord
        {
            get => gpsCoord;
            set
            {
                gpsCoord = value;
                OnChange();
            }
        }

        private DateTime updated;
        public DateTime Updated
        {
            get => updated;
            set
            {
                updated = value;
                OnChange();
            }
        }
        public override void Fill(DbDataReader reader)
        {
            Key = reader.GetInt32("id");
            trainInstanceID = reader.GetInt32("train_instance_id");
            gpsCoord = reader.GetVector2OrNull("lat", "lon");
            updated = reader.GetDateTime("updated");
            Filled = true;
        }

        public override void Fill(Entity<int> other)
        {
            if (!(other is Trace trace)) return;

            trainInstanceID = trace.trainInstanceID;
            gpsCoord = trace.gpsCoord;
            updated = trace.updated;
            Filled = trace.Filled;
        }
    }
}
