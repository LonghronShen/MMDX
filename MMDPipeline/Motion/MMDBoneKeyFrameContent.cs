using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DWORD = System.UInt32;
using Microsoft.Xna.Framework;
using MikuMikuDance.XNA.Misc;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Motion
{
    /// <summary>
    /// MMD用ボーンキーフレーム
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Motion.MMDBoneKeyFrame, MikuMikuDanceCore")]
    public class MMDBoneKeyFrameContent
    {
        /// <summary>
        /// ボーン名
        /// </summary>
        public string BoneName;//[15];
        /// <summary>
        /// フレーム番号
        /// </summary>
        public DWORD FrameNo;
        /// <summary>
        /// スケールベクトル
        /// </summary>
        public Vector3 Scales;
        /// <summary>
        /// 位置ベクトル
        /// </summary>
        public Vector3 Location;
        /// <summary>
        /// クォータニオン
        /// </summary>
        public Quaternion Quatanion;
        /// <summary>
        /// 補完用曲線
        /// </summary>
        /// <remarks>順にX,Y,Z,回転</remarks>
        public BezierCurveContent[] Curve;
        
    }
}
