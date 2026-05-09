using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        void GetCameraParam(float aspectRatio, out  MMDMatrix view, out MMDMatrix proj);
        /// <summary>
        /// カメラ位置
        /// </summary>
        MMDVector3 Position { get; set; }
        /// <summary>
        /// 回転の設定
        /// </summary>
        /// <param name="rotate">回転</param>
        void SetRotation(MMDQuaternion rotate);
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
        void SetVector(MMDVector3 newVector);
    }
}
