using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Model.Physics;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDModel�̃��[�_�N���X
    /// </summary>
    public class MMDModelReader : ContentTypeReader<MMDXModel>
    {
        /// <summary>
        /// ���f���̓ǂݍ���
        /// </summary>
        /// <param name="input">�R���e���c���[�_</param>
        /// <param name="existingInstance">�����I�u�W�F�N�g</param>
        protected override MMDXModel Read(ContentReader input, MMDXModel existingInstance)
        {
            // MMDModelPart�̓ǂݍ���
            var temp = input.ReadObject<List<MMDModelPart>>();
            List<IMMDModelPart> modelParts = new List<IMMDModelPart>();
            foreach (var it in temp)
                modelParts.Add(it);

            //MMDBoneManager�̓ǂݍ���
            MMDBoneManager boneManager = input.ReadObject<MMDBoneManager>();
            IMMDFaceManager faceManager = input.ReadObject<IMMDFaceManager>();

            //�t�����[�V�����̓ǂݍ���
            Dictionary<string, MMDMotion> attachedMotion = input.ReadObject<Dictionary<string, MMDMotion>>();

            //�������̓ǂݍ���
            MMDRigid[] rigids = input.ReadObject<MMDRigid[]>();
            MMDJoint[] joints = input.ReadObject<MMDJoint[]>();

            input.ReadSharedResource<Effect>((effect) => MMDXCore.Instance.EdgeEffect = effect);
            return new MMDXModel(modelParts, boneManager, faceManager, attachedMotion, rigids, joints);
        }
    }
}
