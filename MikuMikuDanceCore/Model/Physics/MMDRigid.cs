using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WORD = System.UInt16;

namespace MikuMikuDance.Core.Model.Physics
{
    /// <summary>
    /// 剛体情報
    /// </summary>
    public class MMDRigid
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name; // 諸データ：名称 // 頭(20byte char)
        /// <summary>
        /// 関連ボーン番号
        /// </summary>
        public string RelatedBoneName; // 諸データ：関連ボーン番号 // 03 00 == 3 // 頭
        /// <summary>
        /// グループ番号
        /// </summary>
        public byte GroupIndex; // 諸データ：グループ // 00
        /// <summary>
        /// 衝突対象グループ
        /// </summary>
        /// <remarks>各ビットがグループ番号に対応しており、ビットが立ってなければそのグループとは衝突しないという実装</remarks>
        public WORD GroupTarget; // 諸データ：グループ：対象 // 0xFFFFとの差 // 38 FE
        /// <summary>
        /// 形状
        /// </summary>
        /// <remarks>0:球、1:箱、2:カプセル</remarks>
        public byte ShapeType; // 形状：タイプ(0:球、1:箱、2:カプセル) // 00 // 球
        /// <summary>
        /// 半径(幅)
        /// </summary>
        public float ShapeWidth; // 形状：半径(幅) // CD CC CC 3F // 1.6
        /// <summary>
        /// 高さ
        /// </summary>
        public float ShapeHeight; // 形状：高さ // CD CC CC 3D // 0.1
        /// <summary>
        /// 奥行き
        /// </summary>
        public float ShapeDepth; // 形状：奥行 // CD CC CC 3D // 0.1
        /// <summary>
        /// 位置(x,y,z)
        /// </summary>
        public float[] Position; //float*3 位置：位置(x, y, z)
        /// <summary>
        /// 回転
        /// </summary>
        public float[] Rotation; //float*3 位置：回転(rad(x), rad(y), rad(z))
        /// <summary>
        /// 質量
        /// </summary>
        public float Weight; // 諸データ：質量 // 00 00 80 3F // 1.0
        /// <summary>
        /// ダンピング１
        /// </summary>
        public float LinerDamping; // 諸データ：移動減 // 00 00 00 00
        /// <summary>
        /// ダンピング２
        /// </summary>
        public float AngularDamping; // 諸データ：回転減 // 00 00 00 00
        /// <summary>
        /// 反発係数
        /// </summary>
        public float Restitution; // 諸データ：反発力 // 00 00 00 00
        /// <summary>
        /// 摩擦力
        /// </summary>
        public float Friction; // 諸データ：摩擦力 // 00 00 00 00
        /// <summary>
        /// 剛体タイプ
        /// </summary>
        /// <remarks>0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)</remarks>
        public byte Type { get; set; } // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
        
    }
}
