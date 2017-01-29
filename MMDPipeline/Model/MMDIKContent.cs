using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDのIK(Inverse Kinematics)データ
    /// </summary>
    public class MMDIKContent
    {
        /// <summary>
        /// 目標位置となるボーン番号
        /// </summary>
        public int IKBoneIndex;
        /// <summary>
        /// エフェクタとなるボーン番号
        /// </summary>
        public int IKTargetBoneIndex;
        /// <summary>
        /// 再帰演算回数
        /// </summary>
        public UInt16 Iteration;
        /// <summary>
        /// IKの影響度
        /// </summary>
        public float ControlWeight;
        /// <summary>
        /// K影響下のボーン
        /// </summary>
        public List<int> IKChildBones = new List<int>();
        
    }
}
