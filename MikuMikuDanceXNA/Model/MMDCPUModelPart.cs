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
#if false//CPUスキニング用。WP7対応とかするときに復活？
    /// <summary>
    /// モデルパーツ(頂点のみ)
    /// </summary>
    public class MMDCPUModelPartP : MMDModelPart
    {
        readonly VertexPosition[] gpuVertices;
        /// <summary>
        /// 頂点配列
        /// </summary>
        protected MMDVertex[] vertices;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertices">頂点配列</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDCPUModelPartP(int triangleCount, MMDVertex[] vertices,int[] vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPosition[vertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPosition), vertices.Length, BufferUsage.WriteOnly);
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
            //頂点位置の計算
#if !XBOX
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position));
#else
            throw new NotImplementedException();
#endif
            /*for (int i = 0; i < vertices.Length; i++)
            {
                SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position);
            }*/
            //GPUバッファへの書き込み
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(MMDFaceManager faceManager)
        {
            faceManager.ApplyToVertex(vertices, VertMap);
        }

    }
    /// <summary>
    /// モデルパーツ(頂点+色)
    /// </summary>
    public class MMDCPUModelPartPVc : MMDModelPart
    {
        readonly VertexPositionColor[] gpuVertices;
        /// <summary>
        /// 頂点配列
        /// </summary>
        protected MMDVertexVc[] vertices;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertices">頂点配列</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDCPUModelPartPVc(int triangleCount, MMDVertexVc[] vertices, int[] vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionColor[vertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Color = new Color(vertices[i].VertexColor);
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
#if !XBOX
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position));
#else
            throw new NotImplementedException();
#endif
            //頂点位置の計算
            /*for (int i = 0; i < vertices.Length; i++)
            {
                SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position);
            }*/
            //GPUバッファへの書き込み
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(MMDFaceManager faceManager)
        {
            faceManager.ApplyToVertex(vertices, VertMap);
        }

    }
    /// <summary>
    /// モデルパーツ(頂点+法線)
    /// </summary>
    public class MMDCPUModelPartPNm : MMDModelPart
    {//スフィアマップだけ貼りつけている物はこれになる……
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
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDCPUModelPartPNm(int triangleCount, MMDVertexNm[] vertices, int[] vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionNormal[vertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormal), vertices.Length, BufferUsage.WriteOnly);
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
#if !XBOX
            //頂点と法線位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal));
#else
            throw new NotImplementedException();
#endif

            /*for (int i = 0; i < vertices.Length; i++)
            {
                SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal);
            }*/
            //GPUバッファへの書き込み
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(MMDFaceManager faceManager)
        {
            faceManager.ApplyToVertex(vertices, VertMap);
        }

    }
    /// <summary>
    /// モデルパーツ(頂点+テクスチャー)
    /// </summary>
    public class MMDCPUModelPartPTx : MMDModelPart
    {
        readonly VertexPositionTexture[] gpuVertices;
        /// <summary>
        /// 頂点配列
        /// </summary>
        protected MMDVertexTx[] vertices;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertices">頂点配列</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDCPUModelPartPTx(int triangleCount, MMDVertexTx[] vertices, int[] vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionTexture[vertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
#if !XBOX
            //頂点位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position));
#else
            throw new NotImplementedException();
#endif

            /*for (int i = 0; i < vertices.Length; i++)
            {
                SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position);
            }*/
            //GPUバッファへの書き込み
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(MMDFaceManager faceManager)
        {
            faceManager.ApplyToVertex(vertices, VertMap);
        }

    }

    /// <summary>
    /// モデルパーツ(頂点+法線+色)
    /// </summary>
    public class MMDCPUModelPartPNmVc : MMDModelPart
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
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDCPUModelPartPNmVc(int triangleCount, MMDVertexNmVc[] vertices, int[] vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionNormalColor[vertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormalColor), vertices.Length, BufferUsage.WriteOnly);
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
                gpuVertices[i].Color = new Color(vertices[i].VertexColor);
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
#if !XBOX
            //頂点と法線位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal));
#else
            throw new NotImplementedException();
#endif

            /*for (int i = 0; i < vertices.Length; i++)
            {
                SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal);
            }*/
            //GPUバッファへの書き込み
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(MMDFaceManager faceManager)
        {
            faceManager.ApplyToVertex(vertices, VertMap);
        }

    }
    /// <summary>
    /// モデルパーツ(頂点+テクスチャー+カラー)
    /// </summary>
    public class MMDCPUModelPartPTxVc : MMDModelPart
    {
        readonly VertexPositionColorTexture[] gpuVertices;
        /// <summary>
        /// 頂点配列
        /// </summary>
        protected MMDVertexTxVc[] vertices;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">三角形の個数</param>
        /// <param name="vertices">頂点配列</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDCPUModelPartPTxVc(int triangleCount, MMDVertexTxVc[] vertices, int[] vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionColorTexture[vertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
                gpuVertices[i].Color = new Color(vertices[i].VertexColor);
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
#if !XBOX
            //頂点位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position));
#else
            throw new NotImplementedException();
#endif

            /*for (int i = 0; i < vertices.Length; i++)
            {
                SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position);
            }*/
            //GPUバッファへの書き込み
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(MMDFaceManager faceManager)
        {
            faceManager.ApplyToVertex(vertices, VertMap);
        }

    }
    /// <summary>
    /// モデルパーツ(頂点+法線+テクスチャー)
    /// </summary>
    public class MMDCPUModelPartPNmTx : MMDModelPart
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
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDCPUModelPartPNmTx(int triangleCount, MMDVertexNmTx[] vertices, int[] vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionNormalTexture[vertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
                gpuVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
#if !XBOX
            //頂点と法線位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal));
#else
            throw new NotImplementedException();
#endif
            /*for (int i = 0; i < vertices.Length; i++)
            {
                SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal);
            }*/
            //GPUバッファへの書き込み
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(MMDFaceManager faceManager)
        {
            faceManager.ApplyToVertex(vertices, VertMap);
        }

    }
    /// <summary>
    /// モデルパーツ(頂点+法線+テクスチャー+カラー)
    /// </summary>
    public class MMDCPUModelPartPNmTxVc : MMDModelPart
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
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDCPUModelPartPNmTxVc(int triangleCount, MMDVertexNmTxVc[] vertices, int[] vertMap, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, vertMap, indexBuffer)
        {
            this.vertices = vertices;
            //GPUリソース作成
            gpuVertices = new VertexPositionNormalTextureColor[vertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormalTextureColor), vertices.Length, BufferUsage.WriteOnly);
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
                gpuVertices[i].Color = new Color(vertices[i].VertexColor);
                gpuVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// ボーンの設定
        /// </summary>
        /// <param name="bones">ボーン配列</param>
        public override void SetSkinMatrix(Matrix[] bones)
        {
#if !XBOX
            //頂点と法線位置の計算
            System.Threading.Tasks.Parallel.For(0, vertices.Length,
                (i) => SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal));
#else
            throw new NotImplementedException();
#endif

            /*for (int i = 0; i < vertices.Length; i++)
            {
                SkinningHelpers.SkinVertex(bones, vertices[i], out gpuVertices[i].Position, out gpuVertices[i].Normal);
            }*/
            //GPUバッファへの書き込み
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        public override void SetFace(MMDFaceManager faceManager)
        {
            faceManager.ApplyToVertex(vertices, VertMap);
        }

    }
#endif
}
