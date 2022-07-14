using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PDF_Manager.Printing.Comon;

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
        public static double LineSpacing2 = printManager.FontHeight * 1.5;


        // 数値を文字列に変換する
        public static string toString(object data, int round = -1,  string style = "F")
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
                else if(0 <= round)
                {
                    string digit = style + round.ToString();
                    result = tmp.ToString(digit);
                }
            }
            return result;
        }



        /// <summary>
        /// タイトルの印字高さ + 改行高
        /// </summary>
        public static double H1 = printManager.FontHeight + printManager.LineSpacing2;


        /// <summary>
        /// 印刷を行う
        /// </summary>
        /// <param name="mc">キャンパス</param>
        /// <param name="page">印字内容</param>
        /// <param name="title">タイトル</param>
        /// <param name="tbl">表</param>
        public static void printTableContents(PdfDocument mc, List<Table> page, string[] titles)
        {
            // 表の印刷
            for (var i = 0; i < page.Count; i++)
            {
                var table = page[i];

                if (0 < i)
                {
                    // 残りの余白（＝CurrentY座標）＜ 次のページの表の高さ
                    mc.NewPage(); // 2ページ目以降は改ページする
                }

                // タイトルの印字
                mc.setCurrentX(printManager.H1PosX);
                foreach (var title in titles)
                {
                    Text.PrtText(mc, title);
                    //mc.addCurrentY(printManager.FontHeight + printManager.LineSpacing2);
                    mc.addCurrentY(printManager.LineSpacing2);
                }

                // 表の印刷
                page[i].PrintTable(mc);
            }


            // 最後の改行
            mc.addCurrentY(printManager.LineSpacing1);
        }

        public static void printTableContentsOnePage(PdfDocument mc, List<Table> page, string[] titles)
        {
            var p = mc.currentPos; // 現在の紙面におけるポジション

            mc.setCurrentX(printManager.H1PosX);
            mc.addCurrentY(printManager.LineSpacing2);

            foreach (var title in titles)
            {
                Text.PrtText(mc, title);
                mc.addCurrentY(printManager.LineSpacing2);
            }

            // 表の印刷
            for (var i = 0; i < page.Count; i++)
            {
                Console.WriteLine(mc.currentPos.Y);
                var table = page[i];

                if (i > 0)
                {
                    //現在の印刷できる高さの取得
                    var PageHihgt = mc.currentPageSize.Height;
                    PageHihgt -= mc.currentPos.Y;

                    // 残りの余白（＝PageHihgt座標）＜ 次のページの表の高さ
                    //これから印刷する高さを取得
                    var CurrentHight = page[i].GetTableHeight();

                    //印刷できるかできないかの判定（できなければ改ページ）
                    if (PageHihgt < CurrentHight)
                    {
                        mc.NewPage();

                        //CurrentHight = table.GetTableHeight();

                        // タイトルの印字
                        foreach (var title in titles)
                        {
                            Text.PrtText(mc, title);
                        }
                    }
                    mc.setCurrentX(printManager.H1PosX);
                    mc.addCurrentY(printManager.LineSpacing2);
                }

                // 表の印刷
                page[i].PrintTable(mc);
            }


            // 最後の改行
            mc.addCurrentY(printManager.LineSpacing1);
        }


        // 図の描画設定
        /// <summary>
        /// 紙面の描画エリア(マージンを引いた範囲）に対する骨組を描画するパディング
        /// </summary>
        public static double padding_Top = 60;

        public static TrimMargins padding = new TrimMargins() { 
            Bottom = 60, 
            Top = printManager.titlePos.Y + printManager.FontHeight +  printManager.LineSpacing2 + padding_Top,             // タイトル範囲を小さくする
            Left = 40, 
            Right = 40
        };

    }
}
