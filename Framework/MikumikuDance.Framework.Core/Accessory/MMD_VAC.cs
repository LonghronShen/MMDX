using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Accessory
{
    /// <summary>
    /// VAC設定(モデルとアクセサリの接続情報)オブジェクト
    /// </summary>
    /// <remarks>vacはアクセサリデータそのものは持ちません</remarks>
    public struct MMD_VAC
    {
        /// <summary>
        /// 位置
        /// </summary>
        public MikumikuDance.Framework.Abstractions.MMDMatrix Transform;
        /// <summary>
        /// 基準ボーン名
        /// </summary>
        public string BoneName;
    }
}
