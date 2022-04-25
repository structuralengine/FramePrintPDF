using Newtonsoft.Json.Linq;
using PDF_Manager.Printing;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Utils;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using Newtonsoft.Json;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PrintInput
{
    private PdfDoc mc;
    private JObject data;
    public Dictionary<string, object> value = new Dictionary<string, object>();
    private int bottomCell = 88;


    public PrintInput(string jsonString)
    {
        // データを読み込む
        data = (JObject.Parse(jsonString));
        value = JObject.FromObject(data).ToObject<Dictionary<string, object>>();


        // PDF ページを準備する
        mc = new PdfDoc();
    }


    /// <summary>
    /// インプットデータの印刷PDFを生成する
    /// </summary>
    public void createPDF()
    {
        //　準備のためのclassの呼び出し
        var pri_ready = new PrintReady();
        //　jsonから取り出した生データを送って，クラスを返す
        object[] class_set = pri_ready.Ready(mc,value);

        //  PDF出力のためのclassの呼び出し
        var exp = new PrintExport();
        //  整形したデータを送る
        exp.Export(mc, class_set);

        // PDFファイルを生成する
        mc.SavePDF();
    }

    public string getPdfSource()
    {
        //　準備のためのclassの呼び出し
        var red = new PrintReady();
        //　jsonから取り出した生データを送る
        object[] dataset = red.Ready(mc, value);

        //  PDF出力のためのclassの呼び出し
        var exp = new PrintExport();
        //  整形したデータを送る
        exp.Export(mc, dataset);

        // PDF を Byte型に変換
        var b = mc.GetPDFBytes();

        // Byte型配列をBase64文字列に変換
        string str = Convert.ToBase64String(b);

        // PDFファイルを生成する
        return str;
    }

    // nodeの印刷
    public List<List<string[]>> node()
    {
        // nodeデータを取得する
        var target = JObject.FromObject(value["node"]).ToObject<Dictionary<string, object>>();

        // 集まったデータはここに格納する
        //ArrayList node_data = new ArrayList();
        List<List<string[]>> node_data = new List<List<string[]>>();

        // 全部の行数
        var row = target.Count;

        var page = 0;
        //var body = new ArrayList();



        while (true)
        {
            if (row > bottomCell)
            {
                List<string[]> body = new List<string[]>() ;
                var half = bottomCell / 2;
                for (var i = 0; i < half; i++)
                {
                    //　各行の配列開始位置を取得する（左段/右段)
                    var j = bottomCell * page + i;
                    var k = bottomCell * page + bottomCell / 2 + i;
                    //　各行のデータを取得する（左段/右段)
                    var targetValue_l = JObject.FromObject(target.ElementAt(j).Value).ToObject<Dictionary<string, double>>();

                    string[] line = new String[8];
                    line[0] = target.ElementAt(j).Key;
                    line[1] = (Math.Round(targetValue_l["x"], 3, MidpointRounding.AwayFromZero)).ToString();
                    line[2] = (Math.Round(targetValue_l["y"], 3, MidpointRounding.AwayFromZero)).ToString();
                    line[3] = (Math.Round(targetValue_l["z"], 3, MidpointRounding.AwayFromZero)).ToString();

                    var targetValue_r = JObject.FromObject(target.ElementAt(k).Value).ToObject<Dictionary<string, double>>();
                    line[4] = target.ElementAt(k).Key;
                    line[5] = (Math.Round(targetValue_r["x"], 3, MidpointRounding.AwayFromZero)).ToString();
                    line[6] = (Math.Round(targetValue_r["y"], 3, MidpointRounding.AwayFromZero)).ToString();
                    line[7] = (Math.Round(targetValue_r["z"], 3, MidpointRounding.AwayFromZero)).ToString();
                    body.Add(line);
                }
                node_data.Add(body);
                row -= bottomCell;
                page++;
            }
            else
            {
                List<string[]> body = new List<string[]>();
                row = decimal.ToInt32(decimal.Ceiling(row / 2));

                for (var i = 0; i < row; i++)
                {
                    //　各行の配列開始位置を取得する（左段/右段)
                    var j = bottomCell * page + i;
                    var k = j + row;
                    //　各行のデータを取得する（左段)
                    var targetValue_l = JObject.FromObject(target.ElementAt(j).Value).ToObject<Dictionary<string, double>>();

                    string[] line = new String[8];
                    line[0]=target.ElementAt(j).Key;
                    line[1]=(Math.Round(targetValue_l["x"], 3, MidpointRounding.AwayFromZero)).ToString();
                    line[2]=(Math.Round(targetValue_l["y"], 3, MidpointRounding.AwayFromZero)).ToString();
                    line[3]=(Math.Round(targetValue_l["z"], 3, MidpointRounding.AwayFromZero)).ToString();

                    if (target.ElementAt(k).Key != null)
                    {
                        //　各行のデータを取得する（右段)
                        var targetValue_r = JObject.FromObject(target.ElementAt(k).Value).ToObject<Dictionary<string, double>>();
                        line[4]=target.ElementAt(k).Key;
                        line[5]=(Math.Round(targetValue_r["x"], 3, MidpointRounding.AwayFromZero)).ToString();
                        line[6]=(Math.Round(targetValue_r["y"], 3, MidpointRounding.AwayFromZero)).ToString();
                        line[7]=(Math.Round(targetValue_r["z"], 3, MidpointRounding.AwayFromZero)).ToString();
                        body.Add(line);
                    }
                }
                node_data.Add(body);
                break;
            }
        }
        return node_data;
    }

    public void member()
    {
        var d = (JObject)data["member"];
        var body = new List<List<string>>();

        for (int i = 0; i < d.Count; i++)
        {
            var line = new List<string>();
            var id = (i + 1).ToString();
            //var len = this.getMemberLength(id);
            line.Add(id);
            line.Add((string)d[id]["ni"]);
            line.Add((string)d[id]["nj"]);
            line.Add((string)d[id]["len"]);
            line.Add((string)d[id]["e"]);
            line.Add((string)d[id]["cg"]);
            line.Add((string)d[id]["len"]);
            body.Add(line);
        }

    }
}

public class main
{
    public main(string jsonString)
    {

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var rnd = new Random();
        for (int i = 0; i < 100; i++)
        {
            users.Add(new User()
            {
                FirstName = "firstname" + rnd.Next(5, 66).ToString(),
                LastName = "lastname" + rnd.Next(5, 66).ToString() + i.ToString(),
                DateofBirth = DateTime.UtcNow.Subtract(new TimeSpan(rnd.Next(10000, 20000), 0, 0, 0, 0))
            });
        }
        GeneratePDF();
    }
    static void GeneratePDF()
    {
        PdfDocument document = new PdfDocument();
        // フォントリゾルバーのグローバル登録
        GlobalFontSettings.FontResolver = new JapaneseFontResolver();
        document.Info.Title = "PDF Example";
        PdfPage page = document.AddPage();
        XFont font = new XFont("MS Gothic", 20, XFontStyle.Regular);

        XGraphics gfx = XGraphics.FromPdfPage(page);

        // DrawString:文字を入力できる
        // DrawString(入れたいデータ,フォント指定（fontの種類，大きさ，太さ）,文字の色,文字の位置(x,y))
        gfx.DrawString("First Name", new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(100, 280));
        gfx.DrawString("Last Name", new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(250, 280));
        gfx.DrawString("Date of Birth", new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(400, 280));

        // DrawString(入れたいデータ,フォント指定（fontの種類，大きさ，太さ）,文字の色,文字の位置(x,y))
        int currentYposition_values = 303;

        // AddPage:ドキュメントのページが更新
        page = document.AddPage();
        // FromPdfPage:ページ更新後の新規ページにおけるgfx
        gfx = XGraphics.FromPdfPage(page);
        currentYposition_values = 53;

        bool firstpage = true;

        for (int i = 0; i < users.Count; i++)
        {
            if (i != 0 && i % 30 == 0)
            {
                // 30行したら改ページ
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                currentYposition_values = 53;

            }
            gfx.DrawString("First Name", new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(100, 10));
            gfx.DrawString("Last Name", new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(250, 10));
            gfx.DrawString("Date of Birth", new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(400, 10));
            gfx.DrawString(users[i].FirstName, new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(100, currentYposition_values));
            gfx.DrawString(users[i].LastName, new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(250, currentYposition_values));
            gfx.DrawString(users[i].DateofBirth.ToShortDateString(), new XFont("Ms Gothic", 15, XFontStyle.Bold), XBrushes.Black, new XPoint(400, currentYposition_values));
            currentYposition_values += 20;
        }

        document.Save("D:\\work\\sasaco\\PDF_generate\\bin\\Debug\\netcoreapp3.1\\work\\Test.pdf");


    }

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



    static List<User> users = new List<User>();

    class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateofBirth { get; set; }
    }
}

//  public getMemberLength(memberNo: string) : number {

//      const memb = this.getMember(memberNo.toString());
//      if (memb.ni === undefined || memb.nj === undefined) {
//          return null;
//         }
//      const ni: string = memb.ni;
//      const nj: string = memb.nj;
//      if (ni === null || nj === null) {
//        return null;
//      }

//      const iPos = this.node.getNodePos(ni)
//      const jPos = this.node.getNodePos(nj)
//      if (iPos == null || jPos == null) {
//        return null;
//      }

//      const xi: number = iPos['x'];
//      const yi: number = iPos['y'];
//      const zi: number = iPos['z'];
//      const xj: number = jPos['x'];
//      const yj: number = jPos['y'];
//      const zj: number = jPos['z'];

//      const result: number = Math.sqrt((xi - xj) ** 2 + (yi - yj) ** 2 + (zi - zj) ** 2);
//      return result;

//}




