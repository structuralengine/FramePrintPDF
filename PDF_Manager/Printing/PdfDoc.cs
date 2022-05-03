using Newtonsoft.Json.Linq;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using PdfSharpCore.Drawing.Layout;

namespace PDF_Manager.Printing
{
    internal class PdfDoc
    {
        public PdfDocument document;
        public XGraphics gfx;
        public XPoint CurrentPos; // 現在の座標
        public XPoint Margine; // マージン
        public XRect rect;
        public XStringFormat format;
        public XTextFormatter tf;
        public XPen xpen;
        public XFont font_mic;
        public XFont font_got;
        public int x;
        public int y;
        public bool judge = false;　// 項目別・改ページするかの判定
        public int bottomCell = 69;　// 1ページに入る行数
        public double dataCount = 0; //  classをまたいで行数をカウントする
        public int single_Yrow = 10;  //　1行あたりの高さ
        public string[,] current_header;
        public int[,] currentHeader_Xspacing;
        CultureInfo ci = new CultureInfo("en-us");
        //public int dimension;
        public string name;
        public string[,] header_content;
        public int[,] header_Xspacing;
        public int[,] body_Xspacing;
        //public string language;

        public PdfDoc(PrintData red)
        {
            //　新規ドキュメントの作成
            document = new PdfDocument();

            // フォントリゾルバーのグローバル登録
            var FontResolver = GlobalFontSettings.FontResolver.GetType();
            if (FontResolver.Name != "JapaneseFontResolver")
                GlobalFontSettings.FontResolver = new JapaneseFontResolver();

            /// マージンを設定する
            x = 80;
            y = 80;

            Margine = new XPoint(x, y);

            // 新しいページを作成する
            NewPage();


            document.Info.Title = "FrameWebForJS";
            PdfPage page = new PdfPage();

            // フォントの設定
            font_mic = new XFont("MS Mincho", 10, XFontStyle.Regular);
            font_got = new XFont("MS Gothic", 10, XFontStyle.Regular);

            dataCount = Margine.Y;

            //// 折り返し
            //tf = new XTextFormatter(gfx);
            //rect = new XRect(25, 50, 150, 34);
            //format = new XStringFormat();
            //xpen = new XPen(XColors.Navy, 0.4);

        }

        /// <summary>
        /// 改ページ
        /// </summary>
        public void NewPage()
        {
            // 白紙をつくる（Ａ４縦）
            PdfPage page = document.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Portrait;

            // XGraphicsオブジェクトを取得
            gfx = XGraphics.FromPdfPage(page);

            // 初期位置を設定する
            CurrentPos = new XPoint(Margine.X, Margine.Y);
            tf = new XTextFormatter(gfx);

        }

        /// <summary>
        /// PDF を Byte型に変換したものを返す
        /// </summary>
        /// <returns></returns>
        public byte[] GetPDFBytes()
        {
            // Creates a new Memory stream
            MemoryStream stream = new MemoryStream();

            // Saves the document as stream
            document.Save(stream);
            document.Close();

            // Converts the PdfDocument object to byte form.
            byte[] docBytes = stream.ToArray();

            return docBytes;
        }

        /// <summary>
        /// PDF を保存する
        /// </summary>
        /// <param name="filename"></param>
        public void SavePDF(string filename = @"../../../TestData/Test.pdf")
        {
            // PDF保存（カレントディレクトリ）
            document.Save(filename);

        }

        // PDFを記述する
        // dataHandle フォントの種類変更と位置変更
        // 0:左詰め　ゴシック
        // 1:左詰め　明朝
        // 2:真ん中　明朝
        // 3:右詰め　明朝
        public void PrintContent(string data, int dataHandle = 3)
        {
            if(data == null)
            {
                return;
            }
            if (dataHandle == 0)
            {
                gfx.DrawString(data, font_got, XBrushes.Black, CurrentPos, XStringFormats.BottomLeft);
            }
            else if (dataHandle == 1)
            {
                gfx.DrawString(data, font_mic, XBrushes.Black, CurrentPos, XStringFormats.BottomLeft);
            }
            else if (dataHandle == 2)
            {
                gfx.DrawString(data, font_mic, XBrushes.Black, CurrentPos, XStringFormats.BottomCenter);
            }
            else if (dataHandle == 3)
            {
                gfx.DrawString(data, font_mic, XBrushes.Black, CurrentPos, XStringFormats.BottomRight);
            }

        }

        // 行数の管理
        public void CurrentRow(double row)
        {
            CurrentPos.Y += single_Yrow * row;

            if (CurrentPos.Y > single_Yrow * bottomCell + Margine.Y)
            {
                NewPage();
                CurrentPos.Y += single_Yrow * 2;
                Header(current_header, currentHeader_Xspacing);
            }
        }

        //　列数の管理
        public void CurrentColumn(int column)
        {
            CurrentPos.X = x + column;
        }

        //　ヘッダー関係
        public void Header(string[,] header_content1, int[,] header_Xspacing1)
        {
            current_header = header_content1;
            currentHeader_Xspacing = header_Xspacing1;
            for (int i = 0; i < current_header.GetLength(0); i++)
            {
                for (int j = 0; j < current_header.GetLength(1); j++)
                {
                    CurrentColumn(currentHeader_Xspacing[i, j]);
                    PrintContent(current_header[i, j], 2);
                }
                CurrentRow(1);
            }
            CurrentPos.Y += single_Yrow;
        }


        //　classをまたいで，改ページするかの判定
        // 次の項目がまたがず入りきるなら同一ページで2行空き，そうでないなら改ページ
        public void DataCountKeep(double value, string title = "")
        {
            if (name != title)
            {
                if ((value + CurrentPos.Y) > Margine.Y + bottomCell * single_Yrow)
                {
                    NewPage();
                }
                else
                {
                    CurrentPos.X = x;
                    CurrentPos.Y += single_Yrow * 2;
                }
            }
        }

        //　タイプ別の改ページ判定
        public void TypeCount(int index, double headerRow, double count, string title)
        {
            double typeCount = CurrentPos.Y + (headerRow + count + 1) * single_Yrow;
            if (index != 0 && typeCount > Margine.Y + bottomCell * single_Yrow)
            {
                NewPage();
                CurrentRow(2);
            }
            else
            {
                if (index != 0) CurrentPos.Y += single_Yrow;
            }
        }

        //　countの値を超える文字数ならば，先頭から指定の文字数を取ったものを返す．
        public string GetText(string text, int count)
        {
            if (text.Length > count)
            {
                return text.Substring(0, count - 1);
            }
            else
            {
                return text;
            }
        }

        public void FsecJudge(List<string[]> data, int j, int numFullWidth = 1)
        {
            double y = CurrentPos.Y;

            while (true)
            {
                y += single_Yrow * numFullWidth;

                // 跨ぎそうなら1行あきらめて，次ページへ．
                if (y > single_Yrow * bottomCell + Margine.Y - 0.5)
                {
                    NewPage();
                    CurrentPos.Y += single_Yrow * 2;
                    Header(current_header, currentHeader_Xspacing);
                    break;
                }

                try
                {
                    // 部材（"m")が入っているかどうか
                    if (data[j + 1][0] != "")
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }

                j++;
            }
        }

        // 結果の印刷（基本形）
        public void PrintResultBasic(List<string[]> data, int LL = 0, string dataType = "fsec")
        {
            // ヘッダーの印刷
            Header(header_content, header_Xspacing);

            for (int j = 0; j < data.Count; j++)
            {
                for (int l = 0; l < data[j].Length; l++)
                {
                    CurrentColumn(body_Xspacing[0, l]); //x方向移動
                    PrintContent(data[j][l]); // print
                }

                if (!(LL == data.Count - 1 && j == data.Count - 1))
                {
                    CurrentRow(1); // y方向移動
                }

                // 断面力データの時には部材ごとにまとまって印刷できるようにする
                //　まとまりごとに0.5行ずつ開ける
                if (dataType == "fsec" && j + 1 < data.Count)
                {                
                    // 部材（"m")が入っているかどうか
                    if (data[j + 1][0] != "")
                    {
                        CurrentRow(0.5);
                        FsecJudge(data, j + 1);
                    }
                }
            }
        }
        /// <summary>
        /// Combine/Pickupのときの準備（LLのfor文の状態にあわせる）
        /// </summary>
        /// <param name="result">disg/fsec/reacのいずれか</param>
        /// <param name="key">combine/pickupのいずれか</param>
        /// <param name="title">ex)case2</param>
        /// <param name="type">ex)x軸方向最大</param>
        /// <param name="data">combine/pickupのデータ全てが入る</param>
        /// <param name="textLen">組合せが一行あたりに入る文字数</param>
        public void PrintResultAnnexingReady(string result, string key, List<string> title, List<string> type, List<List<List<string[]>>> data, int textLen)
        {
            var resultJa = "";
            switch (result)
            {
                case "disg":
                    resultJa = "変位量";
                    break;
                case "fsec":
                    resultJa = "断面力";
                    break;
                case "reac":
                    resultJa = "反力";
                    break;
            }

            DataCountKeep(100, result + key);
            // タイトルの印刷
            PrintContent(key + resultJa, 0);
            CurrentRow(2);

            for (int i = 0; i < data.Count; i++)
            {
                //  1ケースでページをまたぐかどうか
                int count = 0;

                for (int m = 0; m < data[i].Count; m++)
                {
                    count += data[i][m].Count;
                }

                string _title = "";
                if(title.Count > i)
                {
                    _title = title[i];
                }

                TypeCount(i, 8, count, _title);

                // タイトルの印刷
                CurrentColumn(0);
                PrintContent(_title, 0);
                CurrentRow(2);

                PrintResultAnnexing(_title, type, data[i], textLen, result);
            }
        }


        /// <summary>
        /// Combine/Pickup/LLの印刷
        /// </summary>
        /// <param name="title">ex)case2</param>
        /// <param name="type">ex)x軸方向最大</param>
        /// <param name="data">combine/pickup/LLのデータのcase1つぶん</param>
        /// <param name="textLen">組合せが一行あたりに入る文字数</param>
        public void PrintResultAnnexing(string title, List<string> type, List<List<string[]>> data, int textLen, string result = "")
        {
            for (int j = 0; j < data.Count; j++)
            {
                //組み合わせの文字数のカウント
                //textLen:切り替わりの文字数の閾値
                var d1 = data[j];
                if(d1.Count < 1)
                {
                    continue;
                }
                var d2 = d1.First();
                var d3 = d2.Last();
                var d4 = d3.Length;
                int numFullWidth = d4 > textLen ? 2 : 1;
                int _numFullWidth = data[j][0][data[j][0].Length - 1].Length > textLen ? 2 : 1;

                //  1タイプ内でページをまたぐかどうか
                TypeCount(j, 6, data[j].Count * numFullWidth, title);

                // タイプの印刷
                CurrentColumn(0);
                PrintContent(type[j], 0);
                CurrentRow(2);

                // ヘッダーの印刷
                Header(header_content, header_Xspacing);

                for (int k = 0; k < data[j].Count; k++)
                {
                    //2行になるとき，組み合わせとそのほかデータがページ跨ぎしないようにする．
                    if (numFullWidth == 2)
                    {
                        double y = CurrentPos.Y + single_Yrow * 2;
                        // 跨ぎそうなら1行あきらめて，次ページへ．
                        if (y > single_Yrow * bottomCell + Margine.Y)
                        {
                            CurrentRow(2);
                        }
                    }

                    for (int l = 0; l < data[j][k].Length; l++)
                    {
                        CurrentColumn(body_Xspacing[0, l]); //x方向移動

                        // 組み合わせで2行になるとき，1行下に書く．
                        if (l == data[j][k].Length - 1 && numFullWidth == 2)
                        {
                            CurrentRow(1);
                        }

                        PrintContent(data[j][k][l]); // print
                    }
                    CurrentRow(1); // y方向移動

                    // 断面力データの時には部材ごとにまとまって印刷できるようにする
                    //　まとまりごとに0.5行ずつ開ける
                    if (result == "fsec" && k + 1 < data[j].Count)
                    {
                        // 部材（"m")が入っているかどうか
                        if (data[j][k + 1][0] != "")
                        {
                            CurrentRow(0.5);
                            FsecJudge(data[j], k + 1, numFullWidth);
                        }
                    }
                }
            }
        }
    }

    // 日本語フォントのためのフォントリゾルバー
    internal class JapaneseFontResolver : IFontResolver
    {
        public string DefaultFontName => throw new NotImplementedException();

        // MS 明朝
        public static readonly string MS_MINCHO_TTF = "PDF_Manager.fonts.MS Mincho.ttf";


        // MS ゴシック
        private static readonly string MS_GOTHIC_TTF = "PDF_Manager.fonts.MS Gothic.ttf";


        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "MsMincho#Medium":
                    return LoadFontData(MS_MINCHO_TTF);

                case "MsGothic#Medium":
                    return LoadFontData(MS_GOTHIC_TTF);

            }
            return null;
        }

        public FontResolverInfo ResolveTypeface(
                    string familyName, bool isBold, bool isItalic)
        {
            var fontName = familyName.ToLower();

            switch (fontName)
            {
                case "ms gothic":
                    return new FontResolverInfo("MsGothic#Medium");
            }

            // デフォルトのフォント
            return new FontResolverInfo("MsMincho#Medium");
        }

        // 埋め込みリソースからフォントファイルを読み込む
        private byte[] LoadFontData(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new ArgumentException("No resource with name " + resourceName);

                int count = (int)stream.Length;
                byte[] data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }
    }



}
