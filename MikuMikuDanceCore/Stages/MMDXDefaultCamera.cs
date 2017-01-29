using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Misc;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

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
        public Vector3 CameraPos;
        /// <summary>
        /// カメラ方向と距離
        /// </summary>
        public Vector3 CameraVector;
        /// <summary>
        /// カメラの上方向ベクトル
        /// </summary>
        public Vector3 CameraUpVector = Vector3.UnitY;
        /// <summary>
        /// 回転
        /// </summary>
        public Quaternion Rotation = Quaternion.Identity;
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
        public Vector3 Position { get { return CameraPos; } set { CameraPos = value; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MMDXDefaultCamera()
        {
            CameraPos = new Vector3(0, 10, 35);
            CameraVector = new Vector3(0, 0, -35);
            FieldOfView = MathHelper.PiOver4;
            Near = 1;
            Far = 300;
        }

        /// <summary>
        /// カメラ情報
        /// </summary>
        /// <param name="aspectRatio">アスペクト比</param>
        /// <param name="view">ビュー情報</param>
        /// <param name="proj">プロジェクション情報</param>
        public void GetCameraParam(float aspectRatio, out  Matrix view, out Matrix proj)
        {
            Vector3 CameraTarget, trueCameraVector, trueCameraUpVector;
#if SlimDX
            Vector4 temp1, temp2;
            Vector3.Transform(ref CameraVector, ref Rotation, out temp1);
            Vector3.Transform(ref CameraUpVector, ref Rotation, out temp2);
            trueCameraVector = new Vector3(temp1.X, temp1.Y, temp1.Z);
            trueCameraUpVector = new Vector3(temp2.X, temp2.Y, temp2.Z);
#elif XNA
            Vector3.Transform(ref CameraVector, ref Rotation, out trueCameraVector);
            Vector3.Transform(ref CameraUpVector, ref Rotation, out trueCameraUpVector);
#endif
            Vector3.Add(ref CameraPos, ref trueCameraVector, out CameraTarget);
            MMDXMath.CreateLookAtMatrix(ref CameraPos, ref CameraTarget, ref trueCameraUpVector, out view);
            MMDXMath.CreatePerspectiveFieldOfViewMatrix(FieldOfView, aspectRatio, Near, Far, out proj);
        }
        /// <summary>
        /// カメラベクトルの設定
        /// </summary>
        /// <param name="newVector">カメラベクトル</param>
        public void SetVector(Vector3 newVector)
        {
            CameraVector = newVector;
        }

        /// <summary>
        /// 視野角の設定/取得
        /// </summary>
        public void SetRotation(Quaternion rot)
        {
            Rotation = rot;
        }
        
    }
}
