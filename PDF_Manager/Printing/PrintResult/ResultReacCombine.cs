using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing;
using PDF_Manager.Printing.Comon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PDF_Manager.Printing
{
    public class ReacCombine
    {
        public List<Reac> tx_max = new List<Reac>();
        public List<Reac> tx_min = new List<Reac>();
        public List<Reac> ty_max = new List<Reac>();
        public List<Reac> ty_min = new List<Reac>();
        public List<Reac> tz_max = new List<Reac>();
        public List<Reac> tz_min = new List<Reac>();
        public List<Reac> mx_max = new List<Reac>();
        public List<Reac> mx_min = new List<Reac>();
        public List<Reac> my_max = new List<Reac>();
        public List<Reac> my_min = new List<Reac>();
        public List<Reac> mz_max = new List<Reac>();
        public List<Reac> mz_min = new List<Reac>();


        public void Add(string key, Reac value)
        {
            // key と同じ名前の変数を取得する
            Type type = this.GetType();
            FieldInfo field = type.GetField(key);
            if (field == null)
            {
                throw new Exception(String.Format("ReacCombineクラスの変数{0} に値{1}を登録しようとしてエラーが発生しました", key, value));
            }
            var val = (List<Reac>)field.GetValue(this);

            // 変数に値を追加する
            val.Add(value);

            // 変数を更新する
            field.SetValue(this, val);
        }

        public List<Reac> getValue3(int Index)
        {
            if (Index == 0)
            {
                return this.tx_max;
            }
            if (Index == 1)
            {
                return this.tx_min;
            }
            if (Index == 2)
            {
                return this.ty_max;
            }
            if (Index == 3)
            {
                return this.ty_min;
            }
            if (Index == 4)
            {
                return this.tz_max;
            }
            if (Index == 5)
            {
                return this.tz_min;
            }
            if (Index == 6)
            {
                return this.mx_max;
            }
            if (Index == 7)
            {
                return this.mx_min;
            }
            if (Index == 8)
            {
                return this.my_max;
            }
            if (Index == 9)
            {
                return this.my_min;
            }
            if (Index == 10)
            {
                return this.mz_max;
            }
            if (Index == 11)
            {
                return this.mz_min;
            }

            return null;

        }

        public List<Reac> getValue2(int Index)
        {
            if (Index == 0)
            {
                return this.tx_max;
            }
            if (Index == 1)
            {
                return this.tx_min;
            }
            if (Index == 2)
            {
                return this.ty_max;
            }
            if (Index == 3)
            {
                return this.ty_min;
            }
            if (Index == 4)
            {
                return this.mz_max;
            }
            if (Index == 5)
            {
                return this.mz_min;
            }
            return null;

        }
    }



    class ResultReacCombine
    {
        public const string KEY = "reacCombine";

        private Dictionary<string, ReacCombine> reacs = new Dictionary<string, ReacCombine>();
        private Dictionary<string, string> reacnames = new Dictionary<string, string>();


        public ResultReacCombine(Dictionary<string, object> value, string key = ResultReacCombine.KEY)
        {
            if (!value.ContainsKey(key))
                return;

            // データを取得する．
            var target = JObject.FromObject(value[key]).ToObject<Dictionary<string, object>>();


            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var No = dataManager.toString(target.ElementAt(i).Key);  // ケース番号
                var val = JToken.FromObject(target.ElementAt(i).Value);

                var Rec = ((JObject)val).ToObject<Dictionary<string, object>>();
                var _reac = ResultReacCombine.getReacCombine(Rec);

                this.reacs.Add(No, _reac);
            }

            // データを取得する．
            var targetName = JArray.FromObject(value["reacName"]);

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる
                var load = targetName[i];
                string[] loadNew = new string[2];

                loadNew[0] = load[0].ToString();
                loadNew[1] = load[1].ToString();

                reacnames.Add(loadNew[0], loadNew[1]);
            }

        }

        public static ReacCombine getReacCombine(Dictionary<string, object> Rec)
        {
            var _reac = new ReacCombine();

            for (int i = 0; i < Rec.Count; i++)
            {
                var elist = JObject.FromObject(Rec.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                var k = Rec.ElementAt(i).Key;

                for (int j = 0; j < elist.Count; j++)
                {
                    var item = JObject.FromObject(elist.ElementAt(j).Value);

                    var re = new Reac();

                    re.id = dataManager.toString(elist.ElementAt(j).Key);
                    re.tx = dataManager.parseDouble(item["tx"]);
                    re.ty = dataManager.parseDouble(item["ty"]);
                    re.tz = dataManager.parseDouble(item["tz"]);
                    re.mx = dataManager.parseDouble(item["mx"]);
                    re.my = dataManager.parseDouble(item["my"]);
                    re.mz = dataManager.parseDouble(item["mz"]);
                    re.caseStr = dataManager.toString(item["case"]);
                    re.comb = dataManager.toString(item["comb"]);

                    _reac.Add(k, re);
                }
            }
            return _reac;
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
                this.myTable = new Table(5, 9);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 15.0;//節点No
                this.myTable.ColWidth[1] = 60.0;//X方向の支点反力
                this.myTable.ColWidth[2] = 60.0;//Y方向の支点反力
                this.myTable.ColWidth[3] = 60.0;//Z方向の支点反力
                this.myTable.ColWidth[4] = 60.0;//X軸周りの回転反力
                this.myTable.ColWidth[5] = 60.0;//Y軸周りの回転反力
                this.myTable.ColWidth[6] = 60.0;//Z軸周りの回転反力
                this.myTable.ColWidth[7] = 10.0;//調整用
                this.myTable.ColWidth[8] = 80.0;//組合せ

                this.myTable.RowHeight[1] = printManager.LineSpacing2;
                this.myTable.RowHeight[2] = printManager.LineSpacing2;

                this.myTable.AlignX[0, 0] = "L";
                this.myTable.AlignX[1, 0] = "L";
                this.myTable.AlignX[2, 0] = "L";
                this.myTable.AlignX[2, 1] = "R";
                this.myTable.AlignX[2, 2] = "R";
                this.myTable.AlignX[2, 3] = "R";
                this.myTable.AlignX[2, 4] = "R";
                this.myTable.AlignX[2, 5] = "R";
                this.myTable.AlignX[2, 6] = "R";
                this.myTable.AlignX[2, 7] = "R";
                this.myTable.AlignX[3, 0] = "L";
                this.myTable.AlignX[3, 1] = "R";
                this.myTable.AlignX[3, 2] = "R";
                this.myTable.AlignX[3, 3] = "R";
                this.myTable.AlignX[3, 4] = "R";
                this.myTable.AlignX[3, 5] = "R";
                this.myTable.AlignX[3, 6] = "R";
                this.myTable.AlignX[3, 7] = "R";
                this.myTable.AlignX[4, 1] = "R";
                this.myTable.AlignX[4, 2] = "R";
                this.myTable.AlignX[4, 3] = "R";
                this.myTable.AlignX[4, 4] = "R";
                this.myTable.AlignX[4, 5] = "R";
                this.myTable.AlignX[4, 6] = "R";
                this.myTable.AlignX[4, 7] = "R";



                switch (data.language)
                {
                    default:
                        this.title = "Combine反力";
                        this.myTable[2, 0] = "節点";
                        this.myTable[3, 0] = "No";
                        this.myTable[2, 1] = "X方向の";
                        this.myTable[3, 1] = "支点反力";
                        this.myTable[4, 1] = "(kN)";
                        this.myTable[2, 2] = "Y方向の";
                        this.myTable[3, 2] = "支点反力";
                        this.myTable[4, 2] = "(kN)";
                        this.myTable[2, 3] = "Z方向の";
                        this.myTable[3, 3] = "支点反力";
                        this.myTable[4, 3] = "(kN)";
                        this.myTable[2, 4] = "X軸周りの";
                        this.myTable[3, 4] = "回転反力";
                        this.myTable[4, 4] = "(kN・m)";
                        this.myTable[2, 5] = "Y軸周りの";
                        this.myTable[3, 5] = "回転反力";
                        this.myTable[4, 5] = "(kN・m)";
                        this.myTable[2, 6] = "Z軸周りの";
                        this.myTable[3, 6] = "回転反力";
                        this.myTable[4, 6] = "(kN・m)";
                        this.myTable[2, 8] = "組合せ";
                        break;
                }

                //表題の文字位置
            }
            else
            {//2次元

                ///テーブルの作成
                this.myTable = new Table(5, 9);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 15.0;//節点No
                this.myTable.ColWidth[1] = 60.0;//X方向の支点反力
                this.myTable.ColWidth[2] = 60.0;//Y方向の支点反力
                this.myTable.ColWidth[3] = 60.0;//回転反力
                this.myTable.ColWidth[4] = 10.0;//調整用
                this.myTable.ColWidth[5] = 80.0;//組合せ

                this.myTable.RowHeight[1] = printManager.LineSpacing2;
                this.myTable.RowHeight[2] = printManager.LineSpacing2;

                this.myTable.AlignX[0, 0] = "L";
                this.myTable.AlignX[1, 0] = "L";
                this.myTable.AlignX[2, 0] = "L";
                this.myTable.AlignX[2, 1] = "R";
                this.myTable.AlignX[2, 2] = "R";
                this.myTable.AlignX[2, 3] = "R";
                //this.myTable.AlignX[2, 4] = "R";
                this.myTable.AlignX[3, 0] = "L";
                this.myTable.AlignX[3, 1] = "R";
                this.myTable.AlignX[3, 2] = "R";
                this.myTable.AlignX[3, 3] = "R";
                this.myTable.AlignX[3, 4] = "R";
                this.myTable.AlignX[4, 1] = "R";
                this.myTable.AlignX[4, 2] = "R";
                this.myTable.AlignX[4, 3] = "R";
                this.myTable.AlignX[4, 4] = "R";



                switch (data.language)
                {
                    default:
                        this.title = "Combine反力";
                        this.myTable[2, 0] = "節点";
                        this.myTable[3, 0] = "No";
                        this.myTable[2, 1] = "X方向の";
                        this.myTable[3, 1] = "支点反力";
                        this.myTable[4, 1] = "(mm)";
                        this.myTable[2, 2] = "Y方向の";
                        this.myTable[3, 2] = "支点反力";
                        this.myTable[4, 2] = "(mm)";
                        this.myTable[2, 3] = "回転";
                        this.myTable[3, 3] = "拘束力";
                        this.myTable[4, 3] = "(mmrad)";
                        this.myTable[2, 5] = "組合せ";
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
        private Table getPageContents(List<Reac> target)
        {
            int r = this.myTable.Rows;

            int columns = 2;
            int count = this.myTable.Columns;
            int c = count / columns;

            int rows = target.Count;

            // 行コンテンツを生成
            var table = this.myTable.Clone();
            table.ReDim(row: r + rows * 2);

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
                    table[r, j] = printManager.toString(item.tx, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.ty, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.tz, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.mx, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.my, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.mz, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    j++;
                    if (item.caseStr != null)
                    {
                        int len = item.caseStr.Length;
                        var str = item.caseStr;

                        if (len > 25)
                        {
                            foreach (var n in str.SubstringAtCount(24))
                            {
                                table[r, j] = printManager.toString(n, 4);
                                table.AlignX[r, j] = "L";
                                r++;
                            }
                        }
                        else
                        {
                            table[r, j] = printManager.toString(item.caseStr, 4);
                            table.AlignX[r, j] = "L";
                            r++;
                        }
                    }
                }
            }

            else　　//２次元
            {
                int Rows = target.Count / columns;

                for (var i = 0; i < rows; i++)
                {
                    var item = target[i];

                    int j = 0;
                    table[r, j] = printManager.toString(item.id);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.tx, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.ty, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(item.mz, 4);
                    table.AlignX[r, j] = "R";
                    j++;
                    j++;
                    if (item.caseStr != null)
                    {
                        int len = item.caseStr.Length;
                        var str = item.caseStr;

                        if (len > 50)
                        {
                            foreach (var n in str.SubstringAtCount(50))
                            {
                                table[r, j] = printManager.toString(n, 4);
                                table.AlignX[r, j] = "L";
                                r++;
                            }
                        }
                        else
                        {
                            table[r, j] = printManager.toString(item.caseStr, 4);
                            table.AlignX[r, j] = "L";
                            r++;
                        }
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

            if (this.reacs.Count == 0)
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
            if (dimension == 3)  //３次元
            {
                for (int j = 0; j < this.reacs.Count; ++j)
                {   // ケース番号のループ
                    var key = this.reacs.ElementAt(j).Key;  // ケース番号
                    var value = this.reacs.ElementAt(j).Value;

                    var caseNo = this.reacnames.ElementAt(j).Key;
                    var caseName = this.reacnames.ElementAt(j).Value;

                    var ValueKey = new List<string>();
                    ValueKey.Add("X方向の支点反力　最大");
                    ValueKey.Add("X方向の支点反力　最小");
                    ValueKey.Add("Y方向の支点反力　最大");
                    ValueKey.Add("Y方向の支点反力　最小");
                    ValueKey.Add("Z方向の支点反力　最大");
                    ValueKey.Add("Z方向の支点反力　最小");
                    ValueKey.Add("X軸周りの回転反力　最大");
                    ValueKey.Add("X軸周りの回転反力　最小");
                    ValueKey.Add("Y軸周りの回転反力　最大");
                    ValueKey.Add("Y軸周りの回転反力　最小");
                    ValueKey.Add("Z軸周りの回転反力　最大");
                    ValueKey.Add("Z軸周りの回転反力　最小");

                    for (int k = 0; k < 12; ++k)
                    {
                        var tmp1 = value.getValue3(k);

                        while (true)
                        {
                            if (tmp1.Count <= 0)
                                break;

                            if (tmp1[0].caseStr.Length > 24)
                            {
                                rows = rows / 2;
                            }

                            // 1ページに納まる分のデータをコピー
                            var tmp2 = new List<Reac>();
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
                                table[1, 0] = ValueKey[k];
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
            }
            else　　//２次元
            {
                for (int j = 0; j < this.reacs.Count; ++j)
                {   // ケース番号のループ
                    var key = this.reacs.ElementAt(j).Key;  // ケース番号
                    var value = this.reacs.ElementAt(j).Value;

                    var caseNo = this.reacnames.ElementAt(j).Key;
                    var caseName = this.reacnames.ElementAt(j).Value;

                    var ValueKey = new List<string>();
                    ValueKey.Add("X方向の移動反力　最大");
                    ValueKey.Add("X方向の移動反力　最小");
                    ValueKey.Add("Y方向の移動反力　最大");
                    ValueKey.Add("Y方向の移動反力　最小");
                    ValueKey.Add("Z軸周りの回転反力　最大");
                    ValueKey.Add("Z軸周りの回転反力　最小");

                    for (int k = 0; k < 6; ++k)
                    {
                        var tmp1 = value.getValue2(k);

                        while (true)
                        {
                            if (tmp1.Count <= 0)
                                break;

                            if (tmp1[0].caseStr.Length > 24)
                            {
                                rows = rows / 2;
                            }

                            // 1ページに納まる分のデータをコピー
                            var tmp2 = new List<Reac>();
                            for (int i = 0; i < rows; i++)
                            {
                                if (tmp1.Count <= 0)
                                    break;
                                //tmp2.Add(tmp1.First());
                                tmp2.Add(tmp1.First());
                                tmp1.Remove(tmp1.First());
                            }

                            if (tmp2.Count > 0)
                            {
                                var table = this.getPageContents(tmp2);
                                table[0, 0] = caseNo + caseName;
                                table[1, 0] = ValueKey[k];
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

            }

            // 表の印刷
            printManager.printTableContentsOnePage(mc, page, new string[] { this.title });

        }
    }
}
