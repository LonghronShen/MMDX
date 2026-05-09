using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Misc;

namespace MikuMikuDance.Core.Stages
{
    /// <summary>
    /// デフォルトカメラ
    /// </summary>
    public class MMDXDefaultCamera :IMMDXCamera
    {
        /// <summary>
        /// カメラ位置
        /// </summary>
        public MMDVector3 CameraPos;
        /// <summary>
        /// カメラ方向と距離
        /// </summary>
        public MMDVector3 CameraVector;
        /// <summary>
        /// カメラの上方向ベクトル
        /// </summary>
        public MMDVector3 CameraUpVector = MMDVector3.UnitY;
        /// <summary>
        /// 回転
        /// </summary>
        public MMDQuaternion Rotation = MMDQuaternion.Identity;
        /// <summary>
        /// Near面
        /// </summary>
        public float Near { get; set; }
        /// <summary>
        /// Far面
        /// </summary>
        public float Far { get; set; }
        /// <summary>
        /// 視野角
        /// </summary>
        public float FieldOfView { get; set; }
        /// <summary>
        /// カメラ位置
        /// </summary>
        public MMDVector3 Position { get { return CameraPos; } set { CameraPos = value; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MMDXDefaultCamera()
        {
            CameraPos = new MMDVector3(0, 10, 35);
            CameraVector = new MMDVector3(0, 0, -35);
            FieldOfView = MMDMathHelper.PiOver4;
            Near = 1;
            Far = 300;
        }

        /// <summary>
        /// カメラ情報
        /// </summary>
        /// <param name="aspectRatio">アスペクト比</param>
        /// <param name="view">ビュー情報</param>
        /// <param name="proj">プロジェクション情報</param>
        public void GetCameraParam(float aspectRatio, out  MMDMatrix view, out MMDMatrix proj)
        {
            MMDVector3 CameraTarget, trueCameraVector, trueCameraUpVector;
            MMDVector3.Transform(ref CameraVector, ref Rotation, out trueCameraVector);
            MMDVector3.Transform(ref CameraUpVector, ref Rotation, out trueCameraUpVector);
            MMDVector3.Add(ref CameraPos, ref trueCameraVector, out CameraTarget);
            MMDXMath.CreateLookAtMatrix(ref CameraPos, ref CameraTarget, ref trueCameraUpVector, out view);
            MMDXMath.CreatePerspectiveFieldOfViewMatrix(FieldOfView, aspectRatio, Near, Far, out proj);
        }
        /// <summary>
        /// カメラベクトルの設定
        /// </summary>
        /// <param name="newVector">カメラベクトル</param>
        public void SetVector(MMDVector3 newVector)
        {
            CameraVector = newVector;
        }

        /// <summary>
        /// 視野角の設定/取得
        /// </summary>
        public void SetRotation(MMDQuaternion rot)
        {
            Rotation = rot;
        }
        
    }
}
