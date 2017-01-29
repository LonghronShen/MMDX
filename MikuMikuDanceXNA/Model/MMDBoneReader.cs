using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Misc;
using MikuMikuDance.Core.Model;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDBoneのコンテンツリーダー
    /// </summary>
    public class MMDBoneReader : ContentTypeReader<MMDBone>
    {
        /// <summary>
        /// モデルボーンの読み込み
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        protected override MMDBone Read(ContentReader input, MMDBone existingInstance)
        {
            SQTTransform bindPose = input.ReadObject<SQTTransform>();
            Matrix invPose = input.ReadObject<Matrix>();
            //ushort ikp = input.ReadUInt16();
            string name = input.ReadString();
            int sh = input.ReadInt32();
            return new MMDBone(name, bindPose, invPose, sh);
        }
    }
}
