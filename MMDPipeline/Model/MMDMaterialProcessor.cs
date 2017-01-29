using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;
using MikuMikuDance.Resource;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMD用マテリアルプロセッサー
    /// </summary>
    [ContentProcessor(DisplayName = "MMDMaterialProcessor")]
    public class MMDMaterialProcessor : MaterialProcessor
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
                if(basicinput==null)
                    throw new InvalidContentException(string.Format(
                    "MMDProcessorはEffectMaterialContentのみをサポートします" +
                    "入力メッシュは{0}を使用しています。", input.GetType()));
                ExternalReference<EffectContent> effect;
                //リソースからファイルを作成して読み込むという超セコイ方法……
                if (!Directory.Exists("ext"))
                    Directory.CreateDirectory("ext");
                FileStream fs;
                if (context.TargetPlatform == TargetPlatform.Windows)
                {
                    fs = new FileStream(Path.Combine("ext", "MMDWinEffect.fx"), FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(MMDXResource.MMDWinEffect);
                    bw.Close();
                    effect = new ExternalReference<EffectContent>(Path.Combine("ext", "MMDWinEffect.fx"));
                }
                else if (context.TargetPlatform == TargetPlatform.Xbox360)
                {
                    fs = new FileStream(Path.Combine("ext", "MMDXBoxEffect.fx"), FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(MMDXResource.MMDXBoxEffect);
                    bw.Close();
                    effect = new ExternalReference<EffectContent>(Path.Combine("ext", "MMDXBoxEffect.fx"));
                }
                else
                    throw new NotImplementedException("ターゲットプラットフォーム:" + context.TargetPlatform.ToString() + " は対応していません");
                EffectMaterialContent effectcontent = new EffectMaterialContent();
                effectcontent.Effect = effect;
                //パラメータ設定
                effectcontent.OpaqueData.Add("ShaderIndex", ShaderIndex);
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
                        effectcontent.Textures.Add(it.Key, it.Value);
                    }
                }
                //データの渡し
                finalinput = effectcontent;
            }
            //テクスチャの事前アルファ計算は無し
            this.PremultiplyTextureAlpha = false;
            return base.Process(finalinput, context);
        }
    }
}
