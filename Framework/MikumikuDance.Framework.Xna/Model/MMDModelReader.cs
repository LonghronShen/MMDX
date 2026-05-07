using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Model.Physics;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDModel�Υ�`�����饹
    /// </summary>
    public class MMDModelReader : ContentTypeReader<MMDXModel>
    {
        /// <summary>
        /// ��ǥ���i���z��
        /// </summary>
        /// <param name="input">����ƥ�ĥ�`��</param>
        /// <param name="existingInstance">�ȴ楪�֥�������</param>
        protected override MMDXModel Read(ContentReader input, MMDXModel existingInstance)
        {
            // MMDModelPart���i���z��
            var temp = input.ReadObject<List<MMDModelPart>>();
            List<IMMDModelPart> modelParts = new List<IMMDModelPart>();
            foreach (var it in temp)
                modelParts.Add(it);

            //MMDBoneManager���i���z��
            MMDBoneManager boneManager = input.ReadObject<MMDBoneManager>();
            IMMDFaceManager faceManager = input.ReadObject<IMMDFaceManager>();

            //������`�������i���z��
            Dictionary<string, MMDMotion> attachedMotion = input.ReadObject<Dictionary<string, MMDMotion>>();

            //���������i���z��
            MMDRigid[] rigids = input.ReadObject<MMDRigid[]>();
            MMDJoint[] joints = input.ReadObject<MMDJoint[]>();

            input.ReadSharedResource<Effect>((effect) => MMDXCore.Instance.EdgeEffect = new XNAEffectWrapper(effect));
            return new MMDXModel(modelParts, boneManager, faceManager, attachedMotion, rigids, joints);
        }
    }
}
