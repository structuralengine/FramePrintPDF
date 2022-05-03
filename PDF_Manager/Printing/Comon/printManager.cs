using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing.Comon
{
    class printManager
    {
        /// <summary>
        /// デフォルトのフォントサイズ
        /// </summary>
        public static double FontSize = 11;

        /// <summary>
        /// デフォルトフォントの高さ
        /// </summary>
        public static double FontHeight = printManager.FontSize;

        /// <summary>
        /// タイトルを印字する位置（ページのマージンを考慮した位置）
        /// </summary>
        public static XPoint titlePos = new XPoint(0, printManager.FontHeight);

        /// <summary>
        /// 表題を印字するX位置
        /// </summary>
        public static double H1PosX = printManager.titlePos.X + 2;

        /// <summary>
        /// 表を開始するX位置
        /// </summary>
        public static double TablePosX = printManager.H1PosX + 2;

        /// <summary>
        /// 大きめ（テーブルの間など）の改行高さ
        /// </summary>
        public static double LineSpacing1 = printManager.FontHeight + 4;

        /// <summary>
        /// 中くらい（タイトル後など）の改行高さ
        /// </summary>
        public static double LineSpacing2 = printManager.FontHeight + 2;

        /// <summary>
        /// 小さい（テーブル内などの）改行高さ
        /// </summary>
        public static double LineSpacing3 = printManager.FontHeight;


    }
}
