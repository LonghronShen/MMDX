using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Motion;
using MikuMikuDance.Core.Misc;
#if XNA
using Microsoft.Xna.Framework.Graphics;
#endif

namespace MikuMikuDance.XNA.Model
{
#if false //XNAのモデル読み込み超めんどくさい……。読み込みが多いならSlimMMDXを使ったほうがいいかな。(それか、パイプラインを動的に動かす方法で読み込むべき)
    class MMDModelPartFromFileFactory : IMMDModelPartFromFileFactory
    {
        IMMDModelPartFactory normalFactory;
        public MMDModelPartFromFileFactory(IMMDModelPartFactory factory)
        {
            normalFactory = factory;
        }
        #region IMMDModelPartFromFileFactory メンバー

        public MMDModel Load(string filename, Dictionary<string, object> opaqueData)
        {
            //GraphicsDataのチェック
            if (!opaqueData.ContainsKey("GraphicsDevice"))
                throw new ArgumentException("opaqueデータに{\"GraphicsDevice\",GraphicsDevice}がありません", "opaqueData");
            GraphicsDevice graphics = opaqueData["GraphicsDevice"] as GraphicsDevice;
            if (graphics == null)
                throw new ArgumentException("opaqueデータに\"GraphicsDevice\"のキーはありましたが、ValueにGraphicsDeviceが入っていません");

            //PMDのロード
            MikuMikuDance.Model.MMDModel model = MikuMikuDance.Model.ModelManager.Read(filename);
            MikuMikuDance.Model.Ver1.MMDModel1 model1 = model as MikuMikuDance.Model.Ver1.MMDModel1;
            if (model1 == null)
                throw new System.IO.FileLoadException("ファイルのロードに失敗しました", filename);
            List<IMMDModelPart> parts = new List<IMMDModelPart>();
            MMDBoneManager manager;
            Dictionary<string, MMDMotionData> attachedMotion = new Dictionary<string, MMDMotionData>();
            long FaceIndex = 0;
            //ボーンマネージャの作成
            MMDBone[] bones = new MMDBone[model1.Bones.Length];
            for (int i = 0; i < bones.Length; ++i)
            {
                bones[i].BindPose
            }
            //マテリアルごとにモデルを作成
            for (long i = 0; i < model1.Materials.LongLength; ++i)
            {
                List<ushort> vertIndices = new List<ushort>();
                List<int> faceIndices = new List<int>();
                Dictionary<ushort, int> vertMap = new Dictionary<ushort, int>();
                //頂点インデックスの作成
                for (long j = FaceIndex; j < FaceIndex + model1.Materials[i].FaceVertCount; ++j)
                {
                    ushort VertIndex = model1.FaceVertexes[j];
                    int vpos;
                    if (vertMap.TryGetValue(VertIndex, out vpos))
                    {
                        faceIndices.Add(vpos);
                    }
                    else
                    {
                        vertMap.Add(VertIndex, vertIndices.Count);
                        faceIndices.Add(vertIndices.Count);
                        vertIndices.Add(VertIndex);
                    }
                }
                //インデックスバッファの作成(GraphicsDeviceがいる。どうする？)
                IndexBuffer indexbuffer;

                //頂点の作成
                MMDVertexNm[] verts;
                MMDVertexNmTx[] vertsTx = null;
                //頂点型判定と代入
                if (!string.IsNullOrEmpty(model1.Materials[i].TextureFileName))
                {
                    vertsTx = new MMDVertexNmTx[vertIndices.Count];
                    verts = vertsTx;
                }
                else
                    verts = new MMDVertexNm[vertIndices.Count];
                for (int vi = 0; vi < verts.LongLength; ++vi)
                {
                    MikuMikuDance.Model.Ver1.ModelVertex modelvert = model1.Vertexes[vertIndices[vi]];
                    verts[vi].Position = MMDXMath.ToVector3(modelvert.Pos);
                    verts[vi].Normal = MMDXMath.ToVector3(modelvert.NormalVector);
                    verts[vi].BlendWeights = new Microsoft.Xna.Framework.Vector2(modelvert.BoneWeight / 100f, 1.0f - modelvert.BoneWeight / 100f);
                    verts[vi].BlendIndexX = modelvert.BoneNum[0];
                    verts[vi].BlendIndexY = modelvert.BoneNum[1];
                    if (verts[vi].BlendIndexX < 0 && verts[vi].BlendIndexX >= manager.Count)
                    {
                        verts[vi].BlendWeights.X = 0;
                        verts[vi].BlendIndexX = 0;
                    }
                    if (verts[vi].BlendIndexY < 0 && verts[vi].BlendIndexY >= manager.Count)
                    {
                        verts[vi].BlendWeights.Y = 0;
                        verts[vi].BlendIndexY = 0;
                    }
                    if (vertsTx != null)
                        vertsTx[vi].TextureCoordinate = MMDXMath.ToVector2(modelvert.UV);
                }
                //不透明データを作成
                Dictionary<string, object> opaqueData = new Dictionary<string, object>();
                opaqueData.Add("IndexBuffer", indexbuffer);
                parts.Add(normalFactory.Create(faceIndices.Count / 3, verts, opaqueData));
            }
            return new MMDModel(parts, manager, attachedMotion);
        }

        #endregion
    }
#endif
}
