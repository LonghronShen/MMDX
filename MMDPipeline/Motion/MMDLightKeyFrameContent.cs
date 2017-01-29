using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MikuMikuDance.XNA.Motion
{
    /// <summary>
    /// MMDのライティング用キーフレーム
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Motion.MMDLightKeyFrame, MikuMikuDanceCore")]
    public struct MMDLightKeyFrameContent
    {
        /// <summary>
        /// フレームナンバー
        /// </summary>
        public uint FrameNo;
        /// <summary>
        /// ライトの色
        /// </summary>
        public Vector3 Color;
        /// <summary>
        /// ライトの位置
        /// </summary>
        public Vector3 Location;
        
    }
}
