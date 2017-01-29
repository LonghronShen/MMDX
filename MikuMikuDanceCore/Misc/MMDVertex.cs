using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// MMD頂点データ(法線付き)
    /// </summary>
    public class MMDVertexNm
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
    public class MMDVertexNmTx : MMDVertexNm
    {
        /// <summary>
        /// テクスチャ座標
        /// </summary>
        public Vector2 TextureCoordinate;
    }
    /// <summary>
    /// MMD頂点データ(法線、テクスチャ、頂点カラー付き)
    /// </summary>
    public class MMDVertexNmTxVc : MMDVertexNmTx
    {
        /// <summary>
        /// 頂点カラー
        /// </summary>
        public Vector4 VertexColor;
    }
    /// <summary>
    /// MMD頂点データ(法線、頂点カラー付き)
    /// </summary>
    public class MMDVertexNmVc : MMDVertexNm
    {
        /// <summary>
        /// 頂点カラー
        /// </summary>
        public Vector4 VertexColor;
    }
    
}
