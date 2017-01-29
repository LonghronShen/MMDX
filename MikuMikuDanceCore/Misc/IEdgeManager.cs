using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// トゥーンのエッジ描画マネージャ
    /// </summary>
    /// <remarks>エッジ描画のための処理を行う</remarks>
    public interface IEdgeManager
    {
        /// <summary>
        /// エッジ検出モードかどうか
        /// </summary>
        bool IsEdgeDetectionMode { get; }
        /// <summary>
        /// エッジ太さ
        /// </summary>
        float EdgeWidth { get; set; }
        
    }
}
