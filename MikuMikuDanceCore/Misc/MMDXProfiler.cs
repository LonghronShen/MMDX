using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MikuMikuDance.Core.MultiThreads;

#if XNA
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif

namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// MMD内で時間計測用のBeginMarkが呼ばれるとき用のデリゲート
    /// </summary>
    /// <param name="mmdxThreadIndex">MMDXスレッド番号</param>
    /// <param name="key">計測用キー</param>
    /// <param name="color">色</param>
    public delegate void BeginMarkDelegate(int mmdxThreadIndex, string key, Color color);
    /// <summary>
    /// MMD内で時間計測用のEndMarkが呼ばれるとき用のデリゲート
    /// </summary>
    /// <param name="mmdxThreadIndex">MMDXスレッド番号</param>
    /// <param name="key">計測用キー</param>
    public delegate void EndMarkDelegate(int mmdxThreadIndex, string key);
    /// <summary>
    /// MMDX時間計測用クラス
    /// </summary>
    public static class MMDXProfiler
    {
        /// <summary>
        ///  MMD内で時間計測用のBeginMarkが呼ばれるときに発生するイベント
        /// </summary>
        public static event BeginMarkDelegate MMDBeginMark;
        /// <summary>
        /// MMD内で時間計測用のEndMarkが呼ばれるときに発生するイベント
        /// </summary>
        public static event EndMarkDelegate MMDEndMark;

        internal static void BeginMark(string key, Color color)
        {
            if (MMDBeginMark != null)
            {
                MMDBeginMark(0, key, color);
            }
        }
        internal static void EndMark(string key)
        {
            if (MMDEndMark != null)
            {
                MMDEndMark(0, key);
            }
        }
    }
}
