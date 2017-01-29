using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;
using System.Collections;

#if XNA
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif

namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// モーショントラックの管理、ポーズのブレンディング等を行うクラス
    /// </summary>
    public class AnimationPlayer
    {
        Dictionary<string, IMMDMotionTrack> motionTracks = new Dictionary<string, IMMDMotionTrack>();
        MMDBoneManager boneManager;
        IMMDFaceManager faceManager;
        Dictionary<string, SQTTransform> BindPoses;
        Dictionary<string, SQTTransform> Poses;
        Dictionary<string, float> Faces;
        internal AnimationPlayer(MMDBoneManager bones, IMMDFaceManager faces)
        {
            boneManager = bones;
            faceManager = faces;
            BindPoses = new Dictionary<string, SQTTransform>();
            for (int i = 0; i < boneManager.Count; ++i)
            {
                BindPoses.Add(boneManager[i].Name, boneManager[i].BindPose);
            }
            Poses = new Dictionary<string, SQTTransform>(BindPoses.Count);
            Faces = new Dictionary<string, float>(faces.Count);
        }

        /// <summary>
        /// モーショントラックの取得
        /// </summary>
        /// <param name="motionKey">モーショントラック名</param>
        /// <returns>モーショントラック</returns>
        public IMMDMotionTrack this[string motionKey] 
        { 
            get 
            {
#if !XBOX
                IMMDMotionTrack result = null;
                if (!motionTracks.TryGetValue(motionKey, out result))
                    throw new KeyNotFoundException("モーション名 \"" + motionKey.ToString() + "\" は見つかりません");
                return result;
#else
                return motionTracks[motionKey];
#endif
            } 
        }
        /// <summary>
        /// 指定したキーのモーショントラックが含まれているか確認
        /// </summary>
        /// <param name="motionKey">モーションキー</param>
        /// <returns>含まれていればtrue</returns>
        public bool ContainsKey(string motionKey)
        {
            return motionTracks.ContainsKey(motionKey);
        }
        /// <summary>
        /// モーションの追加
        /// </summary>
        /// <param name="motionKey">モーション識別用のモーション名</param>
        /// <param name="motionData">MikuMikuDance MotionData</param>
        public void AddMotion(string motionKey, MMDMotion motionData)
        {
            motionTracks.Add(motionKey, new MMDMotionTrack(motionData, MMDMotionTrackOptions.None));
        }
        /// <summary>
        /// モーションの追加
        /// </summary>
        /// <param name="motionKey">モーション識別用のモーション名</param>
        /// <param name="motionData">MikuMikuDance MotionData</param>
        /// <param name="options">トラックオプション</param>
        public void AddMotion(string motionKey, MMDMotion motionData, MMDMotionTrackOptions options)
        {
            motionTracks.Add(motionKey, new MMDMotionTrack(motionData, options));
        }
        /// <summary>
        /// モーションの追加
        /// </summary>
        /// <param name="motionKey">モーション識別用のモーション名</param>
        /// <param name="motionData">MikuMikuDance IMotionTrack</param>
        public void AddMotion(string motionKey, IMMDMotionTrack motionData)
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
            MMDXProfiler.BeginMark("AnimationPlayerUpdate", MMDXMath.CreateColor(30, 255, 0));
            //差分ポーズを元にした加算ブレンディング
            Poses.Clear();
            Faces.Clear();
            foreach (var track in motionTracks)
            {
                track.Value.Update(elapsedSeconds, track.Key);
                foreach (var subpose in track.Value.SubPoses)
                {
                    SQTTransform pose1, pose2, sub = subpose.Value, result;
                    if (!Poses.TryGetValue(subpose.Key, out pose1))
                    {
                        if (!BindPoses.TryGetValue(subpose.Key, out pose1))
                            continue;//無いモーションは無視
                    }
                    SQTTransform.Multiply(ref sub, ref pose1, out pose2);
                    SQTTransform.Lerp(ref pose1, ref pose2, track.Value.BlendingFactor, out result);
                    Poses[subpose.Key] = result;
                }
                foreach (var subface in track.Value.Faces)
                {
                    float rate1 = 0f, rate2 = subface.Value;
                    if (!Faces.TryGetValue(subface.Key, out rate1))
                    {
                        if (!faceManager.ContainsKey(subface.Key))
                            continue;
                    }
                    Faces[subface.Key] = MathHelper.Lerp(rate1, rate2, track.Value.BlendingFactor);
                }
            }
            //ボーンマネージャへの書き戻し
            foreach (var pose in Poses)
            {
                if (!boneManager[pose.Key].IsPhysics || !MMDCore.Instance.UsePhysics)
                    boneManager[pose.Key].LocalTransform = pose.Value;
            }
            foreach (var face in Faces)
            {
                faceManager[face.Key] = face.Value;
            }
            MMDXProfiler.EndMark("AnimationPlayerUpdate");
        }

        
    }

    
}
