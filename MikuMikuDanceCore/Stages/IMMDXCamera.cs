using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Stages
{
    /// <summary>
    /// カメラ情報インターフェイス
    /// </summary>
    public interface IMMDXCamera
    {
        /// <summary>
        /// カメラ情報
        /// </summary>
        /// <param name="aspectRatio">アスペクト比</param>
        /// <param name="view">ビュー情報</param>
        /// <param name="proj">プロジェクション情報</param>
        void GetCameraParam(float aspectRatio, out  Matrix view, out Matrix proj);
        /// <summary>
        /// カメラ位置
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// 回転の設定
        /// </summary>
        /// <param name="rotate">回転</param>
        void SetRotation(Quaternion rotate);
        /// <summary>
        /// 視野角の設定/取得
        /// </summary>
        float FieldOfView { get; set; }
        /// <summary>
        /// Near面
        /// </summary>
        float Near { get; set; }
        /// <summary>
        /// Far面
        /// </summary>
        float Far { get; set; }
        
        /// <summary>
        /// カメラベクトルの設定
        /// </summary>
        /// <param name="newVector">カメラベクトル</param>
        void SetVector(Vector3 newVector);
    }
}
