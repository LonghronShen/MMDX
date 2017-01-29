using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Misc
{
    /// <summary>
    /// ベジェ曲線クラス
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Misc.BezierCurve, MikuMikuDanceCore")]
    public struct BezierCurveContent
    {
        /// <summary>
        /// ベジェ曲線に用いる点１
        /// </summary>
        public Vector2 v1;
        /// <summary>
        /// ベジェ曲線に用いる点2
        /// </summary>
        public Vector2 v2;
    }
}
