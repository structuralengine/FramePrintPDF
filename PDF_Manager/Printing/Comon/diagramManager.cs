﻿using PdfSharpCore.Drawing;
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
        private double nodeSize = 1;        // 節点の円の大きさ
        private double nodePenWidth = 0.2;  // 節点の線幅

        private PdfDocument mc;

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
            this.mc.xpen = new XPen(XBrushes.Black, nodePenWidth);

            // レイアウト
            this.mode = _mode;

            // 図描画エリアの中心の位置を決定する.
            var paper = this.mc.currentPageSize;    // マージを引いたエリア
            this.AreaSize.Width = paper.Width;
            this.AreaSize.Height = paper.Height;

            Center[0].X = this.mc.Margine.Left + this.AreaSize.Width / 2;
            Center[0].Y = this.mc.Margine.Top + this.AreaSize.Height / 2;

            if (this.mode == Layout.SplitHorizontal)
            {   // ページを上下に分割する場合
                Center[1].X = Center[0].X;
                Center[0].Y = this.mc.Margine.Top + this.AreaSize.Height / 4;
                Center[1].Y = this.mc.Margine.Top + this.AreaSize.Height * 3 / 4; ;
                AreaSize.Height /= 2;
            }
            if (this.mode == Layout.SplitVertical)
            { // ページを左右に分割する場合
                Center[0].X = this.mc.Margine.Left + this.AreaSize.Width / 4;
                Center[1].X = this.mc.Margine.Left + this.AreaSize.Width * 3 / 4;
                Center[1].Y = Center[0].Y;
                AreaSize.Width /= 2;
            }
            // カレント
            this.currentArea = 1;
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
            var x = this.mc.Margine.Left + _x;
            var y = this.mc.Margine.Top + _y;

            XPoint p = new XPoint(x, y);
            XSize z = new XSize(this.nodeSize, this.nodeSize);

            Shape.Drawcircle(this.mc, p, z);
        }
    }
}
