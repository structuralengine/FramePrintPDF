using Newtonsoft.Json.Linq;
using PDF_Manager;
using PDF_Manager.Printing;
using System;
using System.Collections.Generic;

public class PrintInput
{
    private PrintData data;

    public PrintInput(string jsonString)
    {
        // データを読み込む
        JObject data = (JObject.Parse(jsonString));
        var value = data.ToObject<Dictionary<string, object>>(); // JObject.FromObject(data).ToObject<Dictionary<string, object>>();
        //　準備のためのclassの呼び出し
        this.data = new PrintData(value);
    }

    /// <summary>
    /// インプットデータの印刷PDFを生成する
    /// </summary>
    public void createPDF()
    {
        //  PDF出力のためのclassの呼び出し
        //  整形したデータを送る
        var mc = PrintInput.printPDF(this.data);

        // PDFファイルを生成する
        mc.SavePDF();
    }

    /// <summary>
    /// PDFのBase64コードを返す
    /// </summary>
    /// <returns></returns>
    public string getPdfSource()
    {
        //  PDF出力のためのclassの呼び出し
        //  整形したデータを送る
        var mc = PrintInput.printPDF(this.data);

        // PDF を Byte型に変換
        var b = mc.GetPDFBytes();

        // Byte型配列をBase64文字列に変換
        string str = Convert.ToBase64String(b);

        // PDFファイルを生成する
        return str;
    }

    /// <summary>
    /// PDF を生成する
    /// </summary>
    /// <param name="red"></param>
    private static PdfDoc printPDF(PrintData red)
    {
        // PDF ページを準備する
        var mc = new PdfDoc(red);

        // 荷重図
        if (red.printDatas["diagramLoad"] != null)
        {
            mc = InputDiagramLoad.printPDF(mc, red);
            return mc; // 荷重図の指定があったらその他の出力はしない
        }

        // 入力データ
        mc = InputNode.printPDF(mc, red);            // 格点
        mc = InputMember.printPDF(mc, red);          // 部材
        mc = InputElement.printPDF(mc, red);         // 材料
        mc = InputFixNode.FixNodePDF(mc, red);       // 支点   
        mc = InputJoint.printPDF(mc, red);           // 結合
        mc = InputNoticePoints.printPDF(mc, red);    // 着目点
        mc = InputFixMember.printPDF(mc,red);        // バネ
        mc = InputShell.printPDF(mc, red);           // シェル
        mc = InputLoadName.printPDF(mc, red);        // 荷重名称 
        mc = InputLoad.printPDF(mc, red);            // 荷重強度 
        mc = InputDefine.printPDF(mc, red);          // 組み合わせDefine
        mc = InputCombine.printPDF(mc, red);         // 組み合わせCombine
        mc = InputPickup.printPDF(mc, red);          // 組み合わせピックアップ

        // 計算結果データ
        mc = ResultDisg.printPDF(mc, red);
        mc = ResultDisgCombine.printPDF(mc, red);
        mc = ResultDisgPickUp.printPDF(mc, red);
        mc = ResultFsec.printPDF(mc, red);
        mc = ResultFsecCombine.printPDF(mc, red);
        mc = ResultFsecPickUp.printPDF(mc, red);
        mc = ResultReac.ReacPDF(mc, red);
        mc = ResultReacCombine.ReacAnnexingPDF(mc, red);
        mc = ResultReacPickUp.ReacAnnexingPDF(mc, red);

        return mc;
    }


}



