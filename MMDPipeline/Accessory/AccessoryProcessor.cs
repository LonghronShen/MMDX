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
using MikuMikuDance.XNA.Model;
using System.IO;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MikuMikuDance for XNA用アクセサリプロセッサ
    /// </summary>
    [ContentProcessor(DisplayName = "MikuMikuDanceアクセサリ : MikuMikuDance for XNA")]
    public class AccessoryProcessor : ContentProcessor<NodeContent, MMDAccessoryContent>
    {
        ContentProcessorContext context;
        
        MMDAccessoryContent resultModel;
        Dictionary<MaterialContent, MaterialContent> ProcessedMaterial = new Dictionary<MaterialContent, MaterialContent>();

        Color colorKeyColor = new Color(255, 0, 255, 255);
        /// <summary>
        /// カラーキーの色
        /// </summary>
        [DisplayName("カラーキーの色")]
        [Description("モデルのテクスチャーに対してカラーキーが有効な場合、指定した色のピクセルはアルファ値が0の黒で置換されます")]
        [DefaultValue(typeof(Color), "255, 0, 255, 255")]
        public virtual Color ColorKeyColor { get { return colorKeyColor; } set { colorKeyColor = value; } }

        bool colorKeyEnabled = false;
        /// <summary>
        /// カラーキーの有無
        /// </summary>
        [DisplayName("カラーキーの有無")]
        [Description("有効な場合、モデルのテクスチャーに対してカラーキーを有効にします。\"カラーキーの色\"の値に一致するピクセルは、アルファ値が0の黒で置換されます")]
        [DefaultValue(false)]
        public virtual bool ColorKeyEnabled { get { return colorKeyEnabled; } set { colorKeyEnabled = value; } }

        MaterialProcessorDefaultEffect DefaultEffect = MaterialProcessorDefaultEffect.BasicEffect;

        bool generateMipmaps = true;
        /// <summary>
        /// ミップマップの生成
        /// </summary>
        [DisplayName("ミップマップの生成")]
        [Description("有効な場合、モデルのテクスチャーに対して完全なミップマップチェーンが生成されます。既存のミップマップは置換されません")]
        [DefaultValue(true)]
        public virtual bool GenerateMipmaps { get { return generateMipmaps; } set { generateMipmaps = value; } }

        bool premultiplyTextureAlpha = true;
        /// <summary>
        /// 乗算済みアルファ
        /// </summary>
        [Description("有効な場合、モデルのテクスチャーは乗算済みアルファ形式に変換されます。")]
        [DefaultValue(true)]
        [DisplayName("アルファの事前乗算")]
        public virtual bool PremultiplyTextureAlpha { get { return premultiplyTextureAlpha; } set { premultiplyTextureAlpha = value; } }

        bool resizeTexturesToPowerOfTwo = true;
        /// <summary>
        /// テクスチャーサイズを2の累乗にリサイズ
        /// </summary>
        [Description("有効な場合、モデルのテクスチャーは次に大きな2の累乗のサイズに変更され、可能な限りの互換性を保ちます。多くのグラフィックカードは、サイズが2の累乗でないテクスチャーに対応していません。")]
        [DisplayName("テクスチャーサイズを2の累乗にリサイズ")]
        [DefaultValue(true)]
        public virtual bool ResizeTexturesToPowerOfTwo { get { return resizeTexturesToPowerOfTwo; } set { resizeTexturesToPowerOfTwo = value; } }

        TextureProcessorOutputFormat textureFormat = TextureProcessorOutputFormat.DxtCompressed;
        /// <summary>
        /// テクスチャーフォーマット
        /// </summary>
        [Description("処理されるテクスチャーのSurfaceFormat型を指定します。テクスチャーのフォーマットは、無変換、Color(32ビットRGBA)、またはDXT圧縮形式に変換されます。")]
        [DisplayName("テクスチャーフォーマット")]
        [DefaultValue(typeof(TextureProcessorOutputFormat), "DxtCompressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get { return textureFormat; } set { textureFormat = value; } }

        bool edge = false;
        /// <summary>
        /// エッジ描画
        /// </summary>
        [Description("アクセサリにエッジを付けるかどうかが指定します。")]
        [DisplayName("エッジ描画")]
        [DefaultValue(false)]
        public virtual bool Edge { get { return edge; } set { edge = value; } }

        float scaling = 10;
        /// <summary>
        /// スケーリング
        /// </summary>
        [Description("アクセサリのスケーリング値")]
        [DisplayName("スケーリング")]
        [DefaultValue(10)]
        public virtual float Scale { get { return scaling; } set { scaling = value; } }
        /// <summary>
        /// アクセサリの処理
        /// </summary>
        public override MMDAccessoryContent Process(NodeContent input, ContentProcessorContext context)
        {
            this.context = context;
            resultModel = new MMDAccessoryContent();
            ProcessNode(input);
            MMDModelContent.ReadEdgeEffect(context);//エッジ描画用の共用エフェクトを読み込む～
            return resultModel;
        }
        //ノード中のメッシュを処理
        private void ProcessNode(NodeContent input)
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                //余計な頂点データの排除
                //MeshHelper.MergeDuplicatePositions(mesh, 0);
                //MeshHelper.MergeDuplicateVertices(mesh);
                MeshHelper.CalculateNormals(mesh, false);
                //頂点データの抽出
                resultModel.Vertex = new MMDVertexNmTxVcContent[mesh.Geometry.Sum((x) => x.Vertices.Positions.Count)];
                int vertCount = 0;
                //メッシュ内のジオメトリ処理
                foreach (var geometry in mesh.Geometry)
                {
                    ProcessGeometry(geometry, resultModel.Vertex, ref vertCount);
                }

            }
            //子ノード内のメッシュも処理
            foreach (var child in input.Children)
            {
                ProcessNode(child);
            }
        }

        private void ProcessGeometry(GeometryContent geometry, MMDVertexNmTxVcContent[] vertices, ref int vertStart)
        {
            int ShaderIndex = 0;
            //頂点チャンネル名
            string vColorName = VertexChannelNames.EncodeName(VertexElementUsage.Color, 0);
            string normalName = VertexChannelNames.EncodeName(VertexElementUsage.Normal, 0);
            string texCoordName = VertexChannelNames.EncodeName(VertexElementUsage.TextureCoordinate, 0);
            //チャンネル
            VertexChannel<Vector4> vcolors = null;
            VertexChannel<Vector3> normals = null;
            VertexChannel<Vector2> texCoords = null;
            //
            //チャンネルの抽出
            if (geometry.Vertices.Channels.IndexOf(vColorName) >= 0)
            {
                vcolors = geometry.Vertices.Channels[vColorName] as VertexChannel<Vector4>;
                ShaderIndex += 1;
            }
            if (geometry.Vertices.Channels.IndexOf(normalName) >= 0)
                normals = geometry.Vertices.Channels[normalName] as VertexChannel<Vector3>;
            if (geometry.Vertices.Channels.IndexOf(texCoordName) >= 0)
            {
                texCoords = geometry.Vertices.Channels[texCoordName] as VertexChannel<Vector2>;
            }
            //頂点データを配列に抽出
            int triangleCount = geometry.Indices.Count / 3;
            for (int i = 0; i < geometry.Vertices.Positions.Count; i++)
            {
                vertices[i + vertStart] = new MMDVertexNmTxVcContent();
                if (normals != null)
                    vertices[i + vertStart].Normal = normals[i];
                else
                    vertices[i + vertStart].Normal = Vector3.Zero;//来るはず無いけどね……
                if (vcolors != null)
                    vertices[i + vertStart].VertexColor = vcolors[i];
                else
                    vertices[i + vertStart].VertexColor = Vector4.One;
                if (texCoords != null)
                    vertices[i + vertStart].TextureCoordinate = texCoords[i];
                else
                    vertices[i + vertStart].TextureCoordinate = Vector2.Zero;
                vertices[i + vertStart].Position = geometry.Vertices.Positions[i];
                vertices[i + vertStart].Position *= scaling;
            }
            MMDAccessoryPartContent part = new MMDAccessoryPartContent() { VertexCount = geometry.Vertices.Positions.Count, BaseVertex = vertStart, TriangleCount = triangleCount, Edge = Edge };
            vertStart += geometry.Vertices.Positions.Count;

            if (geometry.Material.Textures.Count > 0)
                ShaderIndex += 2;
            //スクリーンチェック
            List<string> screenKey = new List<string>();
            foreach (var it in geometry.Material.Textures)
            {
                if (Path.GetFileName(it.Value.Filename) == "screen.bmp")
                {
                    part.Screen = true;
                    screenKey.Add(it.Key);
                }
            }
            foreach (var key in screenKey)
                geometry.Material.Textures.Remove(key);
            /*if (geometry.Material.Textures.ContainsKey("Texuture") && 
                Path.GetFileName(geometry.Material.Textures["Texture"].Filename) == "screen.bmp")
            {
                part.Screen = true;
                geometry.Material.Textures["Texture"].Filename = null;
            }*/
            part.IndexBuffer = new IndexCollection();
            part.IndexBuffer.AddRange(geometry.Indices);
            part.Material = ProcessMaterial(geometry.Material, ShaderIndex);
            resultModel.Parts.Add(part);
        }
        private MaterialContent ProcessMaterial(MaterialContent materialContent, int ShaderIndex)
        {
            //何回も処理するのを防ぐ仕掛け……
            if (!ProcessedMaterial.ContainsKey(materialContent))
            {
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
                                            "MMDAccessoryMaterialProcessor", processorParameters);
                ProcessedMaterial[materialContent] = processed;
            }
            return ProcessedMaterial[materialContent];
        }
    }
}
