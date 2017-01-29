using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// �A�N�Z�T���̃R���e���c���[�_
    /// </summary>
    public class MMDAccessoryReader : ContentTypeReader<MMDAccessory>
    {
        /// <summary>
        /// �A�N�Z�T���̓ǂݍ���
        /// </summary>
        /// <param name="input">�R���e���c���[�_</param>
        /// <param name="existingInstance">�����I�u�W�F�N�g</param>
        /// <returns>�A�N�Z�T��</returns>
        protected override MMDAccessory Read(ContentReader input, MMDAccessory existingInstance)
        {
            MMDVertexNmTxVc[] vertex = input.ReadObject<MMDVertexNmTxVc[]>();
            List<MMDAccessoryPart> parts = input.ReadObject<List<MMDAccessoryPart>>();
            return new MMDAccessory(vertex, parts);
        }
    }
}
