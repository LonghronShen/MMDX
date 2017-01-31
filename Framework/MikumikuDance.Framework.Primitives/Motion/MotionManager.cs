using System;
using System.IO;
using MikuMikuDance.Motion.Motion2;
using System.Diagnostics;

namespace MikuMikuDance.Motion
{
    /// <summary>
    /// MikuMikuDance(MMD)モーションの読み込みを行うFactory Class
    /// </summary>
    public static class MotionManager
    {
        /// <summary>
        /// ファイルからMMDモーションを読み込む
        /// </summary>
        /// <param name="stream">MMDモーションファイル</param>
        /// <param name="coordinate">変換先座標系</param>
        /// <returns>MMDモーションオブジェクト</returns>
        /// <param name="scale">スケーリング値</param>
        public static MMDMotion Read(Stream stream, CoordinateType coordinate = CoordinateType.LeftHandedCoordinate, float scale = 1.0f)
        {
            //ファイルチェック
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), "The given MMD Motion stream is empty.");
            //戻り値用変数
            MMDMotion result = null;
            //ファイルリーダー
            using (var fs = stream)
            {
                BinaryReader reader = new BinaryReader(fs);
                //マジック文字列
                string magic = MMDMotion2.GetString(reader.ReadBytes(30));
                if (magic.Substring(0, 20) != "Vocaloid Motion Data")
                    throw new FormatException("MMDモーションファイルではありません");
                //バージョン
                int version = Convert.ToInt32(magic.Substring(21));
                if (version == 2)
                    result = new MMDMotion2();
                else
                    throw new FormatException("version=" + version.ToString() + "モデルは対応していません");

                result.Read(reader, coordinate, scale);
                if (fs.Length != fs.Position)
                    Debug.WriteLine("警告：ファイル末尾以降に不明データ?");
            }
            return result;
        }
        /// <summary>
        /// ファイルからMMDモーションを読み込む
        /// </summary>
        /// <param name="filename">MMDモーションファイル</param>
        /// <param name="scale">スケーリング値</param>
        /// <returns>MMDモーションオブジェクト</returns>
        //public static MMDMotion Read(string filename, float scale=0.1f)
        //{
        //    return Read(filename, CoordinateType.LeftHandedCoordinate, scale);
        //}
        /// <summary>
        /// ファイルへの書き出し
        /// </summary>
        /// <param name="stream">ファイル名</param>
        /// <param name="motion">モーション</param>
        /// <param name="scale">スケーリング値</param>
        public static void Write(Stream stream, MMDMotion motion, float scale = 1f)
        {
            //ファイルリーダー
            using (var fs = stream)
            {
                BinaryWriter writer = new BinaryWriter(fs);
                //マジック文字列
                if (motion is MMDMotion2)
                {
                    writer.Write(MMDMotion2.GetBytes("Vocaloid Motion Data 0002", 25));
                    writer.Write((byte)0);
                    writer.Write(MMDMotion2.GetBytes("JKLM", 4));
                }
                else
                    new NotImplementedException("その他のバーションは未作成");

                motion.Write(writer, scale);
            }
        }
    }
}
