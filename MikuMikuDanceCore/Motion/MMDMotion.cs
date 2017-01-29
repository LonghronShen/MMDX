using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// MMDモーションデータ
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    public class MMDMotion
    {
        /// <summary>
        /// ボーンモーションデータ
        /// </summary>
        /// <remarks>ボーンごとに時系列順</remarks>
        public Dictionary<string, List<MMDBoneKeyFrame>> BoneFrames;
        /// <summary>
        /// フェイスモーションデータ
        /// </summary>
        /// <remarks>表情ごとに時系列順</remarks>
        public Dictionary<string, List<MMDFaceKeyFrame>> FaceFrames;
        /// <summary>
        /// カメラモーションデータ
        /// </summary>
        public List<MMDCameraKeyFrame> CameraFrames;
        /// <summary>
        /// ライトモーションデータ
        /// </summary>
        public List<MMDLightKeyFrame> LightFrames;
    }
}
