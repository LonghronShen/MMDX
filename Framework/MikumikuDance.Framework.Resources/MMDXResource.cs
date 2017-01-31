using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace MikuMikuDance.Resource
{

    /// <summary>
    /// MMDX共用リソース
    /// </summary>
    public static class MMDXResource
    {

        /// <summary>
        /// HLSL for Windowsモデル
        /// </summary>
        public static byte[] MMDWinEffect { get { return LoadEmbededResource("MMDWinEffect"); } }
        /// <summary>
        /// HLSL for XBoxモデル
        /// </summary>
        public static byte[] MMDXBoxEffect { get { return LoadEmbededResource("MMDXBoxEffect"); } }
        /// <summary>
        /// HLSL for アクセサリ
        /// </summary>
        public static byte[] AccessoryEffect { get { return LoadEmbededResource("AccessoryEffect"); } }
        /// <summary>
        /// HLSL for エッジ
        /// </summary>
        public static byte[] MMDEdgeEffect { get { return LoadEmbededResource("MMDEdgeEffect"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon01 { get { return LoadEmbededResource("toon01"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon02 { get { return LoadEmbededResource("toon02"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon03 { get { return LoadEmbededResource("toon03"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon04 { get { return LoadEmbededResource("toon04"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon05 { get { return LoadEmbededResource("toon05"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon06 { get { return LoadEmbededResource("toon06"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon07 { get { return LoadEmbededResource("toon07"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon08 { get { return LoadEmbededResource("toon08"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon09 { get { return LoadEmbededResource("toon09"); } }
        /// <summary>
        /// デフォルトToon
        /// </summary>
        public static byte[] toon10 { get { return LoadEmbededResource("toon10"); } }

        private static byte[] LoadEmbededResource(string fileName)
        {
            var asm = typeof(MMDXResource).GetTypeInfo().Assembly;
            var files = asm.GetManifestResourceNames();
            foreach (var file in files)
            {
                if (file.EndsWith(fileName))
                {
                    using (var stream = asm.GetManifestResourceStream(file))
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            ms.Flush();
                            return ms.ToArray();
                        }
                    }
                }
            }
            return null;
        }

    }

}