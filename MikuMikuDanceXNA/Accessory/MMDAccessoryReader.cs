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
    /// アクセサリのコンテンツリーダ
    /// </summary>
    public class MMDAccessoryReader : ContentTypeReader<MMDAccessory>
    {
        /// <summary>
        /// アクセサリの読み込み
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        /// <returns>アクセサリ</returns>
        protected override MMDAccessory Read(ContentReader input, MMDAccessory existingInstance)
        {
            MMDVertexNmTxVc[] vertex = input.ReadObject<MMDVertexNmTxVc[]>();
            List<MMDAccessoryPart> parts = input.ReadObject<List<MMDAccessoryPart>>();
            return new MMDAccessory(vertex, parts);
        }
    }
}
