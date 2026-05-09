using System;
using System.Runtime.InteropServices;

namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Framework-agnostic 2D vector struct, replacing Microsoft.Xna.Framework.Vector2.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MMDVector2 : IEquatable<MMDVector2>
    {
        public float X;
        public float Y;

        public static readonly MMDVector2 Zero = new MMDVector2(0, 0);
        public static readonly MMDVector2 One = new MMDVector2(1, 1);
        public static readonly MMDVector2 UnitX = new MMDVector2(1, 0);
        public static readonly MMDVector2 UnitY = new MMDVector2(0, 1);

        public MMDVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        // ---- Operators ----

        public static MMDVector2 operator +(MMDVector2 a, MMDVector2 b) =>
            new MMDVector2(a.X + b.X, a.Y + b.Y);
        public static MMDVector2 operator -(MMDVector2 a, MMDVector2 b) =>
            new MMDVector2(a.X - b.X, a.Y - b.Y);
        public static MMDVector2 operator *(MMDVector2 a, float s) =>
            new MMDVector2(a.X * s, a.Y * s);
        public static MMDVector2 operator *(float s, MMDVector2 a) =>
            new MMDVector2(a.X * s, a.Y * s);
        public static MMDVector2 operator /(MMDVector2 a, float s) =>
            new MMDVector2(a.X / s, a.Y / s);
        public static MMDVector2 operator -(MMDVector2 v) =>
            new MMDVector2(-v.X, -v.Y);

        // ---- Equality ----

        public bool Equals(MMDVector2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is MMDVector2 other && Equals(other);
        public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2);
        public static bool operator ==(MMDVector2 a, MMDVector2 b) => a.Equals(b);
        public static bool operator !=(MMDVector2 a, MMDVector2 b) => !a.Equals(b);

        public override string ToString() => $"{{X:{X} Y:{Y}}}";

        // ---- Static Methods ----

        public static MMDVector2 Lerp(MMDVector2 a, MMDVector2 b, float t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return new MMDVector2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        }

        public static void Lerp(ref MMDVector2 a, ref MMDVector2 b, float t, out MMDVector2 result)
        {
            t = MathHelper.Clamp(t, 0, 1);
            result = new MMDVector2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        }

        public static MMDVector2 Normalize(MMDVector2 v)
        {
            float len = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
            return len > 0 ? new MMDVector2(v.X / len, v.Y / len) : Zero;
        }

        public static float Distance(MMDVector2 a, MMDVector2 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float Dot(MMDVector2 a, MMDVector2 b) =>
            a.X * b.X + a.Y * b.Y;
    }
}
