using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Motion;

namespace MikuMikuDance.Core.Stages
{
    /// <summary>
    /// ステージ用アニメーションプレイヤー
    /// </summary>
    public class StagePlayer
    {
        Dictionary<string, MMDStageMotionTrack> motionTracks = new Dictionary<string, MMDStageMotionTrack>();

        /// <summary>
        /// モーショントラックの取得
        /// </summary>
        /// <param name="motionKey">モーショントラック名</param>
        /// <returns>モーショントラック</returns>
        public MMDStageMotionTrack this[string motionKey]
        {
            get
            {
                MMDStageMotionTrack result = null;
                if (!motionTracks.TryGetValue(motionKey, out result))
                    throw new KeyNotFoundException("モーション名 \"" + motionKey.ToString() + "\" は見つかりません");
                return result;
            }
        }
        /// <summary>
        /// モーションの追加
        /// </summary>
        /// <param name="motionKey">モーション識別用のモーション名</param>
        /// <param name="motionData">MikuMikuDance MotionData</param>
        public void AddMotion(string motionKey, MMDMotion motionData)
        {
            motionTracks.Add(motionKey, new MMDStageMotionTrack(motionData));
        }
        /// <summary>
        /// モーションの追加
        /// </summary>
        /// <param name="motionKey">モーション識別用のモーション名</param>
        /// <param name="motionData">MikuMikuDance MotionTrack</param>
        public void AddMotion(string motionKey, MMDStageMotionTrack motionData)
        {
            motionTracks.Add(motionKey, motionData);
        }

        /// <summary>
        /// モーションの削除
        /// </summary>
        /// <param name="motionKey"></param>
        public void RemoveMotion(string motionKey)
        {
            motionTracks.Remove(motionKey);
        }

        /// <summary>
        /// 全てのモーションの再生停止
        /// </summary>
        public void StopAll()
        {
            foreach (var motiontrack in motionTracks)
            {
                motiontrack.Value.Stop();
            }
        }
        internal void Update(float elapsedSeconds)
        {
            foreach (var track in motionTracks)
            {
                track.Value.Update(elapsedSeconds);
            }
        }
    }
}
