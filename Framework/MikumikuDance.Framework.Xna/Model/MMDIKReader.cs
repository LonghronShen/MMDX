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
    /// MMDIK¤Î¥¿¥¤¥×¥ê©`¥À
    /// </summary>
    public class MMDIKReader : ContentTypeReader<MMDIK>
    {
        /// <summary>
        /// IK¥Ç©`¥¿¤ÎÕi¤ßÞz¤ß
        /// </summary>
        /// <param name="input">¥³¥ó¥Æ¥ó¥Ä¥ê©`¥À</param>
        /// <param name="existingInstance">¼È´æ¥ª¥Ö¥¸¥§¥¯¥È</param>
        protected override MMDIK Read(ContentReader input, MMDIK existingInstance)
        {
            int ikBoneIndex = input.ReadInt32();
            int ikTargetBoneIndex = input.ReadInt32();
            ushort iteration = input.ReadUInt16();
            float controlWeight = input.ReadSingle();
            List<int> ikchild = input.ReadObject<List<int>>();
            return new MMDIK(ikBoneIndex, ikTargetBoneIndex, iteration, controlWeight, ikchild);
        }
    }
}
