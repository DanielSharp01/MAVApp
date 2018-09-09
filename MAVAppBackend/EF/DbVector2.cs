using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EF
{
    public class DbVector2
    {
        public static DbVector2 Null { get; } = new DbVector2() { X = null, Y = null };
        public bool HasValue => X.HasValue && Y.HasValue;

        public double? X { get; set; }
        public double? Y { get; set; }

        public Vector2 AsVector2 => HasValue ? new Vector2(X.Value, Y.Value) : null;

        public override bool Equals(object obj)
        {
            if (!HasValue && obj == null) return true;
            if (ReferenceEquals(obj, this)) return true;
            if (!(obj is DbVector2 vec)) return false;

            if (HasValue != vec.HasValue) return false;

            return vec.X.HasValue && X.HasValue && Math.Abs(vec.X.Value - X.Value) < double.Epsilon
                && vec.Y.HasValue && Y.HasValue && Math.Abs(vec.Y.Value - Y.Value) < double.Epsilon;
        }

        public static bool operator==(DbVector2 a, DbVector2 b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null) return false;

            return a.Equals(b);
        }

        public static bool operator!=(DbVector2 a, DbVector2 b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();

            return hash;
        }
    }
}
