using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Model
{
#if WINDOWS
    /// <summary>
    /// モデルパーツ(頂点+法線)
    /// </summary>
    public class MMDGPUModelPartPNm : MMDModelPart
    {
        readonly VertexPositionNormal[] gpuVertices;
        /// <summary>
        /// 頂点配列
        /// </summary>
        protected MMDVertexNm[] vertices;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertices">頂点配列</param>
        /// <param name="vertMap">モデルの頂点とMMDの頂点対応</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDGPUModelPartPNm(int triangleCount, MMDVertexNm[] vertices, Dictionary<long, int[]> vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionNormal[vertices.Length];
            vertexBuffer = new WritableVertexBuffer(indexBuffer.GraphicsDevice, vertices.Length * 4, typeof(VertexPositionNormal));
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
            }
            // put the vertices into our vertex buffer
            vertexOffset = vertexBuffer.SetData(gpuVertices);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
            //頂点と法線位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal));
            //GPUバッファへの書き込み
            vertexOffset = vertexBuffer.SetData(gpuVertices);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(IMMDFaceManager faceManager)
        {
            MMDFaceManager.ApplyToVertex((MMDFaceManager)faceManager,vertices, VertMap);
        }

    }
    
    /// <summary>
    /// モデルパーツ(頂点+法線+色)
    /// </summary>
    public class MMDGPUModelPartPNmVc : MMDModelPart
    {
        readonly VertexPositionNormalColor[] gpuVertices;
        /// <summary>
        /// 頂点配列
        /// </summary>
        protected MMDVertexNmVc[] vertices;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertices">頂点配列</param>
        /// <param name="vertMap">モデルの頂点とMMDの頂点対応</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDGPUModelPartPNmVc(int triangleCount, MMDVertexNmVc[] vertices, Dictionary<long, int[]> vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionNormalColor[vertices.Length];
            vertexBuffer = new WritableVertexBuffer(indexBuffer.GraphicsDevice, vertices.Length * 4, typeof(VertexPositionNormalColor));
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
                gpuVertices[i].Color = new Color(vertices[i].VertexColor);
            }
            // put the vertices into our vertex buffer
            vertexOffset = vertexBuffer.SetData(gpuVertices);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
            //頂点と法線位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal));
            //GPUバッファへの書き込み
            vertexOffset = vertexBuffer.SetData(gpuVertices);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(IMMDFaceManager faceManager)
        {
            MMDFaceManager.ApplyToVertex((MMDFaceManager)faceManager, vertices, VertMap);
        }

    }
    /// <summary>
    /// モデルパーツ(頂点+法線+テクスチャー)
    /// </summary>
    public class MMDGPUModelPartPNmTx : MMDModelPart
    {
        readonly VertexPositionNormalTexture[] gpuVertices;
        /// <summary>
        /// 頂点配列
        /// </summary>
        protected MMDVertexNmTx[] vertices;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertices">頂点配列</param>
        /// <param name="vertMap">モデルの頂点とMMDの頂点対応</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDGPUModelPartPNmTx(int triangleCount, MMDVertexNmTx[] vertices, Dictionary<long, int[]> vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionNormalTexture[vertices.Length];
            vertexBuffer = new WritableVertexBuffer(indexBuffer.GraphicsDevice, vertices.Length * 4, typeof(VertexPositionNormalTexture));
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
                gpuVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
            }
            // put the vertices into our vertex buffer
            vertexOffset = vertexBuffer.SetData(gpuVertices);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
            //頂点と法線位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal));
            //GPUバッファへの書き込み
            vertexOffset = vertexBuffer.SetData(gpuVertices);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(IMMDFaceManager faceManager)
        {
            MMDFaceManager.ApplyToVertex((MMDFaceManager)faceManager, vertices, VertMap);
        }

    }
    /// <summary>
    /// モデルパーツ(頂点+法線+テクスチャー+カラー)
    /// </summary>
    public class MMDGPUModelPartPNmTxVc : MMDModelPart
    {
        readonly VertexPositionNormalTextureColor[] gpuVertices;
        /// <summary>
        /// 頂点配列
        /// </summary>
        protected MMDVertexNmTxVc[] vertices;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertices">頂点配列</param>
        /// <param name="vertMap">モデルの頂点とMMDの頂点対応</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDGPUModelPartPNmTxVc(int triangleCount, MMDVertexNmTxVc[] vertices, Dictionary<long, int[]> vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionNormalTextureColor[vertices.Length];
            vertexBuffer = new WritableVertexBuffer(indexBuffer.GraphicsDevice, vertices.Length * 4, typeof(VertexPositionNormalTextureColor));
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
                gpuVertices[i].Color = new Color(vertices[i].VertexColor);
                gpuVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
            }
            // put the vertices into our vertex buffer
            vertexOffset = vertexBuffer.SetData(gpuVertices);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
            //頂点と法線位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal));
            //GPUバッファへの書き込み
            vertexOffset = vertexBuffer.SetData(gpuVertices);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(IMMDFaceManager faceManager)
        {
            MMDFaceManager.ApplyToVertex((MMDFaceManager)faceManager, vertices, VertMap);
        }

    }
#endif
}
