using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using SharpEntities;

namespace MAVAppBackend.Model
{
    public class TestEntity : UpdatableEntity<int>
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

        private int money;
        public int Money
        {
            get => money;
            set
            {
                money = value;
                OnChange();
            }
        }

        private Vector2 coord;
        public Vector2 Coord
        {
            get => coord;
            set
            {
                coord = value;
                OnChange();
            }
        }

        public TestEntity(int key)
            : base(key)
        { }

        protected override void InternalFill(DbDataReader reader)
        {
            Name = reader.GetString("name");
            Money = reader.GetInt32("money");
            Coord = reader.GetVector2OrNull("lat", "lon");
        }

        public override void Fill(UpdatableEntity<int> other)
        {
            if (other is TestEntity test)
            {
                Name = test.Name;
                Money = test.Money;
                Coord = test.Coord.Clone();
            }
        }
    }
}
