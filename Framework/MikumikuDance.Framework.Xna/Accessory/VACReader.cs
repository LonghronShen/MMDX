using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Accessory;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// VACリーダ
    /// </summary>
    public class VACReader : ContentTypeReader<MMD_VAC>
    {
        /// <summary>
        /// VAC読み込み
        /// </summary>
        protected override MMD_VAC Read(ContentReader input, MMD_VAC existingInstance)
        {
            MMD_VAC result;
            result.BoneName = input.ReadString();
            result.Transform = input.ReadMatrix();
            return result;
        }
    }
}
