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



    }
}
