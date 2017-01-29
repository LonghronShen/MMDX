using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Accessory
{
    /// <summary>
    /// VACファクトリー
    /// </summary>
    public interface IMMDVACFactory
    {
        /// <summary>
        /// ファイルから読み込み
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="leftHanded">左手座標系</param>
        /// <returns>MikuMikuDance VAC</returns>
        MMD_VAC Load(string filename, bool leftHanded);
    }
}
