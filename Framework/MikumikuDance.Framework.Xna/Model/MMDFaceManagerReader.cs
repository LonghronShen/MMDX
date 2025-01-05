using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Model;

using SkinVertSet = MikuMikuDance.Core.Model.SkinVertSet;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// FaceManagerのリーダ
    /// </summary>
    public class MMDFaceManagerReader : ContentTypeReader<IMMDFaceManager>
    {
        /// <summary>
        /// 表情マネージャをアセットより読み込む
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        /// <returns>表情マネージャ</returns>
        protected override IMMDFaceManager Read(ContentReader input, IMMDFaceManager existingInstance)
        {
            //var vertData = input.ReadObject<Dictionary<string, SkinVertSet[]>>();

            var vertData = new Dictionary<string, SkinVertSet[]>();

            var total = input.ReadInt32();
            for (int i = 0; i < total; i++)
            {
                var key = input.ReadString();

                var value = new List<SkinVertSet>();
                var valueCount = input.ReadInt32();
                for (int j = 0; j < valueCount; j++)
                {
                    var item = input.ReadObject<SkinVertSet>();
                    value.Add(item);
                }

                vertData.Add(key, value.ToArray());
            }

            return new MMDFaceManager(vertData);
        }
    }
}
