#region Using ステートメント

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace DebugSample
{
    /// <summary>
    /// レイアウトのアライメント位置
    /// </summary>
    [Flags]
    public enum Alignment
    {
        // レイアウトなし
        None = 0,

        // 水平方向のレイアウト群
        // 左側
        Left = 1,
        // 右側
        Right = 2,
        // 水平方向中央
        HorizontalCenter = 4,

        // 垂直方向のレイアウト群
        // 上側
        Top = 8,
        // 下側
        Bottom = 16,
        // 垂直方向中央
        VerticalCenter = 32,

        // 左上
        TopLeft = Top | Left,
        // 右上
        TopRight = Top | Right,
        // 上側、中央
        TopCenter = Top | HorizontalCenter,

        // 左下
        BottomLeft = Bottom | Left,
        // 右下
        BottomRight = Bottom | Right,
        // 下側、中央
        BottomCenter = Bottom | HorizontalCenter,

        // 中央、左側
        CenterLeft = VerticalCenter | Left,
        // 中央、右側
        CenterRight = VerticalCenter | Right,
        // 中央
        Center = VerticalCenter | HorizontalCenter
    }

    /// <summary>
    /// セーフエリアに対応したレイアウト構造体
    /// </summary>
    /// <remarks>
    /// Windows、Xbox 360の両プラットフォームで動作するゲームは様々な解像度、
    /// アスペクト比に対応する必要がある。加えてXbox 360で動作するゲームは
    /// タイトルセーフエリアにも対応する必要がある。
    /// 
    /// この構造体では、レイアウト対象領域(クライアントエリア)と、
    /// セーフエリア領域を保持しAligmentと水平垂直のマージン値から
    /// 指定した矩形を配置する。
    /// 配置後の矩形がセーフエリア外にある場合はセーフエリア内に再配置される。
    /// 
    /// マージンはクライアントエリアの割合で示す。
    /// 
    /// 使用例:
    /// 
    /// Place( region, 0.1f, 0.2f, Aligment.TopLeft );
    /// 
    /// クライアントエリアの左端から10%、上端から20%の部分にregionを配置する
    /// 
    /// 
    /// Place( region, 0.3f, 0.4f, Aligment.BottomRight );
    /// 
    /// クライアントエリアの右端から30%、下端から40%の部分にregionを配置する
    /// 
    /// 
    /// クライアントエリアとセーフエリアを別々に指定できるので、
    /// 画面分割時にも、セーフエリアにタイトルセーフエリアを指定し、
    /// クライアントエリアに分割した画面の領域を指定することで、レイアウトは
    /// 画面分割領域ベースで行いつつも、タイトルセーフエリア内に正しく配置することが
    /// できる。
    /// 
    /// 
    /// セーフエリアについては以下のURLを参照
    /// http://blogs.msdn.com/ito/archive/2008/11/21/safearea-sample.aspx
    ///     /// </remarks>
    public struct Layout
    {
        #region フィールド

        /// <summary>
        /// クライアントエリアの取得と設定
        /// </summary>
        public Rectangle ClientArea;

        /// <summary>
        /// セーフエリアの取得と設定
        /// </summary>
        public Rectangle SafeArea;

        #endregion

        #region 初期化

        /// <summary>
        /// クライアントエリアとセーフエリアを別々に指定して初期化する
        /// </summary>
        /// <param name="client">クライアントエリア</param>
        /// <param name="safeArea">セーフエリア</param>
        public Layout(Rectangle clientArea, Rectangle safeArea)
        {
            ClientArea = clientArea;
            SafeArea = safeArea;
        }

        /// <summary>
        /// クライアントエリアのみを指定して初期化する
        /// セーフエリアはクライアントエリアと同じサイズになる
        /// </summary>
        /// <param name="client">クライアントエリア</param>
        public Layout(Rectangle clientArea)
            : this(clientArea, clientArea)
        {
        }

        /// <summary>
        /// Viewportを指定して初期化する
        /// セーフエリアはViewport.TitleSafeAreaになる
        /// </summary>
        /// <param name="viewport">ビューポート</param>
        public Layout(Viewport viewport)
        {
            ClientArea = new Rectangle((int)viewport.X, (int)viewport.Y,
                                        (int)viewport.Width, (int)viewport.Height);
            SafeArea = viewport.TitleSafeArea;
        }

        #endregion

        /// <summary>
        /// 指定したサイズ矩形のレイアウト
        /// </summary>
        /// <param name="region">配置する矩形</param>
        /// <param name="horizontalMargin">垂直方向のマージン</param>
        /// <param name="verticalMargine">水平方向のマージン</param>
        /// <param name="alignment">アライメント</param>
        /// <returns>配置された矩形</returns>
        public Vector2 Place(Vector2 size, float horizontalMargin,
                                            float verticalMargine, Alignment alignment)
        {
            Rectangle rc = new Rectangle(0, 0, (int)size.X, (int)size.Y);
            rc = Place(rc, horizontalMargin, verticalMargine, alignment);
            return new Vector2(rc.X, rc.Y);
        }

        /// <summary>
        /// 指定した矩形のレイアウト
        /// </summary>
        /// <param name="region">配置する矩形</param>
        /// <param name="horizontalMargin">垂直方向のマージン</param>
        /// <param name="verticalMargine">水平方向のマージン</param>
        /// <param name="alignment">アライメント</param>
        /// <returns>配置された矩形</returns>
        public Rectangle Place(Rectangle region, float horizontalMargin,
                                            float verticalMargine, Alignment alignment)
        {
            // 水平方向のレイアウト
            if ((alignment & Alignment.Left) != 0)
            {
                region.X = ClientArea.X + (int)(ClientArea.Width * horizontalMargin);
            }
            else if ((alignment & Alignment.Right) != 0)
            {
                region.X = ClientArea.X +
                            (int)(ClientArea.Width * (1.0f - horizontalMargin)) -
                            region.Width;
            }
            else if ((alignment & Alignment.HorizontalCenter) != 0)
            {
                region.X = ClientArea.X + (ClientArea.Width - region.Width) / 2 +
                            (int)(horizontalMargin * ClientArea.Width);
            }
            else
            {
                // レイアウトなし
            }

            // 垂直方向のレイアウト
            if ((alignment & Alignment.Top) != 0)
            {
                region.Y = ClientArea.Y + (int)(ClientArea.Height * verticalMargine);
            }
            else if ((alignment & Alignment.Bottom) != 0)
            {
                region.Y = ClientArea.Y +
                            (int)(ClientArea.Height * (1.0f - verticalMargine)) -
                            region.Height;
            }
            else if ((alignment & Alignment.VerticalCenter) != 0)
            {
                region.Y = ClientArea.Y + (ClientArea.Height - region.Height) / 2 +
                            (int)(verticalMargine * ClientArea.Height);
            }
            else
            {
                // レイアウトなし
            }

            // レイアウトした領域をセーフエリア内にあるか確かめる
            if (region.Left < SafeArea.Left)
                region.X = SafeArea.Left;

            if (region.Right > SafeArea.Right)
                region.X = SafeArea.Right - region.Width;

            if (region.Top < SafeArea.Top)
                region.Y = SafeArea.Top;

            if (region.Bottom > SafeArea.Bottom)
                region.Y = SafeArea.Bottom - region.Height;

            return region;
        }

    }
}