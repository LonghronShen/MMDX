using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Model.Physics;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDModel§Œ•Í©`•¿•Ø•È•π
    /// </summary>
    public class MMDModelReader : ContentTypeReader<MMDXModel>
    {
        /// <summary>
        /// •‚•«•Î§Œ’i§ﬂﬁz§ﬂ
        /// </summary>
        /// <param name="input">•≥•Û•∆•Û•ƒ•Í©`•¿</param>
        /// <param name="existingInstance">º»¥Ê•™•÷•∏•ß•Ø•»</param>
        protected override MMDXModel Read(ContentReader input, MMDXModel existingInstance)
        {
            // MMDModelPart§Œ’i§ﬂﬁz§ﬂ
            var temp = input.ReadObject<List<MMDModelPart>>();
            List<IMMDModelPart> modelParts = new List<IMMDModelPart>();
            foreach (var it in temp)
                modelParts.Add(it);

            //MMDBoneManager§Œ’i§ﬂﬁz§ﬂ
            MMDBoneManager boneManager = input.ReadObject<MMDBoneManager>();
            IMMDFaceManager faceManager = input.ReadObject<IMMDFaceManager>();

            //∏∂ Ù•‚©`•∑•Á•Û§Œ’i§ﬂﬁz§ﬂ
            Dictionary<string, MMDMotion> attachedMotion = input.ReadObject<Dictionary<string, MMDMotion>>();

            //ŒÔ¿Ì«ÈàÛ§Œ’i§ﬂﬁz§ﬂ
            MMDRigid[] rigids = input.ReadObject<MMDRigid[]>();
            MMDJoint[] joints = input.ReadObject<MMDJoint[]>();

            input.ReadSharedResource<Effect>((effect) => MMDXCore.Instance.EdgeEffect = effect);
            return new MMDXModel(modelParts, boneManager, faceManager, attachedMotion, rigids, joints);
        }
    }
}
