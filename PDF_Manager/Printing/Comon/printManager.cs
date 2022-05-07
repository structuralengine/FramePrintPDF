using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PDF_Manager.Printing.Comon
{
    class printManager
    {
        /// <summary>
        /// デフォルトのフォントサイズ 
        /// pt ポイント　（1pt = 1/72 インチ)
        /// </summary>
        public static double FontSize = 11;

        /// <summary>
        /// デフォルトフォントの高さ 
        /// pt ポイント
        /// </summary>
        public static double FontHeight = printManager.FontSize;

        /// <summary>
        /// タイトルを印字する位置（ページのマージンを考慮した位置）
        ///  pt ポイント
        /// </summary>
        public static XPoint titlePos = new XPoint(0, printManager.FontHeight);

        /// <summary>
        /// 表題を印字するX位置
        ///  pt ポイント
        /// </summary>
        public static double H1PosX = printManager.titlePos.X + 5;

        /// <summary>
        /// 表を開始するX位置
        ///  pt ポイント
        /// </summary>
        public static double TablePosX = printManager.H1PosX + 10;

        /// <summary>
        /// 大きめ（テーブルの間など）の改行高さ
        ///  pt ポイント
        /// </summary>
        public static double LineSpacing1 = printManager.FontHeight + 15;

        /// <summary>
        /// 中くらい（タイトル後など）の改行高さ
        ///  pt ポイント
        /// </summary>
        public static double LineSpacing2 = printManager.FontHeight + 2;

        /// <summary>
        /// 小さい（テーブル内などの）改行高さ
        ///  pt ポイント
        /// </summary>
        public static double LineSpacing3 = printManager.FontHeight;


        // 数値を文字列に変換する
        public static string toString(object data, int round = 0, string style = null)
        {
            if (data == null)
                return "";

            string result = data.ToString();

            double tmp;
            if(double.TryParse(result, out tmp))
            {   // 数値の場合 四捨五入等の処理を行う
                if (double.IsNaN(tmp))
                {
                    result = "";
                }
                else if (style == null)
                {
                    var digit = "F" + round.ToString();
                    result = Double.IsNaN(Math.Round(tmp, round, MidpointRounding.AwayFromZero)) ? "" : tmp.ToString(digit);
                    if (StringInfo.ParseCombiningCharacters(result).Length > round + 5)
                    {
                        result = tmp.ToString("E2", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                }
                else if (style == "E")
                {
                    result = Double.IsNaN(Math.Round(tmp, round, MidpointRounding.AwayFromZero)) ? "" : tmp.ToString("E2", CultureInfo.CreateSpecificCulture("en-US"));
                }
            } 

            return result;
        }


        /// <summary>
        /// 何行印刷できるか調べる
        /// </summary>
        /// <returns>
        /// return[0] = 1ページ目の印刷可能行数, 
        /// return[1] = 2ページ目以降の印刷可能行数
        /// </returns>
        public static int[] getPrintRowCount(PdfDocument mc, string[,] header_content)
        {
            // タイトルの印字高さ + 改行高
            double H1 = printManager.FontHeight + printManager.LineSpacing2;

            // 表題の印字高さ + 改行高
            double H2 = header_content.GetLength(0) * printManager.FontHeight + printManager.LineSpacing2;

            // 1行当りの高さ + 改行高
            double H3 = printManager.LineSpacing3;

            // 2ページ目以降（ページ全体を使ってよい場合）の行数
            double Hx = mc.currentPageSize.Height;
            Hx -= H1;
            Hx -= H2;
            int rows2 = (int)(Hx / H3); // 切り捨て

            // 1ページ目（現在位置から）の行数
            Hx -= mc.contentY;
            int rows1 = (int)(Hx / H3); // 切り捨て

            return new int[] { rows1, rows2 };
        }


        /// <summary>
        /// 印刷を行う
        /// </summary>
        /// <param name="mc">キャンパス</param>
        /// <param name="page">印字内容</param>
        /// <param name="title">タイトル</param>
        /// <param name="header_content">表題</param>
        /// <param name="header_Xspacing">表題の位置</param>
        /// <param name="body_Xspacing">表内容の位置</param>
        /// <param name="body_align">表の文字位置</param>
        public static void printContent(PdfDocument mc, List<List<string[]>> page, string title,
                                        string[,] header_content, double[] header_Xspacing, 
                                        double[] body_Xspacing, XStringFormat[] body_align)
        {
            // 表の印刷
            int p = 1;
            foreach (var table in page)
            {
                if (1 < p)
                    mc.NewPage(); // 2ページ目以降は改ページする

                // タイトルの印字
                mc.setCurrentX(printManager.H1PosX);
                Text.PrtText(mc, title);
                mc.addCurrentY(printManager.FontHeight + printManager.LineSpacing2);

                // 表題の印字
                for (int i = 0; i < header_content.Rank; i++)
                {
                    for (int j = 0; j < header_content.GetLength(1); j++)
                    {
                        var str = header_content[i, j];
                        if (str.Length <= 0)
                            continue;

                        var x = header_Xspacing[j];
                        mc.setCurrentX(x);
                        Text.PrtText(mc, str);
                    }
                    mc.addCurrentY(printManager.FontHeight);
                }
                mc.addCurrentY(printManager.LineSpacing2); // 中くらい（タイトル後など）の改行高さ

                // 表の印刷
                foreach (var line in table)
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        var str = line[i];
                        if (str == null)
                            continue;
                        if (str.Length <= 0)
                            continue;

                        var x = body_Xspacing[i];
                        mc.setCurrentX(x);

                        var a = body_align[i];

                        Text.PrtText(mc, str, align: a);
                    }
                    mc.addCurrentY(printManager.LineSpacing3); // 小さい（テーブル内などの）改行高さ
                }
                p++;
            }
            // 最後の改行
            mc.addCurrentY(printManager.LineSpacing1);
        }
    }
}
