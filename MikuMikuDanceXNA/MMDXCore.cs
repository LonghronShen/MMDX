using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core;
using MikuMikuDance.XNA.Model;
using MikuMikuDance.Core.Model;
using Microsoft.Xna.Framework.Content;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.Core.Accessory;
using MikuMikuDance.XNA.Accessory;
using Microsoft.Xna.Framework.Graphics;

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
        public Effect EdgeEffect { get; set; }
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
        /// モデルをアセットより読み込む
        /// </summary>
        /// <param name="assetName">アセット名</param>
        /// <param name="content">コンテンツマネージャ</param>
        /// <returns>MMDモデル</returns>
        public MMDModel LoadModel(string assetName, ContentManager content)
        {
            return content.Load<MMDXModel>(assetName);
        }
        /// <summary>
        /// モーションをアセットより読み込む
        /// </summary>
        /// <param name="assetName">アセット名</param>
        /// <param name="content">コンテンツマネージャ</param>
        /// <returns>MMDモデル</returns>
        public MMDMotion LoadMotion(string assetName, ContentManager content)
        {
            return content.Load<MMDMotion>(assetName);
        }

        /// <summary>
        /// アクセサリをアセットより読み込む
        /// </summary>
        /// <param name="assetName">アセット名</param>
        /// <param name="content">コンテンツマネージャ</param>
        /// <returns>アクセサリ</returns>
        public MMDAccessory LoadAccessory(string assetName, ContentManager content)
        {
            return content.Load<MMDAccessory>(assetName);
        }
        /// <summary>
        /// VAC情報をアセットより読み込む
        /// </summary>
        /// <param name="assetName">アセット名</param>
        /// <param name="content">コンテンツマネージャ</param>
        /// <returns>VAC</returns>
        public MMD_VAC LoadVAC(string assetName, ContentManager content)
        {
            MMD_VAC result= content.Load<MMD_VAC>(assetName);
            return result;
        }
    }
}
