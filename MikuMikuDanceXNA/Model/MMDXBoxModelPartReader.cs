using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Core.Misc;


namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// ModelPartの読み込み用タイプリーダ(XBox用)
    /// </summary>
    public class MMDXBoxModelPartReader : ContentTypeReader<MMDModelPart>
    {
        /// <summary>
        /// モデルパーツの読み込み
        /// </summary>
        /// <param name="input">コンテンツリーダ</param>
        /// <param name="existingInstance">既存オブジェクト</param>
        protected override MMDModelPart Read(ContentReader input, MMDModelPart existingInstance)
        {
            //モデルパーツの読み込み
            int triangleCount = input.ReadInt32();
            MMDVertexNm[] Vertices = input.ReadObject<MMDVertexNm[]>();
            Vector2[] extVert = input.ReadObject<Vector2[]>();
            IndexBuffer indexBuffer = input.ReadObject<IndexBuffer>();

            // create the model part from this data
            Dictionary<string, object> OpaqueData = new Dictionary<string, object>();
            OpaqueData.Add("VerticesExtention", extVert);
            OpaqueData.Add("IndexBuffer", indexBuffer);
            MMDModelPart modelPart = null;
            modelPart = MMDXCore.Instance.ModelPartFactory.Create(triangleCount, Vertices, OpaqueData) as MMDModelPart;
            if (modelPart == null)
            {
                throw new ContentLoadException("MMDXCore.ModelPartFactoryがMMDModelPart以外を返すファクトリーになっています。XNAのコンテンツパイプラインを使用する場合はMMDModelPartを返すファクトリーをセットする必要があります");
            }
            // read in the BasicEffect as a shared resource
            input.ReadSharedResource<Effect>(fx => modelPart.Effect = fx);

            return modelPart;
        }
    }
}
