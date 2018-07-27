using MAVAppBackend.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.Model
{
    public class TrainStation : Entity
    {
        public string Name { private set; get; }
        public Train Parent { private set; get; }
        public int Number { private set; get; }

        public TrainStation(int key)
            : base(key)
        { }

        public void Fill(string name, Train parent, int number)
        {
            Name = name;
            Parent = parent;
            Number = number;
            Filled = true;
        }
    }
}
