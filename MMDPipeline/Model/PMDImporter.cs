using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MikuMikuDance.Model;
using MikuMikuDance.Model.Ver1;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MikuMikuDance PMD形式ファイルのインポーター
    /// </summary>
    [ContentImporter(".pmd", DisplayName = "MikuMikuDance PMD : MikuMikuDance for XNA", DefaultProcessor = "MMDModelProcessor")]
    public class PMDImporter : ContentImporter<NodeContent>
    {
        /// <summary>
        /// 読み込み処理
        /// </summary>
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            //pmdファイルを読み込む
            MMDModel model = ModelManager.Read(filename, CoordinateType.RightHandedCoordinate);
            MMDModel1 model1 = model as MMDModel1;
            if (model1 == null)//将来ver2が出た時用
                throw new InvalidContentException("このインポータで読めるのはPMDモデルver1のみです");
            //読み込んだpmdを元にNodeContentに組み上げる
            MMDModelScene scene = MMDModelScene.Create(model1, filename);

            return scene.Root;
        }
    }
}
