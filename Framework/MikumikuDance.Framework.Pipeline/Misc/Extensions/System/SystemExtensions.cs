using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{

    public static class SystemExtensions
    {

        public static void Save(this byte[] data, string path)
        {
            File.WriteAllBytes(path, data ?? new byte[0]);
        }

    }

}