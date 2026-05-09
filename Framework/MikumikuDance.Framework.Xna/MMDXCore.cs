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
    /// MMDXのコアクラス（DI対応、非シングルトン）
    /// </summary>
    public class MMDXCore : MMDCore
    {
        /// <summary>
        /// ContentReader互換のための静的インスタンス（レガシー）。
        /// DI移行後はMMDCore.Currentを使用。
        /// </summary>
        [Obsolete("Use MMDCore.Current instead")]
        public static new MMDXCore Instance
        {
            get { return Current as MMDXCore; }
            set { Current = value; }
        }

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
        /// DI コンストラクタ（完全版）
        /// </summary>
        /// <param name="device">グラフィックデバイス抽象</param>
        /// <param name="loader">コンテンツローダー抽象</param>
        /// <param name="modelPartFactory">モデルパーツファクトリー（省略時はプラットフォームデフォルト）</param>
        public MMDXCore(
            IMMDGraphicsDevice device, 
            IMMDContentLoader loader,
            IMMDModelPartFactory modelPartFactory = null)
            : base(device, loader)
        {
            ModelPartFactory = modelPartFactory;
            InitializeFactory();
        }

        /// <summary>
        /// DI コンストラクタ（全引数指定版）
        /// </summary>
        public MMDXCore(
            IMMDGraphicsDevice device,
            IMMDContentLoader loader,
            IIKSolver ikSolver,
            IIKLimitter ikLimitter,
            IMMDModelPartFactory modelPartFactory = null)
            : base(device, loader, ikSolver, ikLimitter)
        {
            ModelPartFactory = modelPartFactory;
            InitializeFactory();
        }

        /// <summary>
        /// パーツファクトリの初期化
        /// </summary>
        private void InitializeFactory()
        {
            if (ModelPartFactory != null)
                return;
#if WINDOWS
            ModelPartFactory = new MMDGPUModelPartFactory();
#elif XBOX
            ModelPartFactory = new MMDXBoxModelPartFactory();
#else
            throw new NotImplementedException();
#endif
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
