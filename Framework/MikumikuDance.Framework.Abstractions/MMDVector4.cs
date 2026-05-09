using System;
using System.Runtime.InteropServices;

namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Framework-agnostic 4D vector struct, replacing Microsoft.Xna.Framework.Vector4.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MMDVector4 : IEquatable<MMDVector4>
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public static readonly MMDVector4 Zero = new MMDVector4(0, 0, 0, 0);
        public static readonly MMDVector4 One = new MMDVector4(1, 1, 1, 1);

        public MMDVector4(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }

        public MMDVector4(MMDVector3 v, float w)
        {
            X = v.X; Y = v.Y; Z = v.Z; W = w;
        }

        // ---- Operators ----
        public static MMDVector4 operator +(MMDVector4 a, MMDVector4 b) =>
            new MMDVector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static MMDVector4 operator -(MMDVector4 a, MMDVector4 b) =>
            new MMDVector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static MMDVector4 operator *(MMDVector4 a, float s) =>
            new MMDVector4(a.X * s, a.Y * s, a.Z * s, a.W * s);
        public static MMDVector4 operator /(MMDVector4 a, float s) =>
            new MMDVector4(a.X / s, a.Y / s, a.Z / s, a.W / s);

        // ---- Equality ----
        public bool Equals(MMDVector4 other) =>
            X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override bool Equals(object obj) => obj is MMDVector4 other && Equals(other);
        public override int GetHashCode() =>
            X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() << 4) ^ (W.GetHashCode() << 6);
        public static bool operator ==(MMDVector4 a, MMDVector4 b) => a.Equals(b);
        public static bool operator !=(MMDVector4 a, MMDVector4 b) => !a.Equals(b);

        public override string ToString() => $"{{X:{X} Y:{Y} Z:{Z} W:{W}}}";

        // ---- Static Methods ----
        public static MMDVector4 Add(MMDVector4 a, MMDVector4 b) =>
            new MMDVector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);

        public static MMDVector4 Transform(MMDVector3 v, MMDMatrix m)
        {
            return new MMDVector4(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43,
                v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + m.M44);
        }
        public static void Transform(ref MMDVector3 v, ref MMDMatrix m, out MMDVector4 result)
        {
            result = new MMDVector4(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43,
                v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + m.M44);
        }
    }
}
