using Newtonsoft.Json.Linq;
using PDF_Manager.Printing;
using PDF_Manager.Printing.Comon;
using PDF_Manager.Comon;
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
using System.Globalization;

namespace PDF_Manager.Printing
{

    public class Fsec
    {
        public string m;    // 要素番号
        public string n;    // 節点番号
        public double l;    // 着目点距離
        public double fx;
        public double fy;
        public double fz;
        public double mx;
        public double my;
        public double mz;

        // 組み合わせで使う
        public string caseStr = null;
        public string comb = null;

        // インデックスから値を返す関数（3次元）
        public double getValue3D(int Index)
        {
            if (Index == 0)
                return this.fx;
            if (Index == 1)
                return this.fy;
            if (Index == 2)
                return this.fz;
            if (Index == 3)
                return this.mx;
            if (Index == 4)
                return this.my;
            if (Index == 5)
                return this.mz;
            return double.NaN;
        }

        // インデックスから値を返す関数（2次元）
        public double getValue2D(int Index)
        {
            if (Index == 0)
                return this.fx;
            if (Index == 1)
                return this.fy;
            if (Index == 2)
                return this.mz;

            return double.NaN;
        }
        public double getValue2D(string key)
        {
            if (key == "fx")
                return this.fx;
            if (key == "fy")
                return this.fy;
            if (key == "mz")
                return this.mz;

            return double.NaN;
        }
    }


    internal class ResultFsec
    {
        public const string KEY = "fsec";

        public Dictionary<string, object> fsecs = new Dictionary<string, object>();
        private Dictionary<string, string> fsecnames = new Dictionary<string, string>();


        public ResultFsec(Dictionary<string, object> value)
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

                if (val.Type == JTokenType.Array)
                {
                    JArray Fsec = (JArray)val;
                    var _fsec = new List<Fsec>();
                    for (int j = 0; j < Fsec.Count; j++)
                    {
                        JToken item = Fsec[j];

                        var fs = new Fsec();

                        fs.n = dataManager.toString(item["n"]);
                        fs.m = dataManager.toString(item["m"]);
                        fs.l = dataManager.parseDouble(item["l"]);
                        fs.fx = dataManager.parseDouble(item["fx"]);
                        fs.fy = dataManager.parseDouble(item["fy"]);
                        fs.fz = dataManager.parseDouble(item["fz"]);
                        fs.mx = dataManager.parseDouble(item["mx"]);
                        fs.my = dataManager.parseDouble(item["my"]);
                        fs.mz = dataManager.parseDouble(item["mz"]);

                        _fsec.Add(fs);

                    }
                    this.fsecs.Add(key, _fsec);

                }
                else if (val.Type == JTokenType.Object)
                {   // LL：連行荷重の時
                    var Fsec = ((JObject)val).ToObject<Dictionary<string, object>>();
                    var _fsec = ResultFsecCombine.getFsecCombine(Fsec);
                    this.fsecs.Add(key, _fsec);
                }

            }
            if (!value.ContainsKey("fsecName"))
                return;

            // データを取得する．
            var targetName = JArray.FromObject(value["fsecName"]);

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる
                var load = targetName[i];
                string[] loadNew = new string[2];

                loadNew[0] = load[0].ToString();
                loadNew[1] = load[1].ToString();

                fsecnames.Add(loadNew[0], loadNew[1]);

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
                this.myTable = new Table(4, 9);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 15.0;//要素No
                this.myTable.ColWidth[1] = 40.0;//節点No
                this.myTable.ColWidth[2] = 60.0;//着目位置
                this.myTable.ColWidth[3] = 65.0;//軸方向力
                this.myTable.ColWidth[4] = 65.0;//Y方向のせん断力
                this.myTable.ColWidth[5] = 65.0;//Z方向のせん断力
                this.myTable.ColWidth[6] = 65.0;//ねじりﾓｰﾒﾝﾄ
                this.myTable.ColWidth[7] = 65.0;//Y軸周りの曲げモーメント
                this.myTable.ColWidth[8] = 65.0;//Z軸周りの曲げモーメント

                this.myTable.RowHeight[1] = printManager.LineSpacing2;

                this.myTable.AlignX[0, 0] = "L";
                this.myTable.AlignX[1, 0] = "L";
                this.myTable.AlignX[1, 1] = "R";
                this.myTable.AlignX[1, 2] = "R";
                this.myTable.AlignX[1, 3] = "R";
                this.myTable.AlignX[1, 4] = "R";
                this.myTable.AlignX[1, 5] = "R";
                this.myTable.AlignX[1, 6] = "R";
                this.myTable.AlignX[1, 7] = "R";
                this.myTable.AlignX[1, 8] = "R";
                this.myTable.AlignX[2, 0] = "R";
                this.myTable.AlignX[2, 1] = "R";
                this.myTable.AlignX[2, 2] = "R";
                this.myTable.AlignX[2, 3] = "R";
                this.myTable.AlignX[2, 4] = "R";
                this.myTable.AlignX[2, 5] = "R";
                this.myTable.AlignX[2, 6] = "R";
                this.myTable.AlignX[2, 7] = "R";
                this.myTable.AlignX[2, 8] = "R";
                this.myTable.AlignX[3, 1] = "R";
                this.myTable.AlignX[3, 2] = "R";
                this.myTable.AlignX[3, 3] = "R";
                this.myTable.AlignX[3, 4] = "R";
                this.myTable.AlignX[3, 5] = "R";
                this.myTable.AlignX[3, 6] = "R";
                this.myTable.AlignX[3, 7] = "R";
                this.myTable.AlignX[3, 8] = "R";




                switch (data.language)
                {
                    default:
                        this.title = "断面力データ";
                        this.myTable[1, 0] = "部材";
                        this.myTable[2, 0] = "No";
                        this.myTable[1, 1] = "節点";
                        this.myTable[2, 1] = "No";
                        this.myTable[1, 2] = "着目";
                        this.myTable[2, 2] = "位置";
                        this.myTable[3, 2] = "(m)";
                        this.myTable[2, 3] = "軸方向力";
                        this.myTable[3, 3] = "(kN)";
                        this.myTable[1, 4] = "Y軸方向の";
                        this.myTable[2, 4] = "せん断力";
                        this.myTable[3, 4] = "(kN)";
                        this.myTable[1, 5] = "Z軸方向の";
                        this.myTable[2, 5] = "せん断力";
                        this.myTable[3, 5] = "(kN)";
                        this.myTable[1, 6] = "X軸周りの";
                        this.myTable[2, 6] = "ﾓｰﾒﾝﾄ";
                        this.myTable[3, 6] = "(kN・m)";
                        this.myTable[1, 7] = "Y軸周りの";
                        this.myTable[2, 7] = "曲げﾓｰﾒﾝﾄ";
                        this.myTable[3, 7] = "(kN・m)";
                        this.myTable[1, 8] = "Z軸周りの";
                        this.myTable[2, 8] = "曲げﾓｰﾒﾝﾄ";
                        this.myTable[3, 8] = "(kN・m)";
                        break;
                }

                //表題の文字位置
            }
            else
            {//2次元

                ///テーブルの作成
                this.myTable = new Table(3, 6);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 15.0;//要素No
                this.myTable.ColWidth[1] = 40.0;//節点No
                this.myTable.ColWidth[2] = 65.0;//着目位置
                this.myTable.ColWidth[3] = 72.5;//X軸周りの回転量
                this.myTable.ColWidth[4] = 72.5;
                this.myTable.ColWidth[5] = 72.5;

                this.myTable.RowHeight[1] = printManager.LineSpacing2;

                this.myTable.AlignX[0, 0] = "L";
                this.myTable.AlignX[1, 0] = "L";
                this.myTable.AlignX[1, 1] = "R";
                this.myTable.AlignX[1, 2] = "R";
                this.myTable.AlignX[1, 3] = "R";
                this.myTable.AlignX[1, 4] = "R";
                this.myTable.AlignX[1, 5] = "R";
                this.myTable.AlignX[2, 0] = "R";
                this.myTable.AlignX[2, 1] = "R";
                this.myTable.AlignX[2, 2] = "R";
                this.myTable.AlignX[2, 3] = "R";
                this.myTable.AlignX[2, 4] = "R";
                this.myTable.AlignX[2, 5] = "R";

                switch (data.language)
                {
                    default:

                        this.title = "断面力データ";
                        this.myTable[1, 0] = "部材";
                        this.myTable[2, 0] = "No";
                        this.myTable[1, 1] = "節点";
                        this.myTable[2, 1] = "No";
                        this.myTable[1, 2] = "着目位置";
                        this.myTable[2, 2] = "(m)";
                        this.myTable[1, 3] = "軸方向力";
                        this.myTable[2, 3] = "(kN)";
                        this.myTable[1, 4] = "せん断力";
                        this.myTable[2, 4] = "(kN)";
                        this.myTable[1, 5] = "曲げﾓｰﾒﾝﾄ";
                        this.myTable[2, 5] = "(kN・m)";


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
        private Table getPageContents(List<Fsec> target)
        {
            int r = this.myTable.Rows;

            int columns = 2;
            int count = this.myTable.Columns;
            int c = count / columns;

            int rows = target.Count;


            // 行コンテンツを生成
            var table = this.myTable.Clone();
            table.ReDim(row: r + rows);

            //table.RowHeight[r] = printManager.LineSpacing2;

            if (dimension == 3)　　//３次元
            {
                for (var i = 0; i < rows; i++)
                {
                    var item = target[i];

                    int j = 0;
                    table[r, j] = printManager.toString(item.m);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.n);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.l, 3);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.fx, 2);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.fy, 2);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.fz, 2);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.mx, 2);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.my, 2);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.mz, 2);
                    table.AlignX[r, j] = "R";
                    j++;

                    var x = item.m;

                    if (x != "") 
                    {
                        table.RowHeight[r] = printManager.LineSpacing2;
                    }
                    else if (i == 0)
                    {
                        table.RowHeight[r] = printManager.LineSpacing2;
                    }


                    r++;
                }
            }

            else　　//２次元
            {
                int Rows = target.Count / columns;

                for (var i = 0; i < rows; i++)
                {
                    var item = target[i];

                    int j = 0;
                    table[r, j] = printManager.toString(item.m);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.n);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.l, 3);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.fx, 2);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.fy, 2);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.mz, 2);
                    table.AlignX[r, j] = "R";
                    j++;

                    var x = item.m;

                    if (x != "")
                    {
                        table.RowHeight[r] = printManager.LineSpacing2;
                    }
                    else if (i == 0)
                    {
                        table.RowHeight[r] = printManager.LineSpacing2;
                    }

                    r++;

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
            if (this.fsecs.Count == 0)
                return;

            // タイトル などの初期化
            this.printInit(mc, data);

            // 印刷可能な行数
            var printRows = myTable.getPrintRowCount(mc);

            // 行コンテンツを生成
            var page = new List<Table>();

            // 行の高さと隙間の比率（後の修正行数算定の係数）
            double h1 = printManager.LineSpacing2 - printManager.FontHeight;
            double h2 = printManager.FontHeight;
            double CONST = h1 / h2;



            // 1ページ目に入る行数
            int rows = printRows[0];


            // 集計開始
            for (int j = 0; j < this.fsecs.Count; ++j){   // ケース番号のループ
                var key = this.fsecs.ElementAt(j).Key;  // ケース番号
                var tmp1 = new List<Fsec>((List<Fsec>)this.fsecs.ElementAt(j).Value);

                var caseNo = this.fsecnames.ElementAt(j).Key;
                var caseName = this.fsecnames.ElementAt(j).Value;

                var tmp3 = new List<Fsec>();

                while (true)
                {
                    // 1ページに納まる分のデータをコピー
                    var tmp2 = new List<Fsec>();
                    var lost = rows;
                    var RowRevise = 0;

                    // 行の高さと隙間の比率（後の修正行数算定の係数）

                    for (int i = 0; i < rows; i++)
                    {
                        if (tmp1.Count <= 0)
                            break;

                        if (tmp1.First().m != "")
                        {
                            while (true)
                            {
                                tmp3.Add(tmp1.First());
                                tmp1.Remove(tmp1.First());

                                if (tmp1.Count == 0)
                                    break;

                                if (tmp1.First().m != "")
                                    break;
                            }

                        }
                        
                        // その部材が次のページに入らなくて  かつ 現在のページにも入りきらない場合
                        if (tmp3.Count > printRows[1] - CONST && tmp3.Count > lost)
                        {   // 現在のページの続きから印刷していく
                            while(tmp3.Count != 0)
                            {
                                for (int l = 0; l < rows; l++)
                                {
                                    tmp2.Add(tmp3.First());
                                    tmp3.Remove(tmp3.First());
                                    if (tmp3.Count == 0)
                                        break;
                                }
                                if (tmp3.Count == 0)
                                    break;
                                var table = this.getPageContents(tmp2);
                                table[0, 0] = caseNo + caseName;
                                page.Add(table);
                                rows = printRows[1];

                                tmp2.Clear();
                            }
                            var add = tmp2.Count;
                            lost -= add;

                        }
                        else
                        { 
                            var add = tmp3.Count;
                            lost =lost - add;

                            if (RowRevise % 2 == 1)
                            {
                                if (CONST >= 0.5)
                                {
                                    CONST = 1;
                                    lost = lost - (int)CONST;
                                    RowRevise = 0;
                                }
                                else
                                    RowRevise = 0;
                            }

                            if (add > lost)
                                break;  // その部材が現在のページにも入りきらない場合 

                            tmp2.AddRange(tmp3);
                            tmp3.Clear();

                            RowRevise++;

                        }

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
                    //else
                    //{ // 印刷するものもない
                    //    mc.NewPage();
                    //}

                    // 2ページ以降に入る行数
                    rows = printRows[1];

                }
            }
            

            // 表の印刷
            printManager.printTableContentsOnePage(mc, page, new string[] { this.title });
        }


    }


}

