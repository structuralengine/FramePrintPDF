﻿using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing;
using PDF_Manager.Printing.Comon;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PDF_Manager.Printing
{
    public class Disg
    {
        public string id;    // 節点番号
        public double dx;
        public double dy;
        public double dz;
        public double rx;
        public double ry;
        public double rz;

        // 組み合わせで使う
        public string caseStr = null;
        public string comb = null;
    }


    internal class ResultDisg
    {
        public const string KEY = "disg";

        private Dictionary<string, object> disgs = new Dictionary<string, object>();
        private Dictionary<string, string> disgnames = new Dictionary<string, string>();

        public ResultDisg(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(KEY))
                return;

            // データを取得する．
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = dataManager.toString(target.ElementAt(i).Key);  // ケース番号
                var val = JToken.FromObject(target.ElementAt(i).Value);

                if(val.Type == JTokenType.Array)
                {
                    JArray Dis = (JArray)val;
                    var _disg = new List<Disg>();
                    for (int j = 0; j < Dis.Count; j++)
                    {
                        JToken item = Dis[j];

                        var ds = new Disg();

                        ds.id = dataManager.toString(item["id"]);
                        ds.dx = dataManager.parseDouble(item["dx"]);
                        ds.dy = dataManager.parseDouble(item["dy"]);
                        ds.dz = dataManager.parseDouble(item["dz"]);
                        ds.rx = dataManager.parseDouble(item["rx"]);
                        ds.ry = dataManager.parseDouble(item["ry"]);
                        ds.rz = dataManager.parseDouble(item["rz"]);

                        _disg.Add(ds);

                    }
                    this.disgs.Add(key, _disg);

                } 
                else if (val.Type == JTokenType.Object)
                {   // LL：連行荷重の時
                    var Dis = ((JObject)val).ToObject<Dictionary<string, object>>();
                    var _disg = ResultDisgCombine.getDisgCombine(Dis);
                    this.disgs.Add(key, _disg);
                }

            }

            if (!value.ContainsKey("disgName"))
                return;

            // データを取得する．
            var targetName = JArray.FromObject(value["disgName"]);

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる
                var load = targetName[i];
                string[] loadNew = new string[2];

                loadNew[0] = load[0].ToString();
                loadNew[1] = load[1].ToString();

                disgnames.Add(loadNew[0], loadNew[1]);

            }

        }



        ///印刷処理

        ///タイトル
        private string title;
        ///２次元か３次元か
        private int dimension;
        ///テーブル
        private Table myTable;


        ///印刷前の初期化処理
        ///
        private void printInit(PdfDocument mc, PrintData data)
        {
            this.dimension = data.dimension;

            if (this.dimension == 3)
            {///3次元

                ///テーブルの作成
                this.myTable = new Table(4, 7);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 15.0;//節点No
                this.myTable.ColWidth[1] = 80.0;//X方向の移動量
                this.myTable.ColWidth[2] = 80.0;//Y方向の移動量
                this.myTable.ColWidth[3] = 80.0;//Z方向の移動量
                this.myTable.ColWidth[4] = 80.0;//X軸周りの回転量
                this.myTable.ColWidth[5] = 80.0;//Y軸周りの回転量
                this.myTable.ColWidth[6] = 80.0;//Z軸周りの回転量

                this.myTable.RowHeight[1] = printManager.LineSpacing2;

                this.myTable.AlignX[0, 0] = "L";
                this.myTable.AlignX[1, 0] = "L";
                this.myTable.AlignX[1, 1] = "R";
                this.myTable.AlignX[1, 2] = "R";
                this.myTable.AlignX[1, 3] = "R";
                this.myTable.AlignX[1, 4] = "R";
                this.myTable.AlignX[1, 5] = "R";
                this.myTable.AlignX[1, 6] = "R";
                this.myTable.AlignX[2, 0] = "L";
                this.myTable.AlignX[2, 1] = "R";
                this.myTable.AlignX[2, 2] = "R";
                this.myTable.AlignX[2, 3] = "R";
                this.myTable.AlignX[2, 4] = "R";
                this.myTable.AlignX[2, 5] = "R";
                this.myTable.AlignX[2, 6] = "R";
                this.myTable.AlignX[3, 1] = "R";
                this.myTable.AlignX[3, 2] = "R";
                this.myTable.AlignX[3, 3] = "R";
                this.myTable.AlignX[3, 4] = "R";
                this.myTable.AlignX[3, 5] = "R";
                this.myTable.AlignX[3, 6] = "R";



                switch (data.language)
                {
                    default:
                        this.title = "変位量データ";
                        this.myTable[1, 0] = "節点";
                        this.myTable[2, 0] = "No";
                        this.myTable[1, 1] = "X方向の";
                        this.myTable[2, 1] = "移動量";
                        this.myTable[3, 1] = "(mm)";
                        this.myTable[1, 2] = "Y方向の";
                        this.myTable[2, 2] = "移動量";
                        this.myTable[3, 2] = "(mm)";
                        this.myTable[1, 3] = "Z方向の";
                        this.myTable[2, 3] = "移動量";
                        this.myTable[3, 3] = "(mm)";
                        this.myTable[1, 4] = "X軸周りの";
                        this.myTable[2, 4] = "回転量";
                        this.myTable[3, 4] = "(mmrad)";
                        this.myTable[1, 5] = "Y軸周りの";
                        this.myTable[2, 5] = "回転量";
                        this.myTable[3, 5] = "(mmrad)";
                        this.myTable[1, 6] = "Z軸周りの";
                        this.myTable[2, 6] = "回転量";
                        this.myTable[3, 6] = "(mmrad)";
                        break;
                }

                //表題の文字位置
            }
            else
            {//2次元

                ///テーブルの作成
                this.myTable = new Table(4, 8);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 20.0;//節点No
                this.myTable.ColWidth[1] = 72.5;//X方向の移動量
                this.myTable.ColWidth[2] = 72.5;//Y方向の移動量
                this.myTable.ColWidth[3] = 72.5;//X軸周りの回転量
                this.myTable.ColWidth[4] = 40.0;
                this.myTable.ColWidth[5] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[6] = this.myTable.ColWidth[2];
                this.myTable.ColWidth[7] = this.myTable.ColWidth[3];

                this.myTable.RowHeight[1] = printManager.LineSpacing2;

                this.myTable.AlignX[0, 0] = "L";
                this.myTable.AlignX[1, 0] = "R";
                this.myTable.AlignX[1, 1] = "R";
                this.myTable.AlignX[1, 2] = "R";
                this.myTable.AlignX[1, 3] = "R";
                this.myTable.AlignX[1, 4] = "R";
                this.myTable.AlignX[1, 5] = "R";
                this.myTable.AlignX[1, 6] = "R";
                this.myTable.AlignX[1, 7] = "R";
                this.myTable.AlignX[2, 0] = "R";
                this.myTable.AlignX[2, 1] = "R";
                this.myTable.AlignX[2, 2] = "R";
                this.myTable.AlignX[2, 3] = "R";
                this.myTable.AlignX[2, 4] = "R";
                this.myTable.AlignX[2, 5] = "R";
                this.myTable.AlignX[2, 6] = "R";
                this.myTable.AlignX[2, 7] = "R";
                this.myTable.AlignX[3, 1] = "R";
                this.myTable.AlignX[3, 2] = "R";
                this.myTable.AlignX[3, 3] = "R";
                this.myTable.AlignX[3, 4] = "R";
                this.myTable.AlignX[3, 5] = "R";
                this.myTable.AlignX[3, 6] = "R";
                this.myTable.AlignX[3, 7] = "R";

                switch (data.language)
                {
                    default:
                        this.title = "変位量データ";
                        this.myTable[1, 0] = "節点";
                        this.myTable[2, 0] = "No";
                        this.myTable[1, 1] = "X方向の";
                        this.myTable[2, 1] = "移動量";
                        this.myTable[3, 1] = "(mm)";
                        this.myTable[1, 2] = "Y方向の";
                        this.myTable[2, 2] = "移動量";
                        this.myTable[3, 2] = "(mm)";
                        this.myTable[2, 3] = "回転量";
                        this.myTable[3, 3] = "(mmrad)";
                        this.myTable[1, 4] = "節点";
                        this.myTable[2, 4] = "No";
                        this.myTable[1, 5] = "X方向の";
                        this.myTable[2, 5] = "移動量";
                        this.myTable[3, 5] = "(mm)";
                        this.myTable[1, 6] = "Y方向の";
                        this.myTable[2, 6] = "移動量";
                        this.myTable[3, 6] = "(mm)";
                        this.myTable[2, 7] = "回転量";
                        this.myTable[3, 7] = "(mmrad)";

                        break;
                }

            }
        }

        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <param name="rows">行数</param>
        /// <returns>印刷する用の配列</returns>
        private Table getPageContents(List<Disg> target)
        {
            int r = this.myTable.Rows;


            int columns = 2;
            int count = this.myTable.Columns;
            int c = count / columns;

            int rows = target.Count;


            // 行コンテンツを生成
            var table = this.myTable.Clone();
            table.ReDim(row: r + rows);

            table.RowHeight[r] = printManager.LineSpacing2;

            if (dimension == 3)　　//３次元
            {
                for (var i = 0; i < rows; i++)
                {
                    var item = target[i];

                    int j = 0;
                    table[r, j] = printManager.toString(item.id);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.dx, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.dy, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.dz, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.rx, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.ry, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.rz, 4);
                    table.AlignX[r, j] = "R";
                    j++;

                    r++;
                }
            }

            else　　//２次元
            {
                int Rows = target.Count / columns;

                for (var i = 0; i < Rows; i++)
                {
                    for (var j = 0; j < columns; j++)
                    {
                        var index = i + Rows * j; //左側：j=0 ∴index = i, 右側：j=1, ∴index = i+Rows
                        if (target.Count <= index)
                            continue;

                        var item = target[index];

                        table[r + i, 0 + c * j] = printManager.toString(item.id);
                        table.AlignX[r + i, 0 + c * j] = "R";
                        table[r + i, 1 + c * j] = printManager.toString(item.dx, 4);
                        table.AlignX[r + i, 1 + c * j] = "R";
                        table[r + i, 2 + c * j] = printManager.toString(item.dy, 4);
                        table.AlignX[r + i, 2 + c * j] = "R";
                        table[r + i, 3 + c * j] = printManager.toString(item.rz, 4);
                        table.AlignX[r + i, 3 + c * j] = "R";
                    }
                }

            }

            return table;
        }

        /// <summary>
        /// 印刷する
        /// </summary>
        /// <param name="mc"></param>
        public void printPDF(PdfDocument mc, PrintData data)
        {

            if (this.disgs.Count == 0)
                return;

            // タイトル などの初期化
            this.printInit(mc, data);

            // 印刷可能な行数
            var printRows = myTable.getPrintRowCount(mc);

            // 行コンテンツを生成
            var page = new List<Table>();

            // 1ページ目に入る行数
            int rows = printRows[0];

            // 集計開始
            if(dimension == 3)　　//３次元
            {
                for (int j = 0; j < this.disgs.Count; ++j)
                {   // ケース番号のループ
                    var key = this.disgs.ElementAt(j).Key;  // ケース番号
                    var tmp1 = new List<Disg>((List<Disg>)this.disgs.ElementAt(j).Value);

                    var caseNo = this.disgnames.ElementAt(j).Key;
                    var caseName = this.disgnames.ElementAt(j).Value;

                    while (true)
                    {
                        // 1ページに納まる分のデータをコピー
                        var tmp2 = new List<Disg>();
                        for (int i = 0; i < rows; i++)
                        {
                            if (tmp1.Count <= 0)
                                break;
                            tmp2.Add(tmp1.First());
                            tmp1.Remove(tmp1.First());
                        }

                        if (tmp2.Count > 0)
                        {
                            var table = this.getPageContents(tmp2);
                            table[0, 0] = caseNo + caseName;
                            page.Add(table);

                        }
                        else if (tmp1.Count <= 0)
                        {
                            break;
                        }
                        else
                        { // 印刷するものもない
                            mc.NewPage();
                        }

                        // 2ページ以降に入る行数
                        rows = printRows[1];
                    }
                }
            }
            else　　//２次元
            {
                for (int j = 0; j < this.disgs.Count; ++j)
                {   // ケース番号のループ
                    var key = this.disgs.ElementAt(j).Key;  // ケース番号
                    var tmp1 = new List<Disg>((List<Disg>)this.disgs.ElementAt(j).Value);

                    var caseNo = this.disgnames.ElementAt(j).Key;
                    var caseName = this.disgnames.ElementAt(j).Value;

                    while (true)
                    {
                        // 1ページに納まる分のデータをコピー
                        var tmp2 = new List<Disg>();
                        int columns = 2;

                        for (int i = 0; i < columns*rows; i++)
                        {
                            if (tmp1.Count <= 0)
                                break;
                            tmp2.Add(tmp1.First());
                            tmp1.Remove(tmp1.First());
                        }

                        if (tmp2.Count > 0)
                        {
                            int rs = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tmp2.Count) / columns));
                            rows = Math.Min(rows, rs);

                            var table = this.getPageContents(tmp2);
                            table[0, 0] = caseNo + caseName;
                            page.Add(table);

                        }
                        else if (tmp1.Count <= 0)
                        {
                            break;
                        }
                        else
                        { // 印刷するものもない
                            mc.NewPage();
                        }

                        // 2ページ以降に入る行数
                        rows = printRows[1];
                    }
                }

            }

            // 表の印刷
            printManager.printTableContentsOnePage(mc, page, new string[] { this.title });
        }

    }


}
//target.Remove("name");

////LLのとき
//try
//{
//    var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
//    //LLの存在する配列番号を記録しておく
//    LL_list.Add(i);
//    //データを取得する
//    disgAnnex.dataTreat(mc, Elem, "LL");
//}

//基本形の時
//catch
//{
//    JArray Elem = JArray.FromObject(target.ElementAt(i).Value);
//    //データを取得する
//    disgBasic.DisgBasic(mc, Elem);
//}


//for (var i = 0; i < targetName.Count; i++)
//{
//    var key = dataManager.toString(targetName.ElementAt(i).Key);  // ケース番号
//    var val = JToken.FromObject(targetName.ElementAt(i).Value);

//    if (val.Type == JTokenType.Array)
//    {
//        JArray Dis = (JArray)val;
//        var _disgName = new List<disgName>();
//        for (int j = 0; j < Dis.Count; j++)
//        {
//            JToken item = Dis[j];

//            var ds = new disgName();

//            ds.n = dataManager.toString(item["Case"]);
//            ds.n = dataManager.toString(item["荷重名称"]);

//            _disgName.Add(ds);

//        }
//        this.disgNames.Add(key, _disgName);

//    }
//    else if (val.Type == JTokenType.Object)
//    {   // LL：連行荷重の時
//        var Dis = ((JObject)val).ToObject<Dictionary<string, object>>();
//        var _disgName = ResultdisgNameCombine.getdisgNameCombine(Dis);
//        this.disgNames.Add(key, _disgName);
//    }

//}



/*
        private Dictionary<string, object> value = new Dictionary<string, object>();
        public ResultDisgBasic disgBasic;
        public ResultDisgAnnexing disgAnnex;
        public List<string> title = new List<string>();
        public List<int> LL_list = new List<int>();


        /// <summary>
        /// 変位量データの読み取り（LLと基本形の分離）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="value_">全データが入っている</param>
        /// <param name="disgAnnex_">基本データ以外の変位量読み取り，書き込みを行う</param>
        public void init(PdfDoc mc, Dictionary<string, object> value_,ResultDisgAnnexing disgAnnex_)
        {
            value = value_;
            disgAnnex = disgAnnex_;
            disgBasic = new ResultDisgBasic();

            disgBasic.data = new List<List<string[]>>(); 
            disgAnnex.dataLL = new List<List<List<string[]>>>();
            title = new List<string>();

            //変位量データを取得する
            var target = JObject.FromObject(value["disg"]).ToObject<Dictionary<string, object>>();
            var targetName = JArray.FromObject(value["disgName"]);

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる
                var load = targetName[i];
                string[] loadNew = new String[2];

                loadNew[0] = load[0].ToString();
                loadNew[1] = load[1].ToString();

                title.Add(loadNew[0] + loadNew[1].PadLeft(loadNew[1].Length + 2));
                target.Remove("name");

                //LLのとき
                try
                {
                    var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                    //LLの存在する配列番号を記録しておく
                    LL_list.Add(i);
                    //データを取得する
                    disgAnnex.dataTreat(mc, Elem, "LL");
                }

                //基本形の時
                catch
                { 
                    JArray Elem = JArray.FromObject(target.ElementAt(i).Value);
                    //データを取得する
                    disgBasic.DisgBasic(mc, Elem);
                }
            }
        }

        /// <summary>
        /// 変位量データのPDF書き込み
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        public void DisgPDF(PdfDoc mc)
        { 
            int LL_count = 0;
            int LL_count2 = 0;

            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("変位量データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Displacement", 0);
                    break;
            }

            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            // 印刷
            for (int i = 0; i < title.Count; i++)
            {
                //LLの時
                if (LL_list.Contains(i))
                {
                    //  1ケースでページをまたぐかどうか
                    int count = 0;

                    for (int m = 0; m < disgAnnex.dataLL[LL_count].Count; m++)
                    {
                        count += disgAnnex.dataLL[LL_count][m].Count;
                    }

                    mc.TypeCount(i, 8, count, title[i]);

                    // タイトルの印刷 ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    disgAnnex.DisgAnnexingPDF(mc, "LL", title[i], LL_count);
                    LL_count++;
                }
                //基本形の時
                else
                {
                    //  1タイプ内でページをまたぐかどうか
                    mc.TypeCount(i, 6, disgBasic.data[LL_count2].Count, title[i]);

                    // タイプの印刷　ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    disgBasic.DisgBasicPDF(mc, LL_count2);
                    LL_count2++;
                }
            }
   
        }
    }
}

*/