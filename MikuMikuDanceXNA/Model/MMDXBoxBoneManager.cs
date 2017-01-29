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
    /// XBox�����{�[���}�l�[�W���g��(vfetch�Ή�)
    /// </summary>
    class MMDXBoxBoneManager : MMDBoneManager
    {
        VertexSkinning[] skinTransformsXBox;
        /// <summary>
        /// ���̃v���p�e�B�͎g���܂���B
        /// </summary>
        public override Microsoft.Xna.Framework.Matrix[] SkinTransforms
        {
            get
            {
                throw new InvalidOperationException();//���̑���͂����Ȃ��B
            }
        }
        /// <summary>
        /// �X�L�j���O�s��
        /// </summary>
        public VertexSkinning[] SKinTransformXBox { get { return skinTransformsXBox; } }
        
        public MMDXBoxBoneManager(List<MMDBone> bones, List<MMDIK> iks)
            : base(bones, iks)
        {
            skinTransformsXBox = new VertexSkinning[bones.Count];
        }
        /// <summary>
        /// �X�L�j���O�s��̌v�Z
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
