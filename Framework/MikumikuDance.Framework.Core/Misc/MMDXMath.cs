using System;
using MikumikuDance.Framework.Abstractions;

namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// MMD用数学クラス（抽象型版、フレームワーク非依存）
    /// </summary>
    public static class MMDXMath
    {
        /// <summary>
        /// Vector2に変換
        /// </summary>
        public static MMDVector2 ToVector2(float[] vec)
        {
            return new MMDVector2(vec[0], vec[1]);
        }
        /// <summary>
        /// Vector3に変換
        /// </summary>
        public static MMDVector3 ToVector3(float[] vec)
        {
            return new MMDVector3(vec[0], vec[1], vec[2]);
        }
        /// <summary>
        /// Vector4に変換
        /// </summary>
        public static MMDVector4 ToVector4(float[] vec)
        {
            return new MMDVector4(vec[0], vec[1], vec[2], vec[3]);
        }
        /// <summary>
        /// swap関数
        /// </summary>
        public static void Swap<T>(ref T v1, ref T v2)
        {
            T v3 = v1;
            v1 = v2;
            v2 = v3;
        }
        /// <summary>
        /// MinMax関係が成り立つように各要素を修正
        /// </summary>
        public static void CheckMinMax(float[] min, float[] max)
        {
            for (int i = 0; i < min.Length && i < max.Length; i++)
            {
                if (min[i] > max[i])
                    Swap(ref min[i], ref max[i]);
            }
        }

        // ---- Quaternion factoring (framework-agnostic) ----

        /// <summary>
        /// クォータニオンをYaw(Y回転), Pitch(X回転), Roll(Z回転)に分解
        /// </summary>
        public static bool FactoringQuaternionZXY(MMDQuaternion input, out float ZRot, out float XRot, out float YRot)
        {
            MMDQuaternion inputQ = new MMDQuaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            MMDMatrix rot;
            CreateMatrixFromQuaternion(ref inputQ, out rot);
            if (rot.M32 > 1 - 1.0e-4 || rot.M32 < -1 + 1.0e-4)
            {
                XRot = (rot.M32 < 0 ? MMDMathHelper.PiOver2 : -MMDMathHelper.PiOver2);
                ZRot = 0; YRot = (float)Math.Atan2(-rot.M13, rot.M11);
                return false;
            }
            XRot = -(float)Math.Asin(rot.M32);
            ZRot = (float)Math.Asin(rot.M12 / Math.Cos(XRot));
            if (float.IsNaN(ZRot))
            {
                XRot = (rot.M32 < 0 ? MMDMathHelper.PiOver2 : -MMDMathHelper.PiOver2);
                ZRot = 0; YRot = (float)Math.Atan2(-rot.M13, rot.M11);
                return false;
            }
            if (rot.M22 < 0)
                ZRot = MMDMathHelper.Pi - ZRot;
            YRot = (float)Math.Atan2(rot.M31, rot.M33);
            return true;
        }

        /// <summary>
        /// クォータニオンをX,Y,Z回転に分解
        /// </summary>
        public static bool FactoringQuaternionXYZ(MMDQuaternion input, out float XRot, out float YRot, out float ZRot)
        {
            MMDQuaternion inputQ = new MMDQuaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            MMDMatrix rot;
            CreateMatrixFromQuaternion(ref inputQ, out rot);
            if (rot.M13 > 1 - 1.0e-4 || rot.M13 < -1 + 1.0e-4)
            {
                XRot = 0;
                YRot = (rot.M13 < 0 ? MMDMathHelper.PiOver2 : -MMDMathHelper.PiOver2);
                ZRot = -(float)Math.Atan2(-rot.M21, rot.M22);
                return false;
            }
            YRot = -(float)Math.Asin(rot.M13);
            XRot = (float)Math.Asin(rot.M23 / Math.Cos(YRot));
            if (float.IsNaN(XRot))
            {
                XRot = 0;
                YRot = (rot.M13 < 0 ? MMDMathHelper.PiOver2 : -MMDMathHelper.PiOver2);
                ZRot = -(float)Math.Atan2(-rot.M21, rot.M22);
                return false;
            }
            if (rot.M33 < 0)
                XRot = MMDMathHelper.Pi - XRot;
            ZRot = (float)Math.Atan2(rot.M12, rot.M11);
            return true;
        }

        /// <summary>
        /// クォータニオンをY,Z,X回転に分解
        /// </summary>
        public static bool FactoringQuaternionYZX(MMDQuaternion input, out float YRot, out float ZRot, out float XRot)
        {
            MMDQuaternion inputQ = new MMDQuaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            MMDMatrix rot;
            CreateMatrixFromQuaternion(ref inputQ, out rot);
            if (rot.M21 > 1 - 1.0e-4 || rot.M21 < -1 + 1.0e-4)
            {
                YRot = 0;
                ZRot = (rot.M21 < 0 ? MMDMathHelper.PiOver2 : -MMDMathHelper.PiOver2);
                XRot = -(float)Math.Atan2(-rot.M32, rot.M33);
                return false;
            }
            ZRot = -(float)Math.Asin(rot.M21);
            YRot = (float)Math.Asin(rot.M31 / Math.Cos(ZRot));
            if (float.IsNaN(YRot))
            {
                YRot = 0;
                ZRot = (rot.M21 < 0 ? MMDMathHelper.PiOver2 : -MMDMathHelper.PiOver2);
                XRot = -(float)Math.Atan2(-rot.M32, rot.M33);
                return false;
            }
            if (rot.M11 < 0)
                YRot = MMDMathHelper.Pi - YRot;
            XRot = (float)Math.Atan2(rot.M23, rot.M22);
            return true;
        }

        /// <summary>
        /// NaNチェック (Vector3)
        /// </summary>
        public static bool CheckNaN(MMDVector3 vec)
        {
            return float.IsNaN(vec.X) || float.IsNaN(vec.Y) || float.IsNaN(vec.Z);
        }
        /// <summary>
        /// NaNチェック (Matrix)
        /// </summary>
        public static bool CheckNaN(MMDMatrix mat)
        {
            return float.IsNaN(mat.M11) || float.IsNaN(mat.M12) ||
                   float.IsNaN(mat.M13) || float.IsNaN(mat.M14) ||
                   float.IsNaN(mat.M21) || float.IsNaN(mat.M22) ||
                   float.IsNaN(mat.M23) || float.IsNaN(mat.M24) ||
                   float.IsNaN(mat.M31) || float.IsNaN(mat.M32) ||
                   float.IsNaN(mat.M33) || float.IsNaN(mat.M34) ||
                   float.IsNaN(mat.M41) || float.IsNaN(mat.M42) ||
                   float.IsNaN(mat.M43) || float.IsNaN(mat.M44);
        }

        /// <summary>
        /// Color生成
        /// </summary>
        public static MMDColor CreateColor(int r, int g, int b)
        {
            return new MMDColor((byte)r, (byte)g, (byte)b);
        }

        /// <summary>
        /// マトリクスより移動ベクトルを生成
        /// </summary>
        public static void GetTranslation(ref MMDMatrix matrix, out MMDVector3 translation)
        {
            translation = new MMDVector3(matrix.M41, matrix.M42, matrix.M43);
        }

        // ---- BulletX conversion helpers ----

        /// <summary>
        /// マトリクスからBullet用Transformを生成
        /// </summary>
        public static void TobtTransform(ref MMDMatrix matrix, out BulletX.LinerMath.btTransform bttransform)
        {
            BulletX.LinerMath.btMatrix3x3 btm;
            TobtMatrix3x3(ref matrix, out btm);
            bttransform = new BulletX.LinerMath.btTransform(btm, new BulletX.LinerMath.btVector3(matrix.M41, matrix.M42, matrix.M43));
        }

        /// <summary>
        /// マトリクスからBullet用Matrix3x3を生成
        /// </summary>
        public static void TobtMatrix3x3(ref MMDMatrix m, out BulletX.LinerMath.btMatrix3x3 btmatrix)
        {
            btmatrix = new BulletX.LinerMath.btMatrix3x3(
                m.M11, m.M21, m.M31,
                m.M12, m.M22, m.M32,
                m.M13, m.M23, m.M33);
        }

        /// <summary>
        /// Bullet用Transformからマトリクスを生成
        /// </summary>
        public static MMDMatrix ToMatrix(BulletX.LinerMath.btTransform btTransform)
        {
            MMDMatrix m;
            ToMatrix(ref btTransform, out m);
            return m;
        }

        /// <summary>
        /// Bullet用Transformからマトリクスを生成 (ref)
        /// </summary>
        public static void ToMatrix(ref BulletX.LinerMath.btTransform btTransform, out MMDMatrix m)
        {
            ToMatrix(ref btTransform.Basis, out m);
            m.M41 = btTransform.Origin.X;
            m.M42 = btTransform.Origin.Y;
            m.M43 = btTransform.Origin.Z;
            m.M44 = 1;
        }

        /// <summary>
        /// Bullet用Matrix3x3からマトリクスを生成
        /// </summary>
        public static void ToMatrix(ref BulletX.LinerMath.btMatrix3x3 btm, out MMDMatrix m)
        {
            m = MMDMatrix.Identity;
            m.M11 = btm.el0.X; m.M12 = btm.el1.X; m.M13 = btm.el2.X; m.M14 = 0;
            m.M21 = btm.el0.Y; m.M22 = btm.el1.Y; m.M23 = btm.el2.Y; m.M24 = 0;
            m.M31 = btm.el0.Z; m.M32 = btm.el1.Z; m.M33 = btm.el2.Z; m.M34 = 0;
        }

        // ---- Camera/Perspective (delegates to MMD types) ----

        public static void CreateLookAtMatrix(ref MMDVector3 cameraPosition, ref MMDVector3 cameraTarget, ref MMDVector3 cameraUpVector, out MMDMatrix result)
        {
            MMDMatrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out result);
        }

        public static void CreatePerspectiveFieldOfViewMatrix(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out MMDMatrix result)
        {
            MMDMatrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out result);
        }

        // ---- Quaternion/Matrix factory methods (delegates to MMD types) ----

        public static MMDQuaternion CreateQuaternionFromAxisAngle(MMDVector3 axis, float angle)
        {
            return MMDQuaternion.CreateFromAxisAngle(axis, angle);
        }

        public static MMDMatrix CreateTranslationMatrix(float xPosition, float yPosition, float zPosition)
        {
            return MMDMatrix.CreateTranslation(xPosition, yPosition, zPosition);
        }

        public static MMDMatrix CreateMatrixFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            return MMDMatrix.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        public static MMDMatrix CreateRotationXMatrix(float radians)
        {
            return MMDMatrix.CreateRotationX(radians);
        }

        public static MMDMatrix CreateRotationYMatrix(float radians)
        {
            return MMDMatrix.CreateRotationY(radians);
        }

        public static MMDMatrix CreateRotationZMatrix(float radians)
        {
            return MMDMatrix.CreateRotationZ(radians);
        }

        public static MMDQuaternion CreateQuaternionFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            return MMDQuaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        public static MMDQuaternion CreateQuaternionFromRotationMatrix(MMDMatrix matrix)
        {
            return MMDQuaternion.CreateFromRotationMatrix(matrix);
        }

        public static void CreateMatrixFromQuaternion(ref MMDQuaternion quaternion, out MMDMatrix result)
        {
            MMDMatrix.CreateFromQuaternion(ref quaternion, out result);
        }

        public static void CreateTranslationMatrix(ref MMDVector3 position, out MMDMatrix result)
        {
            MMDMatrix.CreateTranslation(ref position, out result);
        }

        public static void CreateScaleMatrix(ref MMDVector3 scales, out MMDMatrix result)
        {
            MMDMatrix.CreateScale(ref scales, out result);
        }
    }
}
