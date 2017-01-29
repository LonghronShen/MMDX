using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Misc;
using System.Collections.ObjectModel;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Model
{

    /// <summary>
    /// モデル関連の各種ヘルパ関数群
    /// </summary>
    public static class SkinningHelpers
    {
#if SlimDX
        /// <summary>
        /// 頂点変換
        /// </summary>
        /// <param name="bones">頂点</param>
        /// <param name="vertin">MMD頂点データ</param>
        /// <param name="outPosition">頂点データ</param>
        /// <param name="outNormal">法線データ</param>
        public static void SkinVertex(Matrix[] bones, MMDVertexNm vertin, out Vector4 outPosition, out Vector3 outNormal)
#else
        /// <summary>
        /// 頂点変換
        /// </summary>
        /// <param name="bones">頂点</param>
        /// <param name="vertin">MMD頂点データ</param>
        /// <param name="outPosition">頂点データ</param>
        /// <param name="outNormal">法線データ</param>
        public static void SkinVertex(Matrix[] bones, MMDVertexNm vertin, out Vector3 outPosition, out Vector3 outNormal)
#endif
        {
            //影響ボーン取得
            int b0 = vertin.BlendIndexX;
            int b1 = vertin.BlendIndexY;
            
            //ボーンマトリックスをブレンド
            Matrix skinnedTransformSum;
            Blend4x3Matrix(ref bones[b0], ref bones[b1], ref vertin.BlendWeights, out skinnedTransformSum);
            //ブレンドしたボーンによる変換を頂点に適応
            Vector3.Transform(ref vertin.Position, ref skinnedTransformSum, out outPosition);
            Vector3.TransformNormal(ref vertin.Normal, ref skinnedTransformSum, out outNormal);
        }

        static void Blend4x3Matrix(ref Matrix m1, ref Matrix m2, ref Vector2 weights, out Matrix blended)
        {
            blended = new Matrix();
            float w1 = weights.X;
            float w2 = weights.Y;

            blended.M11 = (m1.M11 * w1) + (m2.M11 * w2);
            blended.M12 = (m1.M12 * w1) + (m2.M12 * w2);
            blended.M13 = (m1.M13 * w1) + (m2.M13 * w2);
            blended.M21 = (m1.M21 * w1) + (m2.M21 * w2);
            blended.M22 = (m1.M22 * w1) + (m2.M22 * w2);
            blended.M23 = (m1.M23 * w1) + (m2.M23 * w2);
            blended.M31 = (m1.M31 * w1) + (m2.M31 * w2);
            blended.M32 = (m1.M32 * w1) + (m2.M32 * w2);
            blended.M33 = (m1.M33 * w1) + (m2.M33 * w2);
            blended.M41 = (m1.M41 * w1) + (m2.M41 * w2);
            blended.M42 = (m1.M42 * w1) + (m2.M42 * w2);
            blended.M43 = (m1.M43 * w1) + (m2.M43 * w2);

            blended.M14 = 0.0f;
            blended.M24 = 0.0f;
            blended.M34 = 0.0f;
            blended.M44 = 1.0f;
            
        }

        /// <summary>
        /// ボーンインデックス→ボーンオブジェクト化(XNAのインポータ用)
        /// </summary>
        public static void IKSetup(List<MMDIK> iks, List<MMDBone> bones)
        {
            foreach (var ik in iks)
            {
                ik.IKBone = bones[ik.IKBoneIndex];
                ik.IKTargetBone = bones[ik.IKTargetBoneIndex];
                List<MMDBone> ikchilds = new List<MMDBone>();
                foreach (var ikci in ik.ikChildBoneIndex)
                    ikchilds.Add(bones[ikci]);
                ik.IKChildBones = new ReadOnlyCollection<MMDBone>(ikchilds);
            }
        }

        
    }
}
