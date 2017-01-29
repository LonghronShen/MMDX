using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.XNA.Misc;
using Microsoft.Xna.Framework;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// ボーンクラス
    /// </summary>
    public class MMDBoneContent
    {
        /// <summary>
        /// ボーンの名前
        /// </summary>
        public string Name;
        
        /// <summary>
        /// ボーンのバインドポーズ行列
        /// </summary>
        public SQTTransformContent BindPose;
        /// <summary>
        /// ボーンのバインドポーズ逆行列
        /// </summary>
        public Matrix InverseBindPose;
        /// <summary>
        /// ボーンの親のボーンのインデックス情報
        /// </summary>
        /// <remarks>親が無い場合は-1</remarks>
        public int SkeletonHierarchy;
    }
}
