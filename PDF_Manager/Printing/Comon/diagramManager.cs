using PDF_Manager.Printing.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    // 図のレイアウト
    public enum Layout
    {
        Default,            // 1ページに 1つの応力図
        SplitHorizontal,    // 1ページに 2つの応力図で上下に配置
        SplitVertical       // 1ページに 2つの応力図で左右に配置
    }

    internal class diagramManager
    {
        private double nodeSize = 2;        // 節点の円の大きさ
        private double nodePenWidth = 0.2;  // 節点の線幅

        public PdfDocument mc;

        // 図のレイアウト
        private Layout mode;        // 図のレイアウト
        private int Target;           // これから描く図が, 紙面のどの位置なのか
        private XPoint[] Center = new XPoint[2] { new XPoint(0, 0), new XPoint(0, 0) };  // 描く図の紙面における中心位置

        // 図の描画面積
        private XSize AreaSize = new XSize(0, 0);    
        public XSize areaSize
        {
            get
            {
                return this.AreaSize;
            }
        }

        public diagramManager(PdfDocument mc, Layout _mode)
        {
            // キャンパス
            this.mc = mc;

            // 線幅などの初期化
            this.mc.xpen = new XPen(XColors.Black, nodePenWidth);

            // レイアウト
            this.mode = _mode;

            // 図描画エリアの中心の位置を決定する.
            var paper = this.mc.currentPageSize;    // マージを引いたエリア
            this.AreaSize.Width = paper.Width;
            this.AreaSize.Width -= printManager.padding.Left;
            this.AreaSize.Width -= printManager.padding.Right;   //ここでareaseizewidthはパディングを引いたものになっている
            this.AreaSize.Height = paper.Height;
            this.AreaSize.Height -= printManager.padding.Top;
            this.AreaSize.Height -= printManager.padding.Bottom;
            this.AreaSize.Height -= printManager.FontHeight * 2;        // タイトル印字分高さを減らす
            this.AreaSize.Height -= printManager.LineSpacing2;

            Center[0].X = this.AreaSize.Width / 2;
            Center[0].X += printManager.padding.Left;
            Center[0].Y = this.AreaSize.Height / 2;
            Center[0].Y += printManager.padding.Top;
            Center[0].Y += printManager.FontHeight * 2;     // タイトル印字分高さを減らす
            Center[0].Y += printManager.LineSpacing2;

            if (this.mode == Layout.SplitHorizontal)
            {   // ページを上下に分割する場合
                Center[1].X = Center[0].X;

                Center[0].Y = this.AreaSize.Height;
                Center[0].Y -= printManager.padding.Top;
                Center[0].Y /= 4;
                Center[0].Y += printManager.FontHeight * 2;
                Center[0].Y += printManager.LineSpacing2;
                Center[0].Y += printManager.padding.Top * 3 / 2;
                Center[1].Y = this.AreaSize.Height;
                Center[1].Y -= printManager.padding.Top;
                Center[1].Y /= 4;
                Center[1].Y *= 3;
                Center[1].Y += printManager.padding.Top * 5 / 2;
                Center[1].Y += printManager.FontHeight * 3;
                Center[1].Y += printManager.LineSpacing2 * 2;

                AreaSize.Height /= 2;
            }
            if (this.mode == Layout.SplitVertical)
            { // ページを左右に分割する場合
                Center[1].Y = Center[0].Y;

                Center[0].X = this.AreaSize.Width;
                Center[0].X -= printManager.padding.Left;
                Center[0].X /= 4;
                Center[0].X += printManager.padding.Left ;
                Center[1].X = this.AreaSize.Width / 2;
                Center[1].X += printManager.padding.Left /2;
                Center[1].X += Center[0].X;

                AreaSize.Width /= 2;
            }
            // カレント
            this.currentArea = 0;
        }

        /// <summary>
        /// これから描く図が, 紙面のどの位置なのか指定する Layout=Default の場合は関係ない
        /// </summary>
        /// <example>
        /// Layout=SplitHorizontal の場合
        ///    1
        /// ───
        ///    2
        /// Layout=SplitVertical の場合
        ///   │
        /// 1 │ 2
        ///   │
        /// </example>
        public int currentArea
        {
            get
            {
                return this.Target;
            }
            set
            {
                this.Target = value;


            }
        }

        /// <summary>
        /// 節点の印字
        /// </summary>
        public void printNode(double _x, double _y)
        {
            var centerPos = this.Center[this.currentArea];

            var x = centerPos.X + _x - nodeSize / 2;
            var y = centerPos.Y + _y - nodeSize / 2;

            XPoint p = new XPoint(x, y);
            XSize z = new XSize(this.nodeSize, this.nodeSize);

            Shape.Drawcircle(this.mc, p, z);
        }

        /// <summary>
        /// 節点の印字
        /// </summary>
        public void printLine(double _x1, double _y1, double _x2, double _y2)
        {
            var centerPos = this.Center[this.currentArea];

            var x1 = centerPos.X + _x1;
            var y1 = centerPos.Y + _y1;
            var x2 = centerPos.X + _x2;
            var y2 = centerPos.Y + _y2;

            XPoint p = new XPoint(x1, y1);
            XPoint q = new XPoint(x2, y2);

            Shape.DrawLine(this.mc, p, q);
        }
    }
}
