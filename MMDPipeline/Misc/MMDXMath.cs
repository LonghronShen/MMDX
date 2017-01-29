using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MikuMikuDance.XNA.Misc
{
    /// <summary>
    /// MMDX用数学クラス
    /// </summary>
    public static class MMDXMath
    {
        /// <summary>
        /// Vector2に変換
        /// </summary>
        public static Vector2 ToVector2(float[] vec)
        {
            return new Vector2(vec[0], vec[1]);
        }
        /// <summary>
        /// Vector3に変換
        /// </summary>
        public static Vector3 ToVector3(float[] vec)
        {
            return new Vector3(vec[0], vec[1], vec[2]);
        }
        /// <summary>
        /// Vector4に変換
        /// </summary>
        public static Vector4 ToVector4(float[] vec)
        {
            return new Vector4(vec[0], vec[1], vec[2], vec[3]);
        }

        /// <summary>
        /// MinMax関係が成り立つように各要素を修正
        /// </summary>
        /// <param name="min">最小</param>
        /// <param name="max">最大</param>
        public static void CheckMinMax(float[] min, float[] max)
        {
            for (int i = 0; i < min.Length && i < max.Length; i++)
            {
                if (min[i] > max[i])
                    Swap(ref min[i], ref max[i]);
            }
        }
        /// <summary>
        /// swap関数
        /// </summary>
        /// <typeparam name="T">スワップする型</typeparam>
        /// <param name="v1">変数1</param>
        /// <param name="v2">変数2</param>
        public static void Swap<T>(ref T v1, ref T v2)
        {
            T v3 = v1;
            v1 = v2;
            v2 = v3;
        }
    }
}
