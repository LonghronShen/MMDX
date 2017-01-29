using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using BulletX.LinerMath;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace MikuMikuDance.XNA.Misc
{
    /// <summary>
    /// 剛体のデバッグ描画
    /// </summary>
    /// <remarks>一部未実装</remarks>
    public class PhysicsDebugDraw  : IDebugDraw
    {
        BasicEffect effect;
        VertexDeclaration vertexDec;
        VertexPositionColor[] vertex;
        int VertCount = 0;
        DebugDrawModes mode;
        /// <summary>
        /// デバッグ描画モード
        /// </summary>
        public override DebugDrawModes DebugMode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device">グラフィックデバイス</param>
        public PhysicsDebugDraw(GraphicsDevice device)
        {
            effect = new BasicEffect(device);
            effect.VertexColorEnabled = true;
            effect.World = Matrix.Identity;
            vertexDec = VertexPositionColor.VertexDeclaration;//new VertexDeclaration(device, VertexPositionColor.VertexElements);
            vertex = new VertexPositionColor[50];
            DebugMode=(DebugDrawModes.DBG_DrawWireframe | DebugDrawModes.DBG_DrawConstraints);
        }
        /// <summary>
        /// 剛体の描画
        /// </summary>
        /// <param name="graphics">グラフィックデバイス</param>
        public virtual void Draw(GraphicsDevice graphics)
        {
            if (VertCount == 0) return;
            graphics.BlendState = BlendState.AlphaBlend;
            Viewport viewport = graphics.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;
            Matrix View, Proj;
            MMDXCore.Instance.Camera.GetCameraParam(aspectRatio, out View, out Proj);
            //graphics.VertexDeclaration = vertexDec;
            effect.View = View;
            effect.Projection = Proj;
            //effect.Begin();
            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                var pass = effect.CurrentTechnique.Passes[i];
                pass.Apply();
                graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertex, 0, VertCount);
                //pass.End();
            }
            VertCount = 0;
            Array.Clear(vertex, 0, vertex.Length);
            //effect.End();
        }
        /// <summary>
        /// 描画せずにデバッグ描画のスタックをリセットする
        /// </summary>
        public virtual void Reset()
        {
            VertCount = 0;
            Array.Clear(vertex, 0, vertex.Length);
        }

        /// <summary>
        /// 線の描画
        /// </summary>
        /// <param name="from">開始点</param>
        /// <param name="to">終了点</param>
        /// <param name="color">色</param>
        /// <remarks>スーパークラスから呼び出される</remarks>
        public override void drawLine(ref btVector3 from, ref btVector3 to, ref btVector3 color)
        {
            if (vertex.Length <= VertCount * 2)
            {
                Array.Resize(ref vertex, VertCount * 4);
            }
            vertex[VertCount * 2] = new VertexPositionColor(new Vector3(from.X, from.Y, from.Z), new Color(new Vector3(color.X, color.Y, color.Z)));
            vertex[VertCount * 2 + 1] = new VertexPositionColor(new Vector3(to.X, to.Y, to.Z), new Color(new Vector3(color.X, color.Y, color.Z)));
            VertCount++;
        }
        /// <summary>
        /// 接触位置の描画
        /// </summary>
        /// <remarks>未実装</remarks>
        public override void drawContactPoint(ref btVector3 PointOnB,ref btVector3 normalOnB, float distance, int lifeTime,ref btVector3 color)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// エラー情報の描画
        /// </summary>
        /// <param name="warningString">エラー情報</param>
        /// <remarks>スーパークラスから呼び出される</remarks>
        public override void reportErrorWarning(string warningString)
        {
            Debug.WriteLine(warningString);
        }
        /// <summary>
        /// 3D文字の描画
        /// </summary>
        /// <remarks>未実装</remarks>
        public override void draw3dText(ref btVector3 location, string textString)
        {
            throw new NotImplementedException();
        }

    }
}
