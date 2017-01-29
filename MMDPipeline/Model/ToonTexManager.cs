using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MikuMikuDance.Resource;

namespace MikuMikuDance.XNA.Model
{
    class ToonTexManager
    {
        Dictionary<string, string> DefaltToonPath = new Dictionary<string, string>();
        static ToonTexManager instance=null;
        public static ToonTexManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new ToonTexManager();
                return instance;
            }
        }

        private ToonTexManager()
        {
            //リソースからファイルを作成して読み込むという超セコイ方法……
            if (!Directory.Exists("ext"))
                Directory.CreateDirectory("ext");
            //toonを1～10まで保存
            MMDXResource.toon01.Save(Path.Combine("ext", "toon01.bmp"));
            MMDXResource.toon02.Save(Path.Combine("ext", "toon02.bmp"));
            MMDXResource.toon03.Save(Path.Combine("ext", "toon03.bmp"));
            MMDXResource.toon04.Save(Path.Combine("ext", "toon04.bmp"));
            MMDXResource.toon05.Save(Path.Combine("ext", "toon05.bmp"));
            MMDXResource.toon06.Save(Path.Combine("ext", "toon06.bmp"));
            MMDXResource.toon07.Save(Path.Combine("ext", "toon07.bmp"));
            MMDXResource.toon08.Save(Path.Combine("ext", "toon08.bmp"));
            MMDXResource.toon09.Save(Path.Combine("ext", "toon09.bmp"));
            MMDXResource.toon10.Save(Path.Combine("ext", "toon10.bmp"));
            //ファイル名と保存場所の辞書を作成
            DefaltToonPath.Add("toon01.bmp", Path.Combine("ext", "toon01.bmp"));
            DefaltToonPath.Add("toon02.bmp", Path.Combine("ext", "toon02.bmp"));
            DefaltToonPath.Add("toon03.bmp", Path.Combine("ext", "toon03.bmp"));
            DefaltToonPath.Add("toon04.bmp", Path.Combine("ext", "toon04.bmp"));
            DefaltToonPath.Add("toon05.bmp", Path.Combine("ext", "toon05.bmp"));
            DefaltToonPath.Add("toon06.bmp", Path.Combine("ext", "toon06.bmp"));
            DefaltToonPath.Add("toon07.bmp", Path.Combine("ext", "toon07.bmp"));
            DefaltToonPath.Add("toon08.bmp", Path.Combine("ext", "toon08.bmp"));
            DefaltToonPath.Add("toon09.bmp", Path.Combine("ext", "toon09.bmp"));
            DefaltToonPath.Add("toon10.bmp", Path.Combine("ext", "toon10.bmp"));
            
        }

        public string GetToonTexPath(byte toonIndex, string[] toonTextures, string modelfilename)
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
    }
}
