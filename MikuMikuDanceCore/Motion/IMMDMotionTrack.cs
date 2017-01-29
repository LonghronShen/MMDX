using System;
using System.Collections.Generic;
using MikuMikuDance.Core.Misc;
namespace MikuMikuDance.Core.Motion
{

    /// <summary>
    /// モーショントラックインタフェース。モーションの再生管理及びポーズ差分の計算を行う
    /// </summary>
    public interface IMMDMotionTrack
    {

        /// <summary>
        /// ブレンディングファクター
        /// </summary>
        /// <remarks>このトラックのモーションをどの程度モデルに適応するか。0～1の範囲の値。</remarks>
        float BlendingFactor { get; set; }

        /// <summary>
        /// 現在のボーン差分一覧
        /// </summary>
        Dictionary<string, SQTTransform> SubPoses { get; }
        /// <summary>
        /// 現在の表情一覧
        /// </summary>
        Dictionary<string, float> Faces { get; }
        /// <summary>
        /// モーション再生用FPS
        /// </summary>
        decimal FramePerSecond { get; set; }
        /// <summary>
        /// 再生中かどうか
        /// </summary>
        bool IsPlaying { get; }
        /// <summary>
        /// 最大フレーム数
        /// </summary>
        decimal MaxFrame { get; }
        /// <summary>
        /// 現在の再生位置の取得/設定
        /// </summary>
        decimal NowFrame { get; set; }
        /// <summary>
        /// モーション再生終了時に呼ばれるイベント
        /// </summary>
        /// <remarks>トラック名が帰ってくる</remarks>
        event Action<string> OnMotionEnd;
        /// <summary>
        /// トラックオプション
        /// </summary>
        MMDMotionTrackOptions Options { get; set; }
        /// <summary>
        /// 逆再生
        /// </summary>
        bool Reverse { get; set; }
        /// <summary>
        /// モーションの再生
        /// </summary>
        void Start();
        /// <summary>
        /// モーションの再生
        /// </summary>
        /// <param name="LoopPlay">ループ再生</param>
        void Start(bool LoopPlay);
        /// <summary>
        /// モーションの停止
        /// </summary>
        void Stop();
        /// <summary>
        /// 巻き戻し
        /// </summary>
        void Reset();
        /// <summary>
        /// トラック状態の更新
        /// </summary>
        /// <param name="elapsedSeconds">経過時間</param>
        /// <param name="TrackName">トラック名</param>
        /// <remarks>SubPosesとFacesが更新される</remarks>
        void Update(float elapsedSeconds, string TrackName);
    }
}
