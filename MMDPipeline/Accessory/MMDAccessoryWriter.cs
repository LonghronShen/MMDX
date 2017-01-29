using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MikuMikuDance の　アクセサリタイプライター
    /// </summary>
    [ContentTypeWriter]
    public class MMDAccessoryWriter : ContentTypeWriter<MMDAccessoryContent>
    {
        /// <summary>
        /// アクセサリの書き出し
        /// </summary>
        protected override void Write(ContentWriter output, MMDAccessoryContent value)
        {
            output.WriteObject(value.Vertex);
            output.WriteObject(value.Parts);
        }
        /// <summary>
        /// MMDX上での型
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.XNA.Accessory.MMDAccessory, MikuMikuDanceXNA";
        }
        /// <summary>
        /// MMDX上でのリーダ
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.XNA.Accessory.MMDAccessoryReader, MikuMikuDanceXNA";
        }
    }
}
