using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace MikuMikuDance.XNA.Model
{
    static class ImageExtAnalyzer
    {
        public static void Analyze(Image img, out string Extention)
        {
            ImageFormat format = img.RawFormat;
            if (format.Guid == ImageFormat.Bmp.Guid)
                Extention = ".bmp";
            else if (format.Guid == ImageFormat.Emf.Guid)
            {
                Extention = ".png";//エンコーダがないのでpngで
                format = ImageFormat.Png;
            }
            else if (format.Guid == ImageFormat.Exif.Guid)
                Extention = ".jpg";//Exifはjpeg扱い
            else if (format.Guid == ImageFormat.Gif.Guid)
                Extention = ".gif";
            else if (format.Guid == ImageFormat.Icon.Guid)
            {
                Extention = ".png";//エンコーダがないのでpngで
                format = ImageFormat.Png;
            }
            else if (format.Guid == ImageFormat.Jpeg.Guid)
                Extention = ".jpg";
            else if (format.Guid == ImageFormat.MemoryBmp.Guid)
            {
                Extention = ".bmp";
                format = ImageFormat.Bmp;
            }
            else if (format.Guid == ImageFormat.Png.Guid)
                Extention = ".png";
            else if (format.Guid == ImageFormat.Tiff.Guid)
                Extention = ".tif";
            else if (format.Guid == ImageFormat.Wmf.Guid)
            {
                Extention = ".png";//エンコーダがないのでpngで
                format = ImageFormat.Png;
            }
            else
                throw new NotImplementedException("未実装のスフィアマップファイルフォーマット");

        }
    }
}
