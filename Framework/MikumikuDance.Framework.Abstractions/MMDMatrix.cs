using System;
using System.Runtime.InteropServices;

namespace MikumikuDance.Framework.Abstractions
{
    /// <summary>
    /// Framework-agnostic 4x4 matrix struct, replacing Microsoft.Xna.Framework.Matrix.
    /// Row-major storage (M11..M44).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MMDMatrix : IEquatable<MMDMatrix>
    {
        public float M11; public float M12; public float M13; public float M14;
        public float M21; public float M22; public float M23; public float M24;
        public float M31; public float M32; public float M33; public float M34;
        public float M41; public float M42; public float M43; public float M44;

        public static readonly MMDMatrix Identity = new MMDMatrix(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);

        public MMDMatrix(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        // ---- Properties for convenience ----
        public MMDVector3 Translation
        {
            get { return new MMDVector3(M41, M42, M43); }
            set { M41 = value.X; M42 = value.Y; M43 = value.Z; }
        }

        public MMDVector3 Up
        {
            get { return new MMDVector3(M21, M22, M23); }
        }

        public MMDVector3 Down
        {
            get { return new MMDVector3(-M21, -M22, -M23); }
        }

        public MMDVector3 Right
        {
            get { return new MMDVector3(M11, M12, M13); }
        }

        public MMDVector3 Left
        {
            get { return new MMDVector3(-M11, -M12, -M13); }
        }

        public MMDVector3 Forward
        {
            get { return new MMDVector3(-M31, -M32, -M33); }
        }

        public MMDVector3 Backward
        {
            get { return new MMDVector3(M31, M32, M33); }
        }

        // ---- Operators ----
        public static MMDMatrix operator *(MMDMatrix a, MMDMatrix b)
        {
            MMDMatrix r;
            r.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
            r.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
            r.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
            r.M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44;
            r.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
            r.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
            r.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
            r.M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44;
            r.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
            r.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
            r.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
            r.M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44;
            r.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
            r.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
            r.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
            r.M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44;
            return r;
        }

        public static MMDMatrix operator *(MMDMatrix a, float s)
        {
            MMDMatrix r = a;
            r.M11 *= s; r.M12 *= s; r.M13 *= s; r.M14 *= s;
            r.M21 *= s; r.M22 *= s; r.M23 *= s; r.M24 *= s;
            r.M31 *= s; r.M32 *= s; r.M33 *= s; r.M34 *= s;
            r.M41 *= s; r.M42 *= s; r.M43 *= s; r.M44 *= s;
            return r;
        }

        // ---- Equality ----
        public bool Equals(MMDMatrix other) =>
            M11 == other.M11 && M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
            M21 == other.M21 && M22 == other.M22 && M23 == other.M23 && M24 == other.M24 &&
            M31 == other.M31 && M32 == other.M32 && M33 == other.M33 && M34 == other.M34 &&
            M41 == other.M41 && M42 == other.M42 && M43 == other.M43 && M44 == other.M44;
        public override bool Equals(object obj) => obj is MMDMatrix other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + M11.GetHashCode(); hash = hash * 31 + M12.GetHashCode();
                hash = hash * 31 + M13.GetHashCode(); hash = hash * 31 + M14.GetHashCode();
                hash = hash * 31 + M21.GetHashCode(); hash = hash * 31 + M22.GetHashCode();
                hash = hash * 31 + M23.GetHashCode(); hash = hash * 31 + M24.GetHashCode();
                hash = hash * 31 + M31.GetHashCode(); hash = hash * 31 + M32.GetHashCode();
                hash = hash * 31 + M33.GetHashCode(); hash = hash * 31 + M34.GetHashCode();
                hash = hash * 31 + M41.GetHashCode(); hash = hash * 31 + M42.GetHashCode();
                hash = hash * 31 + M43.GetHashCode(); hash = hash * 31 + M44.GetHashCode();
                return hash;
            }
        }
        public static bool operator ==(MMDMatrix a, MMDMatrix b) => a.Equals(b);
        public static bool operator !=(MMDMatrix a, MMDMatrix b) => !a.Equals(b);

        // ---- Static Factory Methods ----

        public static MMDMatrix CreateTranslation(float x, float y, float z)
        {
            MMDMatrix r = Identity;
            r.M41 = x; r.M42 = y; r.M43 = z;
            return r;
        }
        public static void CreateTranslation(ref MMDVector3 position, out MMDMatrix result)
        {
            result = Identity;
            result.M41 = position.X; result.M42 = position.Y; result.M43 = position.Z;
        }

        public static MMDMatrix CreateScale(float x, float y, float z)
        {
            MMDMatrix r = Identity;
            r.M11 = x; r.M22 = y; r.M33 = z;
            return r;
        }
        public static void CreateScale(ref MMDVector3 scales, out MMDMatrix result)
        {
            result = Identity;
            result.M11 = scales.X; result.M22 = scales.Y; result.M33 = scales.Z;
        }

        public static MMDMatrix CreateRotationX(float radians)
        {
            float c = (float)Math.Cos(radians), s = (float)Math.Sin(radians);
            MMDMatrix r = Identity;
            r.M22 = c; r.M23 = s; r.M32 = -s; r.M33 = c;
            return r;
        }

        public static MMDMatrix CreateRotationY(float radians)
        {
            float c = (float)Math.Cos(radians), s = (float)Math.Sin(radians);
            MMDMatrix r = Identity;
            r.M11 = c; r.M13 = -s; r.M31 = s; r.M33 = c;
            return r;
        }

        public static MMDMatrix CreateRotationZ(float radians)
        {
            float c = (float)Math.Cos(radians), s = (float)Math.Sin(radians);
            MMDMatrix r = Identity;
            r.M11 = c; r.M12 = s; r.M21 = -s; r.M22 = c;
            return r;
        }

        public static MMDMatrix CreateFromQuaternion(MMDQuaternion q)
        {
            MMDMatrix r = Identity;
            float qx2 = q.X + q.X, qy2 = q.Y + q.Y, qz2 = q.Z + q.Z;
            float qxx = q.X * qx2, qyy = q.Y * qy2, qzz = q.Z * qz2;
            float qxy = q.X * qy2, qxz = q.X * qz2, qyz = q.Y * qz2;
            float qwx = q.W * qx2, qwy = q.W * qy2, qwz = q.W * qz2;
            r.M11 = 1 - qyy - qzz; r.M12 = qxy + qwz; r.M13 = qxz - qwy;
            r.M21 = qxy - qwz; r.M22 = 1 - qxx - qzz; r.M23 = qyz + qwx;
            r.M31 = qxz + qwy; r.M32 = qyz - qwx; r.M33 = 1 - qxx - qyy;
            return r;
        }
        public static void CreateFromQuaternion(ref MMDQuaternion q, out MMDMatrix result)
        {
            result = CreateFromQuaternion(q);
        }

        public static MMDMatrix CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            return CreateFromQuaternion(MMDQuaternion.CreateFromYawPitchRoll(yaw, pitch, roll));
        }

        public static MMDMatrix CreateLookAt(MMDVector3 cameraPosition, MMDVector3 cameraTarget, MMDVector3 cameraUpVector)
        {
            MMDVector3 forward = MMDVector3.Normalize(cameraTarget - cameraPosition);
            MMDVector3 right = MMDVector3.Normalize(MMDVector3.Cross(forward, cameraUpVector));
            MMDVector3 up = MMDVector3.Cross(right, forward);
            MMDMatrix r = Identity;
            r.M11 = right.X; r.M12 = up.X; r.M13 = -forward.X;
            r.M21 = right.Y; r.M22 = up.Y; r.M23 = -forward.Y;
            r.M31 = right.Z; r.M32 = up.Z; r.M33 = -forward.Z;
            r.M41 = -MMDVector3.Dot(right, cameraPosition);
            r.M42 = -MMDVector3.Dot(up, cameraPosition);
            r.M43 = MMDVector3.Dot(forward, cameraPosition);
            return r;
        }
        public static void CreateLookAt(ref MMDVector3 cameraPosition, ref MMDVector3 cameraTarget, ref MMDVector3 cameraUpVector, out MMDMatrix result)
        {
            result = CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
        }

        public static MMDMatrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            float yScale = (float)(1.0 / Math.Tan(fieldOfView * 0.5));
            float xScale = yScale / aspectRatio;
            MMDMatrix r = Identity;
            r.M11 = xScale; r.M22 = yScale;
            r.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            r.M34 = -1;
            r.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            r.M44 = 0;
            return r;
        }
        public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out MMDMatrix result)
        {
            result = CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
        }

        // ---- Operations ----
        public static MMDMatrix Multiply(MMDMatrix a, MMDMatrix b) => a * b;
        public static void Multiply(ref MMDMatrix a, ref MMDMatrix b, out MMDMatrix result)
        {
            result = a * b;
        }

        public static MMDMatrix Invert(MMDMatrix m)
        {
            float det = m.M11 * (m.M22 * m.M33 * m.M44 + m.M23 * m.M34 * m.M42 + m.M24 * m.M32 * m.M43
                               - m.M24 * m.M33 * m.M42 - m.M23 * m.M32 * m.M44 - m.M22 * m.M34 * m.M43)
                      - m.M12 * (m.M21 * m.M33 * m.M44 + m.M23 * m.M34 * m.M41 + m.M24 * m.M31 * m.M43
                               - m.M24 * m.M33 * m.M41 - m.M23 * m.M31 * m.M44 - m.M21 * m.M34 * m.M43)
                      + m.M13 * (m.M21 * m.M32 * m.M44 + m.M22 * m.M34 * m.M41 + m.M24 * m.M31 * m.M42
                               - m.M24 * m.M32 * m.M41 - m.M22 * m.M31 * m.M44 - m.M21 * m.M34 * m.M42)
                      - m.M14 * (m.M21 * m.M32 * m.M43 + m.M22 * m.M33 * m.M41 + m.M23 * m.M31 * m.M42
                               - m.M23 * m.M32 * m.M41 - m.M22 * m.M31 * m.M43 - m.M21 * m.M33 * m.M42);
            if (Math.Abs(det) < 1e-10f) return Identity;
            float invDet = 1.0f / det;
            MMDMatrix r;
            r.M11 = (m.M22 * m.M33 * m.M44 + m.M23 * m.M34 * m.M42 + m.M24 * m.M32 * m.M43 - m.M24 * m.M33 * m.M42 - m.M23 * m.M32 * m.M44 - m.M22 * m.M34 * m.M43) * invDet;
            r.M12 = (m.M13 * m.M24 * m.M42 + m.M14 * m.M22 * m.M43 + m.M12 * m.M23 * m.M44 - m.M12 * m.M24 * m.M43 - m.M14 * m.M23 * m.M42 - m.M13 * m.M22 * m.M44) * invDet;
            r.M13 = (m.M12 * m.M24 * m.M33 + m.M13 * m.M22 * m.M34 + m.M14 * m.M23 * m.M32 - m.M14 * m.M22 * m.M33 - m.M12 * m.M23 * m.M34 - m.M13 * m.M24 * m.M32) * invDet;
            r.M14 = (m.M13 * m.M22 * m.M44 + m.M14 * m.M23 * m.M42 + m.M12 * m.M24 * m.M43 - m.M14 * m.M22 * m.M43 - m.M12 * m.M23 * m.M44 - m.M13 * m.M24 * m.M42) * invDet;
            r.M21 = (m.M23 * m.M31 * m.M44 + m.M24 * m.M33 * m.M41 + m.M21 * m.M34 * m.M43 - m.M21 * m.M33 * m.M44 - m.M24 * m.M31 * m.M43 - m.M23 * m.M34 * m.M41) * invDet;
            r.M22 = (m.M11 * m.M24 * m.M43 + m.M13 * m.M21 * m.M44 + m.M14 * m.M23 * m.M41 - m.M14 * m.M21 * m.M43 - m.M13 * m.M24 * m.M41 - m.M11 * m.M23 * m.M44) * invDet;
            r.M23 = (m.M14 * m.M21 * m.M33 + m.M11 * m.M23 * m.M34 + m.M13 * m.M24 * m.M31 - m.M13 * m.M21 * m.M34 - m.M11 * m.M24 * m.M33 - m.M14 * m.M23 * m.M31) * invDet;
            r.M24 = (m.M11 * m.M24 * m.M33 + m.M13 * m.M21 * m.M34 + m.M14 * m.M23 * m.M31 - m.M11 * m.M23 * m.M34 - m.M13 * m.M24 * m.M31 - m.M14 * m.M21 * m.M33) * invDet;
            r.M31 = (m.M21 * m.M32 * m.M44 + m.M22 * m.M34 * m.M41 + m.M24 * m.M31 * m.M42 - m.M24 * m.M32 * m.M41 - m.M22 * m.M31 * m.M44 - m.M21 * m.M34 * m.M42) * invDet;
            r.M32 = (m.M14 * m.M21 * m.M42 + m.M11 * m.M22 * m.M44 + m.M12 * m.M24 * m.M41 - m.M12 * m.M21 * m.M44 - m.M11 * m.M24 * m.M42 - m.M14 * m.M22 * m.M41) * invDet;
            r.M33 = (m.M11 * m.M24 * m.M32 + m.M12 * m.M21 * m.M34 + m.M14 * m.M22 * m.M31 - m.M14 * m.M21 * m.M32 - m.M11 * m.M22 * m.M34 - m.M12 * m.M24 * m.M31) * invDet;
            r.M34 = (m.M12 * m.M21 * m.M33 + m.M11 * m.M22 * m.M33 + m.M13 * m.M22 * m.M31 - m.M13 * m.M21 * m.M32 - m.M11 * m.M23 * m.M32 - m.M12 * m.M24 * m.M31) * invDet;
            r.M41 = (m.M22 * m.M31 * m.M43 + m.M23 * m.M32 * m.M41 + m.M21 * m.M33 * m.M42 - m.M21 * m.M32 * m.M43 - m.M23 * m.M31 * m.M42 - m.M22 * m.M33 * m.M41) * invDet;
            r.M42 = (m.M11 * m.M23 * m.M42 + m.M12 * m.M21 * m.M43 + m.M13 * m.M22 * m.M41 - m.M13 * m.M21 * m.M42 - m.M11 * m.M22 * m.M43 - m.M12 * m.M23 * m.M41) * invDet;
            r.M43 = (m.M13 * m.M21 * m.M32 + m.M11 * m.M22 * m.M33 + m.M12 * m.M23 * m.M31 - m.M12 * m.M21 * m.M33 - m.M11 * m.M23 * m.M32 - m.M13 * m.M22 * m.M31) * invDet;
            r.M44 = (m.M11 * m.M22 * m.M33 + m.M12 * m.M23 * m.M31 + m.M13 * m.M21 * m.M32 - m.M13 * m.M22 * m.M31 - m.M11 * m.M23 * m.M32 - m.M12 * m.M21 * m.M33) * invDet;
            return r;
        }
        public static void Invert(ref MMDMatrix m, out MMDMatrix result)
        {
            result = Invert(m);
        }

        public static MMDMatrix Transpose(MMDMatrix m)
        {
            return new MMDMatrix(
                m.M11, m.M21, m.M31, m.M41,
                m.M12, m.M22, m.M32, m.M42,
                m.M13, m.M23, m.M33, m.M43,
                m.M14, m.M24, m.M34, m.M44);
        }

        /// <summary>
        /// Decomposes a matrix into scale, rotation (quaternion), and translation.
        /// Equivalent to XNA Matrix.Decompose.
        /// </summary>
        public void Decompose(out MMDVector3 scale, out MMDQuaternion rotation, out MMDVector3 translation)
        {
            translation = new MMDVector3(M41, M42, M43);

            // Extract scale from each column's length
            scale = new MMDVector3(
                (float)Math.Sqrt(M11 * M11 + M12 * M12 + M13 * M13),
                (float)Math.Sqrt(M21 * M21 + M22 * M22 + M23 * M23),
                (float)Math.Sqrt(M31 * M31 + M32 * M32 + M33 * M33));

            // Detect negative scale (reflection)
            float det = M11 * M22 * M33 + M12 * M23 * M31 + M13 * M21 * M32
                      - M13 * M22 * M31 - M11 * M23 * M32 - M12 * M21 * M33;
            if (det < 0)
            {
                scale.X = -scale.X;
            }

            // Build rotation matrix from normalized axes
            float invSx = 1.0f / scale.X;
            float invSy = 1.0f / scale.Y;
            float invSz = 1.0f / scale.Z;

            float r11 = M11 * invSx; float r12 = M12 * invSx; float r13 = M13 * invSx;
            float r21 = M21 * invSy; float r22 = M22 * invSy; float r23 = M23 * invSy;
            float r31 = M31 * invSz; float r32 = M32 * invSz; float r33 = M33 * invSz;

            // Convert rotation matrix to quaternion
            float trace = r11 + r22 + r33;
            float qw, qx, qy, qz;
            if (trace > 0.0f)
            {
                float s = (float)Math.Sqrt(trace + 1.0f) * 2.0f;
                qw = 0.25f * s;
                qx = (r23 - r32) / s;
                qy = (r31 - r13) / s;
                qz = (r12 - r21) / s;
            }
            else if (r11 > r22 && r11 > r33)
            {
                float s = (float)Math.Sqrt(1.0f + r11 - r22 - r33) * 2.0f;
                qw = (r23 - r32) / s;
                qx = 0.25f * s;
                qy = (r12 + r21) / s;
                qz = (r13 + r31) / s;
            }
            else if (r22 > r33)
            {
                float s = (float)Math.Sqrt(1.0f + r22 - r11 - r33) * 2.0f;
                qw = (r31 - r13) / s;
                qx = (r12 + r21) / s;
                qy = 0.25f * s;
                qz = (r23 + r32) / s;
            }
            else
            {
                float s = (float)Math.Sqrt(1.0f + r33 - r11 - r22) * 2.0f;
                qw = (r12 - r21) / s;
                qx = (r13 + r31) / s;
                qy = (r23 + r32) / s;
                qz = 0.25f * s;
            }
            rotation = new MMDQuaternion(qx, qy, qz, qw);
            rotation.Normalize();
        }
    }
}
