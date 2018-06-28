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

        public Train(int id)
            : base(id)
        { }

        public void Fill(string name)
        {
            Name = name;
            Filled = true;
        }
    }
}
