using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core;
using MikuMikuDance.XNA.Model;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.Core.Accessory;
using MikuMikuDance.XNA.Accessory;
using MikumikuDance.Framework.Abstractions;

namespace MikuMikuDance.XNA
{
    /// <summary>
    /// MMDXのコアクラス
    /// </summary>
    public class MMDXCore : MMDCore
    {
        //シングルトン
        static MMDXCore m_inst2;
        
        /// <summary>
        /// モデルパーツファクトリー
        /// </summary>
        public IMMDModelPartFactory ModelPartFactory { get; set; }

        /// <summary>
        /// スクリーンマネージャ
        /// </summary>
        public ScreenManager ScreenManager { get; set; }
        /// <summary>
        /// エッジ描画用エフェクト
        /// </summary>
        public IMMDEffect EdgeEffect { get; set; }
        /// <summary>
        /// DI コンストラクタ
        /// </summary>
        protected MMDXCore(IMMDGraphicsDevice device, IMMDContentLoader loader)
            : base(device, loader)
        {
#if WINDOWS
            ModelPartFactory = new MMDGPUModelPartFactory();
#elif XBOX
            ModelPartFactory = new MMDXBoxModelPartFactory();
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// 規定のコンストラクタ
        /// </summary>
        protected MMDXCore()
            : base()
        {
#if WINDOWS
            ModelPartFactory = new MMDGPUModelPartFactory();
#elif XBOX
            ModelPartFactory = new MMDXBoxModelPartFactory();
#else
            throw new NotImplementedException();
#endif
        }
        /// <summary>
        /// Singletonインスタンス
        /// </summary>
        public new static MMDXCore Instance
        {
            get
            {
                if (m_inst == null)
                {
                    m_inst2 = new MMDXCore();
                    m_inst = m_inst2;
                }
                if (m_inst2 == null)
                {
                    m_inst2 = m_inst as MMDXCore;
                    if (m_inst2 == null)
                        throw new MMDXException("エラー：m_instの型が" + m_inst.GetType().Name + "。呼び出し順番エラー？");//来るはず無いんだけど……
                }
                return m_inst2;
            }
        }

        /// <summary>
        /// モデルをアセットより読み込む (IMMDContentLoader)
        /// </summary>
        /// <param name="assetName">アセット名</param>
        /// <param name="loader">コンテンツローダー抽象</param>
        /// <returns>MMDモデル</returns>
        public MMDModel LoadModel(string assetName, IMMDContentLoader loader)
        {
            return loader.LoadModel<MMDXModel>(assetName);
        }
        /// <summary>
        /// モデルをアセットより読み込む (ContentManager, レガシー)
        /// </summary>
        public MMDModel LoadModel(string assetName, Microsoft.Xna.Framework.Content.ContentManager content)
        {
            return content.Load<MMDXModel>(assetName);
        }
        /// <summary>
        /// モーションをアセットより読み込む (IMMDContentLoader)
        /// </summary>
        /// <param name="assetName">アセット名</param>
        /// <param name="loader">コンテンツローダー抽象</param>
        public MMDMotion LoadMotion(string assetName, IMMDContentLoader loader)
        {
            return loader.LoadModel<MMDMotion>(assetName);
        }
        /// <summary>
        /// モーションをアセットより読み込む (ContentManager, レガシー)
        /// </summary>
        public MMDMotion LoadMotion(string assetName, Microsoft.Xna.Framework.Content.ContentManager content)
        {
            return content.Load<MMDMotion>(assetName);
        }

        /// <summary>
        /// アクセサリをアセットより読み込む (IMMDContentLoader)
        /// </summary>
        /// <param name="assetName">アセット名</param>
        /// <param name="loader">コンテンツローダー抽象</param>
        public MMDAccessory LoadAccessory(string assetName, IMMDContentLoader loader)
        {
            return loader.LoadModel<MMDAccessory>(assetName);
        }
        /// <summary>
        /// アクセサリをアセットより読み込む (ContentManager, レガシー)
        /// </summary>
        public MMDAccessory LoadAccessory(string assetName, Microsoft.Xna.Framework.Content.ContentManager content)
        {
            return content.Load<MMDAccessory>(assetName);
        }
        /// <summary>
        /// VAC情報をアセットより読み込む (IMMDContentLoader)
        /// </summary>
        /// <param name="assetName">アセット名</param>
        /// <param name="loader">コンテンツローダー抽象</param>
        public MMD_VAC LoadVAC(string assetName, IMMDContentLoader loader)
        {
            return loader.LoadModel<MMD_VAC>(assetName);
        }
        /// <summary>
        /// VAC情報をアセットより読み込む (ContentManager, レガシー)
        /// </summary>
        public MMD_VAC LoadVAC(string assetName, Microsoft.Xna.Framework.Content.ContentManager content)
        {
            MMD_VAC result= content.Load<MMD_VAC>(assetName);
            return result;
        }
    }
}
