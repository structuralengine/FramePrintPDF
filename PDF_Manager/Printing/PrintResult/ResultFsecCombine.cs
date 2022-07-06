using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing;
using PDF_Manager.Printing.Comon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PDF_Manager.Printing
{
    public class FsecCombine
    {
        public List<Fsec> fx_max = new List<Fsec>();
        public List<Fsec> fx_min = new List<Fsec>();
        public List<Fsec> fy_max = new List<Fsec>();
        public List<Fsec> fy_min = new List<Fsec>();
        public List<Fsec> fz_max = new List<Fsec>();
        public List<Fsec> fz_min = new List<Fsec>();
        public List<Fsec> mx_max = new List<Fsec>();
        public List<Fsec> mx_min = new List<Fsec>();
        public List<Fsec> my_max = new List<Fsec>();
        public List<Fsec> my_min = new List<Fsec>();
        public List<Fsec> mz_max = new List<Fsec>();
        public List<Fsec> mz_min = new List<Fsec>();

        public void Add(string key, Fsec value)
        {
            // key と同じ名前の変数を取得する
            Type type = this.GetType();
            FieldInfo field = type.GetField(key);
            if (field == null)
            {
                throw new Exception(String.Format("FsecCombineクラスの変数{0} に値{1}を登録しようとしてエラーが発生しました", key, value));
            }
            var val = (List<Fsec>)field.GetValue(this);

            // 変数に値を追加する
            val.Add(value);

            // 変数を更新する
            field.SetValue(this, val);
        }

        public List<Fsec> getValue3(int Index)
        {
            if (Index == 0)
            {
                return this.fx_max;
            }
            if (Index == 1)
            {
                return this.fx_min;
            }
            if (Index == 2)
            {
                return this.fy_max;
            }
            if (Index == 3)
            {
                return this.fy_min;
            }
            if (Index == 4)
            {
                return this.fz_max;
            }
            if (Index == 5)
            {
                return this.fz_min;
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

        public List<Fsec> getValue2(int Index)
        {
            if (Index == 0)
            {
                return this.fx_max;
            }
            if (Index == 1)
            {
                return this.fx_min;
            }
            if (Index == 2)
            {
                return this.fy_max;
            }
            if (Index == 3)
            {
                return this.fy_min;
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

    class ResultFsecCombine
    {
        public const string KEY = "fsecCombine";

        private Dictionary<string, FsecCombine> Fsecs = new Dictionary<string, FsecCombine>();
        private Dictionary<string, string> fsecnames = new Dictionary<string, string>();


        public ResultFsecCombine(Dictionary<string, object> value, string key = ResultFsecCombine.KEY)
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

                var Fsc = ((JObject)val).ToObject<Dictionary<string, object>>();
                var _Fsec = ResultFsecCombine.getFsecCombine(Fsc);

                this.Fsecs.Add(No, _Fsec);
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

        public static FsecCombine getFsecCombine(Dictionary<string, object> Fsc)
        {
            var _Fsec = new FsecCombine();

            for (int i = 0; i < Fsc.Count; i++)
            {
                JArray elist = JArray.FromObject(Fsc.ElementAt(i).Value);
                var k = Fsc.ElementAt(i).Key; // fx_min fx_max...

                for (int j = 0; j < elist.Count; j++)
                {
                    var item = elist[j];
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
                    fs.caseStr = dataManager.toString(item["case"]);
                    fs.comb = dataManager.toString(item["comb"]);

                    _Fsec.Add(k, fs);
                }


            }
            return _Fsec;
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
                this.myTable = new Table(5, 11);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 15.0;//節点No
                this.myTable.ColWidth[1] = 30.0;//部材No
                this.myTable.ColWidth[2] = 40.0;//着目位置
                this.myTable.ColWidth[3] = 55.0;//軸方向力
                this.myTable.ColWidth[4] = 55.0;//Y方向のせん断力
                this.myTable.ColWidth[5] = 55.0;//Z方向のせん断力
                this.myTable.ColWidth[6] = 55.0;//ねじりモーメント
                this.myTable.ColWidth[7] = 55.0;//Y軸周りの曲げモーメント
                this.myTable.ColWidth[8] = 55.0;//Z軸周りの曲げモーメント
                this.myTable.ColWidth[9] = 10.0;//調整
                this.myTable.ColWidth[10] = 55.0;//組合せ


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
                this.myTable.AlignX[2, 8] = "R";
                this.myTable.AlignX[3, 0] = "R";
                this.myTable.AlignX[3, 1] = "R";
                this.myTable.AlignX[3, 2] = "R";
                this.myTable.AlignX[3, 3] = "R";
                this.myTable.AlignX[3, 4] = "R";
                this.myTable.AlignX[3, 5] = "R";
                this.myTable.AlignX[3, 6] = "R";
                this.myTable.AlignX[3, 7] = "R";
                this.myTable.AlignX[3, 8] = "R";
                this.myTable.AlignX[4, 1] = "R";
                this.myTable.AlignX[4, 2] = "R";
                this.myTable.AlignX[4, 3] = "R";
                this.myTable.AlignX[4, 4] = "R";
                this.myTable.AlignX[4, 5] = "R";
                this.myTable.AlignX[4, 6] = "R";
                this.myTable.AlignX[4, 7] = "R";
                this.myTable.AlignX[4, 8] = "R";



                switch (data.language)
                {
                    default:
                        this.title = "Combine断面力";
                        this.myTable[2, 0] = "部材";
                        this.myTable[3, 0] = "No";
                        this.myTable[2, 1] = "節点";
                        this.myTable[3, 1] = "No";
                        this.myTable[2, 2] = "着目";
                        this.myTable[3, 2] = "位置";
                        this.myTable[4, 2] = "(m)";
                        this.myTable[3, 3] = "軸方向力";
                        this.myTable[4, 3] = "(kN)";
                        this.myTable[2, 4] = "Y軸方向の";
                        this.myTable[3, 4] = "せん断力";
                        this.myTable[4, 4] = "(kN)";
                        this.myTable[2, 5] = "Z軸方向の";
                        this.myTable[3, 5] = "せん断力";
                        this.myTable[4, 5] = "(kN)";
                        this.myTable[2, 6] = "X軸周りの";
                        this.myTable[3, 6] = "ﾓｰﾒﾝﾄ";
                        this.myTable[4, 6] = "(kN・m)";
                        this.myTable[2, 7] = "Y軸周りの";
                        this.myTable[3, 7] = "曲げﾓｰﾒﾝﾄ";
                        this.myTable[4, 7] = "(kN・m)";
                        this.myTable[2, 8] = "Z軸周りの";
                        this.myTable[3, 8] = "曲げﾓｰﾒﾝﾄ";
                        this.myTable[4, 8] = "(kN・m)";
                        this.myTable[2, 10] = "組み合わせ";
                        break;
                }

                //表題の文字位置
            }
            else
            {//2次元

                ///テーブルの作成
                this.myTable = new Table(4, 7);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 15.0;//部材No
                this.myTable.ColWidth[1] = 30.0;//節点No
                this.myTable.ColWidth[2] = 60.0;//着目位置
                this.myTable.ColWidth[3] = 60.0;//軸方向力
                this.myTable.ColWidth[4] = 60.0;//せん断力
                this.myTable.ColWidth[5] = 60.0;//曲げモーメント
                this.myTable.ColWidth[6] = 80.0;//組合せ

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
                this.myTable.AlignX[3, 0] = "R";
                this.myTable.AlignX[3, 1] = "R";
                this.myTable.AlignX[3, 2] = "R";
                this.myTable.AlignX[3, 3] = "R";
                this.myTable.AlignX[3, 4] = "R";
                this.myTable.AlignX[3, 5] = "R";



                switch (data.language)
                {
                    default:
                        this.title = "Combine断面力";
                        this.myTable[2, 0] = "部材";
                        this.myTable[3, 0] = "No";
                        this.myTable[2, 1] = "節点";
                        this.myTable[3, 1] = "No";
                        this.myTable[2, 2] = "着目位置";
                        this.myTable[3, 2] = "(m)";
                        this.myTable[2, 3] = "軸方向力";
                        this.myTable[3, 3] = "(kN)";
                        this.myTable[2, 4] = "せん断力";
                        this.myTable[3, 4] = "(kN)";
                        this.myTable[2, 5] = "曲げﾓｰﾒﾝﾄ";
                        this.myTable[3, 5] = "(kN・m)";
                        this.myTable[2, 6] = "組合せ";
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

            int rows = target.Count;


            // 行コンテンツを生成
            var table = this.myTable.Clone();
            table.ReDim(row: r + rows * 3);

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
                    j++;
                    if (item.caseStr != null)
                    {
                        int len = item.caseStr.Length;
                        var str = item.caseStr;

                        if (len > 24)
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

                    //var x = item.m;

                    //if (x != "")
                    //{
                    //    table.RowHeight[r] = printManager.LineSpacing2;
                    //}
                    //else if (i == 0)
                    //{
                    //    table.RowHeight[r] = printManager.LineSpacing2;
                    //}

                }
            }

            else　　//２次元
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
                    table[r, j] = printManager.toString(item.mz, 2);
                    table.AlignX[r, j] = "R";
                    j++;
                    if (item.caseStr != null)
                    {
                        int len = item.caseStr.Length;
                        var str = item.caseStr;

                        if (len > 24)
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

                    //var x = item.m;

                    //if (x != "")
                    //{
                    //    table.RowHeight[r] = printManager.LineSpacing2;
                    //}
                    //else if (i == 0)
                    //{
                    //    table.RowHeight[r] = printManager.LineSpacing2;
                    //}

                    //r++;

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

            if (this.Fsecs.Count == 0)
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
                for (int j = 0; j < this.Fsecs.Count; ++j)
                {   // ケース番号のループ
                    var key = this.Fsecs.ElementAt(j).Key;  // ケース番号
                    var value = this.Fsecs.ElementAt(j).Value;

                    var caseNo = this.fsecnames.ElementAt(j).Key;
                    var caseName = this.fsecnames.ElementAt(j).Value;

                    var ValueKey = new List<string>();
                    ValueKey.Add("軸方向力　最大");
                    ValueKey.Add("軸方向力　最小");
                    ValueKey.Add("Y方向のせん断力　最大");
                    ValueKey.Add("Y方向のせん断力　最小");
                    ValueKey.Add("Z方向のせん断力　最大");
                    ValueKey.Add("Z方向のせん断力　最小");
                    ValueKey.Add("ねじりモーメント　最大");
                    ValueKey.Add("ねじりモーメント　最小");
                    ValueKey.Add("Y軸周りの曲げモーメント　最大");
                    ValueKey.Add("Y軸周りの曲げモーメント　最小");
                    ValueKey.Add("Z軸周りの曲げモーメント　最大");
                    ValueKey.Add("Z軸周りの曲げモーメント　最小");

                    var tmp3 = new List<Fsec>();

                    for (int k = 0; k < 12; ++k)
                    {
                        var tmp1 = value.getValue3(k);

                        while (true)
                        {
                            if (tmp1.Count <= 0)
                                break;


                            // 1ページに納まる分のデータをコピー
                            var tmp2 = new List<Fsec>();

                            if (tmp3.Count != 0)
                                rows = rows - tmp3.Count();
                                tmp2.AddRange(tmp3);
                                tmp3.Clear();


                            for (int i = 0; i < rows; i++)
                            {
                                if (tmp1.Count <= 0)
                                    break;

                                //if (tmp1.Count < rows)
                                //    rows = tmp1.Count;

                                int len = tmp1[0].caseStr.Length;
                                var str = tmp1[0].caseStr;
                                var pullrows = 0;


                                if (len > 24)
                                {
                                    foreach (var n in str.SubstringAtCount(24))
                                    {
                                        pullrows ++;
                                    }
                                }

                                rows = rows - pullrows;
                                if (rows <= 0)
                                    break;

                                while (true)
                                {
                                    tmp3.Add(tmp1.First());
                                    tmp1.Remove(tmp1.First());

                                    if (tmp1.Count <= 0)
                                        break;

                                    if (tmp1.First().m != "")
                                    {
                                        break;
                                    }
                                }

                                //残りの行にこれから入れるデータが入りきるとき
                                if (rows - tmp2.Count > tmp3.Count)
                                {
                                    tmp2.AddRange(tmp3);
                                    tmp3.Clear();
                                }
                                //残りの行にも、次のページにも入りきらないとき
                                else if (rows - tmp2.Count < tmp3.Count && tmp3.Count > printRows[1])
                                {
                                    //現在のページから連続して印刷
                                    while (tmp3.Count != 0)
                                    {
                                        for (int l = 0; l < rows; l++)
                                        {
                                            tmp2.Add(tmp3.First());
                                            tmp3.Remove(tmp3.First());                                            
                                            if (tmp3.Count == 0)
                                                break;
                                            if (rows - tmp2.Count <= 0)
                                                break;
                                        }
                                        if (tmp3.Count == 0)
                                            break;
                                        if (rows == 0)
                                            break;
                                        var table = this.getPageContents(tmp2);
                                        table[0, 0] = caseNo + caseName;
                                        table[1, 0] = ValueKey[k];
                                        page.Add(table);
                                        tmp2.Clear();
                                    }
                                }
                                //tmp3にためたまま改ページし次のページで印刷
                                else 
                                {
                                    break;
                                }

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
                for (int j = 0; j < this.Fsecs.Count; ++j)
                {   // ケース番号のループ
                    var key = this.Fsecs.ElementAt(j).Key;  // ケース番号
                    var value = this.Fsecs.ElementAt(j).Value;

                    var caseNo = this.fsecnames.ElementAt(j).Key;
                    var caseName = this.fsecnames.ElementAt(j).Value;

                    var ValueKey = new List<string>();
                    ValueKey.Add("軸方向力　最大");
                    ValueKey.Add("軸方向力　最小");
                    ValueKey.Add("Y方向のせん断力　最大");
                    ValueKey.Add("Y方向のせん断力　最小");
                    ValueKey.Add("Z軸周りの曲げモーメント　最大");
                    ValueKey.Add("Z軸周りの曲げモーメント　最小");

                    for (int k = 0; k < 6; ++k)
                    {
                        var tmp1 = value.getValue2(k);

                        while (true)
                        {
                            if (tmp1.Count <= 0)
                                break;

                            //if (tmp1[0].caseStr.Length > 24)
                            //{
                            //    rows = rows / 2;
                            //}

                            // 1ページに納まる分のデータをコピー
                            var tmp2 = new List<Fsec>();
                            for (int i = 0; i < rows; i++)
                            {
                                if (tmp1.Count <= 0)
                                    break;

                                while (true)
                                {
                                    tmp2.Add(tmp1.First());
                                    tmp1.Remove(tmp1.First());
                                    if (tmp1.Count <= 0)
                                        break;
                                    if (tmp1.First().m != "")
                                    {
                                        break;
                                    }
                                    rows -= 1;
                                }
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
