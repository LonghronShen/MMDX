using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
using MikuMikuDance.XNA.Misc;
using MikuMikuDance.Core.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// XBox向けボーンマネージャ拡張(vfetch対応)
    /// </summary>
    class MMDXBoxBoneManager : MMDBoneManager
    {
        VertexSkinning[] skinTransformsXBox;
        /// <summary>
        /// このプロパティは使えません。
        /// </summary>
        public override Microsoft.Xna.Framework.Matrix[] SkinTransforms
        {
            get
            {
                throw new InvalidOperationException();//この操作はさせない。
            }
        }
        /// <summary>
        /// スキニング行列
        /// </summary>
        public VertexSkinning[] SKinTransformXBox { get { return skinTransformsXBox; } }
        
        public MMDXBoxBoneManager(List<MMDBone> bones, List<MMDIK> iks)
            : base(bones, iks)
        {
            skinTransformsXBox = new VertexSkinning[bones.Count];
        }
        /// <summary>
        /// スキニング行列の計算
        /// </summary>
        public override void CalcSkinTransform()
        {
            for (int i = 0; i < Count; ++i)
            {
                Matrix temp;
                Vector3 temp2;
                Matrix.Multiply(ref this[i].InverseBindPose, ref this[i].GlobalTransform, out temp);
                temp.Decompose(out temp2, out skinTransformsXBox[i].Rotation, out skinTransformsXBox[i].Translation);
            }
            
        }
    }
}
