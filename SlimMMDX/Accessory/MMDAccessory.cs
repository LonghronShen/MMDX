using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Accessory;
using SlimDX;
using SlimDX.Direct3D9;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.SlimDX.Accessory
{
    /// <summary>
    /// MMD用アクセサリデータ
    /// </summary>
    public class MMDAccessory : MMDAccessoryBase, IDisposable
    {
        Mesh m_mesh;
        Effect[] m_effects;
        string filename;
        MMDAccessoryFactory factory;
        Matrix ScalingBias = Matrix.Scaling(10, 10, -10);
        /// <summary>
        /// エッジ有効化
        /// </summary>
        /// <remarks>全パーツで有効化されます</remarks>
        public bool Edge { get; set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mesh">メッシュ</param>
        /// <param name="effects">エフェクト</param>
        /// <param name="screen">スクリーンタイプのパーツかどうか</param>
        /// <param name="filename">アクセサリのファイル名</param>
        /// <param name="factory">このアクセサリを作ったファクトリー</param>
        public MMDAccessory(Mesh mesh, Effect[] effects, bool[] screen, string filename, MMDAccessoryFactory factory)
        {
            m_mesh = mesh;
            m_effects = effects;
            Screen = screen;
            Edge = false;
            this.filename = filename;
            this.factory = factory;
            SlimMMDXCore.Instance.LostDevice += OnLostDevice;
            SlimMMDXCore.Instance.ResetDevice += OnResetDevice;
        }
        void OnLostDevice()
        {
            foreach (var effect in m_effects)
                effect.OnLostDevice();
        }
        void OnResetDevice()
        {
            foreach (var effect in m_effects)
                effect.OnResetDevice();
        }

        /// <summary>
        /// アクセサリパーツの描画
        /// </summary>
        /// <param name="Position">描画する位置を示したマトリクス</param>
        protected override void Draw(ref Matrix Position)
        {
            MMDDrawingMode mode = MMDDrawingMode.Normal;
            if (SlimMMDXCore.Instance.EdgeManager != null && SlimMMDXCore.Instance.EdgeManager.IsEdgeDetectionMode)
            {
                mode = MMDDrawingMode.Edge;
            }
            //デバイス設定
            switch (mode)
            {
                case MMDDrawingMode.Normal:
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaFunc, Compare.Greater);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaTestEnable, true);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaRef, 0);
                    break;
                case MMDDrawingMode.Edge:
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaBlendEnable, false);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaFunc, Compare.Always);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaTestEnable, false);
                    SlimMMDXCore.Instance.Device.SetRenderState(RenderState.AlphaRef, 0);
                    break;
                default:
                    throw new NotImplementedException();
            }
            SlimMMDXCore.Instance.Device.SetRenderState(RenderState.CullMode, Cull.None);
            for (int i = 0; i < m_effects.Length; i++)
            {
                Matrix view, projection;
                //カメラ情報の取得
                Viewport viewport = m_effects[i].Device.Viewport;
                float aspectRatio = (float)viewport.Width / (float)viewport.Height;
                SlimMMDXCore.Instance.Camera.GetCameraParam(aspectRatio, out view, out projection);

                //マトリクス処理
                Matrix temp;//10倍スケーリングと左手→右手を(ムリヤリ)やる
                Matrix.Multiply(ref ScalingBias, ref Position, out temp);
                m_effects[i].SetValue("World", temp);// Position);
                m_effects[i].SetValue("View", view);
                m_effects[i].SetValue("Projection", projection);
                m_effects[i].SetValue("EyePosition", SlimMMDXCore.Instance.Camera.Position);
                //ライティング処理
                Vector3 color, dir;
                SlimMMDXCore.Instance.Light.GetLightParam(out color, out dir);
                m_effects[i].SetValue("AmbientLightColor", color);
                m_effects[i].SetValue("DirLight0Direction", dir);
                m_effects[i].SetValue("Edge", Edge);
                //テクニック設定
                switch (mode)
                {
                    case MMDDrawingMode.Normal:
                        m_effects[i].Technique = "MMDEffect";
                        break;
                    case MMDDrawingMode.Edge:
                        m_effects[i].Technique = "MMDNormalDepth";
                        break;
                    default:
                        throw new NotImplementedException();
                }
                //スクリーン処理
                if (Screen[i] && SlimMMDXCore.Instance.ScreenManager != null)
                    m_effects[i].SetTexture("Texture", SlimMMDXCore.Instance.ScreenManager.Screen);
                

                //描画
                m_effects[i].Begin();
                m_effects[i].BeginPass(0);
                m_mesh.DrawSubset(i);

                m_effects[i].EndPass();
                m_effects[i].End();
            }
        }
        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose()
        {
            SlimMMDXCore.Instance.LostDevice -= OnLostDevice;
            SlimMMDXCore.Instance.ResetDevice -= OnResetDevice;
        }
    }
}
