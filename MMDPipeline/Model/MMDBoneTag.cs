using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// ボーン付属の情報
    /// </summary>
    class MMDBoneTag
    {
        //IK情報
        public List<MMDBoneIKTag> IKs = new List<MMDBoneIKTag>();
    }
    /// <summary>
    /// ボーン付属のIK情報
    /// </summary>
    class MMDBoneIKTag
    {
        /// <summary>
        /// 再帰演算回数
        /// </summary>
        public UInt16 Iteration;
        /// <summary>
        /// IKの影響度
        /// </summary>
        public float ControlWeight;
        /// <summary>
        /// K影響下のボーン番号
        /// </summary>
        public BoneContent[] IKChildBones;
        /// <summary>
        /// IKが最初に接続するボーン(エフェクタをあわせるボーン)
        /// </summary>
        public BoneContent IKTargetBone;
    }
}
