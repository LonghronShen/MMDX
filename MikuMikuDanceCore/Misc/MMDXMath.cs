using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if XNA
using Microsoft.Xna.Framework;
#else
using SlimDX;
#endif
#if !XNA
using System.Drawing;
#endif
namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// MMD用数学クラス
    /// </summary>
    public static class MMDXMath
    {
        /// <summary>
        /// Vector2に変換
        /// </summary>
        public static Vector2 ToVector2(float[] vec)
        {
            return new Vector2(vec[0], vec[1]);
        }
        /// <summary>
        /// Vector3に変換
        /// </summary>
        public static Vector3 ToVector3(float[] vec)
        {
            return new Vector3(vec[0], vec[1], vec[2]);
        }
        /// <summary>
        /// Vector4に変換
        /// </summary>
        public static Vector4 ToVector4(float[] vec)
        {
            return new Vector4(vec[0], vec[1], vec[2], vec[3]);
        }
        /// <summary>
        /// swap関数
        /// </summary>
        /// <typeparam name="T">スワップする型</typeparam>
        /// <param name="v1">変数1</param>
        /// <param name="v2">変数2</param>
        public static void Swap<T>(ref T v1, ref T v2)
        {
            T v3 = v1;
            v1 = v2;
            v2 = v3;
        }
        /// <summary>
        /// MinMax関係が成り立つように各要素を修正
        /// </summary>
        /// <param name="min">最小</param>
        /// <param name="max">最大</param>
        public static void CheckMinMax(float[] min, float[] max)
        {
            for (int i = 0; i < min.Length && i < max.Length; i++)
            {
                if (min[i] > max[i])
                    Swap(ref min[i], ref max[i]);
            }
        }
        /// <summary>
        /// クォータニオンをYaw(Y回転), Pitch(X回転), Roll(Z回転)に分解する関数
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="ZRot">Z軸回転</param>
        /// <param name="XRot">X軸回転(-PI/2～PI/2)</param>
        /// <param name="YRot">Y軸回転</param>
        /// <returns>ジンバルロックが発生した時はfalse。ジンバルロックはX軸回転で発生</returns>
        public static bool FactoringQuaternionZXY(Quaternion input, out float ZRot, out float XRot, out float YRot)
        {
            //クォータニオンの正規化
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //マトリクスを生成する
            Matrix rot;
            MMDXMath.CreateMatrixFromQuaternion(ref inputQ, out rot);
            //ヨー(X軸周りの回転)を取得
            if (rot.M32 > 1 - 1.0e-4 || rot.M32 < -1 + 1.0e-4)
            {//ジンバルロック判定
                XRot = (rot.M32 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = 0; YRot = (float)Math.Atan2(-rot.M13, rot.M11);
                return false;
            }
            XRot = -(float)Math.Asin(rot.M32);
            //ロールを取得
            ZRot = (float)Math.Asin(rot.M12 / Math.Cos(XRot));
            if (float.IsNaN(ZRot))
            {//漏れ対策
                XRot = (rot.M32 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = 0; YRot = (float)Math.Atan2(-rot.M13, rot.M11);
                return false;
            }
            if (rot.M22 < 0)
                ZRot = MathHelper.Pi - ZRot;
            //ピッチを取得
            YRot = (float)Math.Atan2(rot.M31, rot.M33);
            return true;
        }

        
        /// <summary>
        /// クォータニオンをX,Y,Z回転に分解する関数
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="XRot">X軸回転</param>
        /// <param name="YRot">Y軸回転(-PI/2～PI/2)</param>
        /// <param name="ZRot">Z軸回転</param>
        /// <returns></returns>
        public static bool FactoringQuaternionXYZ(Quaternion input, out float XRot, out float YRot, out float ZRot)
        {
            //クォータニオンの正規化
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //マトリクスを生成する
            Matrix rot;
            MMDXMath.CreateMatrixFromQuaternion(ref inputQ, out rot);
            //Y軸回りの回転を取得
            if (rot.M13 > 1 - 1.0e-4 || rot.M13 < -1 + 1.0e-4)
            {//ジンバルロック判定
                XRot = 0;
                YRot = (rot.M13 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = -(float)Math.Atan2(-rot.M21, rot.M22);
                return false;
            }
            YRot = -(float)Math.Asin(rot.M13);
            //X軸回りの回転を取得
            XRot = (float)Math.Asin(rot.M23 / Math.Cos(YRot));
            if (float.IsNaN(XRot))
            {//ジンバルロック判定(漏れ対策)
                XRot = 0;
                YRot = (rot.M13 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                ZRot = -(float)Math.Atan2(-rot.M21, rot.M22);
                return false;
            }
            if (rot.M33 < 0)
                XRot = MathHelper.Pi - XRot;
            //Z軸回りの回転を取得
            ZRot = (float)Math.Atan2(rot.M12, rot.M11);
            return true;
        }
        /// <summary>
        /// クォータニオンをY,Z,X回転に分解する関数
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="YRot">Y軸回転</param>
        /// <param name="ZRot">Z軸回転(-PI/2～PI/2)</param>
        /// <param name="XRot">X軸回転</param>
        /// <returns></returns>
        public static bool FactoringQuaternionYZX(Quaternion input, out float YRot, out float ZRot, out float XRot)
        {
            //クォータニオンの正規化
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //マトリクスを生成する
            Matrix rot;
            MMDXMath.CreateMatrixFromQuaternion(ref inputQ, out rot);
            //Z軸回りの回転を取得
            if (rot.M21 > 1 - 1.0e-4 || rot.M21 < -1 + 1.0e-4)
            {//ジンバルロック判定
                YRot = 0;
                ZRot = (rot.M21 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                XRot = -(float)Math.Atan2(-rot.M32, rot.M33);
                return false;
            }
            ZRot = -(float)Math.Asin(rot.M21);
            //Y軸回りの回転を取得
            YRot = (float)Math.Asin(rot.M31 / Math.Cos(ZRot));
            if (float.IsNaN(YRot))
            {//ジンバルロック判定(漏れ対策)
                YRot = 0;
                ZRot = (rot.M21 < 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                XRot = -(float)Math.Atan2(-rot.M32, rot.M33);
                return false;
            }
            if (rot.M11 < 0)
                YRot = MathHelper.Pi - YRot;
            //X軸回りの回転を取得
            XRot = (float)Math.Atan2(rot.M23, rot.M22);
            return true;
        }
        /// <summary>
        /// NaNチェック
        /// </summary>
        /// <param name="vec">チェック対象</param>
        /// <returns>NaNが含まれていればtrue</returns>
        public static bool CheckNaN(Vector3 vec)
        {
            return float.IsNaN(vec.X) || float.IsNaN(vec.Y) || float.IsNaN(vec.Z);
        }
        /// <summary>
        /// NaNチェック
        /// </summary>
        /// <param name="mat">チェック対象</param>
        /// <returns>NaNが含まれていればtrue</returns>
        public static bool CheckNaN(Matrix mat)
        {
            return float.IsNaN(mat.M11) ||
                float.IsNaN(mat.M12) ||
                float.IsNaN(mat.M13) ||
                float.IsNaN(mat.M14) ||
                float.IsNaN(mat.M21) ||
                float.IsNaN(mat.M22) ||
                float.IsNaN(mat.M23) ||
                float.IsNaN(mat.M24) ||
                float.IsNaN(mat.M31) ||
                float.IsNaN(mat.M32) ||
                float.IsNaN(mat.M33) ||
                float.IsNaN(mat.M34) ||
                float.IsNaN(mat.M41) ||
                float.IsNaN(mat.M42) ||
                float.IsNaN(mat.M43) ||
                float.IsNaN(mat.M44);
        }
        /// <summary>
        /// Colorを生成
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <returns>Colorオブジェクト</returns>
        public static Color CreateColor(int r, int g, int b)
        {
#if XNA
            return new Color(r, g, b);
#else
            return Color.FromArgb(r, g, b);
#endif
        }
        /// <summary>
        /// マトリクスより移動ベクトルをを生成
        /// </summary>
        /// <param name="matrix">マトリクス</param>
        /// <param name="translation">移動ベクトル</param>
        public static void GetTranslation(ref Matrix matrix, out Vector3 translation)
        {
#if XNA
            translation = matrix.Translation;
#else
            translation = new Vector3(matrix.M41, matrix.M42, matrix.M43);
#endif
        }
        /// <summary>
        /// マトリクスからBullet用マトリクス(btTransform)を生成
        /// </summary>
        /// <param name="matrix">マトリクス</param>
        /// <param name="bttransform">Bullet用マトリクス</param>
        public static void TobtTransform(ref Matrix matrix, out BulletX.LinerMath.btTransform bttransform)
        {
            BulletX.LinerMath.btMatrix3x3 btm;
            TobtMatrix3x3(ref matrix,out btm);
            bttransform = new BulletX.LinerMath.btTransform(btm, new BulletX.LinerMath.btVector3(matrix.M41, matrix.M42, matrix.M43));
        }
        /// <summary>
        /// マトリクスからBullet用マトリクス(btMatrix3x3)を生成
        /// </summary>
        /// <param name="m">マトリクス</param>
        /// <param name="btmatrix">Bullet用マトリクス</param>
        public static void TobtMatrix3x3(ref Matrix m, out BulletX.LinerMath.btMatrix3x3 btmatrix)
        {
            btmatrix = new BulletX.LinerMath.btMatrix3x3(
                m.M11, m.M21, m.M31,
                m.M12, m.M22, m.M32,
                m.M13, m.M23, m.M33);
        }
        /// <summary>
        /// Bullet用マトリクス(btTransform)からマトリクスを生成
        /// </summary>
        /// <param name="btTransform">Bullet用マトリクス</param>
        /// <returns>マトリクス</returns>
        public static Matrix ToMatrix(BulletX.LinerMath.btTransform btTransform)
        {
            Matrix m;
            ToMatrix(ref btTransform, out m);
            return m;
        }
        /// <summary>
        /// Bullet用マトリクス(btTransform)からマトリクスを生成
        /// </summary>
        /// <param name="btTransform">Bullet用マトリクス</param>
        /// <param name="m">マトリクス</param>
        public static void ToMatrix(ref BulletX.LinerMath.btTransform btTransform,out Matrix m)
        {
            ToMatrix(ref  btTransform.Basis, out m);
            m.M41 = btTransform.Origin.X;
            m.M42 = btTransform.Origin.Y;
            m.M43 = btTransform.Origin.Z;
            m.M44 = 1;
        }
        /// <summary>
        /// Bullet用マトリクス(btMatrix3x3)からマトリクスを生成
        /// </summary>
        /// <param name="btm">Bullet用マトリクス</param>
        /// <param name="m">マトリクス</param>
        public static void ToMatrix(ref BulletX.LinerMath.btMatrix3x3 btm, out Matrix m)
        {
            m = Matrix.Identity;
            m.M11 = btm.el0.X;
            m.M12 = btm.el1.X;
            m.M13 = btm.el2.X;
            m.M14 = 0;
            m.M21 = btm.el0.Y;
            m.M22 = btm.el1.Y;
            m.M23 = btm.el2.Y;
            m.M24 = 0;
            m.M31 = btm.el0.Z;
            m.M32 = btm.el1.Z;
            m.M33 = btm.el2.Z;
            m.M34 = 0;
        }
        /// <summary>
        /// カメラ用マトリクスの生成
        /// </summary>
        /// <param name="cameraPosition">カメラの位置</param>
        /// <param name="cameraTarget">カメラ目標</param>
        /// <param name="cameraUpVector">カメラの上方向</param>
        /// <param name="result">マトリクス</param>
        public static void CreateLookAtMatrix(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix result)
        {
#if XNA
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out result);
#elif SlimDX
            Matrix.LookAtRH(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out result);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// プロジェクションマトリクスの生成
        /// </summary>
        /// <param name="fieldOfView">視野角</param>
        /// <param name="aspectRatio">アスペクト比</param>
        /// <param name="nearPlaneDistance">近くのビュープレーンの距離</param>
        /// <param name="farPlaneDistance">遠くのファープレーンの距離</param>
        /// <param name="result">マトリクス</param>
        public static void CreatePerspectiveFieldOfViewMatrix(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
#if XNA
            Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out result);
#elif SlimDX
            Matrix.PerspectiveFovRH(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out result);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// 軸と回転からクォータニオンを生成
        /// </summary>
        /// <param name="axis">軸</param>
        /// <param name="angle">回転</param>
        /// <returns>クォータニオン</returns>
        public static Quaternion CreateQuaternionFromAxisAngle(Vector3 axis, float angle)
        {
#if XNA
            return Quaternion.CreateFromAxisAngle(axis, angle);
#elif SlimDX
            return Quaternion.RotationAxis(axis, angle);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// 移動マトリクスの生成
        /// </summary>
        /// <param name="xPosition">x</param>
        /// <param name="yPosition">y</param>
        /// <param name="zPosition">z</param>
        /// <returns>移動マトリクス</returns>
        public static Matrix CreateTranslationMatrix(float xPosition, float yPosition, float zPosition)
        {
#if XNA
            return Matrix.CreateTranslation(xPosition, yPosition, zPosition);
#elif SlimDX
            return Matrix.Translation(xPosition, yPosition, zPosition);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// ヨー・ピッチ・ロールから回転マトリクスを生成
        /// </summary>
        /// <param name="yaw">ヨー</param>
        /// <param name="pitch">ピッチ</param>
        /// <param name="roll">ロール</param>
        /// <returns>回転マトリクス</returns>
        public static Matrix CreateMatrixFromYawPitchRoll(float yaw, float pitch, float roll)
        {
#if XNA
            return Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
#elif SlimDX
            return Matrix.RotationYawPitchRoll(yaw, pitch, roll);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// X回転マトリクスを生成
        /// </summary>
        /// <param name="radians">回転量</param>
        /// <returns>回転マトリクス</returns>
        public static Matrix CreateRotationXMatrix(float radians)
        {
#if XNA
            return Matrix.CreateRotationX(radians);
#elif SlimDX
            return Matrix.RotationX(radians);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// Y回転マトリクス
        /// </summary>
        /// <param name="radians">回転量</param>
        /// <returns>回転マトリクス</returns>
        public static Matrix CreateRotationYMatrix(float radians)
        {
#if XNA
            return Matrix.CreateRotationY(radians);
#elif SlimDX
            return Matrix.RotationY(radians);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// Z回転マトリクス
        /// </summary>
        /// <param name="radians">回転量</param>
        /// <returns>回転マトリクス</returns>
        public static Matrix CreateRotationZMatrix(float radians)
        {
#if XNA
            return Matrix.CreateRotationZ(radians);
#elif SlimDX
            return Matrix.RotationZ(radians);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// ヨー・ピッチ・ロールからクォータニオンを生成
        /// </summary>
        /// <param name="yaw">ヨー</param>
        /// <param name="pitch">ピッチ</param>
        /// <param name="roll">ロール</param>
        /// <returns>クォータニオン</returns>
        public static Quaternion CreateQuaternionFromYawPitchRoll(float yaw, float pitch, float roll)
        {
#if XNA
            return Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
#elif SlimDX
            return Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// 回転マトリクスからクォータニオンを生成
        /// </summary>
        /// <param name="matrix">マトリクス</param>
        /// <returns>クォータニオン</returns>
        public static Quaternion CreateQuaternionFromRotationMatrix(Matrix matrix)
        {
#if XNA
            return Quaternion.CreateFromRotationMatrix(matrix);
#elif SlimDX
            return Quaternion.RotationMatrix(matrix);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// クォータニオンから回転マトリクスを生成
        /// </summary>
        /// <param name="quaternion">クォータニオン</param>
        /// <param name="result">回転マトリクス</param>
        public static void CreateMatrixFromQuaternion(ref Quaternion quaternion, out Matrix result)
        {
#if XNA
            Matrix.CreateFromQuaternion(ref quaternion, out result);
#elif SlimDX
            Matrix.RotationQuaternion(ref quaternion, out result);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// 移動マトリクスを生成
        /// </summary>
        /// <param name="position">移動ベクトル</param>
        /// <param name="result">マトリクス</param>
        public static void CreateTranslationMatrix(ref Vector3 position, out Matrix result)
        {
#if XNA
            Matrix.CreateTranslation(ref position, out result);
#elif SlimDX
            Matrix.Translation(ref position, out result);
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// スケーリングマトリクスを生成
        /// </summary>
        /// <param name="scales">スケールベクトル</param>
        /// <param name="result">マトリクス</param>
        public static void CreateScaleMatrix(ref Vector3 scales, out Matrix result)
        {
#if XNA
            Matrix.CreateScale(ref scales, out result);
#elif SlimDX
            Matrix.Scaling(ref scales, out result);
#else
            throw new NotImplementedException();
#endif
        }
    }
}
