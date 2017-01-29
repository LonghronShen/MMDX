using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Model;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDIK�̃^�C�v���[�_
    /// </summary>
    public class MMDIKReader : ContentTypeReader<MMDIK>
    {
        /// <summary>
        /// IK�f�[�^�̓ǂݍ���
        /// </summary>
        /// <param name="input">�R���e���c���[�_</param>
        /// <param name="existingInstance">�����I�u�W�F�N�g</param>
        protected override MMDIK Read(ContentReader input, MMDIK existingInstance)
        {
            int ikBoneIndex = input.ReadInt32();
            int ikTargetBoneIndex = input.ReadInt32();
            ushort iteration = input.ReadUInt16();
            float controlWeight = input.ReadSingle();
            List<int> ikchild = input.ReadObject<List<int>>();
            return new MMDIK(ikBoneIndex,ikTargetBoneIndex, iteration, controlWeight, ikchild);
        }
    }
}
