using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    internal class diagramManager
    {
        private double nodeSize = 1;        // 節点の円の大きさ
        private double nodePenWidth = 0.2;  // 節点の線幅

        private PdfDocument mc;

        private mode


        public diagramManager(PdfDocument mc)
        {
            this.mc = mc;
            this.mc.xpen = new XPen(XBrushes.Black, nodePenWidth);
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
