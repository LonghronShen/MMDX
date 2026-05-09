using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Misc
{

    /// <summary>
    /// 拡大、回転、移動の3つを表す変換
    /// </summary>
    public struct SQTTransform
    {
        #region フィールド
        /// <summary>
        /// 拡大
        /// </summary>
        public MMDVector3 Scales;

        /// <summary>
        /// 回転
        /// </summary>
        public MMDQuaternion Rotation;

        /// <summary>
        /// 平行移動
        /// </summary>
        public MMDVector3 Translation;

        #endregion
        /// <summary>
        /// 恒等SQTTransformを返します
        /// </summary>
        public static SQTTransform Identity { get { return new SQTTransform(new MMDVector3(1, 1, 1), MMDQuaternion.Identity, MMDVector3.Zero); } }
        #region 初期化

        /// <summary>
        /// SQTTransformを生成
        /// </summary>
        /// <param name="scales">スケールベクトル</param>
        /// <param name="rotation">回転クォータニオン</param>
        /// <param name="translation">移動ベクトル</param>
        public SQTTransform(MMDVector3 scales, MMDQuaternion rotation, MMDVector3 translation)
        {
            Scales = scales;
            Rotation = rotation;
            Translation = translation;
        }
        /// <summary>
        /// SQTTransformを生成
        /// </summary>
        /// <param name="scales">スケールベクトル</param>
        /// <param name="rotation">回転クォータニオン</param>
        /// <param name="translation">移動ベクトル</param>
        /// <param name="result">SQTTransform</param>
        public static void Create(ref MMDVector3 scales, ref MMDQuaternion rotation, ref MMDVector3 translation, out SQTTransform result)
        {
            result = new SQTTransform() { Scales = scales, Rotation = rotation, Translation = translation };
        }
        /// <summary>
        /// SQTTransformの乗算
        /// </summary>
        public static void Multiply(ref SQTTransform value1, ref SQTTransform value2, out SQTTransform result)
        {
            result = new SQTTransform();
            // 平行移動の算出
            // 拡大→回転
            MMDVector3 temp = new MMDVector3();
            MMDVector3 newTranslation;
            temp.X = value1.Translation.X * value2.Scales.X;
            temp.Y = value1.Translation.Y * value2.Scales.Y;
            temp.Z = value1.Translation.Z * value2.Scales.Z;
            MMDVector3.Transform(ref temp, ref value2.Rotation, out newTranslation);

            newTranslation.X += value2.Translation.X;
            newTranslation.Y += value2.Translation.Y;
            newTranslation.Z += value2.Translation.Z;

            // 回転部分の結合(回転と拡大は独立だったはず……)
            MMDQuaternion.Multiply(ref value1.Rotation, ref value2.Rotation,
                                        out result.Rotation);
            //拡大部分の結合
            result.Scales.X = value1.Scales.X * value2.Scales.X;
            result.Scales.Y = value1.Scales.Y * value2.Scales.Y;
            result.Scales.Z = value1.Scales.Z * value2.Scales.Z;
            result.Translation = newTranslation;
        }
        /// <summary>
        /// 指定された行列から生成する
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static SQTTransform FromMatrix(MMDMatrix matrix)
        {
            // 行列の分解
            MMDQuaternion rotation;
            MMDVector3 translation;
            MMDVector3 scale;
            matrix.Decompose(out scale, out rotation, out translation);

            
            return new SQTTransform(scale, rotation, translation);
        }

        
        #endregion
        internal MMDMatrix CreateMatrix()
        {
            MMDMatrix result;
            CreateMatrix(out result);
            return result;
        }
        /// <summary>
        /// マトリックスの生成
        /// </summary>
        /// <param name="result">マトリックス</param>
        public void CreateMatrix(out MMDMatrix result)
        {
            MMDMatrix scales;
            MMDMatrix move;
            MMDMatrix rot;
            MMDMatrix temp;
            MMDXMath.CreateScaleMatrix(ref Scales, out scales);
            MMDXMath.CreateTranslationMatrix(ref Translation, out move);
            MMDXMath.CreateMatrixFromQuaternion(ref Rotation, out rot);
            MMDMatrix.Multiply(ref scales, ref rot, out temp);
            MMDMatrix.Multiply(ref temp, ref move, out result);

        }
        

        /// <summary>
        /// 姿勢の線形補間
        /// </summary>
        /// <param name="pose1">姿勢1</param>
        /// <param name="pose2">姿勢2</param>
        /// <param name="amount">補完係数(0-1)</param>
        /// <param name="result">線形補間された姿勢</param>
        /// <remarks>Quaternionは球状線形補間を使用</remarks>
        internal static void Lerp(ref SQTTransform pose1, ref SQTTransform pose2, float amount, out SQTTransform result)
        {
            MMDVector3.Lerp(ref pose1.Scales, ref pose2.Scales, amount, out result.Scales);
            MMDVector3.Lerp(ref pose1.Translation, ref pose2.Translation, amount, out result.Translation);
            MMDQuaternion.Slerp(ref pose1.Rotation, ref pose2.Rotation, amount, out result.Rotation);
        }

        
    }
}
