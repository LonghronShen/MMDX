using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// モーションファクトリー用
    /// </summary>
    public interface IMMDMotionFactory
    {
        /// <summary>
        /// モーションのロード
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="scale">スケーリング値</param>
        /// <returns>モーション</returns>
        MMDMotion Load(string filename, float scale);
    }
}
