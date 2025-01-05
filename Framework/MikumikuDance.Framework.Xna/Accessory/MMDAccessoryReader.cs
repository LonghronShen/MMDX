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
    /// ¥¢¥¯¥»¥µ¥ê¤Î¥³¥ó¥Æ¥ó¥Ä¥ê©`¥À
    /// </summary>
    public class MMDAccessoryReader : ContentTypeReader<MMDAccessory>
    {
        /// <summary>
        /// ¥¢¥¯¥»¥µ¥ê¤ÎÕi¤ßÞz¤ß
        /// </summary>
        /// <param name="input">¥³¥ó¥Æ¥ó¥Ä¥ê©`¥À</param>
        /// <param name="existingInstance">¼È´æ¥ª¥Ö¥¸¥§¥¯¥È</param>
        /// <returns>¥¢¥¯¥»¥µ¥ê</returns>
        protected override MMDAccessory Read(ContentReader input, MMDAccessory existingInstance)
        {
            MMDVertexNmTxVc[] vertex = input.ReadObject<MMDVertexNmTxVc[]>();
            List<MMDAccessoryPart> parts = input.ReadObject<List<MMDAccessoryPart>>();
            return new MMDAccessory(vertex, parts);
        }
    }
}
