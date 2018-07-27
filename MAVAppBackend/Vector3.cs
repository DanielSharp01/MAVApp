using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// 3D double vector
    /// </summary>
    public class Vector3
    {
        /// <summary>
        /// Vector with both coordinates being 0
        /// </summary>
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);

        /// <summary>
        /// X-coordinate
        /// </summary>
        public double X;
        /// <summary>
        /// Y-coordinate
        /// </summary>
        public double Y;
        /// <summary>
        /// Z-coordinate
        /// </summary>
        public double Z;

        /// <param name="x">X-coordinate as string</param>
        /// <param name="y">Y-coordinate as string</param>
        /// <param name="z">Z-coordinate as string</param>
        /// <exception cref="FormatException"></exception>
        public Vector3(string x, string y, string z)
        {
            X = double.Parse(x);
            Y = double.Parse(y);
            Z = double.Parse(z);
        }

        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <param name="z">Z-coordinate</param>
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 Clone()
        {
            return new Vector3(X, Y, Z);
        }

        public static Vector3 operator+(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator-(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator*(double a, Vector3 b)
        {
            return new Vector3(a * b.X, a * b.Y, a * b.Z);
        }

        public static Vector3 operator*(Vector3 a, double b)
        {
            return new Vector3(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3 operator/(Vector3 a, double b)
        {
            return new Vector3(a.X / b, a.Y / b, a.Z / b);
        }

        /// <summary>
        /// Returns the dot product with other vector
        /// </summary>
        /// <param name="o">Other vector</param>
        public double Dot(Vector3 o)
        {
            return X * o.X + Y * o.Y + Z * o.Z;
        }

        /// <summary>
        /// Returns the cross product with other vector
        /// </summary>
        /// <param name="o">Other vector</param>
        public Vector3 Cross(Vector3 o)
        {
            return new Vector3(Y * o.Z - Z * o.Y, X * o.Z - Z * o.X, X * o.Y - Y * o.X);
        }

        /// <summary>
        /// Returns the normalized copy of this vector
        /// </summary>
        public Vector3 Normalize()
        {
            return this / Length;
        }

        /// <summary>
        /// Returns the dot product of two vectors
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        public static double Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        /// <summary>
        /// Returns the cross product of two vectors
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        public Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(a.Y * b.Z - a.Z * b.Y, a.X * b.Z - a.Z * b.X, a.X * b.Y - a.Y * b.X);
        }

        /// <summary>
        /// Returns the normalized copy of a vector
        /// </summary>
        /// <param name="a">Vector to normalize</param>
        public static Vector3 Normalize(Vector3 a)
        {
            return a / a.Length;
        }

        /// <summary>
        /// Linearly interpolates between two vectors regarding a factor. a + (b - a) * s
        /// </summary>
        /// <param name="a">Starting point</param>
        /// <param name="b">Ending point</param>
        /// <param name="s">Interpolation factor</param>
        public static Vector3 Lerp(Vector3 a, Vector3 b, double s)
        {
            return a * (1 - s) + b * s;
        }

        /// <summary>
        /// Returns the string representation of this vector
        /// </summary>
        public override string ToString()
        {
            return $"{X},{Y},{Z}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (!(obj is Vector3 vec)) return false;

            return vec.X == X && vec.Y == Y && vec.Z == Z;
        }

        public static bool operator==(Vector3 a, Vector3 b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null)) return false;

            return a.Equals(b);
        }

        public static bool operator!=(Vector3 a, Vector3 b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            hash = hash * 23 + Z.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Length of the vector
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(LengthSquared);
            }
        }

        /// <summary>
        /// Square length of the vector. Can be used as a substitute when comparing a.Length &lt; b.Length to avoid square roots.
        /// </summary>
        public double LengthSquared
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }
    }
}
