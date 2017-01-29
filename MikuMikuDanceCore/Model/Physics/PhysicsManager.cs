using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using BulletX.BulletDynamics.Dynamics;
using BulletX.BulletDynamics.ConstraintSolver;
using BulletX.BulletCollision.CollisionShapes;
using BulletX.LinerMath;
using BulletX.BulletCollision.CollisionDispatch;
using MikuMikuDance.Core.MultiThreads;
using MikuMikuDance.Core.Misc;

#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Model.Physics
{
    /// <summary>
    /// モデル物理マネージャ
    /// </summary>
    public class PhysicsManager : IDisposable
    {
        internal List<MMDMotionState> motionState;
        private int nReset = 0;
        private bool bDisposing = false;//自爆スイッチ
        private bool bSetup = false;//セットアップスイッチ
        /// <summary>
        /// 剛体
        /// </summary>
        public ReadOnlyCollection<RigidBody> Rigids { get; private set; }

        private short[] groups, masks;
        /// <summary>
        /// 間接
        /// </summary>
        public ReadOnlyCollection<Generic6DofSpringConstraint> Joints { get; private set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rigids">剛体情報</param>
        /// <param name="joints">関節情報</param>
        /// <param name="model">モデル情報</param>
        public PhysicsManager(MMDRigid[] rigids, MMDJoint[] joints, MMDModel model)
        {
            List<RigidBody> rbodies = new List<RigidBody>();
            List<Generic6DofSpringConstraint> dofjoints = new List<Generic6DofSpringConstraint>();
            motionState = new List<MMDMotionState>();
            List<short> groups = new List<short>();
            List<short> masks = new List<short>();
            //剛体の作成
            for (int i = 0; i < rigids.Length; i++)
            {
                MMDMotionState mstate;
                short group;
                rbodies.Add(CreateRigidBody(rigids[i], model, out group, out mstate));
                motionState.Add(mstate);
                groups.Add(group);
                masks.Add((short)rigids[i].GroupTarget);
            }
            //ジョイント(拘束)の作成
            for (int i = 0; i < joints.Length; i++)
            {
                RigidBody phisic0 = rbodies[(int)joints[i].RigidBodyA];
                RigidBody phisic1 = rbodies[(int)joints[i].RigidBodyB];
                dofjoints.Add(CreateJoint(phisic0, phisic1, joints[i], model));
            }

            Rigids = new ReadOnlyCollection<RigidBody>(rbodies);
            this.groups = groups.ToArray();
            this.masks = masks.ToArray();
            Joints = new ReadOnlyCollection<Generic6DofSpringConstraint>(dofjoints);
            bSetup = true;
            //イベントをフック
            PhysicsThreadManager.Instanse.Synchronize += new Action(Update);
            PhysicsThreadManager.Instanse.DropFrame += new Action<int>(DropFrame);
        }
        private RigidBody CreateRigidBody(MMDRigid rigid, MMDModel Model,out short group, out MMDMotionState motionStateStart)
        {
            CollisionShape collision;
            RigidBody body;
            //衝突スキンの作成
            switch (rigid.ShapeType)
            {
                case 0:
                    collision = new SphereShape(rigid.ShapeWidth);
                    break;
                case 1:
                    collision = new BoxShape(new btVector3(rigid.ShapeWidth, rigid.ShapeHeight, rigid.ShapeDepth));
                    break;
                case 2:
                    collision = new CapsuleShape(rigid.ShapeWidth, rigid.ShapeHeight);
                    break;
                default:
                    throw new NotImplementedException("不明な剛体タイプ");
            }


            motionStateStart = new MMDMotionState(rigid, Model);

            btVector3 localInertia = btVector3.Zero;
            //イナーシャの計算
            if (rigid.Type != 0)
                collision.calculateLocalInertia(rigid.Weight, out localInertia);


            //剛体を作成
            body = new RigidBody((rigid.Type != 0 ? rigid.Weight : 0), motionStateStart, collision, localInertia);
            //ダンピング値、摩擦、Restitutionをセット
            body.setDamping(rigid.LinerDamping, rigid.AngularDamping);
            body.Friction = rigid.Friction;
            body.Restitution = rigid.Restitution;

            //ボーン追従型はKinematicにする
            if (rigid.Type == 0)
            {
                body.ActivationState |= ActivationStateFlags.DISABLE_DEACTIVATION;
                body.CollisionFlags = CollisionFlags.CF_KINEMATIC_OBJECT;
                if (!string.IsNullOrEmpty(rigid.RelatedBoneName))
                    Model.BoneManager[rigid.RelatedBoneName].IsPhysics = false;
            }
            else
            {//物理演算型はボーンのフラグをオンにする
                if (!string.IsNullOrEmpty(rigid.RelatedBoneName))
                    Model.BoneManager[rigid.RelatedBoneName].IsPhysics = true;
            }
            //グループのフラグをセット
            group = (short)Math.Pow(2, rigid.GroupIndex);

            return body;
        }
        private Generic6DofSpringConstraint CreateJoint(RigidBody body0, RigidBody body1, MMDJoint joint, MMDModel model)
        {
            Matrix frameInA, frameInB;
            btTransform btFrameInA, btFrameInB;
            Matrix jointPos =MMDXMath.CreateMatrixFromYawPitchRoll(joint.Rotation[1], joint.Rotation[0], joint.Rotation[2])
                * MMDXMath.CreateTranslationMatrix(joint.Position[0], joint.Position[1], joint.Position[2]);
            if (body0.MotionState != null)
            {
                MMDMotionState motionState = (MMDMotionState)body0.MotionState;
                frameInA = MMDXMath.ToMatrix(motionState.GraphicsWorldTrans);
            }
            else
                throw new NotImplementedException("来るハズないのだが");
            frameInA = jointPos * model.Transform * Matrix.Invert(frameInA);
            if (body1.MotionState != null)
            {
                MMDMotionState motionState = (MMDMotionState)body1.MotionState;
                frameInB = MMDXMath.ToMatrix(motionState.GraphicsWorldTrans);
            }
            else
                throw new NotImplementedException("来るハズないのだが");
            frameInB = jointPos * model.Transform * Matrix.Invert(frameInB);
            //frameInB = jointPos * Matrix.Invert(MMDMath.ConvertToMatrix(body1.GetWorldTransformSmart()));
            MMDXMath.TobtTransform(ref frameInA, out btFrameInA);
            MMDXMath.TobtTransform(ref frameInB, out btFrameInB);

            Generic6DofSpringConstraint mConstPoint = new Generic6DofSpringConstraint(body0, body1, btFrameInA, btFrameInB, true);

            //G6Dof設定用変数の準備
            mConstPoint.setLinearLowerLimit(new btVector3(joint.ConstrainPosition1));
            mConstPoint.setLinearUpperLimit(new btVector3(joint.ConstrainPosition2));
            mConstPoint.setAngularLowerLimit(new btVector3(joint.ConstrainRotation1));
            mConstPoint.setAngularUpperLimit(new btVector3(joint.ConstrainRotation2));
            System.Diagnostics.Debug.WriteLine(joint.Name);
            for(int i=0;i<3;i++)
                System.Diagnostics.Debug.WriteLine(joint.ConstrainPosition1[i]);
            for (int i = 0; i < 3; i++)
                System.Diagnostics.Debug.WriteLine(joint.ConstrainPosition2[i]);
            for (int i = 0; i < 3; i++)
                System.Diagnostics.Debug.WriteLine(joint.ConstrainRotation1[i]);
            for (int i = 0; i < 3; i++)
                System.Diagnostics.Debug.WriteLine(joint.ConstrainRotation2[i]);

            for (int i = 0; i < 3; i++)
            {
                mConstPoint.setStiffness(i, joint.SpringPosition[i]);
                mConstPoint.enableSpring(i, true);
                mConstPoint.setStiffness(i + 3, joint.SpringRotation[i]);
                mConstPoint.enableSpring(i + 3, true);
            }
            mConstPoint.calculateTransforms();
            mConstPoint.setEquilibriumPoint();

            return mConstPoint;
        }

        internal void Update()
        {
            if (bDisposing)
            {
                //自爆処理
                //ジョイントを外す
                foreach (var joint in Joints)
                    MMDCore.Instance.Physics.removeConstraint(joint);
                //剛体を外す
                foreach (var rigid in Rigids)
                    MMDCore.Instance.Physics.removeRigidBody(rigid);
                //処理が終わったのでイベントをアンフック
                PhysicsThreadManager.Instanse.Synchronize -= new Action(Update);
                PhysicsThreadManager.Instanse.DropFrame -= new Action<int>(DropFrame);
                bDisposing = false;//念のため……
            }
            else
            {
                if (MMDCore.Instance.UsePhysics)
                {
                    if (bSetup)
                    {
                        //ボディを有効化し、グループとマスクを適応
                        for (int i = 0; i < Rigids.Count; ++i)
                            MMDCore.Instance.Physics.addRigidBody(Rigids[i], groups[i], masks[i]);
                        foreach (var joint in Joints)
                            MMDCore.Instance.Physics.addConstraint(joint);
                        bSetup = false;
                    }
                    for (int i = 0; i < motionState.Count; i++)
                    {
                        if (nReset > 0)
                            Rigids[i].activate(true);
                        motionState[i].Flush(nReset > 0);
                        if (nReset > 0)
                        {
                            btTransform temp;
                            motionState[i].getWorldTransform(out temp);
                            Rigids[i].WorldTransform = temp;
                            Rigids[i].LinearVelocity = btVector3.Zero;
                            Rigids[i].AngularVelocity = btVector3.Zero;
                        }
                    }
                    if (nReset > 0)
                        --nReset;
                }
            }
            
        }
        //フレーム落ち時のごまかし処理
        internal void DropFrame(int DFCount)
        {
            if (MMDCore.Instance.UsePhysics)
            {
                for (int i = 0; i < motionState.Count; i++)
                {
                    motionState[i].FlushDF(DFCount);
                }
            }
        }
        /// <summary>
        /// 剛体位置のリセット
        /// </summary>
        public void Reset()
        {
            nReset = 3;//3回リセットをかける。(理由：モーションとかはリセットしたフレームではリセットされていないから)
        }

        #region IDisposable メンバー
        /// <summary>
        /// 物理エンジンの破棄
        /// </summary>
        public void Dispose()
        {
            //自爆スイッチOn
            bDisposing = true;
        }

        #endregion
    }
}
