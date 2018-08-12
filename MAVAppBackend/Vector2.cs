using System;

namespace MAVAppBackend
{
    /// <summary>
    /// 2D double vector
    /// </summary>
    public class Vector2
    {
        /// <summary>
        /// Vector with both coordinates being 0
        /// </summary>
        public static readonly Vector2 Zero = new Vector2(0, 0);

        /// <summary>
        /// X-coordinate
        /// </summary>
        public double X;
        /// <summary>
        /// Y-coordinate
        /// </summary>
        public double Y;

        /// <param name="x">X-coordinate as string</param>
        /// <param name="y">Y-coordinate as string</param>
        /// <exception cref="FormatException"></exception>
        public Vector2(string x, string y)
        {
            X = double.Parse(x);
            Y = double.Parse(y);
        }

        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2 Clone()
        {
            return new Vector2(X, Y);
        }

        public static Vector2 operator+(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator-(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator*(double a, Vector2 b)
        {
            return new Vector2(a * b.X, a * b.Y);
        }

        public static Vector2 operator*(Vector2 a, double b)
        {
            return new Vector2(a.X * b, a.Y * b);
        }

        public static Vector2 operator/(Vector2 a, double b)
        {
            return new Vector2(a.X / b, a.Y / b);
        }

        /// <summary>
        /// Returns the dot product with other vector
        /// </summary>
        /// <param name="o">Other vector</param>
        public double Dot(Vector2 o)
        {
            return X * o.X + Y * o.Y;
        }

        /// <summary>
        /// Returns the normalized copy of this vector
        /// </summary>
        public Vector2 Normalize()
        {
            return this / Length;
        }

        /// <summary>
        /// Returns the dot product of two vectors
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        public static double Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        /// <summary>
        /// Returns the normalized copy of a vector
        /// </summary>
        /// <param name="a">Vector to normalize</param>
        public static Vector2 Normalize(Vector2 a)
        {
            return a / a.Length;
        }

        /// <summary>
        /// Linearly interpolates between two vectors regarding a factor. a + (b - a) * s
        /// </summary>
        /// <param name="a">Starting point</param>
        /// <param name="b">Ending point</param>
        /// <param name="s">Interpolation factor</param>
        public static Vector2 Lerp(Vector2 a, Vector2 b, double s)
        {
            return a * (1 - s) + b * s;
        }

        /// <summary>
        /// Returns the string representation of this vector
        /// </summary>
        public override string ToString()
        {
            return $"{X},{Y}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (!(obj is Vector2 vec)) return false;

            return Math.Abs(vec.X - X) < double.Epsilon && Math.Abs(vec.Y - Y) < double.Epsilon;
        }

        public static bool operator==(Vector2 a, Vector2 b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null) return false;

            return a.Equals(b);
        }

        public static bool operator!=(Vector2 a, Vector2 b)
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

        /// <summary>
        /// Length of the vector
        /// </summary>
        public double Length => Math.Sqrt(LengthSquared);

        /// <summary>
        /// Square length of the vector. Can be used as a substitute when comparing a.Length &lt; b.Length to avoid square roots.
        /// </summary>
        public double LengthSquared => X * X + Y * Y;
    }
}
