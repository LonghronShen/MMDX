using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
using MikuMikuDance.Resource;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MMD用マテリアルプロセッサー
    /// </summary>
    [ContentProcessor(DisplayName = "MMDAccessoryMaterialProcessor")]
    public class MMDAccessoryMaterialProcessor : MaterialProcessor
    {
        /// <summary>
        /// シェーダーインデックス
        /// </summary>
        public int ShaderIndex { get; set; }
        /// <summary>
        /// プロセッサ処理
        /// </summary>
        public override MaterialContent Process(MaterialContent input, ContentProcessorContext context)
        {
            MaterialContent finalinput;
            if (context.TargetPlatform == TargetPlatform.WindowsPhone)
            {
                finalinput = input;
            }
            else
            {
                BasicMaterialContent basicinput = input as BasicMaterialContent;
                if (basicinput == null)
                    throw new InvalidContentException(string.Format(
                    "MMDProcessorはEffectMaterialContentのみをサポートします" +
                    "入力メッシュは{0}を使用しています。", input.GetType()));
                ExternalReference<EffectContent> effect;
                //リソースからファイルを作成して読み込むという超セコイ方法……
                if (!Directory.Exists("ext"))
                    Directory.CreateDirectory("ext");
                FileStream fs;
                fs = new FileStream(Path.Combine("ext", "MMDAccessoryEffect.fx"), FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(MMDXResource.AccessoryEffect);
                bw.Close();
                effect = new ExternalReference<EffectContent>(Path.Combine("ext", "MMDAccessoryEffect.fx"));

                EffectMaterialContent effectcontent = new EffectMaterialContent();
                effectcontent.Effect = effect;
                //パラメータコピー
                foreach (var data in basicinput.OpaqueData)
                {
                    effectcontent.OpaqueData.Add(data.Key, data.Value);
                }
                //テクスチャのコピー
                if (basicinput.Textures.Count > 0)
                {
                    foreach (var it in basicinput.Textures)
                    {
                        if (string.IsNullOrEmpty(it.Value.Filename))
                            continue;
                        if (it.Value.Filename.IndexOf('*') != -1)
                        {
                            string[] files = it.Value.Filename.Split('*');
                            foreach(var file in files){
                                if (Path.GetExtension(file) == ".sph" || Path.GetExtension(file) == ".spa")
                                {
                                    effectcontent.Textures.Add("Sphere", new ExternalReference<TextureContent>(CreateSpherePath(file)));
                                }
                                else
                                {
                                    effectcontent.Textures.Add(it.Key, new ExternalReference<TextureContent>(file));
                                }
                            }
                        }
                        else if (Path.GetExtension(it.Value.Filename) == ".sph" || Path.GetExtension(it.Value.Filename) == ".spa")
                        {
                            it.Value.Filename = CreateSpherePath(it.Value.Filename);
                            effectcontent.Textures.Add("Sphere", it.Value);
                        }
                        else
                            effectcontent.Textures.Add(it.Key, it.Value);
                    }
                }
                //パラメータ設定
                effectcontent.OpaqueData.Add("ShaderIndex", ShaderIndex);
                //データの渡し
                finalinput = effectcontent;
            }
            //事前アルファOff
            this.PremultiplyTextureAlpha = false;
            return base.Process(finalinput, context);
        }

        private string CreateSpherePath(string file)
        {
            file = Path.GetFullPath(file);
            string dir = Path.Combine(Path.GetDirectoryName(file), "ext");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string newfile = Path.Combine(dir, Path.ChangeExtension(file, ".bmp"));
            File.Copy(file, newfile);
            return file;
        }
    }
}
