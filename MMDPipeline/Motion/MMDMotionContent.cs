using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Motion
{
    /// <summary>
    /// MMDモーションデータ
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Motion.MMDMotion, MikuMikuDanceCore")]
    public class MMDMotionContent
    {
        /// <summary>
        /// ボーンモーションデータ
        /// </summary>
        /// <remarks>ボーンごとに時系列順</remarks>
        public Dictionary<string, List<MMDBoneKeyFrameContent>> BoneFrames;
        /// <summary>
        /// フェイスモーションデータ
        /// </summary>
        /// <remarks>表情ごとに時系列順</remarks>
        public Dictionary<string, List<MMDFaceKeyFrameContent>> FaceFrames;
        /// <summary>
        /// カメラモーションデータ
        /// </summary>
        public List<MMDCameraKeyFrameContent> CameraFrames;
        /// <summary>
        /// ライトモーションデータ
        /// </summary>
        public List<MMDLightKeyFrameContent> LightFrames;
    }
}
