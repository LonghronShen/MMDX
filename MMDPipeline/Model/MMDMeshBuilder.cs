using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MikuMikuDance.Model.Ver1;
using MikuMikuDance.XNA.Misc;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework;
using System.IO;
using System.Drawing;

namespace MikuMikuDance.XNA.Model
{
    //MMDは面ごとにチャンネルが変化するため、XNAのメッシュビルダーでは力不足。
    //メタセコイアインポータを参考に作成する。
    static class MMDMeshBuilder
    {
        public static MeshContent BuildMesh(MMDModel1 model, string filename)
        {
            //メッシュ作成
            MeshContent buildingMesh = new MeshContent();
            //まずは頂点を登録。
            //ひげねこ氏によるとモデルごとのローカル座標である必要があるようだが
            //pmdはモデル一つ＝変換必要なし
            foreach (var vec in model.Vertexes)
                buildingMesh.Positions.Add(MMDXMath.ToVector3(vec.Pos));

            //ジオメトリとマテリアルの作成
            //メモ：頂点が3つ合わさって面、面が幾つか集まってジオメトリ。ジオメトリにマテリアルが付随。ジオメトリの集合がメッシュ
            long FaceIndex = 0;
            Dictionary<ushort, int> vertMap = new Dictionary<ushort, int>();
            //ジオメトリとマテリアルの生成
            for (int i = 0; i < model.Materials.Length; i++)
            {
                GeometryContent geometry = new GeometryContent();
                BasicMaterialContent material = new BasicMaterialContent();
                geometry.Material = material;
                //マテリアル設定
                material.VertexColorEnabled = false;//頂点カラー無し
                material.Alpha = model.Materials[i].Alpha;
                material.DiffuseColor = MMDXMath.ToVector3(model.Materials[i].DiffuseColor);
                material.EmissiveColor = MMDXMath.ToVector3(model.Materials[i].MirrorColor);
                material.SpecularColor = MMDXMath.ToVector3(model.Materials[i].SpecularColor);
                material.SpecularPower = model.Materials[i].Specularity;
                if (!string.IsNullOrEmpty(model.Materials[i].TextureFileName))
                    material.Texture = new ExternalReference<TextureContent>(NormalizeFilepath(model.Materials[i].TextureFileName, filename));
                if (!string.IsNullOrEmpty(model.Materials[i].SphereTextureFileName))
                {
                    if (Path.GetExtension(model.Materials[i].SphereTextureFileName).ToLower() == ".sph")
                    {
                        material.OpaqueData.Add("UseSphere", 1);
                    }
                    else if (Path.GetExtension(model.Materials[i].SphereTextureFileName).ToLower() == ".spa")
                    {
                        material.OpaqueData.Add("UseSphere", 2);
                    }
                    else
                        throw new InvalidContentException("スフィアマップは*.sph, *.spaのみ指定可能です: " + model.Materials[i].SphereTextureFileName);
                    material.Textures.Add("Sphere", new ExternalReference<TextureContent>(ProcessSphere(NormalizeFilepath(model.Materials[i].SphereTextureFileName, filename))));
                }
                else
                {
                    material.OpaqueData.Add("UseSphere", 0);
                }
                //トゥーンのテクスチャを入れる
                string toonTexPath = ToonTexManager.Instance.GetToonTexPath(model.Materials[i].ToonIndex, model.ToonFileNames, filename);
                if (!string.IsNullOrEmpty(toonTexPath))
                {
                    material.Textures.Add("ToonTex", new ExternalReference<TextureContent>(toonTexPath));
                    material.OpaqueData.Add("UseToon", true);
                }
                else
                    material.OpaqueData.Add("UseToon", false);
                //一応エッジ情報突っ込んでおく
                material.OpaqueData.Add("Edge", (model.Materials[i].EdgeFlag != 0));
                //ジオメトリのチャンネル設定
                //法線
                geometry.Vertices.Channels.Add(VertexChannelNames.Normal(0), typeof(Vector3), null);
                //テクスチャ
                if (!string.IsNullOrEmpty(model.Materials[i].TextureFileName))
                    geometry.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(0), typeof(Vector2), null);
                //ボーンウェイト
                geometry.Vertices.Channels.Add(VertexChannelNames.Weights(0), typeof(BoneWeightCollection), null);

                
                //面と頂点をジオメトリに登録
                //このマテリアルに対応する面は今までのマテリアルの面数の合計からこのマテリアルの面数個分
                vertMap.Clear();
                //マテリアルに付随する面を取得
                for (long j = FaceIndex; j < FaceIndex + model.Materials[i].FaceVertCount; j++)
                {
                    //面から頂点番号取得
                    ushort VertIndex = model.FaceVertexes[j];
                    //ジオメトリに登録済みかどうか？
                    int geoVertIndex;
                    if (!vertMap.TryGetValue(VertIndex, out geoVertIndex))
                    {
                        //未登録なので、ジオメトリに登録し、ジオメトリ頂点番号取得
                        geoVertIndex = geometry.Vertices.Add(VertIndex);
                        //頂点マップに登録
                        vertMap.Add(VertIndex, geoVertIndex);
                        //チャンネル情報の登録
                        int channelIndex = 0;
                        //法線登録
                        geometry.Vertices.Channels.Get<Vector3>(channelIndex++)[geoVertIndex] = MMDXMath.ToVector3(model.Vertexes[VertIndex].NormalVector);
                        //テクスチャ座標
                        if (!string.IsNullOrEmpty(model.Materials[i].TextureFileName))
                            geometry.Vertices.Channels.Get<Vector2>(channelIndex++)[geoVertIndex] = MMDXMath.ToVector2(model.Vertexes[VertIndex].UV);
                        //ボーンウェイト
                        BoneWeightCollection boneWeight = new BoneWeightCollection();
                        int boneNum = model.Vertexes[VertIndex].BoneNum[0];
                        if (boneNum >= 0 && boneNum < model.Bones.Length)
                            boneWeight.Add(new BoneWeight(model.Bones[boneNum].BoneName, model.Vertexes[VertIndex].BoneWeight / 100f));
                        boneNum = model.Vertexes[VertIndex].BoneNum[1];
                        if (boneNum >= 0 && boneNum < model.Bones.Length)
                            boneWeight.Add(new BoneWeight(model.Bones[boneNum].BoneName, 1.0f - model.Vertexes[VertIndex].BoneWeight / 100f));
                        geometry.Vertices.Channels.Get<BoneWeightCollection>(channelIndex++)[geoVertIndex] = boneWeight;


                    }
                    //インデックスに登録
                    geometry.Indices.Add(geoVertIndex);
                }
                
                //ジオメトリをモデルに追加
                buildingMesh.Geometry.Add(geometry);
                //面頂点カウント進める
                FaceIndex += model.Materials[i].FaceVertCount;
            }
            //重複頂点データのマージ
            //MeshHelper.MergeDuplicatePositions(buildingMesh, 0);
            //MeshHelper.MergeDuplicateVertices(buildingMesh);
            
            //メッシュ出来たので返却
            return buildingMesh;
        }

        private static string ProcessSphere(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("スフィアマップ:"+path+"が見つかりません", path);
            //スフィアマップは.NETフレームワークを利用して.bmpに変換し、中間ディレクトリに吐き出す
            string midDir = Path.Combine(Path.GetDirectoryName(path), "ext");
            if (!Directory.Exists(midDir))
                Directory.CreateDirectory(midDir);
            string intFilePath = Path.Combine(midDir, Path.GetFileName(path));
            //.NETフレームワークを利用して読み込み
            Image sphere = null;
            try
            {
                sphere = new Bitmap(path);
            }
            catch (OutOfMemoryException)
            {//.tga形式は読み込めないのでこっちで処理
                intFilePath += ".tga";
                File.Copy(path, intFilePath, true);
            }
            //型判別
            if (sphere != null)
            {
                string Extention;
                ImageExtAnalyzer.Analyze(sphere, out Extention);
                intFilePath += Extention;
                File.Copy(path, intFilePath, true);
            }
            return intFilePath;
        }

        private static string NormalizeFilepath(string filename, string modelfilename)
        {
            if (Path.IsPathRooted(filename))
                return filename;
            return Path.Combine(Path.GetDirectoryName(modelfilename), filename);
        }
        
    }
}
