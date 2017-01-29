using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Misc
{
    /// <summary>
    /// MMD頂点データ(法線付き)
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Misc.MMDVertexNm, MikuMikuDanceCore")]
    public class MMDVertexNmContent
    {
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// ボーンウェイト
        /// </summary>
        public Vector2 BlendWeights;
        /// <summary>
        /// 影響ボーン1
        /// </summary>
        public int BlendIndexX;
        /// <summary>
        /// 影響ボーン2
        /// </summary>
        public int BlendIndexY;
        /// <summary>
        /// 法線
        /// </summary>
        public Vector3 Normal;
    }
    /// <summary>
    /// MMD頂点データ(法線、テクスチャ付き)
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Misc.MMDVertexNmTx, MikuMikuDanceCore")]
    public class MMDVertexNmTxContent : MMDVertexNmContent
    {
        /// <summary>
        /// テクスチャ座標
        /// </summary>
        public Vector2 TextureCoordinate;
    }
    /// <summary>
    /// MMD頂点データ(法線、テクスチャ、頂点カラー付き)
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Misc.MMDVertexNmTxVc, MikuMikuDanceCore")]
    public class MMDVertexNmTxVcContent : MMDVertexNmTxContent
    {
        /// <summary>
        /// 頂点カラー
        /// </summary>
        public Vector4 VertexColor;
    }
    /// <summary>
    /// MMD頂点データ(法線、頂点カラー付き)
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Misc.MMDVertexNmTxVc, MikuMikuDanceCore")]
    public class MMDVertexNmVcContent : MMDVertexNmContent
    {
        /// <summary>
        /// 頂点カラー
        /// </summary>
        public Vector4 VertexColor;
    }
    
}
