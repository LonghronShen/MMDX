using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DWORD = System.UInt32;
using MikuMikuDance.Core.Misc;

#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Motion
{
    /// <summary>
    /// MMD用ボーンキーフレーム
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    public class MMDBoneKeyFrame
    {
        /// <summary>
        /// ボーン名
        /// </summary>
        public string BoneName;//[15];
        /// <summary>
        /// フレーム番号
        /// </summary>
        public DWORD FrameNo;
        /// <summary>
        /// スケールベクトル
        /// </summary>
        public Vector3 Scales;
        /// <summary>
        /// 位置ベクトル
        /// </summary>
        public Vector3 Location;
        /// <summary>
        /// クォータニオン
        /// </summary>
        public Quaternion Quatanion;

        /// <summary>
        /// 補完用曲線
        /// </summary>
        /// <remarks>順にX,Y,Z,回転</remarks>
        public BezierCurve[] Curve;

        /// <summary>
        /// 補完
        /// </summary>
        /// <param name="frame1">フレーム1</param>
        /// <param name="frame2">フレーム2</param>
        /// <param name="Progress">進行度合い</param>
        /// <param name="result">補完結果</param>
        public static void Lerp(MMDBoneKeyFrame frame1,MMDBoneKeyFrame frame2, float Progress, out SQTTransform result)
        {
            float ProgX, ProgY, ProgZ,ProgR;
            ProgX = frame2.Curve[0].Evaluate(Progress);
            ProgY = frame2.Curve[1].Evaluate(Progress);
            ProgZ = frame2.Curve[2].Evaluate(Progress);
            ProgR = frame2.Curve[3].Evaluate(Progress);
            float x, y, z;
            Quaternion q;
            Vector3 scales;
            x = MathHelper.Lerp(frame1.Location.X, frame2.Location.X, ProgX);
            y = MathHelper.Lerp(frame1.Location.Y, frame2.Location.Y, ProgY);
            z = MathHelper.Lerp(frame1.Location.Z, frame2.Location.Z, ProgZ);
            Quaternion.Slerp(ref frame1.Quatanion,ref frame2.Quatanion, ProgR,out q );
            //MMDはスケールのアニメーションを含まないので、スケールのベジェ曲線計算は行わない
            Vector3.Lerp(ref frame1.Scales, ref frame2.Scales, Progress, out scales);
            Vector3 t=new Vector3(x, y, z);
            SQTTransform.Create(ref scales, ref q, ref t, out result);
        }
        /// <summary>
        /// このフレームのSQTトランスフォームを取得
        /// </summary>
        /// <param name="result">SQLトランスフォーム</param>
        public void GetSQTTransform(out SQTTransform result)
        {
            SQTTransform.Create(ref Scales, ref Quatanion, ref Location, out result);
        }
    }
}
