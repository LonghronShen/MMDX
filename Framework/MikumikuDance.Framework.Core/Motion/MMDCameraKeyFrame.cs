using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.Core.Stages;


namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// MMD用カメラキーフレーム
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    public struct MMDCameraKeyFrame
    {
        /// <summary>
        /// フレーム番号
        /// </summary>
        public uint FrameNo;
        /// <summary>
        /// 距離
        /// </summary>
        public float Length;
        /// <summary>
        /// 位置
        /// </summary>
        public MMDVector3 Location;
        /// <summary>
        /// 回転
        /// </summary>
        public MMDQuaternion Quatanion;
        /// <summary>
        /// 補完用曲線
        /// </summary>
        /// <remarks>順にX,Y,Z,回転,距離,視野角</remarks>
        public BezierCurve[] Curve;
        /// <summary>
        /// 視野角
        /// </summary>
        public float ViewAngle;


        /// <summary>
        /// カメラフレーム同士の線形補間をカメラに適用
        /// </summary>
        /// <param name="camera1">フレーム1</param>
        /// <param name="camera2">フレーム2</param>
        /// <param name="Progress">進行度合い</param>
        /// <param name="camera">適用するカメラ</param>
        public static void Lerp(MMDCameraKeyFrame camera1, MMDCameraKeyFrame camera2, float Progress, IMMDXCamera camera)
        {
            float ProgX, ProgY, ProgZ, ProgR, ProgD, ProgV;
            ProgX = camera2.Curve[0].Evaluate(Progress);
            ProgY = camera2.Curve[1].Evaluate(Progress);
            ProgZ = camera2.Curve[2].Evaluate(Progress);
            ProgR = camera2.Curve[3].Evaluate(Progress);
            ProgD = camera2.Curve[4].Evaluate(Progress);
            ProgV = camera2.Curve[5].Evaluate(Progress);
            float x, y, z;
            x = MMDMathHelper.Lerp(camera1.Location.X, camera2.Location.X, ProgX);
            y = MMDMathHelper.Lerp(camera1.Location.Y, camera2.Location.Y, ProgY);
            z = MMDMathHelper.Lerp(camera1.Location.Z, camera2.Location.Z, ProgZ);
            //新しいカメラ位置の計算
            float Length = MMDMathHelper.Lerp(camera1.Length, camera2.Length, ProgD);
            MMDQuaternion Rotate = MMDQuaternion.Slerp(camera1.Quatanion, camera2.Quatanion, ProgR);

            camera.SetVector(new MMDVector3(0, 0, Length));

            MMDVector3 temp = new MMDVector3(0, 0, -Length);
            MMDVector3 temp2;
            MMDVector3.Transform(ref temp, ref Rotate, out temp2);
            MMDVector4 temp2;
            MMDVector3.Transform(ref temp, ref Rotate, out temp2);
            camera.Position = new MMDVector3(temp2.X + x, temp2.Y + y, temp2.Z + z);

            camera.SetRotation(Rotate);
            camera.FieldOfView = MMDMathHelper.Lerp(camera1.ViewAngle, camera2.ViewAngle, ProgV);
        }
    }
}
