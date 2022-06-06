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
    }


    internal class ResultFsec
    {
        public const string KEY = "fsec";

        private Dictionary<string, object> fsecs = new Dictionary<string, object>();
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
                this.myTable.ColWidth[1] = 15.0;//節点No
                this.myTable.ColWidth[2] = 15.0;//着目位置
                this.myTable.ColWidth[3] = 80.0;//軸方向力
                this.myTable.ColWidth[4] = 80.0;//Y方向のせん断力
                this.myTable.ColWidth[5] = 80.0;//Z方向のせん断力
                this.myTable.ColWidth[6] = 80.0;//ねじりﾓｰﾒﾝﾄ
                this.myTable.ColWidth[7] = 80.0;//Y軸周りの曲げモーメント
                this.myTable.ColWidth[8] = 80.0;//Z軸周りの曲げモーメント

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
                this.myTable = new Table(4, 6);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 20.0;//要素No
                this.myTable.ColWidth[1] = 20.0;//節点No
                this.myTable.ColWidth[2] = 20.0;//着目位置
                this.myTable.ColWidth[3] = 72.5;//X軸周りの回転量
                this.myTable.ColWidth[4] = 72.5;
                this.myTable.ColWidth[5] = 72.5;

                this.myTable.RowHeight[1] = printManager.LineSpacing2;

                this.myTable.AlignX[0, 0] = "L";
                this.myTable.AlignX[1, 0] = "R";
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
                this.myTable.AlignX[3, 1] = "R";
                this.myTable.AlignX[3, 2] = "R";
                this.myTable.AlignX[3, 3] = "R";
                this.myTable.AlignX[3, 4] = "R";
                this.myTable.AlignX[3, 5] = "R";

                switch (data.language)
                {
                    default:

                        this.title = "断面力データ";
                        this.myTable[0, 0] = "部材";
                        this.myTable[1, 0] = "No";
                        this.myTable[0, 1] = "節点";
                        this.myTable[1, 1] = "No";
                        this.myTable[0, 2] = "着目位置";
                        this.myTable[1, 2] = "(m)";
                        this.myTable[0, 3] = "軸方向力";
                        this.myTable[1, 3] = "(kN)";
                        this.myTable[0, 4] = "せん断力";
                        this.myTable[1, 4] = "(kN)";
                        this.myTable[0, 5] = "曲げﾓｰﾒﾝﾄ";
                        this.myTable[1, 5] = "(kN・m)";


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

            table.RowHeight[r] = printManager.LineSpacing2;

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

            // 1ページ目に入る行数
            int rows = printRows[0];

            // 集計開始
            for (int j = 0; j < this.fsecs.Count; ++j)
                {   // ケース番号のループ
                    var key = this.fsecs.ElementAt(j).Key;  // ケース番号
                    var tmp1 = new List<Fsec>((List<Fsec>)this.fsecs.ElementAt(j).Value);

                    var caseNo = this.fsecnames.ElementAt(j).Key;
                    var caseName = this.fsecnames.ElementAt(j).Value;

                    while (true)
                    {
                        // 1ページに納まる分のデータをコピー
                        var tmp2 = new List<Fsec>();
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
            

            // 表の印刷
            printManager.printTableContents(mc, page, new string[] { this.title });
        }


    }


}

/*
        private Dictionary<string, object> value = new Dictionary<string, object>();
        public ResultFsecBasic fsecBasic;
        public ResultFsecAnnexing fsecAnnex;
        public List<string> title = new List<string>();
        public List<int> LL_list = new List<int>();

        /// <summary>
        /// 断面力データの読み取り（LLと基本形の分離）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="value_">全データが入っている</param>
        /// <param name="disgAnnex_">基本データ以外の断面力読み取り，書き込みを行う</param>
        public void init(PdfDoc mc, Dictionary<string, object> value_, ResultFsecAnnexing fsecAnnex_)
        {
            value = value_;
            fsecAnnex = fsecAnnex_;
            fsecBasic = new ResultFsecBasic();

            fsecBasic.data = new List<List<string[]>>();
            fsecAnnex.dataLL = new List<List<List<string[]>>>();
            title = new List<string>();

            //断面力データを取得する
            var target = JObject.FromObject(value["fsec"]).ToObject<Dictionary<string, object>>();
            var targetName = JArray.FromObject(value["fsecName"]);

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる．
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
                    fsecAnnex.dataTreat(mc, Elem, "LL");
                }

                //基本形の時
                catch
                {
                    JArray Elem = JArray.FromObject(target.ElementAt(i).Value);
                    //データを取得する
                    fsecBasic.FsecBasic(mc, Elem);
                }
            }

        }

        /// <summary>
        /// 断面力データのPDF書き込み
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        public void FsecPDF(PdfDoc mc)
        {
            int LL_count = 0;
            int LL_count2 = 0;

            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("断面力データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Internal Member Forces and Momemts", 0);
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

                    for (int m = 0; m < fsecAnnex.dataLL[LL_count].Count; m++)
                    {
                        count += fsecAnnex.dataLL[LL_count][m].Count;
                    }

                    mc.TypeCount(i, 8, count, title[i]);

                    // タイトルの印刷 ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    fsecAnnex.FsecAnnexingPDF(mc, "LL", title[i], LL_count);
                    LL_count++;
                }
                //基本形の時
                else
                {
                    //  1タイプ内でページをまたぐかどうか
                    mc.TypeCount(i, 6, fsecBasic.data[LL_count2].Count, title[i]);

                    // タイプの印刷　ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    fsecBasic.FsecBasicPDF(mc, LL_count2);
                    LL_count2++;
                }
            }


            //// 全行の取得
            ////int count = 2;
            //for (int i = 0; i < title.Count; i++)
            //{
            //    count += (data[i].Count + 5) * mc.single_Yrow + 1;
            //}
            //// 改ページ判定
            //mc.DataCountKeep(count,"fsec");

            ////　ヘッダー
            //string[,] header_content = {
            //    { "部材", "節点","", "FX", "FY", "FZ", "MX", "MY","MZ" },
            //    { "No", "No", "DIST", "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)" },
            //};
            //// ヘッダーのx方向の余白
            //int[,] header_Xspacing = {
            //    { 10, 50, 105, 160, 210, 260, 310,360,410 },
            //    { 10, 50, 105, 160, 210, 260, 310,360,410 },
            //};

            //// ボディーのx方向の余白　-1
            //int[,] body_Xspacing = {
            //    { 17, 57, 118, 174, 224, 274, 324,374,424 }
            //};

            //// タイトルの印刷
            //mc.PrintContent("断面力", 0);
            //mc.CurrentRow(2);

            // 印刷
            //mc.PrintResultBasic(title, data, header_content, header_Xspacing, body_Xspacing);
        }
    }
}

*/