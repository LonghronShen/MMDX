using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// ボーンマネージャクラス
    /// </summary>
    public class MMDBoneManagerContent
    {
        /// <summary>
        /// ボーンデータ一覧
        /// </summary>
        public List<MMDBoneContent> bones;
        /// <summary>
        /// IK一覧
        /// </summary>
        public List<MMDIKContent> iks;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bones">ボーン一覧</param>
        /// <param name="iks">IK一覧</param>
        public MMDBoneManagerContent(List<MMDBoneContent> bones, List<MMDIKContent> iks)
        {
            this.bones = bones;
            this.iks = iks;
        }
    }
}
