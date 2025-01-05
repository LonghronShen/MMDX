using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using MikuMikuDance.Core.Model;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDBoneManager¤Î¥ê©`¥À
    /// </summary>
    public class MMDBoneManagerReader : ContentTypeReader<MMDBoneManager>
    {
        /// <summary>
        /// ¥Ü©`¥ó¥Þ¥Í©`¥¸¥ã¤ÎÕi¤ßÞz¤ß
        /// </summary>
        /// <param name="input">¥³¥ó¥Æ¥ó¥Ä¥ê©`¥À</param>
        /// <param name="existingInstance">¼È´æ¥ª¥Ö¥¸¥§¥¯¥È</param>
        protected override MMDBoneManager Read(ContentReader input, MMDBoneManager existingInstance)
        {
            List<MMDBone> bones = input.ReadObject<List<MMDBone>>();
            List<MMDIK> iks = input.ReadObject<List<MMDIK>>();
            //¥Ü©`¥ó¥¤¥ó¥Ç¥Ã¥¯¥¹¡ú¥Ü©`¥ó¥ª¥Ö¥¸¥§¥¯¥È»¯
            SkinningHelpers.IKSetup(iks, bones);
#if !XBOX
            return new MMDBoneManager(bones, iks);
#else
            return new MMDXBoxBoneManager(bones, iks);
#endif
        }

    }
}
