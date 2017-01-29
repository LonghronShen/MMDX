using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Accessory
{
#if WINDOWS
    /// <summary>
    /// アクセサリ生成ファクトリー用インタフェース
    /// </summary>
    public interface IMMDAccessoryFactory
    {
        /// <summary>
        /// ファイルから生成
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>生成したアクセサリ</returns>
        MMDAccessoryBase Load(string filename);
    }
#endif
}
