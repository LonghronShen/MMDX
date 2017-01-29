using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Motion
{
    /// <summary>
    /// MMD用カメラキーフレーム
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Motion.MMDCameraKeyFrame, MikuMikuDanceCore")]
    public struct MMDCameraKeyFrameContent
    {
        /// <summary>
        /// フレーム番号
        /// </summary>
        public uint FrameNo;
        /// <summary>
        /// 距離
        /// </summary>
        public float Length;
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Location;
        /// <summary>
        /// 回転
        /// </summary>
        public Quaternion Quatanion;
        /// <summary>
        /// 補完用曲線
        /// </summary>
        /// <remarks>順にX,Y,Z,回転,距離,視野角</remarks>
        public BezierCurveContent[] Curve;
        /// <summary>
        /// 視野角
        /// </summary>
        public float ViewAngle;
    }
}
