using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MikuMikuDance.XNA.Misc;
using Microsoft.Xna.Framework;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// モデルパーツ
    /// </summary>
    public class MMDModelPartContent
    {
        /// <summary>
        /// モデルのトライアングル数
        /// </summary>
        public int TriangleCount;
        /// <summary>
        /// モデルの頂点
        /// </summary>
        public MMDVertexNmContent[] Vertices;
        /// <summary>
        /// XBox用拡張モデル頂点
        /// </summary>
        public Vector2[] extVertices;
        /// <summary>
        /// インデックスコレクション
        /// </summary>
        public IndexCollection IndexCollection;
        /// <summary>
        /// マテリアル
        /// </summary>
        public MaterialContent Material;
        /// <summary>
        /// 元の頂点番号対応
        /// </summary>
        public Dictionary<long, int[]> VertMap;
    }
}
