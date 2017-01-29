using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// ModelPart�̓ǂݍ��ݗp�^�C�v���[�_
    /// </summary>
    public class MMDGPUModelPartReader : ContentTypeReader<MMDModelPart>
    {
        /// <summary>
        /// ���f���p�[�c�̓ǂݍ���
        /// </summary>
        /// <param name="input">�R���e���c���[�_</param>
        /// <param name="existingInstance">�����I�u�W�F�N�g</param>
        protected override MMDModelPart Read(ContentReader input, MMDModelPart existingInstance)
        {
            //���f���p�[�c�̓ǂݍ���
            int triangleCount = input.ReadInt32();
            MMDVertexNm[] Vertices = input.ReadObject<MMDVertexNm[]>();
            Dictionary<long, int[]> VertMap = input.ReadObject<Dictionary<long, int[]>>();
            IndexBuffer indexBuffer = input.ReadObject<IndexBuffer>();

            // create the model part from this data
            Dictionary<string, object> OpaqueData = new Dictionary<string, object>();
            OpaqueData.Add("VertMap", VertMap);
            OpaqueData.Add("IndexBuffer", indexBuffer);
            MMDModelPart modelPart = null;
            modelPart = MMDXCore.Instance.ModelPartFactory.Create(triangleCount, Vertices, OpaqueData) as MMDModelPart;
            if (modelPart == null)
            {
                throw new ContentLoadException("MMDXCore.ModelPartFactory��MMDModelPart�ȊO��Ԃ��t�@�N�g���[�ɂȂ��Ă��܂��BXNA�̃R���e���c�p�C�v���C�����g�p����ꍇ��MMDModelPart��Ԃ��t�@�N�g���[���Z�b�g����K�v������܂�");
            }
            // read in the BasicEffect as a shared resource
            input.ReadSharedResource<Effect>(fx => modelPart.Effect = fx);

            return modelPart;
        }
    }
}
