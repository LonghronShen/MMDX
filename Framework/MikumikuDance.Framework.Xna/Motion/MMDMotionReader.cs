using Microsoft.Xna.Framework.Content;
using MikuMikuDance.Core.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuDance.XNA.Motion
{
    public class MMDMotionReader : ContentTypeReader<MMDMotion>
    {
        /// <summary>
        /// IKデータの読み込み
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        protected override MMDMotion Read(ContentReader input, MMDMotion existingInstance)
        {
            var boneFrames = input.ReadDictionary<string, List<MMDBoneKeyFrame>>();
            var faceFrames = input.ReadDictionary<string, List<MMDFaceKeyFrame>>();

            var cameraFrames = input.ReadObject<List<MMDCameraKeyFrame>>();
            var lightFrames = input.ReadObject<List<MMDLightKeyFrame>>();

            return new MMDMotion()
            {
                BoneFrames = boneFrames,
                FaceFrames = faceFrames,
                CameraFrames = cameraFrames,
                LightFrames = lightFrames,
            };
        }
    }
}
