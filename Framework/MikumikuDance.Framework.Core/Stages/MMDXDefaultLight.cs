using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikumikuDance.Framework.Abstractions;

namespace MikuMikuDance.Core.Stages
{
    /// <summary>
    /// MMDXのデフォルトカメラ
    /// </summary>
    public class MMDXDefaultLight : IMMDXLight
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MMDXDefaultLight()
        {
            LightColor = new MMDVector3(164.0f / 255.0f, 164.0f / 255.0f, 164.0f / 255.0f);
            LightDirection = new MMDVector3(-0.5f, -1f, -0.5f);
        }

        #region IMMDXLight メンバー
        /// <summary>
        /// ライト情報取得
        /// </summary>
        /// <param name="color">ライト色</param>
        /// <param name="dir">ライト方向</param>
        public void GetLightParam(out MMDVector3 color, out MMDVector3 dir)
        {
            color = LightColor;
            dir = LightDirection;
        }

        /// <summary>
        /// ライト色の設定
        /// </summary>
        public MMDVector3 LightColor { get; set; }

        /// <summary>
        /// ライト方向の設定
        /// </summary>
        public MMDVector3 LightDirection { get; set; }

        #endregion
    }
}
