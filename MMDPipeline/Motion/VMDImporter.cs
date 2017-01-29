using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MikuMikuDance.Motion;
using TImport = MikuMikuDance.Motion.Motion2.MMDMotion2;

namespace MikuMikuDance.XNA.Motion
{
    /// <summary>
    /// MikuMikuDance VMD(Vocaloid Motion Data)インポータ
    /// </summary>
    [ContentImporter(".vmd", DisplayName = "MikuMikuDance Motion : MikuMikuDance for XNA", DefaultProcessor = "MMDMotionProcessor")]
    public class VMDImporter : ContentImporter<TImport>
    {
        /// <summary>
        /// インポート処理
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="context">コンテントインポータ</param>
        /// <returns>MMDモーションデータ</returns>
        /// <remarks>XNAのインポータから呼び出される</remarks>
        public override TImport Import(string filename, ContentImporterContext context)
        {
            //Identityを設定(コンテンツのロード元の判別等に必要)
            ContentIdentity Identity = new ContentIdentity(filename);

            filename = Path.GetFullPath(filename);
            TImport result;
            try
            {
                result = MotionManager.Read(filename, CoordinateType.RightHandedCoordinate) as TImport;
            }
            catch (Exception e)
            {
                throw new InvalidContentException("モーションファイルの読み込みに失敗しました。モーションファイルが壊れている可能性があります。MikuMikuDanceで出力しなおすと上手くいくかもしれません", e);
            }
            if (result == null)
                throw new InvalidContentException();
            if (result.Motions == null && result.LightMotions == null && result.FaceMotions == null && result.CameraMotions == null)
                throw new InvalidContentException();
            return result;
        }
    }
}
