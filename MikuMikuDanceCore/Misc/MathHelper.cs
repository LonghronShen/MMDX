using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Misc
{
#if !XNA
    /// <summary>
    /// MathHelper(非XNA用)
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// 円周率/4
        /// </summary>
        public const float PiOver4 = (float)(Math.PI / 4);
        /// <summary>
        /// 円周率/2
        /// </summary>
        public const float PiOver2 = (float)(Math.PI / 2);
        /// <summary>
        /// 円周率
        /// </summary>
        public const float Pi = (float)Math.PI;
        /// <summary>
        /// 値を最大値と最小値の範囲に収める
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>収めた値</returns>
        public static float Clamp(float value, float min, float max)
        {
            if (min > value)
                return min;
            else if (max < value)
                return max;
            return value;
        }
        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="start">開始値</param>
        /// <param name="end">終了値</param>
        /// <param name="factor">進行度合い</param>
        /// <returns>戻り値</returns>
        public static float Lerp(float start, float end, float factor)
        {
            return start + ((end - start) * factor);
        }


        /// <summary>
        /// ラジアンに変換
        /// </summary>
        /// <param name="degree">度</param>
        /// <returns>ラジアン</returns>
        public static float ToRadians(float degree)
        {
            return degree * Pi / 180.0f;
        }
    }
#endif
}
