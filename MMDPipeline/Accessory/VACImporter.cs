using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Text;
using System.IO;

namespace MikuMikuDance.XNA.Accessory
{
    /// <summary>
    /// MikuMikuDanceの.vac設定ファイルをインポートするためのライブラリ
    /// </summary>
    [ContentImporter(".vac", DisplayName = "MikuMikuDance VAC : MikuMikuDance for XNA", CacheImportedData = true, DefaultProcessor = "VACProcessor")]
    public class VACImporter : ContentImporter<VACContent2>
    {
        /// <summary>
        /// インポート処理
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="context">コンテントインポータ</param>
        /// <returns>VACデータ</returns>
        /// <remarks>XNAのインポータから呼び出される</remarks>
        public override VACContent2 Import(string filename, ContentImporterContext context)
        {
            float scale;
            Vector3 move, rot;
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
                move = new Vector3(Convert.ToSingle(data[0]), Convert.ToSingle(data[1]), Convert.ToSingle(data[2]));
                //回転
                data = sr.ReadLine().Split(',');
                rot = new Vector3(
                    MathHelper.ToRadians(Convert.ToSingle(data[0])),
                    MathHelper.ToRadians(Convert.ToSingle(data[1])),
                    MathHelper.ToRadians(Convert.ToSingle(data[2])));
                //ボーン名
                bone = sr.ReadLine();
                int num;
                if (int.TryParse(sr.ReadLine().Trim(), out num))
                    shadow = (num != 0);
                sr.Close();

            }
            return new VACContent2()
            {
                BoneName = bone,
                Shadow = shadow,
                Scale = scale,
                Rot = rot,
                Trans = move
            };
        }
    }
}
