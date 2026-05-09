using System;
using System.Runtime.InteropServices;

namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Framework-agnostic 3D vector struct, replacing Microsoft.Xna.Framework.Vector3.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MMDVector3 : IEquatable<MMDVector3>
    {
        public float X;
        public float Y;
        public float Z;

        public static readonly MMDVector3 Zero = new MMDVector3(0, 0, 0);
        public static readonly MMDVector3 One = new MMDVector3(1, 1, 1);
        public static readonly MMDVector3 UnitX = new MMDVector3(1, 0, 0);
        public static readonly MMDVector3 UnitY = new MMDVector3(0, 1, 0);
        public static readonly MMDVector3 UnitZ = new MMDVector3(0, 0, 1);

        public MMDVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public MMDVector3(float value)
        {
            X = Y = Z = value;
        }

        // ---- Properties ----

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public float LengthSquared => X * X + Y * Y + Z * Z;

        // ---- Operators ----

        public static MMDVector3 operator +(MMDVector3 a, MMDVector3 b) =>
            new MMDVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static MMDVector3 operator -(MMDVector3 a, MMDVector3 b) =>
            new MMDVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static MMDVector3 operator *(MMDVector3 a, float s) =>
            new MMDVector3(a.X * s, a.Y * s, a.Z * s);
        public static MMDVector3 operator *(float s, MMDVector3 a) =>
            new MMDVector3(a.X * s, a.Y * s, a.Z * s);
        public static MMDVector3 operator /(MMDVector3 a, float s) =>
            new MMDVector3(a.X / s, a.Y / s, a.Z / s);
        public static MMDVector3 operator -(MMDVector3 v) =>
            new MMDVector3(-v.X, -v.Y, -v.Z);

        // ---- Equality ----

        public bool Equals(MMDVector3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object obj) => obj is MMDVector3 other && Equals(other);
        public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() << 4);
        public static bool operator ==(MMDVector3 a, MMDVector3 b) => a.Equals(b);
        public static bool operator !=(MMDVector3 a, MMDVector3 b) => !a.Equals(b);

        public override string ToString() => $"{{X:{X} Y:{Y} Z:{Z}}}";

        // ---- Static Methods (ref versions for perf) ----

        public static MMDVector3 Add(MMDVector3 a, MMDVector3 b) =>
            new MMDVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static void Add(ref MMDVector3 a, ref MMDVector3 b, out MMDVector3 result) =>
            result = new MMDVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static MMDVector3 Subtract(MMDVector3 a, MMDVector3 b) =>
            new MMDVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static MMDVector3 Multiply(MMDVector3 a, float s) =>
            new MMDVector3(a.X * s, a.Y * s, a.Z * s);
        public static void Multiply(ref MMDVector3 a, float s, out MMDVector3 result) =>
            result = new MMDVector3(a.X * s, a.Y * s, a.Z * s);

        public static MMDVector3 Normalize(MMDVector3 v)
        {
            float len = v.Length;
            return len > 0 ? v / len : Zero;
        }

        public static MMDVector3 Cross(MMDVector3 a, MMDVector3 b) =>
            new MMDVector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);

        public static float Dot(MMDVector3 a, MMDVector3 b) =>
            a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static MMDVector3 Lerp(MMDVector3 a, MMDVector3 b, float t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return new MMDVector3(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
        }
        public static void Lerp(ref MMDVector3 a, ref MMDVector3 b, float t, out MMDVector3 result)
        {
            t = MathHelper.Clamp(t, 0, 1);
            result = new MMDVector3(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
        }

        /// <summary>
        /// Transforms a vector by a matrix.
        /// </summary>
        public static MMDVector3 Transform(MMDVector3 v, MMDMatrix m)
        {
            return new MMDVector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43);
        }
        public static void Transform(ref MMDVector3 v, ref MMDMatrix m, out MMDVector3 result)
        {
            result = new MMDVector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43);
        }
        public static void Transform(ref MMDVector3 v, ref MMDQuaternion q, out MMDVector3 result)
        {
            float qx2 = q.X + q.X, qy2 = q.Y + q.Y, qz2 = q.Z + q.Z;
            float qxx = q.X * qx2, qyy = q.Y * qy2, qzz = q.Z * qz2;
            float qxy = q.X * qy2, qxz = q.X * qz2, qyz = q.Y * qz2;
            float qwx = q.W * qx2, qwy = q.W * qy2, qwz = q.W * qz2;
            result = new MMDVector3(
                v.X * (1 - qyy - qzz) + v.Y * (qxy - qwz) + v.Z * (qxz + qwy),
                v.X * (qxy + qwz) + v.Y * (1 - qxx - qzz) + v.Z * (qyz - qwx),
                v.X * (qxz - qwy) + v.Y * (qyz + qwx) + v.Z * (1 - qxx - qyy));
        }
        public static void Transform(ref MMDVector3 v, ref MMDQuaternion q, out MMDVector4 result)
        {
            float qx2 = q.X + q.X, qy2 = q.Y + q.Y, qz2 = q.Z + q.Z;
            float qxx = q.X * qx2, qyy = q.Y * qy2, qzz = q.Z * qz2;
            float qxy = q.X * qy2, qxz = q.X * qz2, qyz = q.Y * qz2;
            float qwx = q.W * qx2, qwy = q.W * qy2, qwz = q.W * qz2;
            result = new MMDVector4(
                v.X * (1 - qyy - qzz) + v.Y * (qxy - qwz) + v.Z * (qxz + qwy),
                v.X * (qxy + qwz) + v.Y * (1 - qxx - qzz) + v.Z * (qyz - qwx),
                v.X * (qxz - qwy) + v.Y * (qyz + qwx) + v.Z * (1 - qxx - qyy),
                0);
        }

        /// <summary>
        /// Transforms a normal vector by a matrix (no translation component).
        /// </summary>
        public static MMDVector3 TransformNormal(MMDVector3 v, MMDMatrix m)
        {
            return new MMDVector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33);
        }
        public static void TransformNormal(ref MMDVector3 v, ref MMDMatrix m, out MMDVector3 result)
        {
            result = new MMDVector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33);
        }

        public static float Distance(MMDVector3 a, MMDVector3 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
