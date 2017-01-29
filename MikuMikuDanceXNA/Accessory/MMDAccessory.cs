using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Accessory;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MMD用アクセサリデータ
    /// </summary>
    public class MMDAccessory : MMDAccessoryBase
    {
        VertexBuffer vertexBuffer;
        ReadOnlyCollection<MMDAccessoryPart> m_parts;
        /// <summary>
        /// パーツデータ
        /// </summary>
        public ReadOnlyCollection<MMDAccessoryPart> Parts { get { return m_parts; } }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="vertices">頂点データ</param>
        /// <param name="parts">パーツ情報</param>
        public MMDAccessory(MMDVertexNmTxVc[] vertices, IList<MMDAccessoryPart> parts)
        {
            this.vertexBuffer = new VertexBuffer(parts[0].indices.GraphicsDevice, typeof(VertexPositionNormalTextureColor), vertices.Length, BufferUsage.WriteOnly);
            VertexPositionNormalTextureColor[] gpuVertices = new VertexPositionNormalTextureColor[vertices.Length];
            //初期値代入
            for (int i = 0; i < vertices.Length; i++)
            {
                gpuVertices[i].Position = vertices[i].Position;
                gpuVertices[i].Normal = vertices[i].Normal;
                gpuVertices[i].Color = new Color(vertices[i].VertexColor);
                gpuVertices[i].TextureCoordinate = vertices[i].TextureCoordinate;
            }
            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertices.Length);
            m_parts = new ReadOnlyCollection<MMDAccessoryPart>(parts);
        }
        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="Position">アクセサリの位置</param>
        protected override void Draw(ref Matrix Position)
        {
            if (Parts.Count == 0)
                return;
            MMDDrawingMode mode = MMDDrawingMode.Normal;
            if (MMDXCore.Instance.EdgeManager != null && MMDXCore.Instance.EdgeManager.IsEdgeDetectionMode)
            {
                mode = MMDDrawingMode.Edge;
            }
            GraphicsDevice graphics = Parts[0].Effect.GraphicsDevice;
            graphics.SetVertexBuffer(vertexBuffer);
            for (int i = 0; i < Parts.Count; ++i)
            {
                Parts[i].SetParams(mode, ref Position);
                Parts[i].Draw(mode);
            }
            graphics.Indices = null;
        }
    }
}
