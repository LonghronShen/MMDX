using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MikuMikuDance.Core.Misc;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif
#if !XNA
using System.Drawing;
#endif

namespace MikuMikuDance.Core.Model
{
    /// <summary>
    /// ボーンマネージャクラス
    /// </summary>
    public class MMDBoneManager : IEnumerable<MMDBone>
    {
        ReadOnlyCollection<MMDBone> bones;
        /// <summary>
        /// IK一覧
        /// </summary>
        public ReadOnlyCollection<MMDIK> IKs { get; private set; }
        Dictionary<string, int> boneDic;
        /// <summary>
        /// スキニング行列
        /// </summary>
        public virtual Matrix[] SkinTransforms { get; private set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bones">ボーン一覧</param>
        /// <param name="iks">IK一覧</param>
        public MMDBoneManager(List<MMDBone> bones, List<MMDIK> iks)
        {
            this.bones = new ReadOnlyCollection<MMDBone>(bones);
            this.IKs = new ReadOnlyCollection<MMDIK>(iks);
            boneDic = new Dictionary<string, int>();
            for (int i = 0; i < bones.Count; i++)
            {
                boneDic.Add(bones[i].Name, i);
            }
            SkinTransforms = new Matrix[bones.Count];
        }
        /// <summary>
        /// ボーン取得
        /// </summary>
        /// <param name="index">ボーン番号</param>
        /// <returns>ボーンオブジェクト</returns>
        public MMDBone this[int index] { get { return bones[index]; } }
        /// <summary>
        /// ボーン取得
        /// </summary>
        /// <param name="key">ボーン名</param>
        /// <returns>ボーンオブジェクト</returns>
        public MMDBone this[string key] { get { return bones[boneDic[key]]; } }
        /// <summary>
        /// ボーン番号取得
        /// </summary>
        /// <param name="key">ボーン名</param>
        /// <returns>ボーン番号</returns>
        /// <remarks>ボーン名が存在しない場合は-1が返却される</remarks>
        public int IndexOf(string key)
        {
            int result;
            if (!boneDic.TryGetValue(key, out result))
                return -1;
            return result;
        }
        /// <summary>
        /// ボーン数
        /// </summary>
        public int Count { get { return bones.Count; } }


        /// <summary>
        /// グローバルトランスフォームの更新
        /// </summary>
        public virtual void CalcGlobalTransform()
        {
            MMDXProfiler.BeginMark("BoneManager.CalcGlobalTransform", MMDXMath.CreateColor(40, 255, 0));
            bones[0].LocalTransform.CreateMatrix(out bones[0].GlobalTransform);
            for (int i = 1; i < bones.Count; ++i)
            {
                int parentBone = bones[i].SkeletonHierarchy;
                Matrix local;
                bones[i].LocalTransform.CreateMatrix(out local);
                if (parentBone >= bones.Count)
                {
                    bones[i].GlobalTransform = local;
                }
                else
                {
                    Matrix.Multiply(ref local, ref bones[parentBone].GlobalTransform, out bones[i].GlobalTransform);
                }
            }
            MMDXProfiler.EndMark("BoneManager.CalcGlobalTransform");
        }
        
        /// <summary>
        /// スキニング行列の計算
        /// </summary>
        public virtual void CalcSkinTransform()
        {
            MMDXProfiler.BeginMark("BoneManager.CalcSkinTransform", MMDXMath.CreateColor(40, 255, 0));
            for (int i = 0; i < bones.Count; ++i)
                Matrix.Multiply(ref bones[i].InverseBindPose, ref bones[i].GlobalTransform, out SkinTransforms[i]);
            MMDXProfiler.EndMark("BoneManager.CalcSkinTransform");
        }
        /// <summary>
        /// IK計算
        /// </summary>
        public virtual void CalcIK()
        {
            bool UpdateFlag = false;
            for (int i = 0; i < IKs.Count; ++i)
            {
                if (MMDCore.Instance.IKSolver.Solve(IKs[i], this))
                    UpdateFlag = true;
            }
            if (UpdateFlag)
                CalcGlobalTransform();
        }

        #region IEnumerable<MMDBone> メンバー
        /// <summary>
        /// IEnumeratorオブジェクトの取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator<MMDBone> GetEnumerator()
        {
            return BoneEnumerator.GetObject(this);
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
        private class BoneEnumerator : IEnumerator<MMDBone>
        {
            MMDBoneManager manager;
            int current;
            private BoneEnumerator(MMDBoneManager manager)
            {
                this.manager = manager;
                current = 0;
            }
            static Queue<BoneEnumerator> ObjPool = new Queue<BoneEnumerator>(10);
            public static BoneEnumerator GetObject(MMDBoneManager manager)
            {
                lock (ObjPool)
                {
                    if (ObjPool.Count == 0)
                        return new BoneEnumerator(manager);
                    BoneEnumerator result = ObjPool.Dequeue();
                    result.manager = manager;
                    return result;
                }
            }
            #region IEnumerator<MMDBone> メンバー

            public MMDBone Current
            {
                get { return manager[current]; }
            }

            #endregion

            #region IDisposable メンバー

            public void Dispose()
            {
                ObjPool.Enqueue(this);
            }

            #endregion

            #region IEnumerator メンバー

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                ++current;
                return current < manager.Count;
            }

            public void Reset()
            {
                current = 0;
            }

            #endregion
        }
    }
}
