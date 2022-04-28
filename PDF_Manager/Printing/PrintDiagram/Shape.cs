using PDF_Manager.Printing;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Manager.Printing.PrintDiagram
{
    internal class Shape
    {
        /// <summary>
        /// 直線を描く
        /// </summary>
        /// <param name="_myCanvas"></param>
        /// <param name="_pt1"></param>
        /// <param name="_pt2"></param>
        /// <param name="_PenWidth"></param>
        static public void DrawLine(PdfDoc _myCanvas, XPoint _pt1, XPoint _pt2, double _PenWidth)
        {
            _myCanvas.xpen.Width = _PenWidth;
            _myCanvas.gfx.DrawLine(_myCanvas.xpen, _pt1, _pt2);
        }

        static public void DrawLine(PdfDoc _myCanvas, XPoint _pt1, XPoint _pt2, double _PenWidth, XColor col)
        {
            _myCanvas.xpen.Color = col;
            Shape.DrawLine(_myCanvas, _pt1, _pt2, _PenWidth);
        }

        /// <summary>
        /// 破線を描く
        /// </summary>
        /// <param name="_myCanvas"></param>
        /// <param name="_pt1"></param>
        /// <param name="_pt2"></param>
        /// <param name="_PenWidth"></param>
        /// <param name="_Interval">破線の距離</param>
        static public void DrawDashLine(PdfDoc _myCanvas, XPoint _pt1, XPoint _pt2, double _PenWidth, double _Interval)
        {
            var LenX = _pt2.X - _pt1.X;
            var LenY = _pt2.Y - _pt1.Y;
            var Length = (double)Math.Sqrt(Math.Pow(LenX, 2) + Math.Pow(LenY, 2));
            int num1 = (int)Math.Round(Length / _Interval, 0);
            int num2 = (num1 % 2 == 1) ? num1 + 1 : num1;
            int num3 = num2 / 2;
            var IntX = LenX / num2;
            var IntY = LenY / num2;

            XPoint p1 = _pt1;
            XPoint p2 = new XPoint();
            for (int i = 0; i < num3; i++){
                p2.X = p1.X + IntX;
                p2.Y = p1.Y + IntY;
                _myCanvas.gfx.DrawLine(_myCanvas.xpen, p1, p2);
                p1.X = p2.X + IntX;
                p1.Y = p2.Y + IntY;
            }
        }

        /// <summary>
        /// 円を描く
        /// </summary>
        /// <param name="_myCanvas"></param>
        /// <param name="_pt0"></param>
        /// <param name="_size"></param>
        /// <param name="_PenWidth"></param>
        static public void Drawcircle(PdfDoc _myCanvas, XPoint _pt0, SizeF _size, double _PenWidth)
        {
            _myCanvas.gfx.DrawEllipse(_myCanvas.xpen, _pt0.X, _pt0.Y, _size.Width, _size.Height);
        }



    }
}
