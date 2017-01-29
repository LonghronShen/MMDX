using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Stages;

#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// MMDのライティング用キーフレーム
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    public struct MMDLightKeyFrame
    {
        /// <summary>
        /// フレームナンバー
        /// </summary>
        public uint FrameNo;
        /// <summary>
        /// ライトの色
        /// </summary>
        public Vector3 Color;
        /// <summary>
        /// ライトの位置
        /// </summary>
        public Vector3 Location;
        /// <summary>
        /// ライトの補間
        /// </summary>
        /// <param name="light1">フレーム1</param>
        /// <param name="light2">フレーム2</param>
        /// <param name="Progress">進行度合い</param>
        /// <param name="light">適用するライト</param>
        public static void Lerp(MMDLightKeyFrame light1, MMDLightKeyFrame light2, float Progress, IMMDXLight light)
        {
            light.LightColor = Vector3.Lerp(light1.Color, light2.Color, Progress);
            light.LightDirection = Vector3.Lerp(light1.Location, light2.Location, Progress);
        }
    }
}
