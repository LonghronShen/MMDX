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
    /// MMDIKのタイプリーダ
    /// </summary>
    public class MMDIKReader : ContentTypeReader<MMDIK>
    {
        /// <summary>
        /// IKデータの読み込み
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        protected override MMDIK Read(ContentReader input, MMDIK existingInstance)
        {
            int ikBoneIndex = input.ReadInt32();
            int ikTargetBoneIndex = input.ReadInt32();
            ushort iteration = input.ReadUInt16();
            float controlWeight = input.ReadSingle();
            List<int> ikchild = input.ReadObject<List<int>>();
            return new MMDIK(ikBoneIndex,ikTargetBoneIndex, iteration, controlWeight, ikchild);
        }
    }
}
