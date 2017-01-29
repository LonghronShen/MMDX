using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletX.LinerMath;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.Core.MultiThreads;

#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Model.Physics
{
    /// <summary>
    /// MikuMikuDance用のMotionState
    /// </summary>
    public class MMDMotionState : IMotionState
    {
        /// <summary>
        /// graphicsWorldTransform
        /// </summary>
        public btTransform GraphicsWorldTrans { get { return m_graphicsWorldTrans/*[PhysicsThreadManager.Instanse.BufferNum]*/; } }
        btTransform/*[]*/ m_graphicsWorldTrans;
        btTransform m_centerOfMassOffset;
        btTransform m_startWorldTrans;
        MMDRigid m_rigid;
        Matrix m_rigidBias;
        MMDModel m_model;
        //フレーム落ち用にボーンの位置を記録しておく
        Matrix beforeBone;
        bool enableBeforeBone;
        //関連ボーン名を取得しておく
        string RelatedBoneName;
        /// <summary>
        /// ユーザーデータ
        /// </summary>
        public object UserData { get; set; }
        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        /// <param name="rigid">MMD剛体データ</param>
        /// <param name="Model">MMDモデルデータ</param>
        public MMDMotionState(MMDRigid rigid, MMDModel Model)
        {
            UserData = null;
            m_rigid = rigid;
            //初期の姿勢を計算
            Matrix startTransform;
            if (!string.IsNullOrEmpty(rigid.RelatedBoneName))
                startTransform = Model.BoneManager[rigid.RelatedBoneName].GlobalTransform;
            else
            {
                if (Model.BoneManager.IndexOf("センター") >= 0)
                    startTransform = Model.BoneManager[Model.BoneManager.IndexOf("センター")].GlobalTransform;
                else
                    startTransform = Matrix.Identity;
            }
            //ボーンと剛体とのズレを計算
            Matrix RigidBias = CreateRigidBias(rigid);
            //初期姿勢を計算
            startTransform = RigidBias * startTransform * Model.Transform;
            //初期の姿勢をMotionStateに設定
            btTransform startTrans;
            MMDXMath.TobtTransform(ref startTransform, out startTrans);
            
                
            m_graphicsWorldTrans = startTrans;
            m_centerOfMassOffset = btTransform.Identity;
            m_startWorldTrans = startTrans;
            //これからの計算ように確保
            m_rigidBias = RigidBias;
            RelatedBoneName =  rigid.RelatedBoneName;
            m_model = Model;
        }
        //graphicsWorldTransの更新
        internal void Flush(bool reset)
        {
            if (string.IsNullOrEmpty(RelatedBoneName))
                return;
            if (m_rigid.Type == 0 || reset)
            {
                Matrix temp, temp2, temp3 = m_model.Transform;
                temp = m_rigidBias * m_model.BoneManager[RelatedBoneName].GlobalTransform;
                Matrix.Multiply(ref temp, ref temp3, out temp2);
                if (!reset)
                    MMDXMath.TobtTransform(ref temp2,out m_graphicsWorldTrans);
                else
                {
                    MMDXMath.TobtTransform(ref temp2, out m_graphicsWorldTrans);
                }
            }
            else
            {
                Matrix RigidPos;
                MMDXMath.ToMatrix(ref m_graphicsWorldTrans, out RigidPos);
                Matrix temp1, temp2, temp3;
                Matrix.Invert(ref m_rigidBias, out temp1);
                Matrix.Multiply(ref temp1, ref RigidPos, out temp2);
                temp1 = Matrix.Invert(m_model.Transform);
                Matrix.Multiply(ref temp2, ref temp1, out temp3);
                m_model.BoneManager[RelatedBoneName].GlobalTransform = temp3;
                //フレーム落ち対策……
                beforeBone = temp3;
                enableBeforeBone = true;
            }

        }
        internal void FlushDF(int DFCount)
        {
            if (enableBeforeBone && m_rigid.Type != 0)
                m_model.BoneManager[RelatedBoneName].GlobalTransform = beforeBone;
        }
        /// <summary>
        /// synchronizes world transform from user to physics
        /// </summary>
        public virtual void getWorldTransform(out btTransform centerOfMassWorldTrans)
        {
            centerOfMassWorldTrans = m_centerOfMassOffset.inverse() * m_graphicsWorldTrans;

        }

        /// <summary>
        /// synchronizes world transform from physics to user
        /// Bullet only calls the update of worldtransform for active objects
        /// </summary>
        /// <param name="centerOfMassWorldTrans"></param>
        public virtual void setWorldTransform(btTransform centerOfMassWorldTrans)
        {
            m_graphicsWorldTrans = centerOfMassWorldTrans * m_centerOfMassOffset;

        }
        Matrix CreateRigidBias(MMDRigid rigid)
        {
            return MMDXMath.CreateMatrixFromYawPitchRoll(rigid.Rotation[1], rigid.Rotation[0], rigid.Rotation[2])
                * MMDXMath.CreateTranslationMatrix(rigid.Position[0], rigid.Position[1], rigid.Position[2]);
        }
    }
}
