using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DWORD = System.UInt32;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// 剛体情報
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Model.Physics.MMDJoint, MikuMikuDanceCore")]
    public class MMDJointContent
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name; // 諸データ：名称 // 右髪1(char*20)
        /// <summary>
        /// 剛体A
        /// </summary>
        public DWORD RigidBodyA; // 諸データ：剛体A
        /// <summary>
        /// 剛体B
        /// </summary>
        public DWORD RigidBodyB; // 諸データ：剛体B
        /// <summary>
        /// 位置(x, y, z)
        /// </summary>
        public float[] Position; //float*3 諸データ：位置(x, y, z) // 諸データ：位置合せでも設定可
        /// <summary>
        /// 回転(rad(x), rad(y), rad(z))
        /// </summary>
        public float[] Rotation; //float*3 諸データ：回転(rad(x), rad(y), rad(z))
        /// <summary>
        /// 移動制限1(x, y, z)
        /// </summary>
        public float[] ConstrainPosition1; //float*3 制限：移動1(x, y, z)
        /// <summary>
        /// 移動制限2(x, y, z)
        /// </summary>
        public float[] ConstrainPosition2; //float*3 制限：移動2(x, y, z)
        /// <summary>
        /// 回転制限1(rad(x), rad(y), rad(z))
        /// </summary>
        public float[] ConstrainRotation1; //float*3 制限：回転1(rad(x), rad(y), rad(z))
        /// <summary>
        /// 回転制限2(rad(x), rad(y), rad(z))
        /// </summary>
        public float[] ConstrainRotation2; //float*3 制限：回転2(rad(x), rad(y), rad(z))
        /// <summary>
        /// 平行移動に対するばねの戻る強さ：移動(x, y, z)
        /// </summary>
        public float[] SpringPosition; //float*3 ばね：移動(x, y, z)
        /// <summary>
        /// 回転に対するばねの戻る強さ：回転(rad(x), rad(y), rad(z))
        /// </summary>
        public float[] SpringRotation; //float*3 ばね：回転(rad(x), rad(y), rad(z))
    }
}
