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
    /// MMDModelのリーダクラス
    /// </summary>
    public class MMDModelReader : ContentTypeReader<MMDXModel>
    {
        /// <summary>
        /// モデルの読み込み
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        protected override MMDXModel Read(ContentReader input, MMDXModel existingInstance)
        {
            // MMDModelPartの読み込み
            var temp = input.ReadObject<List<MMDModelPart>>();
            List<IMMDModelPart> modelParts = new List<IMMDModelPart>();
            foreach (var it in temp)
                modelParts.Add(it);

            //MMDBoneManagerの読み込み
            MMDBoneManager boneManager = input.ReadObject<MMDBoneManager>();
            IMMDFaceManager faceManager = input.ReadObject<IMMDFaceManager>();

            //付属モーションの読み込み
            Dictionary<string, MMDMotion> attachedMotion = input.ReadObject<Dictionary<string, MMDMotion>>();

            //物理情報の読み込み
            MMDRigid[] rigids = input.ReadObject<MMDRigid[]>();
            MMDJoint[] joints = input.ReadObject<MMDJoint[]>();

            input.ReadSharedResource<Effect>((effect) => MMDXCore.Instance.EdgeEffect = effect);
            return new MMDXModel(modelParts, boneManager, faceManager, attachedMotion, rigids, joints);
        }
    }
}
