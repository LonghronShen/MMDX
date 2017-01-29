using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

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
        public Matrix Transform;
        /// <summary>
        /// 基準ボーン名
        /// </summary>
        public string BoneName;
    }
}
