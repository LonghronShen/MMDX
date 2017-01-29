using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using TInput = MikuMikuDance.XNA.Accessory.VACContent2;
using TOutput = MikuMikuDance.XNA.Accessory.VACContent;
using System.ComponentModel;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// VACのプロセッサ
    /// </summary>
    [ContentProcessor(DisplayName = "MikuMikuDance VAC : MikuMikuDance for XNA")]
    public class VACProcessor : ContentProcessor<TInput, TOutput>
    {
        bool leftHanded = true;
        /// <summary>
        /// 左手座標系から右手座標系への変換
        /// </summary>
        [DefaultValue(true)]
        [DisplayName("左手→右手への変換")]
        [Description("VACファイルをMikuMikuDance標準の左手座標系で記述している場合はtrueを指定")]
        public bool LeftHanded { get { return leftHanded; } set { leftHanded = value; } }
        /// <summary>
        /// VACProcessor
        /// </summary>
        /// <param name="input">VACContentデータ</param>
        /// <param name="context">プロセッサコンテキスト</param>
        /// <returns>MMD_VAC</returns>
        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            if (LeftHanded)
            {
                input.Trans.Z = -input.Trans.Z;
                input.Rot.X = -input.Rot.X;
                input.Rot.Y = -input.Rot.Y;
            }
            return new TOutput
            {
                BoneName = input.BoneName,
                Transform = Matrix.CreateScale(input.Scale)
                * Matrix.CreateFromYawPitchRoll(input.Rot.Y, input.Rot.X, input.Rot.Z)
                * Matrix.CreateTranslation(input.Trans)
            };
        }
    }
}
