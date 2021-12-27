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
        public string[] current_header;
        public int[] currentHeader_Xspacing;
        public int[] currentBody_Xspacing;
        public int[] currentHeader_Yspacing;
        public int currentXposition_values = 0;


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
        public void PrintContent(int dataHandle, string data)
        {
            if (dataHandle == 0)
            {
                gfx.DrawString(data, font_got, XBrushes.Black, CurrentPos);
            }
            else if(dataHandle == 1)
            {
                gfx.DrawString(data, font_mic, XBrushes.Black, CurrentPos);
            }
        }

        // 行数の管理
        public void CurrentRow(int row)
        {
            CurrentPos.Y += single_Yrow * row;

            if (CurrentPos.Y > single_Yrow * bottomCell)
            {
                NewPage();
                CurrentPos.Y += single_Yrow * 2;
                Header(current_header, currentHeader_Xspacing, currentHeader_Yspacing);
            }
        }

        //　列数の管理
        public void CurrentColumn(int column)
        {
            CurrentPos.X = x + column;
        }

        //　ヘッダー関係
        public void Header(string[] header_content, int[] header_Xspacing, int[] header_Yspacing)
        {
            current_header = header_content;
            currentHeader_Xspacing = header_Xspacing;
            currentHeader_Yspacing = header_Yspacing;
            for (int i = 0; i < current_header.Length; i++)
            {
                CurrentColumn(currentHeader_Xspacing[i]);
                CurrentRow(currentHeader_Yspacing[i]);
                //CurrentPos.Y += single_Yrow * currentHeader_Yspacing[i];
                PrintContent(1, current_header[i]);
                //gfx.DrawString(, font_mic, XBrushes.Black, CurrentPos);
            }
            //CurrentPos.X = x;
            CurrentPos.Y += single_Yrow*2;
        }


        //　classをまたいで，改ページするかの判定
        // 次の項目がまたがず入りきるなら同一ページ，そうでないなら改ページ
        public bool DataCountKeep(double value)
        {
            if ((value + dataCount) > Margine.Y + bottomCell * single_Yrow)
            {
                judge = true;
                dataCount = Margine.Y + value % (Margine.Y + bottomCell * single_Yrow);
            }
            else
            {
                judge = false;
                dataCount += value;
            }
            return judge;
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
