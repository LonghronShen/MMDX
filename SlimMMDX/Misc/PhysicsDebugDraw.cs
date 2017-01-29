using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletX.LinerMath;
using SlimDX.Direct3D9;
using SlimDX;
using MikuMikuDance.Core.Misc;
using System.Runtime.InteropServices;

namespace MikuMikuDance.SlimDX.Misc
{
    [StructLayout(LayoutKind.Sequential)] 
    struct PDDVertex
    {
        public Vector3 pos;
        public int color;
        public const VertexFormat FVF = VertexFormat.Position | VertexFormat.Diffuse;
    }
    /// <summary>
    /// 剛体のデバッグ描画
    /// </summary>
    /// <remarks>一部未実装</remarks>
    public class PhysicsDebugDraw : IDebugDraw
    {
        PDDVertex[] lines;
        int primitiveCount;
        /// <summary>
        /// デバッグ描画モード
        /// </summary>
        public override DebugDrawModes DebugMode { get; set; }
        /// <summary>
        /// デバッグ描画
        /// </summary>
        /// <param name="MaxLine">確保する最大ライン数</param>
        public PhysicsDebugDraw(int MaxLine)
        {
            lines = new PDDVertex[MaxLine * 2];
            DebugMode = DebugDrawModes.DBG_DrawWireframe;
            
        }
        /// <summary>
        /// 剛体の描画
        /// </summary>
        /// <param name="device">SlimDXデバイス</param>
        public void Draw(Device device)
        {
            device.VertexFormat = PDDVertex.FVF;
            Viewport viewport = device.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;
            Matrix view, proj;
            SlimMMDXCore.Instance.Camera.GetCameraParam(aspectRatio, out view, out proj);
            device.SetTransform(TransformState.World, Matrix.Identity);
            device.SetTransform(TransformState.View, view);
            device.SetTransform(TransformState.Projection, proj);
            device.DrawUserPrimitives(PrimitiveType.LineList, primitiveCount, lines);
            primitiveCount = 0;
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
            lines[primitiveCount * 2].pos = new Vector3(from.X, from.Y, from.Z);
            lines[primitiveCount * 2].color = ((int)(color.X*256)) * (2 ^ 16) + ((int)(color.Y*256)) * (2 ^ 8) + ((int)(color.Z*256));
            lines[primitiveCount * 2 + 1].pos = new Vector3(to.X, to.Y, to.Z);
            lines[primitiveCount * 2 + 1].color = ((int)(color.X * 256)) * (2 ^ 16) + ((int)(color.Y * 256)) * (2 ^ 8) + ((int)(color.Z * 256));
            ++primitiveCount;
        }

        /// <summary>
        /// 接触位置の描画
        /// </summary>
        /// <remarks>未実装</remarks>
        public override void drawContactPoint(ref btVector3 PointOnB, ref btVector3 normalOnB, float distance, int lifeTime, ref btVector3 color)
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
            System.Diagnostics.Debug.WriteLine(warningString);
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
