using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// VAC中間データ
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Accessory.MMD_VAC, MikuMikuDanceCore")]
    public struct VACContent
    {
        /// <summary>
        /// ボーン名
        /// </summary>
        public string BoneName;
        /// <summary>
        /// 位置
        /// </summary>
        public Matrix Transform;
    }
    /// <summary>
    /// VAC中間データ
    /// </summary>
    public struct VACContent2
    {
        /// <summary>
        /// ボーン名
        /// </summary>
        public string BoneName;
        /// <summary>
        /// シャドウ
        /// </summary>
        public bool Shadow;
        /// <summary>
        /// 拡大
        /// </summary>
        public float Scale;
        /// <summary>
        /// 回転
        /// </summary>
        public Vector3 Rot;
        /// <summary>
        /// 移動
        /// </summary>
        public Vector3 Trans;
    }
}
