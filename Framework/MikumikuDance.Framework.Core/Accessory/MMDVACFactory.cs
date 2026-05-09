using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MikuMikuDance.Core.Misc;
using MikumikuDance.Framework.Abstractions;

namespace MikuMikuDance.Core.Accessory
{
#if !(XBOX || PORTABLE)
    class MMDVACFactory : IMMDVACFactory
    {
    #region IMMDVACFactory メンバー

        public MMD_VAC Load(string filename, bool leftHanded)
        {
            float scale;
            MMDVector3 move, rot;
            string bone;
            bool shadow = false;
            using (StreamReader sr = new StreamReader(filename, Encoding.GetEncoding(932)))
            {
                //アクセサリ名とxファイル名は読まない
                sr.ReadLine();
                sr.ReadLine();
                //拡大率
                scale = Convert.ToSingle(sr.ReadLine());
                //位置
                string[] data = sr.ReadLine().Split(',');
                move = new MMDVector3(Convert.ToSingle(data[0]), Convert.ToSingle(data[1]), Convert.ToSingle(data[2]));
                //回転
                data = sr.ReadLine().Split(',');
                rot = new MMDVector3(
                    MMDMathHelper.ToRadians(Convert.ToSingle(data[0])),
                    MMDMathHelper.ToRadians(Convert.ToSingle(data[1])),
                    MMDMathHelper.ToRadians(Convert.ToSingle(data[2])));
                //ボーン名
                bone = sr.ReadLine();
                int num;
                if (int.TryParse(sr.ReadLine().Trim(), out num))
                    shadow = (num != 0);
                sr.Close();

            }
            if (leftHanded)
            {
                move.Z = -move.Z;
                rot.X = -rot.X;
                rot.Y = -rot.Y;
            }
            MMD_VAC result = new MMD_VAC
            {
                BoneName = bone
            };
            MMDVector3 sclvec = new MMDVector3(scale);
            MMDMatrix temp1, temp2, temp3;
            MMDXMath.CreateScaleMatrix(ref sclvec, out temp1);
            temp2 = MMDXMath.CreateMatrixFromYawPitchRoll(rot.X, rot.Y, rot.Z);
            MMDMatrix.Multiply(ref temp1, ref temp2, out temp3);
            MMDXMath.CreateTranslationMatrix(ref move, out temp1);
            MMDMatrix.Multiply(ref temp3, ref temp1, out result.Transform);
            return result;
        }

    #endregion
    }
#endif
}
