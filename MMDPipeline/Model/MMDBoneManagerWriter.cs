using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// BoneManager用タイプライター
    /// </summary>
    [ContentTypeWriter]
    public class MMDBoneManagerWriter : ContentTypeWriter<MMDBoneManagerContent>
    {
        /// <summary>
        /// 書き出し処理
        /// </summary>
        protected override void Write(ContentWriter output, MMDBoneManagerContent value)
        {
            output.WriteObject(value.bones);
            output.WriteObject(value.iks);
        }
        /// <summary>
        /// MMDX上での型を指定
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.Core.Model.MMDBoneManager, MikuMikuDanceCore";
        }
        /// <summary>
        /// MMDX上でのリーダを指定
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "MikuMikuDance.XNA.Model.MMDBoneManagerReader, MikuMikuDanceXNA";
        }
    }
}
