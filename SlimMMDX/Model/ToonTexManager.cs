using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Resource;
using System.IO;

namespace MikuMikuDance.SlimDX.Model
{
    static class ToonTexManager
    {
        static Dictionary<string, string> DefaltToonPath = new Dictionary<string,string>();
        

        public static string GetToonTexPath(byte toonIndex, string[] toonTextures, string modelfilename)
        {
            if (toonTextures != null)
            {
                if (toonIndex != 0xff)
                {
                    string toonPath;
                    if (!DefaltToonPath.TryGetValue(toonTextures[toonIndex], out toonPath))
                    {
                        //独自toon?
                        //そのままパスを返す
                        if (Path.IsPathRooted(toonTextures[toonIndex]))
                            return toonTextures[toonIndex];
                        return Path.Combine(Path.GetDirectoryName(modelfilename), toonTextures[toonIndex]);
                    }
                    return toonPath;//パスを渡す～
                }
                else
                    return null;//トゥーン無し
            }
            //デフォルトトゥーンを返す
            return DefaltToonPath["toon" + (toonIndex < 10 ? "0" : "") + toonIndex.ToString() + ".bmp"];
        }

        internal static void Setup(string[] toonTexPath)
        {
            //ファイル名と保存場所の辞書を作成
            DefaltToonPath.Add("toon01.bmp", toonTexPath[0]);
            DefaltToonPath.Add("toon02.bmp", toonTexPath[1]);
            DefaltToonPath.Add("toon03.bmp", toonTexPath[2]);
            DefaltToonPath.Add("toon04.bmp", toonTexPath[3]);
            DefaltToonPath.Add("toon05.bmp", toonTexPath[4]);
            DefaltToonPath.Add("toon06.bmp", toonTexPath[5]);
            DefaltToonPath.Add("toon07.bmp", toonTexPath[6]);
            DefaltToonPath.Add("toon08.bmp", toonTexPath[7]);
            DefaltToonPath.Add("toon09.bmp", toonTexPath[8]);
            DefaltToonPath.Add("toon10.bmp", toonTexPath[9]);
        }
    }
}
