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
    /// MikuMikuDance VACライタ
    /// </summary>
    [ContentTypeWriter]
    public class VACWriter : ContentTypeWriter<VACContent>
    {
        /// <summary>
        /// パイプライン書き出し
        /// </summary>
        protected override void Write(ContentWriter output, VACContent value)
        {
            output.Write(value.BoneName);
            output.Write(value.Transform);
        }
        /// <summary>
        /// 読み込み先の型
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.Core.Accessory.MMD_VAC, MikuMikuDanceCore";
        }
        /// <summary>
        /// 読み出す用のタイプリーダ
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.XNA.Accessory.VACReader, MikuMikuDanceXNA";
        }
    }
}
