using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Misc;
using SlimDX.Direct3D9;
using SlimDX;
using MikuMikuDance.Resource;
using System.Runtime.InteropServices;
using System.Drawing;

namespace MikuMikuDance.SlimDX.Misc
{
    [StructLayout(LayoutKind.Sequential)]
    struct ScreenVertex
    {
        public Vector4 Position;
        public Vector2 Texture;

        public static VertexElement[] VertexElements = new []
        {
            new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
            new VertexElement(0, 16,DeclarationType.Float2,DeclarationMethod.Default,DeclarationUsage.TextureCoordinate,0),
            VertexElement.VertexDeclarationEnd
        };
    }
    /// <summary>
    /// トゥーンのエッジ描画マネージャ
    /// </summary>
    /// <remarks>エッジ描画のための処理を行う</remarks>
    public class EdgeManager : IEdgeManager, IDisposable
    {
        Texture EdgeMap;
        Surface renderSurface;
        Surface depthBuffer;

        Surface oldTarget = null;
        Surface oldDepth = null;

        Effect effect = null;
        VertexBuffer vertex;
        ScreenVertex[] screenVertex;
        VertexDeclaration vertexDec;

        bool bEdgeDetectionMode = false;
        int width, height;

        /// <summary>
        /// エッジ検出モードかどうか
        /// </summary>
        public bool IsEdgeDetectionMode
        {
            get { return bEdgeDetectionMode; }
        }
        /// <summary>
        /// エッジ幅
        /// </summary>
        public float EdgeWidth { get; set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="width">バックバッファ幅</param>
        /// <param name="height">バックバッファ高さ</param>
        public EdgeManager(int width, int height)
        {
            EdgeWidth = 1f;
            this.width = width;
            this.height = height;
            Setup();
            SlimMMDXCore.Instance.LostDevice += OnLostDevice;
            SlimMMDXCore.Instance.ResetDevice += OnResetDevice;
        }
        private void Setup()
        {
            try
            {
                EdgeMap = new Texture(SlimMMDXCore.Instance.Device, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            }
            catch (Direct3D9Exception)
            {
                //あれ？ABGRで無いとダメなん？
                EdgeMap = new Texture(SlimMMDXCore.Instance.Device, width, height, 1, Usage.RenderTarget, Format.A8B8G8R8, Pool.Default);
            }
            renderSurface = EdgeMap.GetSurfaceLevel(0);
            depthBuffer = Surface.CreateDepthStencil(SlimMMDXCore.Instance.Device, width, height, Format.D16, MultisampleType.None, 0, true);
            //エッジ用エフェクト読み込み
            if (effect != null)
            {
                effect.OnResetDevice();
            }
            else
            {
                effect = Effect.FromMemory(SlimMMDXCore.Instance.Device, MMDXResource.MMDEdgeEffect,
#if DEBUG
 ShaderFlags.OptimizationLevel0 | ShaderFlags.Debug
#else
                        ShaderFlags.OptimizationLevel3
#endif
);
            }
            effect.SetValue("EdgeWidth", EdgeWidth);
            effect.Technique = "MMDEdgeEffect";
            
            effect.SetValue("ScreenResolution", new Vector2(width, height));
            CreateScreenVertex();
        }
        void CreateScreenVertex()
        {
            screenVertex = new ScreenVertex[6];
            screenVertex[0].Position = new Vector4(0, 0, 0.5f, 1.0f);
            screenVertex[0].Texture = new Vector2(0, 0);
            screenVertex[1].Position = new Vector4(width, 0, 0.5f, 1.0f);
            screenVertex[1].Texture = new Vector2(1, 0);
            screenVertex[2].Position = new Vector4(width, height, 0.5f, 1.0f);
            screenVertex[2].Texture = new Vector2(1, 1);
            screenVertex[3].Position = new Vector4(0, 0, 0.5f, 1.0f);
            screenVertex[3].Texture = new Vector2(0, 0);
            screenVertex[4].Position = new Vector4(width, height, 0.5f, 1.0f);
            screenVertex[4].Texture = new Vector2(1, 1);
            screenVertex[5].Position = new Vector4(0, height, 0.5f, 1.0f);
            screenVertex[5].Texture = new Vector2(0, 1);

            vertex = new VertexBuffer(SlimMMDXCore.Instance.Device, 6 * Marshal.SizeOf(typeof(ScreenVertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);
            DataStream stream = vertex.Lock(0, 0, LockFlags.None);
            stream.WriteRange(screenVertex);
            vertex.Unlock();
            vertexDec = new VertexDeclaration(SlimMMDXCore.Instance.Device, ScreenVertex.VertexElements);
        }
        /// <summary>
        /// 画面サイズ変更
        /// </summary>
        /// <param name="width">バックバッファ幅</param>
        /// <param name="height">バックバッファ高さ</param>
        public void ChangeSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            OnLostDevice();
            Setup();
        }
        void OnLostDevice()
        {
            vertex.Dispose();
            vertexDec.Dispose();
            effect.OnLostDevice();
            renderSurface.Dispose();
            EdgeMap.Dispose();
            depthBuffer.Dispose();
        }
        void OnResetDevice()
        {
            Setup();
        }
        /// <summary>
        /// エッジ検出モード開始
        /// </summary>
        /// <remarks>エッジ検出モード中に描画するとエッジマネージャにエッジが描画される(エッジ検出モード対応オブジェクトのみ。モデル。アクセサリのみ)</remarks>
        public void StartEdgeDetection()
        {
            if (bEdgeDetectionMode)
                throw new InvalidOperationException("すでにエッジ検出モードは開始しています");
            //レンダリングターゲットを退避
            oldTarget = SlimMMDXCore.Instance.Device.GetRenderTarget(0);
            oldDepth = SlimMMDXCore.Instance.Device.DepthStencilSurface;

            //レンダリングターゲットを変更
            SlimMMDXCore.Instance.Device.SetRenderTarget(0, renderSurface);
            SlimMMDXCore.Instance.Device.DepthStencilSurface = depthBuffer;

            //レンダーターゲットをクリア
            SlimMMDXCore.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(0f, 0f, 0f, 0f), 1.0f, 0);
            
            bEdgeDetectionMode = true;
        }

        /// <summary>
        /// エッジ検出モード終了
        /// </summary>
        public void EndEdgeDetection()
        {
            if (!bEdgeDetectionMode)
                throw new InvalidOperationException("エッジ検出モードを開始していません");
            //レンダーターゲットを元に戻す
            SlimMMDXCore.Instance.Device.SetRenderTarget(0, oldTarget);
            SlimMMDXCore.Instance.Device.DepthStencilSurface = oldDepth;
            oldTarget.Dispose();
            oldDepth.Dispose();
            
            bEdgeDetectionMode = false;
        }

        /// <summary>
        /// 検出したエッジを描画する
        /// </summary>
        /// <remarks>2Dで描画されるので注意</remarks>
        public void DrawEdge()
        {
            //エフェクト設定
            effect.SetValue("EdgeWidth", EdgeWidth);
            effect.SetTexture("Texture", EdgeMap);
            //頂点バッファのセット処理
            SlimMMDXCore.Instance.Device.VertexDeclaration = vertexDec;
            SlimMMDXCore.Instance.Device.SetStreamSource(0, vertex, 0, Marshal.SizeOf(typeof(ScreenVertex)));

            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaFunc, Compare.Greater);
            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaTestEnable, true);
            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaRef, 0);

            effect.Begin();
            effect.BeginPass(0);
            SlimMMDXCore.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose()
        {
            vertex.Dispose();
            vertexDec.Dispose();
            effect.Dispose();
            renderSurface.Dispose();
            EdgeMap.Dispose();
            depthBuffer.Dispose(); 
            SlimMMDXCore.Instance.LostDevice -= OnLostDevice;
            SlimMMDXCore.Instance.ResetDevice -= OnResetDevice;
        }
    }
}
