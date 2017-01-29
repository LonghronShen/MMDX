using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using MikuMikuDance.Core.Model;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDBoneManager�̃��[�_
    /// </summary>
    public class MMDBoneManagerReader : ContentTypeReader<MMDBoneManager>
    {
        /// <summary>
        /// �{�[���}�l�[�W���̓ǂݍ���
        /// </summary>
        /// <param name="input">�R���e���c���[�_</param>
        /// <param name="existingInstance">�����I�u�W�F�N�g</param>
        protected override MMDBoneManager Read(ContentReader input, MMDBoneManager existingInstance)
        {
            List<MMDBone> bones = input.ReadObject<List<MMDBone>>();
            List<MMDIK> iks = input.ReadObject<List<MMDIK>>();
            //�{�[���C���f�b�N�X���{�[���I�u�W�F�N�g��
            SkinningHelpers.IKSetup(iks, bones);
#if !XBOX
            return new MMDBoneManager(bones, iks);
#else
            return new MMDXBoxBoneManager(bones, iks);
#endif
        }
        
    }
}
