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
        JObject data = JObject.Parse(jsonString);
        var value = data.ToObject<Dictionary<string, object>>(); 
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
    /// <param name="data"></param>
    private static PdfDocument printPDF(PrintData data)
    {
        // PDF ページを準備する
        var mc = new PdfDocument(data);

        // 荷重図
        if (data.printDatas.ContainsKey(InputDiagramLoad.KEY))
        {
            InputDiagramLoad diaLoad = (InputDiagramLoad)data.printDatas[InputDiagramLoad.KEY];
            diaLoad.printPDF(mc, data);                        // 格点
            return mc; // 荷重図の指定があったらその他の出力はしない
        }
        //断面力図
        if (data.printDatas.ContainsKey(InputDiagramFsec.KEY))
        {
            InputDiagramFsec diaFsec = (InputDiagramFsec)data.printDatas[InputDiagramFsec.KEY];
            diaFsec.printPDF(mc, data);                        // 格点
            return mc; // 断面力図の指定があったらその他の出力はしない
        }

       //入力データ
       //  格点
       ((InputNode)data.printDatas[InputNode.KEY]).printPDF(mc, data);
       // 部材
       ((InputMember)data.printDatas[InputMember.KEY]).printPDF(mc, data);
       // 材料
       ((InputElement)data.printDatas[InputElement.KEY]).printPDF(mc, data);
       // 支点
       ((InputFixNode)data.printDatas[InputFixNode.KEY]).printPDF(mc, data);

        /*
        mc = InputJoint.printPDF(mc, data);           // 結合
        mc = InputNoticePoints.printPDF(mc, data);    // 着目点
        mc = InputFixMember.printPDF(mc,data);        // バネ
        mc = InputShell.printPDF(mc, data);           // シェル
        */
        //荷重名称
        //((InputLoadName)data.printDatas[InputLoadName.KEY]).printPDF(mc, data);
        /*
        mc = InputLoad.printPDF(mc, data);            // 荷重強度 
        mc = InputDefine.printPDF(mc, data);          // 組み合わせDefine
        mc = InputCombine.printPDF(mc, data);         // 組み合わせCombine
        mc = InputPickup.printPDF(mc, data);          // 組み合わせピックアップ

        */


        // 計算結果データ
        // 変位量
        ((ResultDisg)data.printDatas[ResultDisg.KEY]).printPDF(mc, data);
        ((ResultReac)data.printDatas[ResultReac.KEY]).printPDF(mc, data);
        ((ResultFsec)data.printDatas[ResultFsec.KEY]).printPDF(mc, data);
        ((ResultDisgCombine)data.printDatas[ResultDisgCombine.KEY]).printPDF(mc, data);
        ((ResultReacCombine)data.printDatas[ResultReacCombine.KEY]).printPDF(mc, data);
        //((ResultFsecCombine)data.printDatas[ResultFsecCombine.KEY]).printPDF(mc, data);


        /*
        mc = ResultDisg.printPDF(mc, data);
        mc = ResultDisgCombine.printPDF(mc, data);
        mc = ResultDisgPickup.printPDF(mc, data);
        mc = ResultFsec.printPDF(mc, data);
        mc = ResultFsecCombine.printPDF(mc, data);
        mc = ResultFsecPickup.printPDF(mc, data);
        mc = ResultReac.ReacPDF(mc, data);
        mc = ResultReacCombine.ReacAnnexingPDF(mc, data);
        mc = ResultReacPickup.ReacAnnexingPDF(mc, data);
        */
        return mc;
    }


}



