using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Misc
{
    /// <summary>
    /// クォータニオンを使った回転と平行移動を表すことのできる頂点変換用構造体
    /// 通常の行列の半分以下のメモリ使用量になる
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Misc.SQTTransform, MikuMikuDanceCore")]
    public struct SQTTransformContent
    {
        #region フィールド

        /// <summary>
        /// 拡大
        /// </summary>
        public Vector3 Scales;

        /// <summary>
        /// 回転
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// 平行移動
        /// </summary>
        public Vector3 Translation;


        #endregion
        /// <summary>
        /// 恒等QuatTransformを返します
        /// </summary>
        public static SQTTransformContent Identity { get { return new SQTTransformContent(Vector3.One, Quaternion.Identity, Vector3.Zero); } }
        #region 初期化

        /// <summary>
        /// SQTTransformを生成
        /// </summary>
        /// <param name="scales">スケールベクトル</param>
        /// <param name="rotation">回転クォータニオン</param>
        /// <param name="translation">移動ベクトル</param>
        public SQTTransformContent(Vector3 scales, Quaternion rotation, Vector3 translation)
        {
            Scales = scales;
            Rotation = rotation;
            Translation = translation;
        }

        /// <summary>
        /// 指定された行列から生成する
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static SQTTransformContent FromMatrix(Matrix matrix)
        {
            // 行列の分解
            Quaternion rotation;
            Vector3 translation;
            Vector3 scale;
            matrix.Decompose(out scale, out rotation, out translation);

            
            return new SQTTransformContent(scale, rotation, translation);
        }

        

        #endregion

        
    }
}
