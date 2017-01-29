using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// IK角度制限インターフェイス
    /// </summary>
    public interface IIKLimitter
    {
        /// <summary>
        /// 制限の適用
        /// </summary>
        /// <param name="bone">対象となるボーン</param>
        void Adjust(MMDBone bone);
        /// <summary>
        /// 回転軸制限の適用
        /// </summary>
        /// <param name="boneName">対象となるボーン名</param>
        /// <param name="rotationAxis">回転軸</param>
        void Adjust(string boneName, ref Vector3 rotationAxis);
    }
}
