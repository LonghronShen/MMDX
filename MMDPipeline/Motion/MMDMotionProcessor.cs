using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using TInput = MikuMikuDance.Motion.Motion2.MMDMotion2;
using System.ComponentModel;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Motion
{
    /// <summary>
    /// MikuMikuDanceのモーションデータデータをXNAにインポートするためのプロセッサ
    /// </summary>
    [ContentProcessor(DisplayName = "MikuMikuDanceモーション : MikuMikuDance for XNA")]
    public class MMDMotionProcessor : ContentProcessor<TInput, MMDMotionContent>
    {
        
        /// <summary>
        /// コンテンツ変換
        /// </summary>
        /// <param name="input">MMDモーションデータ</param>
        /// <param name="context">コンテントプロセッサ</param>
        /// <returns>MMDモーションデータ for XNA</returns>
        public override MMDMotionContent Process(TInput input, ContentProcessorContext context)
        {
            MMDMotionContent result = new MMDMotionContent();
            //ボーンモーションデータの変換
            MMDBoneKeyFrameContent[] BoneFrames = new MMDBoneKeyFrameContent[input.Motions.LongLength];
            if (input.Motions.LongLength > int.MaxValue)
                throw new InvalidContentException("ボーンモーション数が多すぎます。MikuMikuDanceXNAは.NET Compact Frameworkの制限のため、32bit符号付き整数最大値までしかモーション数をサポートしていません");
            for (long i = 0; i < input.Motions.LongLength; i++)
            {
                BoneFrames[i] = new MMDBoneKeyFrameContent();
                BoneFrames[i].BoneName = input.Motions[i].BoneName;
                BoneFrames[i].FrameNo = input.Motions[i].FrameNo;

                BoneFrames[i].Curve = new BezierCurveContent[4];
                for (int j = 0; j < BoneFrames[i].Curve.Length; j++)
                {
                    BezierCurveContent curve = new BezierCurveContent();
                    curve.v1 = new Vector2((float)input.Motions[i].Interpolation[0][0][j] / 128f, (float)input.Motions[i].Interpolation[0][1][j] / 128f);
                    curve.v2 = new Vector2((float)input.Motions[i].Interpolation[0][2][j] / 128f, (float)input.Motions[i].Interpolation[0][3][j] / 128f);
                    BoneFrames[i].Curve[j] = curve;
                }
                BoneFrames[i].Scales = Vector3.One;
                BoneFrames[i].Location = new Vector3(input.Motions[i].Location[0], input.Motions[i].Location[1], input.Motions[i].Location[2]);
                BoneFrames[i].Quatanion = new Quaternion(input.Motions[i].Quatanion[0], input.Motions[i].Quatanion[1], input.Motions[i].Quatanion[2], input.Motions[i].Quatanion[3]);
                BoneFrames[i].Quatanion.Normalize();
            }
            result.BoneFrames = MotionHelper.SplitBoneMotion(BoneFrames);
            //表情モーションの変換
            MMDFaceKeyFrameContent[] FaceFrames = new MMDFaceKeyFrameContent[input.FaceMotions.LongLength];
            if (FaceFrames.LongLength > int.MaxValue)
                throw new InvalidContentException("表情モーション数が多すぎます。MikuMikuDanceXNAは.NET Compact Frameworkの制限のため、32bit符号付き整数最大値までしかモーション数をサポートしていません");
            for (long i = 0; i < input.FaceMotions.Length; i++)
            {
                FaceFrames[i] = new MMDFaceKeyFrameContent();
                FaceFrames[i].Rate = input.FaceMotions[i].Rate;
                FaceFrames[i].FaceName = input.FaceMotions[i].FaceName;
                FaceFrames[i].FrameNo = input.FaceMotions[i].FrameNo;
                float temp = input.FaceMotions[i].FrameNo;
            }
            result.FaceFrames = MotionHelper.SplitFaceMotion(FaceFrames);
            //カメラモーションの変換
            MMDCameraKeyFrameContent[] CameraFrames = new MMDCameraKeyFrameContent[input.CameraMotions.LongLength];
            if (CameraFrames.LongLength > int.MaxValue)
                throw new InvalidContentException("カメラモーション数が多すぎます。MikuMikuDanceXNAは.NET Compact Frameworkの制限のため、32bit符号付き整数最大値までしかモーション数をサポートしていません");
            for (long i = 0; i < input.CameraMotions.Length; i++)
            {
                CameraFrames[i] = new MMDCameraKeyFrameContent();
                CameraFrames[i].FrameNo = input.CameraMotions[i].FrameNo;
                CameraFrames[i].Length = input.CameraMotions[i].Length;
                CameraFrames[i].Location = MMDXMath.ToVector3(input.CameraMotions[i].Location);
                CameraFrames[i].Quatanion = Quaternion.CreateFromYawPitchRoll(input.CameraMotions[i].Rotate[1], input.CameraMotions[i].Rotate[0], input.CameraMotions[i].Rotate[2]);
                CameraFrames[i].ViewAngle = MathHelper.ToRadians(input.CameraMotions[i].ViewingAngle);
                CameraFrames[i].Curve = new BezierCurveContent[6];
                for (int j = 0; j < CameraFrames[i].Curve.Length; j++)
                {
                    BezierCurveContent curve = new BezierCurveContent();
                    curve.v1 = new Vector2((float)input.CameraMotions[i].Interpolation[j][0] / 128f, (float)input.CameraMotions[i].Interpolation[j][2] / 128f);
                    curve.v2 = new Vector2((float)input.CameraMotions[i].Interpolation[j][1] / 128f, (float)input.CameraMotions[i].Interpolation[j][3] / 128f);
                    CameraFrames[i].Curve[j] = curve;
                }
            }
            result.CameraFrames = new List<MMDCameraKeyFrameContent>(CameraFrames);
            //ライトモーションの変換
            MMDLightKeyFrameContent[] LightFrames = new MMDLightKeyFrameContent[input.LightMotions.LongLength];
            if (LightFrames.LongLength > int.MaxValue)
                throw new InvalidContentException("ライトモーション数が多すぎます。MikuMikuDanceXNAは.NET Compact Frameworkの制限のため、32bit符号付き整数最大値までしかモーション数をサポートしていません");
            for (long i = 0; i < input.LightMotions.Length; i++)
            {
                LightFrames[i] = new MMDLightKeyFrameContent();
                LightFrames[i].FrameNo = input.LightMotions[i].FrameNo;
                LightFrames[i].Color = MMDXMath.ToVector3(input.LightMotions[i].Color);
                LightFrames[i].Location = MMDXMath.ToVector3(input.LightMotions[i].Location);
            }
            result.LightFrames = new List<MMDLightKeyFrameContent>(LightFrames);
            //XNA用に変換したデータを返却
            return result;
        }
    }
}
