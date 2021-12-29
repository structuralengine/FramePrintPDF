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
using System.Threading.Tasks;

namespace PDF_Manager.Printing
{
    internal class PdfDoc
    {
        public PdfDocument document;
        public XGraphics gfx;
        public XPoint CurrentPos; // 現在の座標
        public XPoint Margine; // マージン
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
        public int[,] currentBody_Xspacing;
        public int[,] currentHeader_Yspacing;

        public PdfDoc()
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
        public void SavePDF(string filename = "HelloWorld.pdf")
        {
            // PDF保存（カレントディレクトリ）
            //document.Save(filename);
            document.Save("D:\\work\\sasaco\\PDF_generate\\bin\\Debug\\netcoreapp3.1\\work\\Test.pdf");

        }

        // PDFを記述する
        public void PrintContent(string data, int dataHandle = 1)
        {
            if (dataHandle == 0)
            {
                gfx.DrawString(data, font_got, XBrushes.Black, CurrentPos);
            }
            else if (dataHandle == 1)
            {
                gfx.DrawString(data, font_mic, XBrushes.Black, CurrentPos);
            }
        }

        // 行数の管理
        public void CurrentRow(int row)
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
        public void Header(string[,] header_content, int[,] header_Xspacing)
        {
            current_header = header_content;
            currentHeader_Xspacing = header_Xspacing;
            for (int i = 0; i < current_header.GetLength(0); i++)
            {
                for (int j = 0; j < current_header.GetLength(1); j++)
                {
                    CurrentColumn(currentHeader_Xspacing[i, j]);
                    PrintContent(current_header[i, j]);
                }
                CurrentRow(1);
            }
            CurrentPos.Y += single_Yrow;
        }


        //　classをまたいで，改ページするかの判定
        // 次の項目がまたがず入りきるなら同一ページで2行空き，そうでないなら改ページ
        public void DataCountKeep(double value)
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

        //　タイプ別の改ページ判定
        public void TypeCount(int index, double headerRow, double count, string title)
        {
            double typeCount = CurrentPos.Y + (headerRow + count) * single_Yrow;
            if (typeCount > Margine.Y + bottomCell * single_Yrow)
            {
                NewPage();
                CurrentRow(2);
            }
            else
            {
                if (index != 0) CurrentPos.Y += single_Yrow;
            }
        }

        // データの精査
        // (data,stringのままか(true)，四捨五入等の処理を行いたいか(false),四捨五入する時の桁数,指数形式の表示など)
        public string TypeChange(JToken data, bool jud = false, int round = 0, string style = "none")
        {
            string newDataString = "";

            // 四捨五入等の処理を行う
            if (jud == false)
            {
                if (data.Type == JTokenType.Null) data = double.NaN;
                double newDataDouble = double.Parse(data.ToString());
                if (style == "none")
                {
                    newDataString = Double.IsNaN(Math.Round(newDataDouble, round, MidpointRounding.AwayFromZero)) ? "" : newDataDouble.ToString();
                }
                else
                {
                    newDataString = Double.IsNaN(Math.Round(newDataDouble, round, MidpointRounding.AwayFromZero)) ? "" : newDataDouble.ToString(style);
                }
            }

            // すぐにstringにする
            else if (jud == true)
            {
                if (data.Type == JTokenType.Null) data = "";
                newDataString = data.ToString();
            }
            return newDataString;
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
