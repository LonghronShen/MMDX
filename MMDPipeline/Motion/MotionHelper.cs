using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.XNA.Motion
{
    static class MotionHelper
    {

        internal static Dictionary<string, List<MMDBoneKeyFrameContent>> SplitBoneMotion(MMDBoneKeyFrameContent[] keyframes)
        {
            Dictionary<string, List<MMDBoneKeyFrameContent>> result = new Dictionary<string, List<MMDBoneKeyFrameContent>>();
            foreach (var keyframe in keyframes)
            {
                if (!result.ContainsKey(keyframe.BoneName))
                    result.Add(keyframe.BoneName, new List<MMDBoneKeyFrameContent>());
                result[keyframe.BoneName].Add(keyframe);
            }
            foreach (var boneframes in result)
            {
                boneframes.Value.Sort((x, y) => (int)((long)x.FrameNo - (long)y.FrameNo));
            }
            return result;
        }

        internal static Dictionary<string, List<MMDFaceKeyFrameContent>> SplitFaceMotion(MMDFaceKeyFrameContent[] keyframes)
        {
            Dictionary<string, List<MMDFaceKeyFrameContent>> result = new Dictionary<string, List<MMDFaceKeyFrameContent>>();
            foreach (var keyframe in keyframes)
            {
                if (!result.ContainsKey(keyframe.FaceName))
                    result.Add(keyframe.FaceName, new List<MMDFaceKeyFrameContent>());
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
