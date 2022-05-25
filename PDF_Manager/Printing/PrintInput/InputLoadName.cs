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
using PDF_Manager.Printing.Comon;

namespace PDF_Manager.Printing
{
    public class LoadName
    {
        public double rate;
        public string symbol;
        public string Actual_load;
        public string name;
        public int fix_node;
        public int element;
        public int fix_member;
        public int joint;
    }

    internal class InputLoadName
    {
        public const string KEY = "loadName";

        private Dictionary<int, LoadName> loadnames = new Dictionary<int, LoadName>();

        public InputLoadName(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(InputLoad.KEY))
                return;

            //nodeデータを取得する
            var target = JObject.FromObject(value[InputLoad.KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = target.ElementAt(i).Key;
                var index = int.Parse(key);
                var item = JObject.FromObject(target.ElementAt(i).Value);
                var ln = new LoadName();


                ln.rate = dataManager.parseDouble(item["rate"]);
                ln.symbol = dataManager.toString(item["symbol"]);
                ln.name = dataManager.toString(item["name"]);
                ln.fix_node = dataManager.parseInt(item["fix_node"]);
                ln.element = dataManager.parseInt(item["element"]);
                ln.fix_member = dataManager.parseInt(item["fix_member"]);
                ln.joint = dataManager.parseInt(item["joint"]);

                this.loadnames.Add(index, ln);
            }
        }

        ///印刷処理

        ///タイトル
        private string title;
        ///２次元か３次元か
        private int dimension;
        ///テーブル
        private Table myTable;
        ///節点情報
        private InputNode Node = null;
        ///材料情報
        private InputElement Element = null; 


        ///印刷前の初期化処理
        ///
        private void printInit(PdfDocument mc, PrintData data)
        {
            this.dimension = data.dimension;

            if (this.dimension == 3)
            {///3次元

                ///テーブルの作成
                this.myTable = new Table(2, 9);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 45.0;//Case
                this.myTable.ColWidth[1] = 60.00;//割増係数
                this.myTable.ColWidth[2] = 30.0;//記号
                this.myTable.ColWidth[3] = 240.0;//荷重名称
                this.myTable.ColWidth[4] = 25.0;//支点
                this.myTable.ColWidth[5] = 25.0;//断面
                this.myTable.ColWidth[6] = 25.0;//部材
                this.myTable.ColWidth[7] = 25.0;//地盤

                switch (data.language)
                {
                    default:
                        this.title = "基本荷重DATA";
                        this.myTable[0, 0] = "Case";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "割増係数";
                        this.myTable[1, 2] = "記号";
                        this.myTable[1, 3] = "荷重名称";
                        this.myTable[0, 4] = "　　構造系条件";
                        this.myTable[1, 4] = "支点";
                        this.myTable[1, 5] = "断面";
                        this.myTable[1, 6] = "バネ";
                        this.myTable[1, 7] = "結合";
                        break;
                }

                //表題の文字位置
                this.myTable.AlignX[0, 5] = "L";    // 左寄せ
            }
            else
            {//2次元

                ///テーブルの作成
                this.myTable = new Table(2, 11);

                ///テーブルの幅
                this.myTable.ColWidth[0] = 45.0;//Case
                this.myTable.ColWidth[1] = 60.00;//割増係数
                this.myTable.ColWidth[2] = 30.0;//記号
                this.myTable.ColWidth[3] = 240.0;//荷重名称
                this.myTable.ColWidth[4] = 25.0;//支点
                this.myTable.ColWidth[5] = 25.0;//断面
                this.myTable.ColWidth[6] = 25.0;//部材
                this.myTable.ColWidth[7] = 25.0;//地盤

                switch (data.language)
                {
                    default:
                        this.title = "基本荷重DATA";
                        this.myTable[0, 0] = "Case";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "割増係数";
                        this.myTable[1, 2] = "記号";
                        this.myTable[1, 3] = "荷重名称";
                        this.myTable[0, 5] = "構造系条件";
                        this.myTable[1, 4] = "支点";
                        this.myTable[1, 5] = "断面";
                        this.myTable[1, 6] = "バネ";
                        this.myTable[1, 7] = "結合";
                        break;
                }

                //表題の文字位置
                this.myTable.AlignX[1, 2] = "L";    // 左寄せ
                this.myTable.AlignX[0, 5] = "L";    // 左寄せ
            }
        }

        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <param name="rows">行数</param>
        /// <returns>印刷する用の配列</returns>
        private Table getPageContents(Dictionary<int, LoadName> target)
        {
            int r = this.myTable.Rows;
            int rows = target.Count;

            // 行コンテンツを生成
            var table = this.myTable.Clone();
            table.ReDim(row: r + rows);

            table.RowHeight[r] = printManager.LineSpacing2;

            for (var i = 0; i < rows; i++)
            {
                int No = target.ElementAt(i).Key;
                LoadName item = target.ElementAt(i).Value;

                int j = 0;
                table[r, j] = No.ToString();
                table.AlignX[r, j] = "R";
                j++;
                table[r, j] = printManager.toString(item.rate, 3);
                ///table.AlignX[r, j] = "R";
                j++;
                table[r, j] = printManager.toString(item.symbol);
                table.AlignX[r, j] = "L";
                j++;
                table[r, j] = printManager.toString(item.name);
                table.AlignX[r, j] = "L";
                j++;
                table[r, j] = printManager.toString(item.fix_node);
                j++;
                table[r, j] = printManager.toString(item.element);
                j++;
                table[r, j] = printManager.toString(item.fix_member);
                j++;
                table[r, j] = printManager.toString(item.joint);
                j++;

                r++;
            }

            return table;
        }

        /// <summary>
        /// 印刷する
        /// </summary>
        /// <param name="mc"></param>
        public void printPDF(PdfDocument mc, PrintData data)
        {

            // タイトル などの初期化
            this.printInit(mc, data);

            // 印刷可能な行数
            var printRows = myTable.getPrintRowCount(mc);

            // 行コンテンツを生成
            var page = new List<Table>();

            // 1ページ目に入る行数
            int rows = printRows[0];

            // 集計開始
            var tmp1 = new Dictionary<int, LoadName>(this.loadnames); // clone
            while (true)
            {
                // 1ページに納まる分のデータをコピー
                var tmp2 = new Dictionary<int, LoadName>();
                for (int i = 0; i < rows; i++)
                {
                    if (tmp1.Count <= 0)
                        break;
                    tmp2.Add(tmp1.First().Key, tmp1.First().Value);
                    tmp1.Remove(tmp1.First().Key);
                }

                if (tmp2.Count > 0)
                {
                    var table = this.getPageContents(tmp2);
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



            // 表の印刷
            printManager.printTableContents(mc, page, new string[] { this.title });
        }

    }
}