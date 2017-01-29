
using System;
namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// 再生オプション
    /// </summary>
    [Flags()]
    public enum MMDMotionTrackOptions
    {
        /// <summary>
        /// 無し
        /// </summary>
        None = 0x0,
        /// <summary>
        /// 停止中もボーンや表情を更新する
        /// </summary>
        UpdateWhenStopped = 0x1,
        /// <summary>
        /// 現在の再生位置間にキーフレームが無くなったボーンも、最後のフレームのままだとして、再生を継続する
        /// </summary>
        ExtendedMode = 0x2,
    }
}
