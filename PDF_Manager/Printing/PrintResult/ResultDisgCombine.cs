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
    public class DisgCombine
    {
        public List<Disg> dx_max = new List<Disg>();
        public List<Disg> dx_min = new List<Disg>();
        public List<Disg> dy_max = new List<Disg>();
        public List<Disg> dy_min = new List<Disg>();
        public List<Disg> dz_max = new List<Disg>();
        public List<Disg> dz_min = new List<Disg>();
        public List<Disg> rx_max = new List<Disg>();
        public List<Disg> rx_min = new List<Disg>();
        public List<Disg> ry_max = new List<Disg>();
        public List<Disg> ry_min = new List<Disg>();
        public List<Disg> rz_max = new List<Disg>();
        public List<Disg> rz_min = new List<Disg>();

        public void Add(string key, Disg value)
        {
            // key と同じ名前の変数を取得する
            Type type = this.GetType();
            FieldInfo field = type.GetField(key);
            if (field == null)
            {
                throw new Exception(String.Format("DisgCombineクラスの変数{0} に値{1}を登録しようとしてエラーが発生しました", key, value));
            }
            var val = (List<Disg>)field.GetValue(this);

            // 変数に値を追加する
            val.Add(value);

            // 変数を更新する
            field.SetValue(this, val);
        }

        public List<Disg> getValue(int Index)
        {
            if (Index == 0)
            {
                return this.dx_max;
            }
            if (Index == 1)
            {
                return this.dx_min;
            }
            if (Index == 2)
            {
                return this.dy_max;
            }
            if (Index == 3)
            {
                return this.dy_min;
            }
            if (Index == 4)
            {
                return this.dz_max;
            }
            if (Index == 5)
            {
                return this.dz_min;
            }
            if (Index == 6)
            {
                return this.rx_max;
            }
            if (Index == 7)
            {
                return this.rx_min;
            }
            if (Index == 8)
            {
                return this.ry_max;
            }
            if (Index == 9)
            {
                return this.ry_min;
            }
            if (Index == 10)
            {
                return this.rz_max;
            }
            if (Index == 11)
            {
                return this.rz_min;
            }

            return null;

        }

    }


    //文字数分割用メソッド
    public static class StringExtensions
    {
        public static string[] SubstringAtCount(this string self, int count)
        {
            var result = new List<string>();
            var length = (int)Math.Ceiling((double)self.Length / count);

            for (int i = 0; i < length; i++)
            {
                int start = count * i;
                if (self.Length <= start)
                {
                    break;
                }
                if (self.Length < start + count)
                {
                    result.Add(self.Substring(start));
                }
                else
                {
                    result.Add(self.Substring(start, count));
                }
            }

            return result.ToArray();
        }
    }

    internal class ResultDisgCombine
    {
        public const string KEY = "disgCombine";

        private Dictionary<string, DisgCombine> disgs = new Dictionary<string, DisgCombine>();
        private Dictionary<string, string> disgnames = new Dictionary<string, string>();
        private Dictionary<string, string> disgcasenames = new Dictionary<string, string>();

        public ResultDisgCombine(Dictionary<string, object> value, string key = ResultDisgCombine.KEY)
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

                var Dis = ((JObject)val).ToObject<Dictionary<string, object>>();
                var _disg = ResultDisgCombine.getDisgCombine(Dis);

                this.disgs.Add(No, _disg);
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

        public static DisgCombine getDisgCombine(Dictionary<string, object> Dis)
        {
            var _disg = new DisgCombine();

            for (int i = 0; i < Dis.Count; i++)
            {
                var elist = JObject.FromObject(Dis.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                var k = Dis.ElementAt(i).Key;

                for (int j = 0; j < elist.Count; j++)
                {
                    var item = JObject.FromObject(elist.ElementAt(j).Value);

                    var ds = new Disg();

                    ds.id = dataManager.toString(elist.ElementAt(j).Key);
                    ds.dx = dataManager.parseDouble(item["dx"]);
                    ds.dy = dataManager.parseDouble(item["dy"]);
                    ds.dz = dataManager.parseDouble(item["dz"]);
                    ds.rx = dataManager.parseDouble(item["rx"]);
                    ds.ry = dataManager.parseDouble(item["ry"]);
                    ds.rz = dataManager.parseDouble(item["rz"]);
                    ds.caseStr = dataManager.toString(item["case"]);
                    ds.comb = dataManager.toString(item["comb"]);

                    _disg.Add(k, ds);
                }
            }
            return _disg;
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
                this.myTable.ColWidth[1] = 60.0;//X方向の移動量
                this.myTable.ColWidth[2] = 60.0;//Y方向の移動量
                this.myTable.ColWidth[3] = 60.0;//Z方向の移動量
                this.myTable.ColWidth[4] = 60.0;//X軸周りの回転量
                this.myTable.ColWidth[5] = 60.0;//Y軸周りの回転量
                this.myTable.ColWidth[6] = 60.0;//Z軸周りの回転量
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
                        this.title = "Combine変位量";
                        this.myTable[2, 0] = "節点";
                        this.myTable[3, 0] = "No";
                        this.myTable[2, 1] = "X方向の";
                        this.myTable[3, 1] = "移動量";
                        this.myTable[4, 1] = "(mm)";
                        this.myTable[2, 2] = "Y方向の";
                        this.myTable[3, 2] = "移動量";
                        this.myTable[4, 2] = "(mm)";
                        this.myTable[2, 3] = "Z方向の";
                        this.myTable[3, 3] = "移動量";
                        this.myTable[4, 3] = "(mm)";
                        this.myTable[2, 4] = "X軸周りの";
                        this.myTable[3, 4] = "回転量";
                        this.myTable[4, 4] = "(mmrad)";
                        this.myTable[2, 5] = "Y軸周りの";
                        this.myTable[3, 5] = "回転量";
                        this.myTable[4, 5] = "(mmrad)";
                        this.myTable[2, 6] = "Z軸周りの";
                        this.myTable[3, 6] = "回転量";
                        this.myTable[4, 6] = "(mmrad)";
                        this.myTable[2, 8] = "組合せ";
                        break;
                }

                //表題の文字位置
            }
            //else
            //{//2次元

            //    ///テーブルの作成
            //    this.myTable = new Table(4, 8);

            //    ///テーブルの幅
            //    this.myTable.ColWidth[0] = 20.0;//節点No
            //    this.myTable.ColWidth[1] = 72.5;//X方向の移動量
            //    this.myTable.ColWidth[2] = 72.5;//Y方向の移動量
            //    this.myTable.ColWidth[3] = 72.5;//X軸周りの回転量
            //    this.myTable.ColWidth[4] = 40.0;
            //    this.myTable.ColWidth[5] = this.myTable.ColWidth[1];
            //    this.myTable.ColWidth[6] = this.myTable.ColWidth[2];
            //    this.myTable.ColWidth[7] = this.myTable.ColWidth[3];

            //    this.myTable.RowHeight[1] = printManager.LineSpacing2;

            //    this.myTable.AlignX[0, 0] = "L";
            //    this.myTable.AlignX[1, 0] = "R";
            //    this.myTable.AlignX[1, 1] = "R";
            //    this.myTable.AlignX[1, 2] = "R";
            //    this.myTable.AlignX[1, 3] = "R";
            //    this.myTable.AlignX[1, 4] = "R";
            //    this.myTable.AlignX[1, 5] = "R";
            //    this.myTable.AlignX[1, 6] = "R";
            //    this.myTable.AlignX[1, 7] = "R";
            //    this.myTable.AlignX[2, 0] = "R";
            //    this.myTable.AlignX[2, 1] = "R";
            //    this.myTable.AlignX[2, 2] = "R";
            //    this.myTable.AlignX[2, 3] = "R";
            //    this.myTable.AlignX[2, 4] = "R";
            //    this.myTable.AlignX[2, 5] = "R";
            //    this.myTable.AlignX[2, 6] = "R";
            //    this.myTable.AlignX[2, 7] = "R";
            //    this.myTable.AlignX[3, 1] = "R";
            //    this.myTable.AlignX[3, 2] = "R";
            //    this.myTable.AlignX[3, 3] = "R";
            //    this.myTable.AlignX[3, 4] = "R";
            //    this.myTable.AlignX[3, 5] = "R";
            //    this.myTable.AlignX[3, 6] = "R";
            //    this.myTable.AlignX[3, 7] = "R";

            //    switch (data.language)
            //    {
            //        default:
            //            this.title = "変位量データ";
            //            this.myTable[1, 0] = "節点";
            //            this.myTable[2, 0] = "No";
            //            this.myTable[1, 1] = "X方向の";
            //            this.myTable[2, 1] = "移動量";
            //            this.myTable[3, 1] = "(mm)";
            //            this.myTable[1, 2] = "Y方向の";
            //            this.myTable[2, 2] = "移動量";
            //            this.myTable[3, 2] = "(mm)";
            //            this.myTable[2, 3] = "回転量";
            //            this.myTable[3, 3] = "(mmrad)";
            //            this.myTable[1, 4] = "節点";
            //            this.myTable[2, 4] = "No";
            //            this.myTable[1, 5] = "X方向の";
            //            this.myTable[2, 5] = "移動量";
            //            this.myTable[3, 5] = "(mm)";
            //            this.myTable[1, 6] = "Y方向の";
            //            this.myTable[2, 6] = "移動量";
            //            this.myTable[3, 6] = "(mm)";
            //            this.myTable[2, 7] = "回転量";
            //            this.myTable[3, 7] = "(mmrad)";

            //            break;
            //    }

            //}
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
            table.ReDim(row: r + rows*2);

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
                    j++;
                    if(item.caseStr != null)
                    {
                        int len = item.caseStr.Length;
                        var str = item.caseStr;

                        if(len > 24)
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

            //else　　//２次元
            //{
            //    int Rows = target.Count / columns;

            //    for (var i = 0; i < Rows; i++)
            //    {
            //        for (var j = 0; j < columns; j++)
            //        {
            //            var index = i + Rows * j; //左側：j=0 ∴index = i, 右側：j=1, ∴index = i+Rows
            //            if (target.Count <= index)
            //                continue;

            //            var item = target[index];

            //            table[r + i, 0 + c * j] = printManager.toString(item.id);
            //            table.AlignX[r + i, 0 + c * j] = "R";
            //            table[r + i, 1 + c * j] = printManager.toString(item.dx, 4);
            //            table.AlignX[r + i, 1 + c * j] = "R";
            //            table[r + i, 2 + c * j] = printManager.toString(item.dy, 4);
            //            table.AlignX[r + i, 2 + c * j] = "R";
            //            table[r + i, 3 + c * j] = printManager.toString(item.rz, 4);
            //            table.AlignX[r + i, 3 + c * j] = "R";
            //        }
            //    }

            //}

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
            if (dimension == 3)  //３次元
            {
                for (int j = 0; j < this.disgs.Count; ++j)
                {   // ケース番号のループ
                    var key = this.disgs.ElementAt(j).Key;  // ケース番号
                    var value = this.disgs.ElementAt(j).Value;

                    var caseNo = this.disgnames.ElementAt(j).Key;
                    var caseName = this.disgnames.ElementAt(j).Value;

                    var ValueKey = new List<string>();
                    ValueKey.Add("X方向の移動量　最大");
                    ValueKey.Add("X方向の移動量　最小");
                    ValueKey.Add("Y方向の移動量　最大");
                    ValueKey.Add("Y方向の移動量　最小");
                    ValueKey.Add("Z方向の移動量　最大");
                    ValueKey.Add("Z方向の移動量　最小");
                    ValueKey.Add("X軸周りの回転量　最大");
                    ValueKey.Add("X軸周りの回転量　最小");
                    ValueKey.Add("Y軸周りの回転量　最大");
                    ValueKey.Add("Y軸周りの回転量　最小");
                    ValueKey.Add("Z軸周りの回転量　最大");
                    ValueKey.Add("Z軸周りの回転量　最小");

                    for (int k = 0; k < 12; ++k)
                    {
                        var tmp1 = value.getValue(k);

                        while (true)
                        {

                            // 1ページに納まる分のデータをコピー
                            var tmp2 = new List<Disg>();
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

                            ////改行したため入りきらなかった場合
                            //if (tmp2.Count > 0)
                            //{
                            //    printManager.printTableContentsOnePage(mc, page, new string[] { this.title });
                            //    page = new List<Table>();
                            //    mc.NewPage();

                            //    var table = this.getPageContents(tmp2);
                            //    table[0, 0] = caseNo + caseName;
                            //    table[1, 0] = ValueKey[k];
                            //    page.Add(table);
                            //}

                        }

                    }
                }
            }
            //else　　//２次元
            //{
            //    for (int j = 0; j < this.disgs.Count; ++j)
            //    {   // ケース番号のループ
            //        var key = this.disgs.ElementAt(j).Key;  // ケース番号
            //        var tmp1 = new List<Disg>((List<Disg>)this.disgs.ElementAt(j).Value);

            //        var caseNo = this.disgnames.ElementAt(j).Key;
            //        var caseName = this.disgnames.ElementAt(j).Value;

            //        while (true)
            //        {
            //            // 1ページに納まる分のデータをコピー
            //            var tmp2 = new List<Disg>();
            //            int columns = 2;

            //            for (int i = 0; i < columns * rows; i++)
            //            {
            //                if (tmp1.Count <= 0)
            //                    break;
            //                tmp2.Add(tmp1.First());
            //                tmp1.Remove(tmp1.First());
            //            }

            //            if (tmp2.Count > 0)
            //            {
            //                int rs = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tmp2.Count) / columns));
            //                rows = Math.Min(rows, rs);

            //                var table = this.getPageContents(tmp2);
            //                table[0, 0] = caseNo + caseName;
            //                page.Add(table);

            //            }
            //            else if (tmp1.Count <= 0)
            //            {
            //                break;
            //            }
            //            else
            //            { // 印刷するものもない
            //                mc.NewPage();
            //            }

            //            // 2ページ以降に入る行数
            //            rows = printRows[1];
            //        }
            //    }

            //}

            // 表の印刷
            printManager.printTableContentsOnePage(mc, page, new string[] { this.title });

        }



    }
}
