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
    /// MMDBoneManagerのリーダ
    /// </summary>
    public class MMDBoneManagerReader : ContentTypeReader<MMDBoneManager>
    {
        /// <summary>
        /// ボーンマネージャの読み込み
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        protected override MMDBoneManager Read(ContentReader input, MMDBoneManager existingInstance)
        {
            List<MMDBone> bones = input.ReadObject<List<MMDBone>>();
            List<MMDIK> iks = input.ReadObject<List<MMDIK>>();
            //ボーンインデックス→ボーンオブジェクト化
            SkinningHelpers.IKSetup(iks, bones);
#if !XBOX
            return new MMDBoneManager(bones, iks);
#else
            return new MMDXBoxBoneManager(bones, iks);
#endif
        }
        
    }
}
