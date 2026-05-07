using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// ModelPart���i���z���å����ץ�`��
    /// </summary>
    public class MMDGPUModelPartReader : ContentTypeReader<MMDModelPart>
    {
        /// <summary>
        /// ��ǥ�ѩ`�Ĥ��i���z��
        /// </summary>
        /// <param name="input">����ƥ�ĥ�`��</param>
        /// <param name="existingInstance">�ȴ楪�֥�������</param>
        protected override MMDModelPart Read(ContentReader input, MMDModelPart existingInstance)
        {
            //��ǥ�ѩ`�Ĥ��i���z��
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
                throw new ContentLoadException("MMDXCore.ModelPartFactory��MMDModelPart����򷵤��ե����ȥ�`�ˤʤäƤ��ޤ���XNA�Υ���ƥ�ĥѥ��ץ饤���ʹ�ä�����Ϥ�MMDModelPart�򷵤��ե����ȥ�`�򥻥åȤ����Ҫ������ޤ�");
            }
            // read in the BasicEffect as a shared resource
            input.ReadSharedResource<Effect>(fx => modelPart.Effect = new XNAEffectWrapper(fx));

            return modelPart;
        }
    }
}
