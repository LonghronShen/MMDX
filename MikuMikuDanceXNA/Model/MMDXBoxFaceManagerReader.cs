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
    /// FaceManagerのリーダ
    /// </summary>
    public class MMDXBoxFaceManagerReader : ContentTypeReader<IMMDFaceManager>
    {
        /// <summary>
        /// 表情マネージャをアセットより読み込む
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        /// <returns>表情マネージャ</returns>
        protected override IMMDFaceManager Read(ContentReader input, IMMDFaceManager existingInstance)
        {
            var vertData = input.ReadObject<Vector4[]>();
            var faceRates = input.ReadObject<Dictionary<string, int>>();

            return new MMDXBoxFaceManager(vertData, faceRates);
        }
    }
}
