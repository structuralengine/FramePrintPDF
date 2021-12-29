﻿using PdfSharpCore;
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
        public XPoint CurrentPosHeader; // 現在の座標
        public XPoint CurrentPosBody; // 現在の座標
        public XPoint Margine; // マージン
        public XFont font_mic;
        public XFont font_got;
        public int x;
        public int y;
        public bool judge = false;
        public int Threshold = 73;
        public int dataCount = 0;

        public PdfDoc()
        {
            //　新規ドキュメントの作成
            document = new PdfDocument();

            // フォントリゾルバーのグローバル登録
            var FontResolver = GlobalFontSettings.FontResolver.GetType();
            if(FontResolver.Name != "JapaneseFontResolver")
                GlobalFontSettings.FontResolver = new JapaneseFontResolver();

            /// マージンを設定する
            x = 50;
            y = 50;

            Margine = new XPoint(x, y);

            // 新しいページを作成する
            NewPage();


            document.Info.Title = "FrameWebForJS";
            PdfPage page = new PdfPage();

            // フォントの設定
            font_mic = new XFont("MS Mincho", 10, XFontStyle.Regular);
            font_got = new XFont("MS Gothic", 10, XFontStyle.Regular);
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
            CurrentPosHeader = new XPoint(Margine.X, Margine.Y);

            CurrentPosBody = new XPoint(Margine.X, Margine.Y);

        }

        /// <summary>
        /// PDF を Byte型に変換したものを返す
        /// </summary>
        /// <returns></returns>
        public byte[] getPDFBytes()
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
        public void savePDF(string filename = "HelloWorld.pdf")
        {
            // PDF保存（カレントディレクトリ）
            //document.Save(filename);
            document.Save("D:\\work\\sasaco\\PDF_generate\\bin\\Debug\\netcoreapp3.1\\work\\Test.pdf");

        }
        

        public bool dataCountKeep(int value)
        {
            if (value > dataCount)
            {
                judge = true;
                dataCount = value % Threshold;
            }
            else
            {
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
