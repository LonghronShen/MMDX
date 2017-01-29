using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// モデルの描画モード
    /// </summary>
    public enum MMDDrawingMode
    {
        /// <summary>
        /// 標準描画
        /// </summary>
        Normal,
        /// <summary>
        /// エッジ検出モード
        /// </summary>
        /// <remarks>エッジ情報を得るためのプリレンダリング(エッジ検出モード)</remarks>
        Edge,
    }
}
