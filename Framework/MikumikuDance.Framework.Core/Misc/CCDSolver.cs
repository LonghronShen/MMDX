using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;


namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// CCD-IKソルバー
    /// </summary>
    /// <remarks>Cyclic-Coordinate-Descent(CCD)法によるIK計算クラス</remarks>
    public class CCDSolver : IIKSolver
    {
        const double errToleranceSq = 1.0e-8f;

        /// <summary>
        /// IKリミッター（DI対応、null時はMMDCore.Current.IKLimitterを使用）
        /// </summary>
        public IIKLimitter IKLimitter { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ikLimitter">IKリミッター（省略時はMMDCore.Current.IKLimitter）</param>
        public CCDSolver(IIKLimitter ikLimitter = null)
        {
            IKLimitter = ikLimitter;
        }

        private IIKLimitter ResolveLimitter()
        {
            return IKLimitter ?? (MMDCore.Current != null ? MMDCore.Current.IKLimitter : null);
        }

        /// <summary>
        /// IKのソルブ
        /// </summary>
        /// <param name="ik">対象IK</param>
        /// <param name="BoneManager">ボーンマネージャ</param>
        /// <returns>呼び出し側でUpdateGlobalをもう一度呼ぶ場合はtrue</returns>
        public bool Solve(MMDIK ik, MMDBoneManager BoneManager)
        {
            MMDVector3 localTargetPos = MMDVector3.Zero;
            MMDVector3 localEffectorPos = MMDVector3.Zero;
            IIKLimitter limitter = ResolveLimitter();
            //エフェクタとなるボーンを取得
            MMDBone effector = ik.IKTargetBone;
            //IK対象のボーンのGlobalを更新(別のIK影響下のボーンからIKチェインが出ている場合があるので)
            MMDMatrix local;
            for (int i = ik.IKChildBones.Count - 1; i >= 0; --i)
            {//順番に親子関係になっている。(Processorでチェックかけてある
                //GlobalTransformを仮更新
                int parentBone = ik.IKChildBones[i].SkeletonHierarchy;
                ik.IKChildBones[i].LocalTransform.CreateMatrix(out local);
                MMDMatrix.Multiply(ref local, ref BoneManager[parentBone].GlobalTransform, out ik.IKChildBones[i].GlobalTransform);
            }
            effector.LocalTransform.CreateMatrix(out local);
            MMDMatrix.Multiply(ref local, ref BoneManager[effector.SkeletonHierarchy].GlobalTransform, out effector.GlobalTransform);

            //ターゲット位置の取得
            MMDVector3 targetPos; 
            MMDXMath.GetTranslation(ref ik.IKBone.GlobalTransform, out targetPos);

            //最大ループ回数分ループ
            for (int it = 0; it < ik.Iteration; ++it)
            {
                for (int nodeIndex = 0; nodeIndex < ik.IKChildBones.Count; ++nodeIndex)
                {//子ノードを子から順番に……
                    MMDBone node = ik.IKChildBones[nodeIndex];
                    //エフェクタの位置
                    MMDVector3 effectorPos;
                    MMDXMath.GetTranslation(ref effector.GlobalTransform, out effectorPos);
                    // 注目ノードの位置の取得
                    MMDVector3 jointPos;
                    MMDXMath.GetTranslation(ref node.GlobalTransform, out jointPos);

                    // ワールド座標系から注目ノードの局所座標系への変換
                    MMDMatrix invCoord;
                    MMDMatrix.Invert(ref node.GlobalTransform, out invCoord);
                    // 各ベクトルの座標変換を行い、検索中のボーンi基準の座標系にする
                    // (1) 注目ノード→エフェクタ位置へのベクトル(a)(注目ノード)
                    MMDVector3.Transform(ref effectorPos, ref invCoord, out localEffectorPos);
                    // (2) 基準関節i→目標位置へのベクトル(b)(ボーンi基準座標系)
                    MMDVector3.Transform(ref targetPos, ref invCoord, out localTargetPos);
                    // (1) 基準関節→エフェクタ位置への方向ベクトル
                    MMDVector3 basis2Effector = MMDVector3.Normalize(localEffectorPos);
                    // (2) 基準関節→目標位置への方向ベクトル
                    MMDVector3 basis2Target = MMDVector3.Normalize(localTargetPos);

                    // 回転角
                    float rotationDotProduct = MMDVector3.Dot(basis2Effector, basis2Target);
                    float rotationAngle = (float)Math.Acos(rotationDotProduct);

                    //回転量制限をかける
                    if (rotationAngle > MMDMathHelper.Pi * ik.ControlWeight * (nodeIndex + 1))
                        rotationAngle = MMDMathHelper.Pi * ik.ControlWeight * (nodeIndex + 1);
                    if (rotationAngle < -MMDMathHelper.Pi * ik.ControlWeight * (nodeIndex + 1))
                        rotationAngle = -MMDMathHelper.Pi * ik.ControlWeight * (nodeIndex + 1);

                    // 回転軸
                    MMDVector3 rotationAxis = MMDVector3.Cross(basis2Effector, basis2Target);
                    if (limitter != null)
                        limitter.Adjust(node.Name, ref rotationAxis);
                    rotationAxis.Normalize();

                    if (!float.IsNaN(rotationAngle) && rotationAngle > 1.0e-3f && !MMDXMath.CheckNaN(rotationAxis))
                    {
                        // 関節回転量の補正
                        MMDQuaternion subRot = MMDXMath.CreateQuaternionFromAxisAngle(rotationAxis, rotationAngle);
                        MMDQuaternion.Multiply(ref subRot, ref node.LocalTransform.Rotation, out node.LocalTransform.Rotation);
                        if (limitter != null)
                            limitter.Adjust(node);
                        //関係ノードのグローバル座標更新
                        for (int i = nodeIndex; i >= 0; --i)
                        {//順番に親子関係になっている。(Processorでチェックかけてある
                            //GlobalTransformを仮更新
                            int parentBone = ik.IKChildBones[i].SkeletonHierarchy;
                            ik.IKChildBones[i].LocalTransform.CreateMatrix(out local);
                            MMDMatrix.Multiply(ref local, ref BoneManager[parentBone].GlobalTransform, out ik.IKChildBones[i].GlobalTransform);
                        }
                        effector.LocalTransform.CreateMatrix(out local);
                        MMDMatrix.Multiply(ref local, ref BoneManager[effector.SkeletonHierarchy].GlobalTransform, out effector.GlobalTransform);
                    }
                }
            }
            return true;//UpdateGlobalをもう一度呼ぶ
            //IKチェインにぶら下がってるIK影響外のボーンを更新するため。
        }


    }
}
