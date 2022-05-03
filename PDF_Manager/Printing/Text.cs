using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using PdfSharpCore;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using Newtonsoft.Json.Linq;
using PDF_Manager.Printing;
using PdfSharpCore.Utils;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using Newtonsoft.Json;

namespace PDF_Manager.Printing
{
    internal class Text
    {
        static public void PrtText(PdfDoc mc, string str)
        {
            if (str == null)
                return;

            // 文字列描画
            mc.gfx.DrawString(str, mc.font_mic, XBrushes.Black, mc.CurrentPos);

            mc.CurrentPos.X += str.Length * 10;
            mc.CurrentPos.Y += 10;

        }

        //static public void GeneratePDF(PdfDoc mc, List<List<string[]>> nodeData)
        //{
        //    // フォントリゾルバーのグローバル登録


        //    // DrawString:文字を入力できる
        //    // DrawString(入れたいデータ,フォント指定（fontの種類，大きさ，太さ）,文字の色,文字の位置(x,y))
        //    //gfx.DrawString("First Name", new XFont("Ms Gothic", 10, XFontStyle.Bold), XBrushes.Black, new XPoint(100, 280));
        //    //gfx.DrawString("Last Name", new XFont("Ms Gothic", 10, XFontStyle.Bold), XBrushes.Black, new XPoint(250, 280));
        //    //gfx.DrawString("Date of Birth", new XFont("Ms Gothic", 10, XFontStyle.Bold), XBrushes.Black, new XPoint(400, 280));

        //    // DrawString(入れたいデータ,フォント指定（fontの種類，大きさ，太さ）,文字の色,文字の位置(x,y))

        //    // AddPage:ドキュメントのページが更新
        //    //page = document.AddPage();
        //    // FromPdfPage:ページ更新後の新規ページにおけるgfx
        //    //gfx = XGraphics.FromPdfPage(page);
        //    int currentXposition_values = 20;
        //    int currentYposition_values = 20;

        //    bool firstpage = true;

        //    for (int i = 0; i < nodeData.Count; i++)
        //    {
        //        for (int j = 0; j < nodeData[i].Count; j++)
        //        {
        //            if (j != 0 && j % 44 == 0)
        //            {
        //                mc.NewPage();
        //            }
        //            mc.gfx.DrawString("id", mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosHeader.X + currentXposition_values * 0, mc.CurrentPosHeader.Y + currentYposition_values));
        //            mc.gfx.DrawString("x", mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosHeader.X + currentXposition_values * 1, mc.CurrentPosHeader.Y + currentYposition_values));
        //            mc.gfx.DrawString("y", mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosHeader.X + currentXposition_values * 2, mc.CurrentPosHeader.Y + currentYposition_values));
        //            mc.gfx.DrawString("z", mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosHeader.X + currentXposition_values * 3, mc.CurrentPosHeader.Y + currentYposition_values));
        //            mc.gfx.DrawString("id", mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosHeader.X + currentXposition_values * 4, mc.CurrentPosHeader.Y + currentYposition_values));
        //            mc.gfx.DrawString("x", mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosHeader.X + currentXposition_values * 5, mc.CurrentPosHeader.Y + currentYposition_values));
        //            mc.gfx.DrawString("y", mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosHeader.X + currentXposition_values * 6, mc.CurrentPosHeader.Y + currentYposition_values));
        //            mc.gfx.DrawString("z", mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosHeader.X + currentXposition_values * 7, mc.CurrentPosHeader.Y + currentYposition_values));
        //            mc.gfx.DrawString(nodeData[i][j][0], mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosBody.X + currentXposition_values * 0, mc.CurrentPosBody.Y + currentYposition_values));
        //            mc.gfx.DrawString(nodeData[i][j][1], mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosBody.X + currentXposition_values * 1, mc.CurrentPosBody.Y + currentYposition_values));
        //            mc.gfx.DrawString(nodeData[i][j][2], mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosBody.X + currentXposition_values * 2, mc.CurrentPosBody.Y + currentYposition_values));
        //            mc.gfx.DrawString(nodeData[i][j][3], mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosBody.X + currentXposition_values * 3, mc.CurrentPosBody.Y + currentYposition_values));
        //            mc.gfx.DrawString(nodeData[i][j][4], mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosBody.X + currentXposition_values * 4, mc.CurrentPosBody.Y + currentYposition_values));
        //            mc.gfx.DrawString(nodeData[i][j][5], mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosBody.X + currentXposition_values * 5, mc.CurrentPosBody.Y + currentYposition_values));
        //            mc.gfx.DrawString(nodeData[i][j][6], mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosBody.X + currentXposition_values * 6, mc.CurrentPosBody.Y + currentYposition_values));
        //            mc.gfx.DrawString(nodeData[i][j][7], mc.font_mic, XBrushes.Black, new XPoint(mc.CurrentPosBody.X + currentXposition_values * 7, mc.CurrentPosBody.Y + currentYposition_values));
        //            mc.CurrentPosBody.Y += 10;
        //        }
        //    }
        //}

    }
}

