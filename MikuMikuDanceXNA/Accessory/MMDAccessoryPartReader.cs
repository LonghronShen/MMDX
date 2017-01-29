using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// �A�N�Z�T���p�[�c�̃R���e���g���[�_
    /// </summary>
    public class MMDAccessoryPartReader : ContentTypeReader<MMDAccessoryPart>
    {
        /// <summary>
        /// �A�N�Z�T���p�[�c�̓ǂݍ���
        /// </summary>
        /// <param name="input">�R���e���c���[�_</param>
        /// <param name="existingInstance">�����p�[�c</param>
        /// <returns>�A�N�Z�T���p�[�c</returns>
        protected override MMDAccessoryPart Read(ContentReader input, MMDAccessoryPart existingInstance)
        {
            int vertexCount = input.ReadInt32();
            IndexBuffer indices = input.ReadObject<IndexBuffer>();
            int baseVertex = input.ReadInt32();
            int triangleCount = input.ReadInt32();
            bool screen = input.ReadBoolean();
            bool edge = input.ReadBoolean();
            MMDAccessoryPart result = new MMDAccessoryPart(vertexCount, indices, baseVertex, triangleCount, screen, edge);
            input.ReadSharedResource<Effect>((effect) => result.Effect = effect);
            input.ReadSharedResource<Effect>((effect) => MMDXCore.Instance.EdgeEffect = effect);
            return result;
        }
    }
}
