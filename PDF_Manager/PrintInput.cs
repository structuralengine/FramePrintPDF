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
        if (data.printDatas.ContainsKey(DiagramInput.KEY))
        {
            DiagramInput diaLoad = (DiagramInput)data.printDatas[DiagramInput.KEY];
            diaLoad.printPDF(mc, data);                        // 格点
            return mc; // 荷重図の指定があったらその他の出力はしない
        }
        //断面力図
        if (data.printDatas.ContainsKey(DiagramResult.KEY))
        {
            DiagramResult diaFsec = (DiagramResult)data.printDatas[DiagramResult.KEY];
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
        // 結合
//        ((InputJoint)data.printDatas[InputJoint.KEY]).printPDF(mc, data);
        // 着目点
        ((InputNoticePoints)data.printDatas[InputNoticePoints.KEY]).printPDF(mc, data);
        // バネ
//        ((InputFixMember)data.printDatas[InputFixMember.KEY]).printPDF(mc, data);
        // シェル
//        ((InputShell)data.printDatas[InputShell.KEY]).printPDF(mc, data);
        // 荷重名称
        ((InputLoadName)data.printDatas[InputLoadName.KEY]).printPDF(mc, data);
        // 荷重強度 
//        ((InputLoad)data.printDatas[InputLoad.KEY]).printPDF(mc, data);
        // 組み合わせDefine
//        ((InputDefine)data.printDatas[InputDefine.KEY]).printPDF(mc, data);
        // 組み合わせCombine
//        ((InputCombine)data.printDatas[InputCombine.KEY]).printPDF(mc, data);
        // 組み合わせピックアップ
//        ((InputPickup)data.printDatas[InputPickup.KEY]).printPDF(mc, data);


        // 計算結果データ
        // 変位量
        ((ResultDisg)data.printDatas[ResultDisg.KEY]).printPDF(mc, data);
        // 反力
        ((ResultReac)data.printDatas[ResultReac.KEY]).printPDF(mc, data);
        // 断面力
        ((ResultFsec)data.printDatas[ResultFsec.KEY]).printPDF(mc, data);

        // 組み合わせ変位量
        ((ResultDisgCombine)data.printDatas[ResultDisgCombine.KEY]).printPDF(mc, data);
        // 組み合わせ反力
        ((ResultReacCombine)data.printDatas[ResultReacCombine.KEY]).printPDF(mc, data);
        // 組み合わせ断面力
        ((ResultFsecCombine)data.printDatas[ResultFsecCombine.KEY]).printPDF(mc, data);

        // ピックアップ変位量
        ((ResultDisgPickup)data.printDatas[ResultDisgPickup.KEY]).printPDF(mc, data);
        // ピックアップ反力
        ((ResultReacPickup)data.printDatas[ResultReacPickup.KEY]).printPDF(mc, data);
        // ピックアップ断面力
        ((ResultFsecPickup)data.printDatas[ResultFsecPickup.KEY]).printPDF(mc, data);

        return mc;
    }


}



