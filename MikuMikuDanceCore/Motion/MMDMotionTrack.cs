using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Misc;
using System.Diagnostics;
#if XNA
using Microsoft.Xna.Framework;
#endif
namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// モーショントラック。モーションの再生管理及びポーズ差分の計算を行う
    /// </summary>
    public class MMDMotionTrack : MikuMikuDance.Core.Motion.IMMDMotionTrack
    {
        /// <summary>
        /// デフォルトモーション再生FPS
        /// </summary>
        public const decimal DefaultFPS = 30m;

        bool bLoopPlay = false;
        bool bReverse = false;
        decimal m_NowFrame = 0;
        decimal m_MaxFrame = 0;
        bool bStart = false;
        bool bUserChangeFrame = false;
        //モーションデータ
        Dictionary<string, List<MMDBoneKeyFrame>> boneFrames;
        Dictionary<string, List<MMDFaceKeyFrame>> faceFrames;
        //モーションデータの読み出し位置
        Dictionary<string, int> bonePos = new Dictionary<string, int>();
        Dictionary<string, int> facePos = new Dictionary<string, int>();
        //ブレンディングファクター
        float m_blendingFactor = 1;

        //トラックから抽出されたボーンの差分一覧
        Dictionary<string, SQTTransform> subPoses;
        /// <summary>
        /// 現在のボーン差分一覧
        /// </summary>
        public Dictionary<string, SQTTransform> SubPoses { get { return subPoses; } }
        Dictionary<string, float> faces;
        //トラックから抽出された表情値の一覧
        /// <summary>
        /// 現在の表情一覧
        /// </summary>
        public Dictionary<string, float> Faces { get { return faces; } }
        
        /// <summary>
        /// ブレンディングファクター
        /// </summary>
        /// <remarks>このトラックのモーションをどの程度モデルに適応するか。0～1の範囲の値。</remarks>
        public float BlendingFactor { 
            get { return m_blendingFactor; }
            set { m_blendingFactor = MathHelper.Clamp(value, 0, 1); }
        }
        /// <summary>
        /// モーション再生用FPS
        /// </summary>
        public decimal FramePerSecond { get; set; }
        /// <summary>
        /// 現在の再生位置
        /// </summary>
        public decimal NowFrame { get { return m_NowFrame; } set { m_NowFrame = value; bUserChangeFrame = true; } }
        /// <summary>
        /// 最大フレーム数
        /// </summary>
        public decimal MaxFrame { get { return m_MaxFrame; } }
        /// <summary>
        /// 再生中かどうか
        /// </summary>
        public bool IsPlaying { get { return bStart; } }
        /// <summary>
        /// トラックオプション
        /// </summary>
        public MMDMotionTrackOptions Options { get; set; }
        /// <summary>
        /// 逆再生
        /// </summary>
        public bool Reverse { get { return bReverse; } set { bReverse = value; } }
        /// <summary>
        /// モーション再生終了時に呼ばれるイベント
        /// </summary>
        /// <remarks>トラック名が帰ってくる</remarks>
        public event Action<string> OnMotionEnd;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionData">モーションデータ</param>
        /// <param name="options">トラックオプション</param>
        public MMDMotionTrack(MMDMotion motionData, MMDMotionTrackOptions options)
        {
            Options = options;
            //ボーンの配列抜き出し
            boneFrames = motionData.BoneFrames;
            //表情の配列抜き出し
            faceFrames = motionData.FaceFrames;
            //モーションのFPS=30
            FramePerSecond = DefaultFPS;
            //差分一覧を作成
            subPoses = new Dictionary<string, SQTTransform>(motionData.BoneFrames.Count);
            //表情一覧を作成
            faces = new Dictionary<string, float>(motionData.FaceFrames.Count);
            //現在の再生位置を設定&最大フレーム数のチェック
            foreach (var it in motionData.BoneFrames)
            {
                bonePos.Add(it.Key, 0);
                foreach (var it2 in it.Value)
                {
                    if (it2.FrameNo > m_MaxFrame)
                        m_MaxFrame = it2.FrameNo;
                }
            }
            foreach (var it in motionData.FaceFrames)
            {
                facePos.Add(it.Key, 0);
                foreach (var it2 in it.Value)
                {
                    if (it2.FrameNo > m_MaxFrame)
                        m_MaxFrame = it2.FrameNo;
                }
            }
        }

        /// <summary>
        /// モーションの再生
        /// </summary>
        public void Start()
        {
            Start(false);
        }
        /// <summary>
        /// モーションの再生
        /// </summary>
        /// <param name="LoopPlay">ループ再生</param>
        public void Start(bool LoopPlay)
        {
            bLoopPlay = LoopPlay;
            bStart = true;
        }
        /// <summary>
        /// モーションの停止
        /// </summary>
        public void Stop()
        {
            InnerStop();
            //TimeUpdate();
        }
        void InnerStop()
        {
            bStart = false;
        }
        /// <summary>
        /// 巻き戻し
        /// </summary>
        public void Reset()
        {
            bStart = false;
            if (bReverse)
                m_NowFrame = m_MaxFrame;
            else
                m_NowFrame = 0;
            Rewind();
        }
        /// <summary>
        /// トラック状態の更新
        /// </summary>
        /// <param name="elapsedSeconds">経過時間</param>
        /// <param name="TrackName">トラック名</param>
        /// <remarks>SubPosesとFacesが更新される</remarks>
        public void Update(float elapsedSeconds, string TrackName)
        {
            bool CallMotionEnd = TimeUpdate(elapsedSeconds);
            SubPoses.Clear();
            Faces.Clear();
            if (!bStart && (Options & MMDMotionTrackOptions.UpdateWhenStopped) == 0)
                return;
            //ボーンの更新
            foreach (var frameList in boneFrames)
            {
                //カーソル位置の更新
                int CursorPos = bonePos[frameList.Key];
                if (!bUserChangeFrame && ((bReverse && CursorPos == 0) || (!bReverse && CursorPos == frameList.Value.Count)) && (Options & MMDMotionTrackOptions.ExtendedMode) == 0)
                {
                    continue;//このボーンの再生終わり
                }
                if (!bReverse)
                {
                    for (; CursorPos < frameList.Value.Count && frameList.Value[CursorPos].FrameNo < m_NowFrame; ++CursorPos) ;
                    for (; CursorPos > 0 && frameList.Value[CursorPos - 1].FrameNo > m_NowFrame; --CursorPos) ;
                }
                else
                {
                    for (; CursorPos > 0 && frameList.Value[CursorPos - 1].FrameNo > m_NowFrame; --CursorPos) ;
                    for (; CursorPos < frameList.Value.Count && frameList.Value[CursorPos].FrameNo < m_NowFrame; ++CursorPos) ;
                }
                bonePos[frameList.Key] = CursorPos;
                if (CursorPos == 0)
                {//逆再生時の最終フレーム
                    SQTTransform subPose;
                    frameList.Value[0].GetSQTTransform(out subPose);
                    SubPoses.Add(frameList.Key, subPose);
                }
                else if (CursorPos == frameList.Value.Count)
                {//通常再生時の最終フレーム
                    SQTTransform subPose;
                    frameList.Value[CursorPos - 1].GetSQTTransform(out subPose);
                    SubPoses.Add(frameList.Key, subPose);
                }
                else
                {
                    //時間経過取得
                    float Progress = ((float)m_NowFrame - (float)frameList.Value[CursorPos - 1].FrameNo) / ((float)frameList.Value[CursorPos].FrameNo - (float)frameList.Value[CursorPos - 1].FrameNo);
                    
                    
                    SQTTransform subPose;
                    MMDBoneKeyFrame pose1 = frameList.Value[CursorPos - 1], pose2 = frameList.Value[CursorPos];
                    MMDBoneKeyFrame.Lerp(pose1, pose2, Progress, out subPose);
                    SubPoses.Add(frameList.Key, subPose);
                }
            }
            foreach (var frameList in faceFrames)
            {
                int CursorPos = facePos[frameList.Key];
                if (!bUserChangeFrame && ((bReverse && CursorPos == 0) || (!bReverse && CursorPos == frameList.Value.Count)) && (Options & MMDMotionTrackOptions.ExtendedMode) == 0)
                    continue;//この表情の再生終わり
                if (!bReverse)
                {
                    for (; CursorPos < frameList.Value.Count && frameList.Value[CursorPos].FrameNo < m_NowFrame; ++CursorPos) ;
                    for (; CursorPos > 0 && frameList.Value[CursorPos - 1].FrameNo > m_NowFrame; --CursorPos) ;
                }
                else
                {
                    for (; CursorPos > 0 && frameList.Value[CursorPos - 1].FrameNo > m_NowFrame; --CursorPos) ;
                    for (; CursorPos < frameList.Value.Count && frameList.Value[CursorPos].FrameNo < m_NowFrame; ++CursorPos) ;
                }
                facePos[frameList.Key] = CursorPos;
                if (CursorPos == 0)
                {//逆再生時の最終フレーム
                    Faces.Add(frameList.Key, frameList.Value[0].Rate);
                }
                else if ( CursorPos == frameList.Value.Count)
                {
                    Faces.Add(frameList.Key, frameList.Value[CursorPos - 1].Rate);
                }
                else
                {
                    //時間経過取得
                    float Progress = ((float)m_NowFrame - (float)frameList.Value[CursorPos - 1].FrameNo) / ((float)frameList.Value[CursorPos].FrameNo - (float)frameList.Value[CursorPos - 1].FrameNo);
                    MMDFaceKeyFrame pose1 = frameList.Value[CursorPos - 1], pose2 = frameList.Value[CursorPos];
                    Faces.Add(frameList.Key, MMDFaceKeyFrame.Lerp(pose1, pose2, Progress));
                }
            }
            bUserChangeFrame = false;
            if (CallMotionEnd && OnMotionEnd != null)
                OnMotionEnd(TrackName);
        }

        private bool TimeUpdate(float elapsedSeconds)
        {
            if (!bStart)
                return false;
            bool result = false;
            if (!bReverse)
            {
                m_NowFrame += ((decimal)elapsedSeconds) * FramePerSecond;
            }
            else
            {
                m_NowFrame -= ((decimal)elapsedSeconds) * FramePerSecond;
            }
            if (m_NowFrame > m_MaxFrame)
            {
                if (bLoopPlay)
                    Rewind();
                else
                {
                    InnerStop();
                    result = true;
                    m_NowFrame = m_MaxFrame;
                }
            }
            if (m_NowFrame < 0)
            {
                if (bLoopPlay)
                    Rewind();
                else
                {
                    InnerStop();
                    result = true;
                    m_NowFrame = 0;
                }
            } 
            return result;
        }
        //巻き戻し処理
        private void Rewind()
        {
            if (!bReverse)
            {
                while (m_NowFrame >= m_MaxFrame && m_MaxFrame != 0)
                    m_NowFrame -= m_MaxFrame;
                //ボーンの更新
                foreach (var frameList in boneFrames)
                    bonePos[frameList.Key] = 0;
                foreach (var faceList in faceFrames)
                    facePos[faceList.Key] = 0;
            }
            else
            {
                while (m_NowFrame <= 0 && m_MaxFrame != 0)
                    m_NowFrame += m_MaxFrame;
                //ボーンの更新
                foreach (var frameList in boneFrames)
                    bonePos[frameList.Key] = frameList.Value.Count - 1;
                foreach (var faceList in faceFrames)
                    facePos[faceList.Key] = faceList.Value.Count - 1;
            }
        }
    }
}
