using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using MikuMikuDance.XNA.Misc;
using MikuMikuDance.XNA.Motion;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace MikuMikuDance.XNA.Model
{
    static class SkinningHelpers
    {

        public static MMDBoneManagerContent CreateBoneManager(NodeContent input, ContentProcessorContext context, int maxBones, out Dictionary<string, MMDMotionContent> motionData)
        {
            ValidateMesh(input, context, null);

            //スケルトンを探す
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
            {
                motionData = new Dictionary<string, MMDMotionContent>();
                return null;
            }
            //スケルトン全体の座標をモデル座標に焼き付ける
            FlattenTransforms(input, skeleton);

            //スケルトンデータの読み取り
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            //ボーン数チェック(int.Maxまで可能)
            if (bones.Count > maxBones)
            {
                throw new InvalidContentException(string.Format("スケルトンに{0}本のボーンが含まれています。サポートされている最大ボーン数は{1}本です.", bones.Count, maxBones));
            }
            List<MMDBoneContent> mmdBones = new List<MMDBoneContent>();
            List<MMDIKContent> mmdIKs = new List<MMDIKContent>();
            foreach (BoneContent bone in bones)
            {
                MMDBoneContent mmdBone = new MMDBoneContent();
                mmdBone.BindPose = SQTTransformContent.FromMatrix(bone.Transform);
                mmdBone.InverseBindPose = Microsoft.Xna.Framework.Matrix.Invert(bone.AbsoluteTransform);
                mmdBone.SkeletonHierarchy = bones.IndexOf(bone.Parent as BoneContent);
                if (bone.OpaqueData.ContainsKey("MMDBoneTag"))
                {
                    MMDBoneTag tag = bone.OpaqueData["MMDBoneTag"] as MMDBoneTag;
                    if (tag != null && tag.IKs.Count > 0)
                    {
                        foreach (var ik in tag.IKs)
                        {
                            MMDIKContent mmdik = new MMDIKContent();
                            mmdik.IKBoneIndex = bones.IndexOf(bone);
                            mmdik.IKTargetBoneIndex = bones.IndexOf(ik.IKTargetBone);
                            mmdik.Iteration = ik.Iteration;
                            mmdik.ControlWeight = ik.ControlWeight;
                            foreach (var ikchild in ik.IKChildBones)
                            {
                                mmdik.IKChildBones.Add(bones.IndexOf(ikchild));
                            }
                            mmdIKs.Add(mmdik);
                        }
                    }
                }
                
                mmdBone.Name = bone.Name;
                mmdBones.Add(mmdBone);
            }
            //IKターゲットの親子関係チェック
            foreach (var ik in mmdIKs)
            {
                if (mmdBones[ik.IKTargetBoneIndex].SkeletonHierarchy != ik.IKChildBones[0])
                    throw new InvalidContentException("IKTargetBoneの親がIKChildの0番目になっていません。モデル構造に問題がある可能性があります");
                for (int i = 0; i < ik.IKChildBones.Count - 1; i++)
                {
                    if (mmdBones[ik.IKChildBones[i]].SkeletonHierarchy != ik.IKChildBones[i + 1])
                        throw new InvalidContentException("IKChildの親子関係が正しくセットされていません。モデル構造に問題がある可能性があります");
                }
            }

            //アニメーションデータをMMDX形式に変更

            motionData = ProcessAnimations(skeleton.Animations, bones);

            return new MMDBoneManagerContent(mmdBones, mmdIKs);
        }

        /// <summary>
        /// モデルに付属しているアニメーションデータをMMD形式に変換する
        /// </summary>
        static Dictionary<string, MMDMotionContent> ProcessAnimations(AnimationContentDictionary animations, IList<BoneContent> bones)
        {
            if (animations.Count == 0)
                return new Dictionary<string, MMDMotionContent>();//空を返す
            // ボーン名からインデックスに変換する辞書テーブルを作る
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            //各アニメーションをMMDMotionData形式に変換
            Dictionary<string, MMDMotionContent> results;
            results = new Dictionary<string, MMDMotionContent>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                MMDMotionContent motion = ProcessAnimation(animation.Value, boneMap, bones);

                results.Add(animation.Key, motion);
            }

            return results;
        }

        /// <summary>
        /// アニメーションをMMDMotionData形式に変更
        /// </summary>
        static MMDMotionContent ProcessAnimation(AnimationContent animation, Dictionary<string, int> boneMap, IList<BoneContent> bones)
        {
            List<MMDBoneKeyFrameContent> keyframes = new List<MMDBoneKeyFrameContent>();

            //アニメーションチャンネルを処理
            foreach (var channel in animation.Channels)
            {
                //対応ボーン無しのアニメーションは無視
                if (!boneMap.ContainsKey(channel.Key))
                    continue;
                // キーフレームの変換
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    //MMDのモーション値*バインドポーズ＝モデルのモーション値なので、バインドポーズの逆をかける
                    Matrix InvBind = Matrix.Invert(bones[boneMap[channel.Key]].Transform);
                    Matrix transform = keyframe.Transform * InvBind;

                    // 行列の分解
                    SQTTransformContent rotTrans = SQTTransformContent.FromMatrix(transform);
                    rotTrans.Rotation.Normalize();

                    keyframes.Add(new MMDBoneKeyFrameContent()
                    {
                        BoneName = channel.Key,
                        FrameNo = (uint)System.Math.Round((double)keyframe.Time.Ticks * 30.0 / (double)Stopwatch.Frequency),
                        Location = rotTrans.Translation,
                        Quatanion = rotTrans.Rotation,
                        Scales = rotTrans.Scales,
                        Curve = CreateIdentityCurve()
                    });
                }
            }

            //ソート
            keyframes.Sort((x,y)=>(int)((long)x.FrameNo-(long)y.FrameNo));

            //エラーチェック
            if (keyframes.Count == 0)
                throw new InvalidContentException(
                    "キーフレームの無いアニメーションです。");
            
            //MMDMotionDataに落し込み
            MMDMotionContent result = new MMDMotionContent();
            //キーフレームリストを配列に変換
            result.BoneFrames = MotionHelper.SplitBoneMotion(keyframes.ToArray());
            //その他のものは空配列にする
            result.FaceFrames = new Dictionary<string, List<MMDFaceKeyFrameContent>>();
            result.CameraFrames = new List<MMDCameraKeyFrameContent>();
            result.LightFrames = new List<MMDLightKeyFrameContent>();
            return result;
        }



        /// <summary>
        /// このメッシュはスキンアニメーションに向いたものかチェックする
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context,
                                 string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // メッシュの整合性を調べる
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "{0}メッシュは{1}ボーンの子です。" +
                        "SkinnedModelProcessorはメッシュがボーンの子であるケースに" +
                        "対応していません。",
                        mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null,
                        "{0}メッシュはスキニング情報がないので変換しません",
                        mesh.Name);

                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                // このノードがボーンなら、ボーン内を調査中であることを覚えておく
                parentBoneName = node.Name;
            }

            // 再帰的処理(調査中にノードが消去されるので、
            // 子供達のコピーを走査する)
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }

        /// <summary>
        /// メッシュがスキニング情報をもっているか調べる
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 全てが同じ座標空間になるように、不必要な変換行列を
        /// モデル・ジオメトリに焼き付ける
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // スケルトンは処理しない
                if (child == skeleton)
                    continue;

                // ローカル変換行列をジオメトリに焼きつける
                MeshHelper.TransformScene(child, child.Transform);

                // 焼き付けたので、ローカル座標変換行列は
                // 単位行列(Matrix.Identity)になる
                child.Transform = Matrix.Identity;

                // 再帰呼び出し
                FlattenTransforms(child, skeleton);
            }
        }

        private static BezierCurveContent[] CreateIdentityCurve()
        {
            BezierCurveContent[] result = new BezierCurveContent[4];
            for (int i = 0; i < result.Length; i++)
            {
                result[i].v1 = new Vector2(0.25f, 0.25f);
                result[i].v2 = new Vector2(0.75f, 0.75f);
            }
            return result;
        }

        
    }
}
