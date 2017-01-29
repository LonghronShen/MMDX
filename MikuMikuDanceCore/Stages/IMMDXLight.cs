using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Stages
{
    /// <summary>
    /// ライト情報インターフェイス
    /// </summary>
    public interface IMMDXLight
    {
        /// <summary>
        /// ライト情報取得
        /// </summary>
        /// <param name="color">ライト色</param>
        /// <param name="dir">ライト方向</param>
        void GetLightParam(out Vector3 color, out Vector3 dir);

        /// <summary>
        /// ライト色の設定
        /// </summary>
        Vector3 LightColor{ get; set; }

        /// <summary>
        /// ライト方向の設定
        /// </summary>
        Vector3 LightDirection { get; set; }

    }
}
