using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !XBOX
using MikuMikuDance.Motion;
#endif
using System.IO;
using MikuMikuDance.Core.Misc;
#if XNA
using Microsoft.Xna.Framework;
#else
using SlimDX;
#endif

namespace MikuMikuDance.Core.Motion
{
#if !XBOX
    class MMDMotionFactory : IMMDMotionFactory
    {

        #region IMMDMotionFactory メンバー
        public MMDMotion Load(string filename, float scale)
        {
            filename = Path.GetFullPath(filename);
            MikuMikuDance.Motion.Motion2.MMDMotion2 input;
            input = MotionManager.Read(filename, CoordinateType.RightHandedCoordinate, scale) as MikuMikuDance.Motion.Motion2.MMDMotion2;
            if (input == null)
                return null;
            MMDMotion result = new MMDMotion();
            //ボーンモーションデータの変換
            MMDBoneKeyFrame[] BoneFrames = new MMDBoneKeyFrame[input.Motions.LongLength];
            for (long i = 0; i < input.Motions.LongLength; i++)
            {
                BoneFrames[i] = new MMDBoneKeyFrame();
                BoneFrames[i].BoneName = input.Motions[i].BoneName;
                BoneFrames[i].FrameNo = input.Motions[i].FrameNo;

                BoneFrames[i].Curve = new BezierCurve[4];
                for (int j = 0; j < BoneFrames[i].Curve.Length; j++)
                {
                    BezierCurve curve = new BezierCurve();
                    curve.v1 = new Vector2((float)input.Motions[i].Interpolation[0][0][j] / 128f, (float)input.Motions[i].Interpolation[0][1][j] / 128f);
                    curve.v2 = new Vector2((float)input.Motions[i].Interpolation[0][2][j] / 128f, (float)input.Motions[i].Interpolation[0][3][j] / 128f);
                    BoneFrames[i].Curve[j] = curve;
                }
                BoneFrames[i].Scales = new Vector3(1, 1, 1);
                BoneFrames[i].Location = new Vector3(input.Motions[i].Location[0], input.Motions[i].Location[1], input.Motions[i].Location[2]);
                BoneFrames[i].Quatanion = new Quaternion(input.Motions[i].Quatanion[0], input.Motions[i].Quatanion[1], input.Motions[i].Quatanion[2], input.Motions[i].Quatanion[3]);
                BoneFrames[i].Quatanion.Normalize();
            }
            result.BoneFrames = MotionHelper.SplitBoneMotion(BoneFrames);
            //表情モーションの変換
            MMDFaceKeyFrame[] FaceFrames = new MMDFaceKeyFrame[input.FaceMotions.LongLength];
            for (long i = 0; i < input.FaceMotions.Length; i++)
            {
                FaceFrames[i] = new MMDFaceKeyFrame();
                FaceFrames[i].Rate = input.FaceMotions[i].Rate;
                FaceFrames[i].FaceName = input.FaceMotions[i].FaceName;
                FaceFrames[i].FrameNo = input.FaceMotions[i].FrameNo;
                float temp = input.FaceMotions[i].FrameNo;
            }
            result.FaceFrames = MotionHelper.SplitFaceMotion(FaceFrames);
            //カメラモーションの変換
            MMDCameraKeyFrame[] CameraFrames = new MMDCameraKeyFrame[input.CameraMotions.LongLength];
            for (long i = 0; i < input.CameraMotions.Length; i++)
            {
                CameraFrames[i] = new MMDCameraKeyFrame();
                CameraFrames[i].FrameNo = input.CameraMotions[i].FrameNo;
                CameraFrames[i].Length = input.CameraMotions[i].Length;
                CameraFrames[i].Location = MMDXMath.ToVector3(input.CameraMotions[i].Location);
                CameraFrames[i].Quatanion = MMDXMath.CreateQuaternionFromYawPitchRoll(input.CameraMotions[i].Rotate[1], input.CameraMotions[i].Rotate[0], input.CameraMotions[i].Rotate[2]);
                CameraFrames[i].ViewAngle = MathHelper.ToRadians(input.CameraMotions[i].ViewingAngle);
                CameraFrames[i].Curve = new BezierCurve[6];
                for (int j = 0; j < CameraFrames[i].Curve.Length; j++)
                {
                    BezierCurve curve = new BezierCurve();
                    curve.v1 = new Vector2((float)input.CameraMotions[i].Interpolation[j][0] / 128f, (float)input.CameraMotions[i].Interpolation[j][2] / 128f);
                    curve.v2 = new Vector2((float)input.CameraMotions[i].Interpolation[j][1] / 128f, (float)input.CameraMotions[i].Interpolation[j][3] / 128f);
                    CameraFrames[i].Curve[j] = curve;
                }
            }
            result.CameraFrames = new List<MMDCameraKeyFrame>(CameraFrames);
            //ライトモーションの変換
            MMDLightKeyFrame[] LightFrames = new MMDLightKeyFrame[input.LightMotions.LongLength];
            for (long i = 0; i < input.LightMotions.Length; i++)
            {
                LightFrames[i] = new MMDLightKeyFrame();
                LightFrames[i].FrameNo = input.LightMotions[i].FrameNo;
                LightFrames[i].Color = MMDXMath.ToVector3(input.LightMotions[i].Color);
                LightFrames[i].Location = MMDXMath.ToVector3(input.LightMotions[i].Location);
            }
            result.LightFrames = new List<MMDLightKeyFrame>(LightFrames);
            //変換したデータを返却
            return result;

        }

        #endregion
    }
#endif
}
