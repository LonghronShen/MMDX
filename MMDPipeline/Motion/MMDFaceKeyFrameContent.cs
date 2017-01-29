using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Motion
{
    /// <summary>
    /// 表情キーフレーム
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Motion.MMDFaceKeyFrame, MikuMikuDanceCore")]
    public class MMDFaceKeyFrameContent
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
    }
}
