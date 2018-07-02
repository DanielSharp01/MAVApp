using MAVAppBackend.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.Model
{
    public class Train : Entity
    {
        public string Name { private set; get; }
        public string From { private set; get; }
        public Station FromStation { private set; get; }
        public string To { private set; get; }
        public Station ToStation { private set; get; }
        public string Type { private set; get; }
        public DateTime? ExpiryDate { private set; get; }

        public Train(int id)
            : base(id)
        { }

        public void Fill(string name, string from, Station fromStation, string to, Station toStation, string type, DateTime? expiryDate)
        {
            Name = name;
            From = from;
            To = to;
            FromStation = fromStation;
            ToStation = toStation;
            Type = type;
            ExpiryDate = expiryDate;
            Filled = true;
        }

        public void APIFill(string name, string from, string to, string type)
        {
            Name = name;
            From = from;
            To = to;

            FromStation = From == null ? null : Database.Instance.StationMapper.GetByName(from, false);
            if (!FromStation.Filled) FromStation = null;
            ToStation = To == null ? null : Database.Instance.StationMapper.GetByName(to, false);
            if (!ToStation.Filled) ToStation = null;

            Type = type;
        }

        public void APIFill(DateTime expiryDate)
        {
            ExpiryDate = expiryDate;
        }
    }
}
