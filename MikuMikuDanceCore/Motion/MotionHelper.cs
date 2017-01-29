using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Motion
{
    static class MotionHelper
    {

        internal static Dictionary<string, List<MMDBoneKeyFrame>> SplitBoneMotion(MMDBoneKeyFrame[] keyframes)
        {
            Dictionary<string, List<MMDBoneKeyFrame>> result = new Dictionary<string, List<MMDBoneKeyFrame>>();
            foreach (var keyframe in keyframes)
            {
                if (!result.ContainsKey(keyframe.BoneName))
                    result.Add(keyframe.BoneName, new List<MMDBoneKeyFrame>());
                result[keyframe.BoneName].Add(keyframe);
            }
            foreach (var boneframes in result)
            {
                boneframes.Value.Sort((x, y) => (int)((long)x.FrameNo - (long)y.FrameNo));
            }
            return result;
        }

        internal static Dictionary<string, List<MMDFaceKeyFrame>> SplitFaceMotion(MMDFaceKeyFrame[] keyframes)
        {
            Dictionary<string, List<MMDFaceKeyFrame>> result = new Dictionary<string, List<MMDFaceKeyFrame>>();
            foreach (var keyframe in keyframes)
            {
                if (!result.ContainsKey(keyframe.FaceName))
                    result.Add(keyframe.FaceName, new List<MMDFaceKeyFrame>());
                result[keyframe.FaceName].Add(keyframe);
            }
            foreach (var boneframes in result)
            {
                boneframes.Value.Sort((x, y) => (int)((long)x.FrameNo - (long)y.FrameNo));
            }
            return result;
        }
    }
}

