using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Accessory;
using MikuMikuDance.Resource;
using SlimDX.Direct3D9;
using SlimDX;
using System.Runtime.InteropServices;
using System.IO;
using MikuMikuDance.SlimDX.Misc;

namespace MikuMikuDance.SlimDX.Accessory
{
    /// <summary>
    /// アクセサリ用ファクトリー
    /// </summary>
    public class MMDAccessoryFactory : IMMDAccessoryFactory
    {
        private string BuildPath(string modelAbsPath, string resourcePath)
        {
            if (resourcePath == null)
                return null;
            if (Path.IsPathRooted(resourcePath))
                return resourcePath;
            string dir = Path.GetDirectoryName(modelAbsPath);
            return Path.Combine(dir, resourcePath);
        }

        #region IMMDAccessoryFactory メンバー
        /// <summary>
        /// ファイルから読み込み
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>アクセサリ</returns>
        public MMDAccessoryBase Load(string filename)
        {
            return InnerLoad(filename);
        }
        internal MMDAccessory InnerLoad(string filename)
        {
            filename = Path.GetFullPath(filename);
            Mesh mesh = Mesh.FromFile(SlimMMDXCore.Instance.Device, filename, MeshFlags.Managed);
            ExtendedMaterial[] materials = mesh.GetMaterials();
            //法線を付けておく
            if ((mesh.VertexFormat & VertexFormat.Normal) == 0)
            {
                //法線情報を加えたメッシュを複製
                Mesh tempMesh = mesh.Clone(SlimMMDXCore.Instance.Device, MeshFlags.Managed, mesh.VertexFormat | VertexFormat.Normal);
                //法線を計算
                tempMesh.ComputeNormals();

                //メッシュを置き換える
                mesh.Dispose();
                mesh = tempMesh;
            }
            
            bool[] Screen = new bool[materials.Length];
            Effect[] effects = new Effect[materials.Length];
            // テクスチャーを読み込み
            if (materials.Length >= 1)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    Texture texture = null, sphere = null;

                    //エフェクトの読み込み
                    effects[i] = Effect.FromMemory(SlimMMDXCore.Instance.Device, MMDXResource.AccessoryEffect,
#if DEBUG
 ShaderFlags.OptimizationLevel0 | ShaderFlags.Debug
#else
                        ShaderFlags.OptimizationLevel3
#endif
);
                    //テクスチャ名を確認
                    if (!string.IsNullOrEmpty(materials[i].TextureFileName))
                    {
                        // テクスチャーを読み込む
                        string texfile = materials[i].TextureFileName;
                        string spherefile = null;
                        if (texfile.IndexOf('*') != -1)
                        {
                            string[] temp = texfile.Split('*');
                            if (Path.GetExtension(temp[0]) == ".sph" || Path.GetExtension(temp[0]) == ".spa")
                            {
                                spherefile = temp[0];
                                texfile = temp[1];
                            }
                            else
                            {
                                spherefile = temp[1];
                                texfile = temp[0];
                            }
                        }
                        if (Path.GetExtension(texfile) == ".sph" || Path.GetExtension(texfile) == ".spa")
                        {
                            spherefile = texfile;
                            texfile = null;
                        }
                        texfile = BuildPath(filename, texfile);
                        spherefile = BuildPath(filename, spherefile);
                        if (!File.Exists(texfile) && Path.GetFileName(texfile) == "screen.bmp")
                        {//特殊：MMDのスクリーン機能
                            Screen[i] = true;
                            texfile = null;
                        }
                        else
                            Screen[i] = false;
                        if (!string.IsNullOrEmpty(texfile))
                            texture = Texture.FromFile(SlimMMDXCore.Instance.Device, texfile);
                        if (!string.IsNullOrEmpty(spherefile))
                            sphere = Texture.FromFile(SlimMMDXCore.Instance.Device, spherefile);
                    }
                    //エフェクト設定
                    if (texture != null)
                        effects[i].SetTexture("Texture", texture);
                    if (sphere != null)
                        effects[i].SetTexture("Sphere", sphere);
                    effects[i].SetValue("DiffuseColor", materials[i].MaterialD3D.Diffuse.ToVector3());
                    effects[i].SetValue("Alpha", materials[i].MaterialD3D.Diffuse.Alpha);
                    effects[i].SetValue("EmissiveColor", materials[i].MaterialD3D.Emissive.ToVector3());
                    effects[i].SetValue("SpecularColor", materials[i].MaterialD3D.Specular.ToVector3());
                    effects[i].SetValue("SpecularPower", materials[i].MaterialD3D.Power);
                    //シェーダインデックス設定
                    int shaderIndex = 0;
                    shaderIndex += (mesh.VertexFormat & VertexFormat.Diffuse) != 0 ? 1 : 0;
                    shaderIndex += ((texture) != null || Screen[i]) ? 2 : 0;
                    shaderIndex += (sphere) != null ? 4 : 0;
                    effects[i].SetValue("ShaderIndex", shaderIndex);
                }


            }
            return new MMDAccessory(mesh, effects, Screen, filename, this);
            
        }

        #endregion


    }
}
