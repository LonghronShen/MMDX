using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MikuMikuDanceアクセサリ
    /// </summary>
    public class MMDAccessoryPartContent
    {
        /// <summary>
        /// 頂点数
        /// </summary>
        public int VertexCount;
        /// <summary>
        /// インデックスバッファ
        /// </summary>
        public IndexCollection IndexBuffer;
        /// <summary>
        /// インデックスのオフセット
        /// </summary>
        public int BaseVertex;
        /// <summary>
        /// メッシュ数
        /// </summary>
        public int TriangleCount;
        /// <summary>
        /// マテリアル
        /// </summary>
        public MaterialContent Material;
        /// <summary>
        /// スクリーンを使用するかどうか
        /// </summary>
        public bool Screen;
        /// <summary>
        /// エッジを適用するかどうか
        /// </summary>
        public bool Edge;
    }
}
