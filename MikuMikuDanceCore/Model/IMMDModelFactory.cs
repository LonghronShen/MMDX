using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Model
{
#if WINDOWS
    /// <summary>
    /// ファイルからモデルを読むファクトリー
    /// </summary>
    public interface IMMDModelFactory
    {
        /// <summary>
        /// ファイルから生成
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="opaqueData">不透明データ</param>
        /// <returns>生成したモデル</returns>
        MMDModel Load(string filename, Dictionary<string, object> opaqueData);
    }
#endif
}
