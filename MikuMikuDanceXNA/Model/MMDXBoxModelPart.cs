using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Model
{
#if XBOX
    /// <summary>
    /// XBox用モデルパーツ(法線あり)
    /// </summary>
    public class MMDXBoxModelPartPNm : MMDModelPart
    {
        VertexPositionNormal[] mainVertices;
        VertexXBoxExtend[] extVertices;
        //箱ではこれらの頂点の変更は行わない
        VertexBuffer mainVertexBuffer;
        VertexBuffer externaiVertexBuffer;

        VertexBufferBinding[] bindings = new VertexBufferBinding[4];
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">ポリゴン数</param>
        /// <param name="vertices">頂点データ</param>
        /// <param name="extvert">XBox用拡張頂点</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDXBoxModelPartPNm(int triangleCount, MMDVertexNm[] vertices, Vector2[] extvert, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, null, indexBuffer)
        {
            if (vertices.Length != extvert.Length)
                throw new MMDXException("標準頂点と拡張頂点の長さが一致しない");
            //頂点配列を作成
            mainVertices = new VertexPositionNormal[vertices.Length];
            extVertices = new VertexXBoxExtend[extvert.Length];
            //配列にデータを入れる
            for (int i = 0; i < mainVertices.Length; ++i)
            {
                mainVertices[i].Position = vertices[i].Position;
                mainVertices[i].Normal = vertices[i].Normal;
                extVertices[i].BlendIndices = new Vector2(vertices[i].BlendIndexX, vertices[i].BlendIndexY);
                extVertices[i].BlendWeight = vertices[i].BlendWeights;
                extVertices[i].FacePtr = extvert[i];
            }
            //頂点バッファ作成
            mainVertexBuffer = new VertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormal), vertices.Length, BufferUsage.WriteOnly);
            externaiVertexBuffer = new VertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexXBoxExtend), extVertices.Length, BufferUsage.WriteOnly);
            //頂点バッファにデータを流しこみ
            mainVertexBuffer.SetData(mainVertices);
            externaiVertexBuffer.SetData(extVertices);
            //頂点バッファをバインドしておく
            bindings[0] =new VertexBufferBinding(mainVertexBuffer);
            bindings[1] = new VertexBufferBinding(externaiVertexBuffer);
        }
        /// <summary>
        /// スキニング行列の適用
        /// </summary>
        /// <param name="skinTransforms">スキニング行列</param>
        public override void SetSkinMatrix(Matrix[] skinTransforms) { }//なにもしない
        /// <summary>
        /// 表情の適用
        /// </summary>
        /// <param name="faceManager">表情マネージャ</param>
        public override void SetFace(IMMDFaceManager faceManager) { }//なにもしない
        /// <summary>
        /// エフェクトに各種値を適用
        /// </summary>
        /// <param name="world">ワールド</param>
        /// <param name="mode">モデル描画モード</param>
        public override void SetParams(MMDDrawingMode mode, ref Matrix world)
        {
            base.SetParams(mode, ref world);
            Effect.Parameters["faceRates"].SetValue(model.FaceManagerXBox.FaceRates);
        }
        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="mode">描画モード</param>
        public override void Draw(MMDDrawingMode mode)
        {
            GraphicsDevice graphics = Effect.GraphicsDevice;
            //レンダーステートセットアップ
            SetUpRenderState(mode, model.Culling, graphics);
            //スキニング行列を書き込んだ頂点をセット
            bindings[2] = new VertexBufferBinding(model.SkinVertBuffer, model.BufferOffset);
            bindings[3] = new VertexBufferBinding(model.FaceManagerXBox.FaceVertBuffer);
            //バッファセット
            graphics.Indices = indexBuffer;
            graphics.SetVertexBuffers(bindings);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);
            }

            graphics.Indices = null;
            graphics.SetVertexBuffer(null);
        }
    }
    /// <summary>
    /// XBox用モデルパーツ(法線、頂点色あり)
    /// </summary>
    public class MMDXBoxModelPartPNmVc : MMDModelPart
    {
        VertexPositionNormalColor[] mainVertices;
        VertexXBoxExtend[] extVertices;
        //箱ではこれらの頂点の変更は行わない
        VertexBuffer mainVertexBuffer;
        VertexBuffer externaiVertexBuffer;

        VertexBufferBinding[] bindings = new VertexBufferBinding[4];
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">ポリゴン数</param>
        /// <param name="vertices">頂点データ</param>
        /// <param name="extvert">XBox用拡張頂点</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDXBoxModelPartPNmVc(int triangleCount, MMDVertexNmVc[] vertices, Vector2[] extvert, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, null, indexBuffer)
        {
            if (vertices.Length != extvert.Length)
                throw new MMDXException("標準頂点と拡張頂点の長さが一致しない");
            //頂点配列を作成
            mainVertices = new VertexPositionNormalColor[vertices.Length];
            extVertices = new VertexXBoxExtend[extvert.Length];
            //配列にデータを入れる
            for (int i = 0; i < mainVertices.Length; ++i)
            {
                mainVertices[i].Position = vertices[i].Position;
                mainVertices[i].Normal = vertices[i].Normal;
                mainVertices[i].Color = new Color(vertices[i].VertexColor);
                extVertices[i].BlendIndices = new Vector2(vertices[i].BlendIndexX, vertices[i].BlendIndexY);
                extVertices[i].BlendWeight = vertices[i].BlendWeights;
                extVertices[i].FacePtr = extvert[i];
            }
            //頂点バッファ作成
            mainVertexBuffer = new VertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormalColor), vertices.Length, BufferUsage.WriteOnly);
            externaiVertexBuffer = new VertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexXBoxExtend), extVertices.Length, BufferUsage.WriteOnly);
            //頂点バッファにデータを流しこみ
            mainVertexBuffer.SetData(mainVertices);
            externaiVertexBuffer.SetData(extVertices);
            //頂点バッファをバインドしておく
            bindings[0] = new VertexBufferBinding(mainVertexBuffer);
            bindings[1] = new VertexBufferBinding(externaiVertexBuffer);
        }
        /// <summary>
        /// スキニング行列の適用
        /// </summary>
        /// <param name="skinTransforms">スキニング行列</param>
        public override void SetSkinMatrix(Matrix[] skinTransforms)
        {

        }
        /// <summary>
        /// 表情の適用
        /// </summary>
        /// <param name="faceManager">表情マネージャ</param>
        public override void SetFace(IMMDFaceManager faceManager)
        {

        }
        /// <summary>
        /// モデルの描画
        /// </summary>
        public override void Draw(MMDDrawingMode mode)
        {
            GraphicsDevice graphics = Effect.GraphicsDevice;
            //レンダーステートセットアップ
            SetUpRenderState(mode, model.Culling, graphics);
            //スキニング行列を書き込んだ頂点をセット
            bindings[2] = new VertexBufferBinding(model.SkinVertBuffer, model.BufferOffset);
            bindings[3] = new VertexBufferBinding(model.FaceManagerXBox.FaceVertBuffer);
            //バッファセット
            graphics.Indices = indexBuffer;
            graphics.SetVertexBuffers(bindings);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);
            }

            graphics.Indices = null;
            graphics.SetVertexBuffer(null);
        }
    }
    /// <summary>
    /// XBox用モデルパーツ(法線、テクスチャあり)
    /// </summary>
    public class MMDXBoxModelPartPNmTx : MMDModelPart
    {
        VertexPositionNormalTexture[] mainVertices;
        VertexXBoxExtend[] extVertices;
        //箱ではこれらの頂点の変更は行わない
        VertexBuffer mainVertexBuffer;
        VertexBuffer externaiVertexBuffer;

        VertexBufferBinding[] bindings = new VertexBufferBinding[4];
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">ポリゴン数</param>
        /// <param name="vertices">頂点データ</param>
        /// <param name="extvert">XBox用拡張頂点</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDXBoxModelPartPNmTx(int triangleCount, MMDVertexNmTx[] vertices, Vector2[] extvert, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, null, indexBuffer)
        {
            if (vertices.Length != extvert.Length)
                throw new MMDXException("標準頂点と拡張頂点の長さが一致しない");
            //頂点配列を作成
            mainVertices = new VertexPositionNormalTexture[vertices.Length];
            extVertices = new VertexXBoxExtend[extvert.Length];
            //配列にデータを入れる
            for (int i = 0; i < mainVertices.Length; ++i)
            {
                mainVertices[i].Position = vertices[i].Position;
                mainVertices[i].Normal = vertices[i].Normal;
                mainVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
                extVertices[i].BlendIndices = new Vector2(vertices[i].BlendIndexX, vertices[i].BlendIndexY);
                extVertices[i].BlendWeight = vertices[i].BlendWeights;
                extVertices[i].FacePtr = extvert[i];
            }
            //頂点バッファ作成
            mainVertexBuffer = new VertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            externaiVertexBuffer = new VertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexXBoxExtend), extVertices.Length, BufferUsage.WriteOnly);
            //頂点バッファにデータを流しこみ
            mainVertexBuffer.SetData(mainVertices);
            externaiVertexBuffer.SetData(extVertices);
            //頂点バッファをバインドしておく
            bindings[0] = new VertexBufferBinding(mainVertexBuffer);
            bindings[1] = new VertexBufferBinding(externaiVertexBuffer);
        }
        /// <summary>
        /// スキニング行列の適用
        /// </summary>
        /// <param name="skinTransforms">スキニング行列</param>
        public override void SetSkinMatrix(Matrix[] skinTransforms)
        {

        }
        /// <summary>
        /// 表情の適用
        /// </summary>
        /// <param name="faceManager">表情マネージャ</param>
        public override void SetFace(IMMDFaceManager faceManager)
        {

        }
        /// <summary>
        /// モデルの描画
        /// </summary>
        public override void Draw(MMDDrawingMode mode)
        {
            GraphicsDevice graphics = Effect.GraphicsDevice;
            //レンダーステートセットアップ
            SetUpRenderState(mode, model.Culling, graphics);
            //スキニング行列を書き込んだ頂点をセット
            bindings[2] = new VertexBufferBinding(model.SkinVertBuffer, model.BufferOffset);
            bindings[3] = new VertexBufferBinding(model.FaceManagerXBox.FaceVertBuffer);
            //バッファセット
            graphics.Indices = indexBuffer;
            graphics.SetVertexBuffers(bindings);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);
            }

            graphics.Indices = null;
            graphics.SetVertexBuffer(null);
        }
    }
    /// <summary>
    /// XBox用モデルパーツ(法線、頂点色、テクスチャあり)
    /// </summary>
    public class MMDXBoxModelPartPNmTxVc : MMDModelPart
    {
        VertexPositionNormalTextureColor[] mainVertices;
        VertexXBoxExtend[] extVertices;
        //箱ではこれらの頂点の変更は行わない
        VertexBuffer mainVertexBuffer;
        VertexBuffer externaiVertexBuffer;

        VertexBufferBinding[] bindings = new VertexBufferBinding[4];
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="triangleCount">ポリゴン数</param>
        /// <param name="vertices">頂点データ</param>
        /// <param name="extvert">XBox用拡張頂点</param>
        /// <param name="indexBuffer">インデックスバッファ</param>
        public MMDXBoxModelPartPNmTxVc(int triangleCount, MMDVertexNmTxVc[] vertices, Vector2[] extvert, IndexBuffer indexBuffer)
            : base(triangleCount, vertices.Length, null, indexBuffer)
        {
            if (vertices.Length != extvert.Length)
                throw new MMDXException("標準頂点と拡張頂点の長さが一致しない");
            //頂点配列を作成
            mainVertices = new VertexPositionNormalTextureColor[vertices.Length];
            extVertices = new VertexXBoxExtend[extvert.Length];
            //配列にデータを入れる
            for (int i = 0; i < mainVertices.Length; ++i)
            {
                mainVertices[i].Position = vertices[i].Position;
                mainVertices[i].Normal = vertices[i].Normal;
                mainVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
                mainVertices[i].Color = new Color(vertices[i].VertexColor);
                extVertices[i].BlendIndices = new Vector2(vertices[i].BlendIndexX, vertices[i].BlendIndexY);
                extVertices[i].BlendWeight = vertices[i].BlendWeights;
                extVertices[i].FacePtr = extvert[i];
            }
            //頂点バッファ作成
            mainVertexBuffer = new VertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormalTextureColor), vertices.Length, BufferUsage.WriteOnly);
            externaiVertexBuffer = new VertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexXBoxExtend), extVertices.Length, BufferUsage.WriteOnly);
            //頂点バッファにデータを流しこみ
            mainVertexBuffer.SetData(mainVertices);
            externaiVertexBuffer.SetData(extVertices);
            //頂点バッファをバインドしておく
            bindings[0] = new VertexBufferBinding(mainVertexBuffer);
            bindings[1] = new VertexBufferBinding(externaiVertexBuffer);
        }
        /// <summary>
        /// スキニング行列の適用
        /// </summary>
        /// <param name="skinTransforms">スキニング行列</param>
        public override void SetSkinMatrix(Matrix[] skinTransforms)
        {

        }
        /// <summary>
        /// 表情の適用
        /// </summary>
        /// <param name="faceManager">表情マネージャ</param>
        public override void SetFace(IMMDFaceManager faceManager)
        {

        }
        /// <summary>
        /// モデルの描画
        /// </summary>
        public override void Draw(MMDDrawingMode mode)
        {
            GraphicsDevice graphics = Effect.GraphicsDevice;
            //レンダーステートセットアップ
            SetUpRenderState(mode, model.Culling, graphics);
            //スキニング行列を書き込んだ頂点をセット
            bindings[2] = new VertexBufferBinding(model.SkinVertBuffer, model.BufferOffset);
            bindings[3] = new VertexBufferBinding(model.FaceManagerXBox.FaceVertBuffer);
            //バッファセット
            graphics.Indices = indexBuffer;
            graphics.SetVertexBuffers(bindings);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);
            }

            graphics.Indices = null;
            graphics.SetVertexBuffer(null);
        }
    }
#endif
}
