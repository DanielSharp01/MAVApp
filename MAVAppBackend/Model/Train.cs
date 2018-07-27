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
        public string To { private set; get; }
        public string Type { private set; get; }
        public Polyline Polyline { private set; get; }
        public DateTime? ExpiryDate { private set; get; }

        public Train(int key)
            : base(key)
        { }

        public void Fill(string name, string from, string to, string type, DateTime? expiryDate, Polyline Polyline)
        {
            Name = name;
            From = from;
            To = to;
            Type = type;
            ExpiryDate = expiryDate;
            Filled = true;
        }

        public void APIFill(string name, string from, string to, string type)
        {
            Name = name;
            From = from;
            To = to;
            Type = type;
        }

        public void APIFill(DateTime expiryDate)
        {
            ExpiryDate = expiryDate;
        }
    }
}
