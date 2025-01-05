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
    public class SkinVertSetReader : ContentTypeReader<SkinVertSet>
    {
        protected override SkinVertSet Read(ContentReader input, SkinVertSet existingInstance)
        {
            var index = input.ReadInt32();
            var vector = input.ReadVector3();

            return new SkinVertSet()
            {
                index = index,
                vector = vector
            };
        }
    }
}
