using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;
using MikuMikuDance.XNA.Misc;


namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MikuMikuDance for XNA用モデルプロセッサー
    /// </summary>
    /// <remarks>各種インポータと互換</remarks>
    [ContentProcessor(DisplayName = "MikuMikuDanceモデル : MikuMikuDance for XNA")]
    class MMDModelProcessor : ContentProcessor<NodeContent, MMDModelContent>
    {
        ContentProcessorContext context;
        MMDModelContent resultModel;
        
        Dictionary<MaterialContent, MaterialContent> ProcessedMaterial = new Dictionary<MaterialContent, MaterialContent>();

        Color colorKeyColor = new Color(255, 0, 255, 255);
        [DisplayName("カラーキーの色")]
        [Description("モデルのテクスチャーに対してカラーキーが有効な場合、指定した色のピクセルはアルファ値が0の黒で置換されます")]
        [DefaultValue(typeof(Color), "255, 0, 255, 255")]
        public virtual Color ColorKeyColor { get { return colorKeyColor; } set { colorKeyColor = value; } }

        bool colorKeyEnabled = false;
        [DisplayName("カラーキーの有無")]
        [Description("有効な場合、モデルのテクスチャーに対してカラーキーを有効にします。\"カラーキーの色\"の値に一致するピクセルは、アルファ値が0の黒で置換されます")]
        [DefaultValue(false)]
        public virtual bool ColorKeyEnabled { get { return colorKeyEnabled; } set { colorKeyEnabled = value; } }

        MaterialProcessorDefaultEffect DefaultEffect = MaterialProcessorDefaultEffect.BasicEffect;
        
        bool generateMipmaps = true;
        [DisplayName("ミップマップの生成")]
        [Description("有効な場合、モデルのテクスチャーに対して完全なミップマップチェーンが生成されます。既存のミップマップは置換されません")]
        [DefaultValue(true)]
        public virtual bool GenerateMipmaps { get { return generateMipmaps; } set { generateMipmaps = value; } }

        bool premultiplyTextureAlpha = true;
        [Description("有効な場合、モデルのテクスチャーは乗算済みアルファ形式に変換されます。")]
        [DefaultValue(true)]
        [DisplayName("アルファの事前乗算")]
        public virtual bool PremultiplyTextureAlpha { get { return premultiplyTextureAlpha; } set { premultiplyTextureAlpha = value; } }
        
        bool resizeTexturesToPowerOfTwo = true;
        [Description("有効な場合、モデルのテクスチャーは次に大きな2の累乗のサイズに変更され、可能な限りの互換性を保ちます。多くのグラフィックカードは、サイズが2の累乗でないテクスチャーに対応していません。")]
        [DisplayName("テクスチャーサイズを2の累乗にリサイズ")]
        [DefaultValue(true)]
        public virtual bool ResizeTexturesToPowerOfTwo { get { return resizeTexturesToPowerOfTwo; } set { resizeTexturesToPowerOfTwo = value; } }

        TextureProcessorOutputFormat textureFormat = TextureProcessorOutputFormat.DxtCompressed;
        [Description("処理されるテクスチャーのSurfaceFormat型を指定します。テクスチャーのフォーマットは、無変換、Color(32ビットRGBA)、またはDXT圧縮形式に変換されます。")]
        [DisplayName("テクスチャーフォーマット")]
        [DefaultValue(typeof(TextureProcessorOutputFormat), "DxtCompressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get { return textureFormat; } set { textureFormat = value; } }

        public override MMDModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            MMDModelScene mmdScene = null;
            this.context = context;
            resultModel = new MMDModelContent();
            //PMD用のデータが入っているかチェック(他のインポータからはデータが無い可能性あり)
            if (input.OpaqueData.ContainsKey("MMDScene"))
            {
                //抽出
                mmdScene = (MMDModelScene)input.OpaqueData["MMDScene"];
                //物理情報
                resultModel.Rigids = mmdScene.Rigids;
                resultModel.Joints = mmdScene.Joints;
                resultModel.FaceManager = mmdScene.FaceManager;
            }
            else
            {
                resultModel.FaceManager = new MMDFaceManagerContent();
            }
            //ボーン情報及びアニメーションデータの抽出
            resultModel.BoneManager = SkinningHelpers.CreateBoneManager(input, context, int.MaxValue, out resultModel.AttachedMotionData);
            //メッシュの処理
            ProcessNode(input);
            //XBox用表情頂点ポインタの適用
            foreach (var part in resultModel.ModelParts)
            {
                foreach (var it in resultModel.FaceManager.vertData2)
                {
                    if (part.VertMap.ContainsKey(it.Key))
                    {
                        foreach (var i in part.VertMap[it.Key])
                        {
                            //ポインタ情報の付与
                            SkinVertPtr ptr = resultModel.FaceManager.vertPtr[it.Key];
                            part.extVertices[i] = new Vector2(ptr.Pos, ptr.Count);
                        }
                    }
                }
            }
            //共用のエッジエフェクトの読み込み
            MMDModelContent.ReadEdgeEffect(context);
            return resultModel;
        }

        //ノード中のメッシュを処理
        private void ProcessNode(NodeContent input)
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                MeshHelper.CalculateNormals(mesh, false);
                //メッシュ内のジオメトリ処理
                foreach (var geometry in mesh.Geometry)
                {
                    ProcessGeometry(geometry);
                }
            }
            //子ノード内のメッシュも処理
            foreach (var child in input.Children)
            {
                ProcessNode(child);
            }
        }

        private void ProcessGeometry(GeometryContent geometry)
        {
            //頂点データのうち、ボーンウェイトチャンネルはBoneWeightCollectionになっている場合があるので、先に処理
            for (int i = 0; i < geometry.Vertices.Channels.Count; i++)
            {
                //BaseName=="Weights"となっているのがボーンウェイトチャンネル
                string channelName = geometry.Vertices.Channels[i].Name;
                string baseName = VertexChannelNames.DecodeBaseName(channelName);
                if (baseName == "Weights")
                {
                    ProcessWeightChannel(geometry, i);
                }
            }

            //頂点チャンネル名
            string vColorName = VertexChannelNames.EncodeName(VertexElementUsage.Color, 0);
            string normalName = VertexChannelNames.EncodeName(VertexElementUsage.Normal, 0);
            string texCoordName = VertexChannelNames.EncodeName(VertexElementUsage.TextureCoordinate, 0);
            string blendWeightName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, 0);
            string blendIndexName = VertexChannelNames.EncodeName(VertexElementUsage.BlendIndices, 0);
            //チャンネル
            VertexChannel<Vector4> vcolors = null;
            VertexChannel<Vector3> normals = null;
            VertexChannel<Vector2> texCoords = null;
            VertexChannel<Vector2> blendWeights = null;
            VertexChannel<Vector2> blendIndices = null;
            //チャンネルの抽出
            if (geometry.Vertices.Channels.IndexOf(vColorName) >= 0)
                vcolors = geometry.Vertices.Channels[vColorName] as VertexChannel<Vector4>;
            if (geometry.Vertices.Channels.IndexOf(normalName) >= 0)
                normals = geometry.Vertices.Channels[normalName] as VertexChannel<Vector3>;
            else
                throw new InvalidContentException("法線無いモデルも法線を自動生成しているはずなんだけど……");
            if (geometry.Vertices.Channels.IndexOf(texCoordName) >= 0)
                texCoords = geometry.Vertices.Channels[texCoordName] as VertexChannel<Vector2>;
            if (geometry.Vertices.Channels.IndexOf(blendWeightName) >= 0)
                blendWeights = geometry.Vertices.Channels[blendWeightName] as VertexChannel<Vector2>;
            if (geometry.Vertices.Channels.IndexOf(blendIndexName) >= 0)
                blendIndices = geometry.Vertices.Channels[blendIndexName] as VertexChannel<Vector2>;
            //頂点データを配列に抽出
            int triangleCount = geometry.Indices.Count / 3;
            MMDVertexNmContent[] vertices;
            int ShaderIndex = 0;
            //頂点の種類ごとに配列を生成
            if (texCoords != null)
            {
                if (vcolors != null)
                {
                    vertices = new MMDVertexNmTxVcContent[geometry.Vertices.VertexCount];
                    ShaderIndex = 3;
                }
                else
                {
                    vertices = new MMDVertexNmTxContent[geometry.Vertices.VertexCount];
                    ShaderIndex = 2;
                }
            }
            else
            {
                if (vcolors != null)
                {
                    vertices = new MMDVertexNmVcContent[geometry.Vertices.VertexCount];
                    ShaderIndex = 1;
                }
                else
                {
                    vertices = new MMDVertexNmContent[geometry.Vertices.VertexCount];
                    ShaderIndex = 0;
                }
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                //頂点ごとにデータの挿入（……汚いコードだなぁ……)
                MMDVertexNmContent vertN;
                if (texCoords != null)
                {
                    MMDVertexNmTxContent vertNTx;
                    if (vcolors != null)
                    {
                        MMDVertexNmTxVcContent vertNTxVc = new MMDVertexNmTxVcContent();
                        vertNTxVc.VertexColor = vcolors[i];
                        vertNTx = vertNTxVc;
                    }
                    else
                        vertNTx = new MMDVertexNmTxContent();
                    vertNTx.TextureCoordinate = texCoords[i];
                    vertN = vertNTx;
                }
                else
                {
                    if (vcolors != null)
                    {
                        MMDVertexNmVcContent vertNVc = new MMDVertexNmVcContent();
                        vertNVc.VertexColor = vcolors[i];
                        vertN = vertNVc;
                    }
                    else
                        vertN = new MMDVertexNmContent();
                }
                vertN.Normal = normals[i];
                vertices[i] = vertN;
                

                vertices[i].Position = geometry.Vertices.Positions[i];
                if (blendWeights == null || blendIndices == null)
                {
                    vertices[i].BlendIndexX = 0;
                    vertices[i].BlendIndexY = 0;
                    vertices[i].BlendWeights = Vector2.Zero;
                }
                else
                {
                    vertices[i].BlendWeights = blendWeights[i];
                    vertices[i].BlendIndexX = (int)blendIndices[i].X;
                    vertices[i].BlendIndexY = (int)blendIndices[i].Y;
                }
            }

            //マテリアルの変換
            MaterialContent material = ProcessMaterial(geometry.Material, ShaderIndex);

            //頂点マップの作成
            Dictionary<long, int[]> vertMap = new Dictionary<long, int[]>();
            Dictionary<long, List<int>> vertMapTemp = new Dictionary<long, List<int>>();
            for (int i = 0; i < geometry.Vertices.PositionIndices.Count; ++i)
            {
                if (!vertMapTemp.ContainsKey(geometry.Vertices.PositionIndices[i]))
                {
                    vertMapTemp.Add(geometry.Vertices.PositionIndices[i], new List<int>());
                }
                vertMapTemp[geometry.Vertices.PositionIndices[i]].Add(i);
            }
            foreach (var it in vertMapTemp)
                vertMap.Add(it.Key, it.Value.ToArray());
            //処理済みのジオメトリを返却用モデルに追加
            resultModel.AddModelPart(triangleCount, geometry.Indices, vertMap, vertices, material);

        }
        //BoneWeightCollectionの変換
        private void ProcessWeightChannel(GeometryContent geometry, int vertexChannelIndex)
        {
            //スケルトンを取得
            BoneContent skeleton = MeshHelper.FindSkeleton(geometry.Parent);
            //ボーン名→インデックスの辞書を作成
            Dictionary<string, int> boneIndices = new Dictionary<string, int>();
            IList<BoneContent> flattenedBones = MeshHelper.FlattenSkeleton(skeleton);
            for (int i = 0; i < flattenedBones.Count; i++)
            {
                boneIndices.Add(flattenedBones[i].Name, i);
            }

            //BoneWeightCollectionをIndicesとWeightsに変換
            VertexChannel<BoneWeightCollection> inputWeights = geometry.Vertices.Channels[vertexChannelIndex] as VertexChannel<BoneWeightCollection>;
            Vector2[] outputIndices = new Vector2[inputWeights.Count];
            Vector2[] outputWeights = new Vector2[inputWeights.Count];
            for (int i = 0; i < inputWeights.Count; i++)
            {
                ConvertWeights(inputWeights[i], boneIndices, outputIndices, outputWeights, i, geometry);
            }

            //IndicesとWeightsのチャンネルを作成
            int usageIndex = VertexChannelNames.DecodeUsageIndex(inputWeights.Name);
            string indicesName = VertexChannelNames.EncodeName(VertexElementUsage.BlendIndices, usageIndex);
            string weightsName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, usageIndex);

            //変換済みのデータを各チャンネルに流しこむ
            geometry.Vertices.Channels.Insert(vertexChannelIndex + 1, indicesName, outputIndices);
            geometry.Vertices.Channels.Insert(vertexChannelIndex + 2, weightsName, outputWeights);

            //BoneWeightCollectionのチャンネルの削除
            geometry.Vertices.Channels.RemoveAt(vertexChannelIndex);
        }

        //BoneWeightCollectionをIndicesとWeightsに変更
        static void ConvertWeights(BoneWeightCollection inputWeights, Dictionary<string, int> boneIndices, Vector2[] outIndices, Vector2[] outWeights, int vertexIndex, GeometryContent geometry)
        {
            //一つの頂点に対して、関連ボーンは2つまで
            const int maxWeights = 2;

            //処理用変数
            int[] tempIndices = new int[maxWeights];
            float[] tempWeights = new float[maxWeights];
            
            //3つ以上のボーンは無視して、ウェイトを正規化する
            inputWeights.NormalizeWeights(maxWeights);

            //ボーンウェイトを取得
            for (int i = 0; i < inputWeights.Count; i++)
            {
                if (i >= maxWeights)
                    throw new InvalidContentException("MikuMikuDanceXNAは1つの頂点に対して関連付けられるボーンは2つまでしかサポートしていません。このモデルには関連ボーンが" + inputWeights.Count.ToString() + "本ある頂点が存在します");
                BoneWeight weight = inputWeights[i];

                tempIndices[i] = boneIndices[weight.BoneName];
                tempWeights[i] = weight.Weight;
            }

            //残りは0で埋める
            for (int i = inputWeights.Count; i < maxWeights; i++)
            {
                tempIndices[i] = 0;
                tempWeights[i] = 0;
            }

            //変換済みの物をVector4に変換して出力
            outIndices[vertexIndex] = new Vector2(tempIndices[0], tempIndices[1]);
            outWeights[vertexIndex] = new Vector2(tempWeights[0], tempWeights[1]);
        }
        private MaterialContent ProcessMaterial(MaterialContent materialContent,int ShaderIndex)
        {
            //何回も処理するのを防ぐ仕掛け……
            if (!ProcessedMaterial.ContainsKey(materialContent))
            {
                MMDMaterialProcessor processor = new MMDMaterialProcessor();
                OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();
                processorParameters["DefaultEffect"] = DefaultEffect;
                processorParameters["ColorKeyColor"] = ColorKeyColor;
                processorParameters["ColorKeyEnabled"] = ColorKeyEnabled;
                processorParameters["TextureFormat"] = TextureFormat;
                processorParameters["GenerateMipmaps"] = GenerateMipmaps;
                processorParameters["PremultiplyTextureAlpha"] = PremultiplyTextureAlpha;
                processorParameters["ResizeTexturesToPowerOfTwo"] =
                    ResizeTexturesToPowerOfTwo;
                
                processorParameters["ShaderIndex"] = ShaderIndex;
                MaterialContent processed = context.Convert<MaterialContent, MaterialContent>(materialContent,
                                            "MMDMaterialProcessor", processorParameters);
                ProcessedMaterial[materialContent] = processed;
            }
            return ProcessedMaterial[materialContent];
        }

        
    }
}
