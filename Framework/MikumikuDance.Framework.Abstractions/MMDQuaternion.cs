using System;
using System.Runtime.InteropServices;

namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Framework-agnostic quaternion struct, replacing Microsoft.Xna.Framework.Quaternion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MMDQuaternion : IEquatable<MMDQuaternion>
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public static readonly MMDQuaternion Identity = new MMDQuaternion(0, 0, 0, 1);

        public MMDQuaternion(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }

        // ---- Operators ----
        public static MMDQuaternion operator +(MMDQuaternion a, MMDQuaternion b) =>
            new MMDQuaternion(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static MMDQuaternion operator *(MMDQuaternion a, MMDQuaternion b) =>
            Multiply(a, b);
        public static MMDQuaternion operator -(MMDQuaternion a, MMDQuaternion b) =>
            new MMDQuaternion(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static MMDQuaternion operator -(MMDQuaternion q) =>
            new MMDQuaternion(-q.X, -q.Y, -q.Z, -q.W);

        // ---- Equality ----
        public bool Equals(MMDQuaternion other) =>
            X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override bool Equals(object obj) => obj is MMDQuaternion other && Equals(other);
        public override int GetHashCode() =>
            X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() << 4) ^ (W.GetHashCode() << 6);
        public static bool operator ==(MMDQuaternion a, MMDQuaternion b) => a.Equals(b);
        public static bool operator !=(MMDQuaternion a, MMDQuaternion b) => !a.Equals(b);

        public override string ToString() => $"{{X:{X} Y:{Y} Z:{Z} W:{W}}}";

        // ---- Instance Methods ----
        public void Normalize()
        {
            float len = (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
            if (len > 0) { X /= len; Y /= len; Z /= len; W /= len; }
            else { X = Y = Z = 0; W = 1; }
        }

        // ---- Static Methods ----
        public static MMDQuaternion Multiply(MMDQuaternion a, MMDQuaternion b)
        {
            return new MMDQuaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
        }
        public static void Multiply(ref MMDQuaternion a, ref MMDQuaternion b, out MMDQuaternion result)
        {
            result = new MMDQuaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
        }

        /// <summary>Spherical linear interpolation.</summary>
        public static MMDQuaternion Slerp(MMDQuaternion a, MMDQuaternion b, float t)
        {
            float cosOmega = a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
            if (cosOmega < 0) { b = -b; cosOmega = -cosOmega; }
            float k0, k1;
            if (cosOmega > 0.9999f)
            {
                k0 = 1 - t; k1 = t;
            }
            else
            {
                float sinOmega = (float)Math.Sqrt(1 - cosOmega * cosOmega);
                float omega = (float)Math.Atan2(sinOmega, cosOmega);
                float invSin = 1 / sinOmega;
                k0 = (float)Math.Sin((1 - t) * omega) * invSin;
                k1 = (float)Math.Sin(t * omega) * invSin;
            }
            return new MMDQuaternion(
                a.X * k0 + b.X * k1,
                a.Y * k0 + b.Y * k1,
                a.Z * k0 + b.Z * k1,
                a.W * k0 + b.W * k1);
        }
        public static void Slerp(ref MMDQuaternion a, ref MMDQuaternion b, float t, out MMDQuaternion result)
        {
            result = Slerp(a, b, t);
        }

        public static MMDQuaternion CreateFromAxisAngle(MMDVector3 axis, float angle)
        {
            float half = angle * 0.5f;
            float sin = (float)Math.Sin(half);
            return new MMDQuaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, (float)Math.Cos(half));
        }

        public static MMDQuaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float cy = (float)Math.Cos(yaw * 0.5f), sy = (float)Math.Sin(yaw * 0.5f);
            float cp = (float)Math.Cos(pitch * 0.5f), sp = (float)Math.Sin(pitch * 0.5f);
            float cr = (float)Math.Cos(roll * 0.5f), sr = (float)Math.Sin(roll * 0.5f);
            return new MMDQuaternion(
                sr * cp * cy - cr * sp * sy,
                cr * sp * cy + sr * cp * sy,
                cr * cp * sy - sr * sp * cy,
                cr * cp * cy + sr * sp * sy);
        }

        public static MMDQuaternion CreateFromRotationMatrix(MMDMatrix m)
        {
            float t = m.M11 + m.M22 + m.M33;
            float s;
            float x, y, z, w;
            if (t > 0)
            {
                s = (float)Math.Sqrt(t + 1) * 2;
                w = 0.25f * s;
                x = (m.M23 - m.M32) / s;
                y = (m.M31 - m.M13) / s;
                z = (m.M12 - m.M21) / s;
            }
            else if (m.M11 > m.M22 && m.M11 > m.M33)
            {
                s = (float)Math.Sqrt(1 + m.M11 - m.M22 - m.M33) * 2;
                w = (m.M23 - m.M32) / s;
                x = 0.25f * s;
                y = (m.M21 + m.M12) / s;
                z = (m.M31 + m.M13) / s;
            }
            else if (m.M22 > m.M33)
            {
                s = (float)Math.Sqrt(1 + m.M22 - m.M11 - m.M33) * 2;
                w = (m.M31 - m.M13) / s;
                x = (m.M21 + m.M12) / s;
                y = 0.25f * s;
                z = (m.M32 + m.M23) / s;
            }
            else
            {
                s = (float)Math.Sqrt(1 + m.M33 - m.M11 - m.M22) * 2;
                w = (m.M12 - m.M21) / s;
                x = (m.M31 + m.M13) / s;
                y = (m.M32 + m.M23) / s;
                z = 0.25f * s;
            }
            return new MMDQuaternion(x, y, z, w);
        }
    }
}
