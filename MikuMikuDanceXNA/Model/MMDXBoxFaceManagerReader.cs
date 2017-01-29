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
    /// FaceManager�̃��[�_
    /// </summary>
    public class MMDXBoxFaceManagerReader : ContentTypeReader<IMMDFaceManager>
    {
        /// <summary>
        /// �\��}�l�[�W�����A�Z�b�g���ǂݍ���
        /// </summary>
        /// <param name="input">�R���e���c���[�_</param>
        /// <param name="existingInstance">�����I�u�W�F�N�g</param>
        /// <returns>�\��}�l�[�W��</returns>
        protected override IMMDFaceManager Read(ContentReader input, IMMDFaceManager existingInstance)
        {
            var vertData = input.ReadObject<Vector4[]>();
            var faceRates = input.ReadObject<Dictionary<string, int>>();

            return new MMDXBoxFaceManager(vertData, faceRates);
        }
    }
}
