using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using MikuMikuDance.Core.Misc;
#endif

namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// 表情キーフレーム
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    public class MMDFaceKeyFrame
    {
        /// <summary>
        /// 表情名
        /// </summary>
        public string FaceName;
        /// <summary>
        /// フレーム番号
        /// </summary>
        public uint FrameNo;
        /// <summary>
        /// 表情適応割合
        /// </summary>
        public float Rate;
        /// <summary>
        /// 表情の補完
        /// </summary>
        /// <param name="frame1">フレーム1</param>
        /// <param name="frame2">フレーム2</param>
        /// <param name="progress">進行度合い</param>
        /// <returns>表情適用量</returns>
        public static float Lerp(MMDFaceKeyFrame frame1, MMDFaceKeyFrame frame2, float progress)
        {
            return MathHelper.Lerp(frame1.Rate, frame2.Rate, progress);
        }
    }
}
