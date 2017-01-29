using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.XNA.Misc;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MikuMikuDance アクセサリ
    /// </summary>
    public class MMDAccessoryContent
    {
        /// <summary>
        /// 頂点データ
        /// </summary>
        public MMDVertexNmTxVcContent[] Vertex;
        /// <summary>
        /// アクセサリのパーツ
        /// </summary>
        public List<MMDAccessoryPartContent> Parts = new List<MMDAccessoryPartContent>();
    }
}
