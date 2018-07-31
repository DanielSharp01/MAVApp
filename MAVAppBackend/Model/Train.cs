using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using SharpEntities;

namespace MAVAppBackend.Model
{
    public class Train : UpdatableEntity<int>
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnChange();
            }
        }

        private string type;
        public string Type
        {
            get => type;
            set
            {
                type = value;
                OnChange();
            }
        }

        private Polyline polyline;
        public Polyline Polyline
        {
            get => polyline;
            set
            {
                polyline = value;
                OnChange();
            }
        }

        private DateTime? expiryDate;
        public DateTime? ExpiryDate
        {
            get => expiryDate;
            set
            {
                expiryDate = value;
                OnChange();
            }
        }

        public Train(int id)
            : base(id)
        { }

        public override void Fill(UpdatableEntity<int> other)
        {
            if (other is Train train)
            {
                Name = train.Name;
                Type = train.Type;
                Polyline = train.Polyline;
                ExpiryDate = train.ExpiryDate;
                Filled = train.Filled;
            }
        }

        protected override void InternalFill(DbDataReader reader)
        {
            Name = reader.GetStringOrNull("name");
            Type = reader.GetStringOrNull("type");
            Polyline = reader.GetPolylineOrNull("polyline");
            ExpiryDate = reader.GetDateTimeOrNull("expiry_date");
        }
    }
}
