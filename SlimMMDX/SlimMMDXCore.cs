using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core;
using SlimDX.Direct3D9;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.SlimDX.Model;
using MikuMikuDance.Core.Model;
using MikuMikuDance.SlimDX.Accessory;

namespace MikuMikuDance.SlimDX
{
    /// <summary>
    /// SlimMMDX用のMMDコアクラス
    /// </summary>
    public class SlimMMDXCore : MMDCore
    {
        //セットアップ用
        static Device s_device = null;
        static Type s_factory = null;
        /// <summary>
        /// Singletonオブジェクト
        /// </summary>
        protected static SlimMMDXCore m_inst2 = null;
        /// <summary>
        /// スクリーンマネージャ
        /// </summary>
        public ScreenManager ScreenManager { get; set; }

        /// <summary>
        /// ロストデバイスイベント
        /// </summary>
        public event Action LostDevice;
        /// <summary>
        /// リセットデバイスイベント
        /// </summary>
        public event Action ResetDevice;

        /// <summary>
        /// Instanceプロパティ(Singleton)
        /// </summary>
        /// <remarks>このプロパティを使用する前にSetup関数を呼ぶこと</remarks>
        public static new SlimMMDXCore Instance
        {
            get
            {
                if (m_inst == null)
                {
                    if (s_device == null)
                        throw new MMDXException("Instanceプロパティを使用する前にSetupを呼び出す必要があります");
                    m_inst2 = new SlimMMDXCore();
                    m_inst = m_inst2;
                }
                if (m_inst2 == null)
                {
                    m_inst2 = m_inst as SlimMMDXCore;
                    if (m_inst2 == null)
                        throw new MMDXException("エラー：m_instの型が" + m_inst.GetType().Name + "。呼び出し順番エラー？");//来るはず無いんだけど……
                }
                return m_inst2;
            }
        }
        /// <summary>
        /// SlimMMDXのセットアップ
        /// </summary>
        /// <param name="device">SlimDXのDirectXDevice</param>
        /// <param name="toonTexPath">デフォルトのトゥーンテクスチャのパスが入った10個の配列(toon01.bmpとか)</param>
        /// <param name="factory">ModelFactoryを差し替える場合、差し替え先のTypeを入れる</param>
        public static void Setup(Device device, string[] toonTexPath, Type factory=null)
        {
            if (factory != null)
            {
                if (!factory.IsClass && factory.IsAbstract && !factory.IsPublic)
                    throw new ArgumentException("factoryにはpublicクラスを指定すること。抽象型、インターフェイス、値型は指定できません。", "factory");
                if (factory.GetInterface(typeof(IMMDModelFactory).Name) == null)
                {
                    throw new ArgumentException("factoryにはIMMDModelFactoryを継承した型を指定する必要があります");
                }
            }
            s_device = device;
            ToonTexManager.Setup(toonTexPath);
            s_factory = factory;
        }

        //members
        Device m_device;
        /// <summary>
        /// SlimDXデバイス
        /// </summary>
        public Device Device { get { return m_device; } }

        //Singleton
        /// <summary>
        /// シングルトン用コンストラクタ
        /// </summary>
        protected SlimMMDXCore()
            : base()
        {
            m_device = s_device;
            if (s_factory == null)
            {
                ModelFactoryFromFile = new MMDModelFactory();
            }
            else
            {
                ModelFactoryFromFile = (IMMDModelFactory)Activator.CreateInstance(s_factory);
            }
            AccessoryFactoryFromFile = new MMDAccessoryFactory();
        }
        /// <summary>
        /// デバイスロスト時に呼び出す
        /// </summary>
        public void OnLostDevice()
        {
            if (LostDevice != null)
            {
                LostDevice();
            }
        }
        /// <summary>
        /// デバイスリセット時に呼び出す
        /// </summary>
        public void OnResetDevice()
        {
            if (ResetDevice != null)
            {
                ResetDevice();
            }
        }
        /// <summary>
        /// 破棄処理
        /// </summary>
        public override void Dispose()
        {
            m_device = null;
            base.Dispose();
        }
    }
}
