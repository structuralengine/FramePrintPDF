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
using PDF_Manager.Comon;

public class PrintInput
{
    private PdfDoc mc;
    private JObject data;
    public Dictionary<string, object> value = new Dictionary<string, object>();
    //private int bottomCell = 88;


    public PrintInput(string jsonString)
    {
        // データを読み込む
        this.data = (JObject.Parse(jsonString));
        this.value = JObject.FromObject(this.data).ToObject<Dictionary<string, object>>();

        // PDF ページを準備する
        mc = new PdfDoc();
    }

   
    /// <summary>
    /// インプットデータの印刷PDFを生成する
    /// </summary>
    public void createPDF()
    {
        //　準備のためのclassの呼び出し
        var pri_ready = new dataManager();
        //　jsonから取り出した生データを送って，クラスを返す
        object[] class_set = pri_ready.Ready(value);

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
        var red = new dataManager();
        //　jsonから取り出した生データを送る
        object[] dataset = red.Ready(value);

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

 
}



