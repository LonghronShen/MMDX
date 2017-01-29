using System;
using MikuMikuDance.Core.Misc;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Model
{
    /// <summary>
    /// MMDモデルパーツインターフェイス
    /// </summary>
    public interface IMMDModelPart : IDisposable
    {
        /// <summary>
        /// 各種パラメータのセット
        /// </summary>
        /// <param name="mode">モデル描画モード</param>
        /// <param name="world">ワールドマトリクス</param>
        void SetParams(MMDDrawingMode mode, ref Matrix world);
        /// <summary>
        /// スキン行列の設定
        /// </summary>
        /// <param name="skinTransforms">スキン行列配列</param>
        void SetSkinMatrix(Matrix[] skinTransforms);

        /// <summary>
        /// モデルパーツの描画
        /// </summary>
        /// <param name="mode">モデル描画モード</param>
        void Draw(MMDDrawingMode mode);
        /// <summary>
        /// 表情の設定
        /// </summary>
        /// <param name="faceManager"></param>
        void SetFace(IMMDFaceManager faceManager);
        /// <summary>
        /// モデル追加時に呼ばれる
        /// </summary>
        /// <param name="model">親モデル</param>
        void SetModel(MMDModel model);
    }
}
