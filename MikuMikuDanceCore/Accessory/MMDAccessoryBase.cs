using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Accessory
{
    /// <summary>
    /// アクセサリー抽象クラス
    /// </summary>
    public abstract class MMDAccessoryBase
    {
        /// <summary>
        /// アクセサリーを保持しているモデル
        /// </summary>
        public MMDModel Model { get; set; }
        /// <summary>
        /// モデルとアクセサリとの接続情報
        /// </summary>
        public MMD_VAC VAC;
        /// <summary>
        /// アクセサリーの位置
        /// </summary>
        /// <remarks>モデルがアクセサリーを保持中は保持した状態を基準とした座標系</remarks>
        public Matrix Transform = Matrix.Identity;
        /// <summary>
        /// テクスチャとしてスクリーンを適応するフラグ
        /// </summary>
        protected bool[] Screen;

        
        /// <summary>
        /// アクセサリーの位置を取得
        /// </summary>
        /// <param name="position">位置を示したMatrix</param>
        public void GetPosition(out Matrix position)
        {
            if (Model != null && !string.IsNullOrEmpty(VAC.BoneName))
            {
                Matrix temp, temp2;
                Matrix.Multiply(ref Transform, ref VAC.Transform, out temp);
                Matrix.Multiply(ref temp, ref Model.BoneManager[VAC.BoneName].GlobalTransform, out temp2);
                Matrix.Multiply(ref temp2, ref Model.Transform, out position);
            }
            else
                position = Transform;
        }

        /// <summary>
        /// アクセサリーの描画
        /// </summary>
        public void Draw()
        {
            Matrix Position;
            GetPosition(out Position);
            Draw(ref Position);
        }
        /// <summary>
        /// アクセサリーを描画
        /// </summary>
        /// <param name="Position">描画する位置</param>
        protected abstract void Draw(ref Matrix Position);
        
    }
}
