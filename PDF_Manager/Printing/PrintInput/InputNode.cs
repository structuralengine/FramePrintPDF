using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDF_Manager.Printing
{
    public class Vector3
    {
        public double x;
        public double y;
        public double z;
    }

    internal class InputNode
    {
        # region 初期化・データの集計

        public const string KEY = "node";

        private Dictionary<string, Vector3> nodes = new Dictionary<string, Vector3>();

        /// <summary>
        /// データを読み込む
        /// </summary>
        /// <param name="dataManager"></param>
        /// <param name="value"></param>
        public InputNode(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(KEY))
                return;

            //nodeデータを取得する
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for(var i=0; i< target.Count; i++)
            {
                var key = target.ElementAt(i).Key;
                var item = JObject.FromObject(target.ElementAt(i).Value);

                var pos = new Vector3();
                pos.x = dataManager.parseDouble(item["x"]);
                pos.y = dataManager.parseDouble(item["y"]);
                pos.z = dataManager.parseDouble(item["z"]);
                this.nodes.Add(key, pos);
            }
        }

        #endregion


        #region 印刷処理

        // タイトル
        private string title;
        // 2次元か3次元か
        private int dimension;
        // テーブル
        private Table myTable;

        /// <summary>
        /// 印刷前の初期化処理
        /// </summary>
        private int printInit(PdfDocument mc, PrintData data)
        {
            int columns = 0; // 列数で３次元の場合は2列、2次元の場合は3列

            this.dimension = data.dimension;
            if (this.dimension == 3)
            {   // 3次元
                columns = 2;

                int cols = columns * 4;

                //テーブルの作成
                this.myTable = new Table(2, cols);

                // テーブルの幅
                for (var i=0; i < cols; i++)
                    this.myTable.ColWidth[i] = 65.0;
                this.myTable.ColWidth[0] = 45.0; // 格点No
                this.myTable.ColWidth[4] = this.myTable.ColWidth[0];

                // ヘッダー
                switch (data.language)
                {
                    case "en":
                        this.title = "Node Data";
                        this.myTable[0, 0] = "Node";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "X";
                        this.myTable[1, 2] = "Y";
                        this.myTable[1, 3] = "Z";
                        break;

                    case "cn":
                        this.title = "节点";
                        this.myTable[0, 0] = "节点";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "X";
                        this.myTable[1, 2] = "Y";
                        this.myTable[1, 3] = "Z";
                        break;

                    default:
                        this.title = "格点データ";
                        this.myTable[0, 0] = "格点";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "X";
                        this.myTable[1, 2] = "Y";
                        this.myTable[1, 3] = "Z";
                        break;
                }
                this.myTable[0, 4] = this.myTable[0, 0];
                this.myTable[1, 4] = this.myTable[1, 0];
                this.myTable[1, 5] = this.myTable[1, 1];
                this.myTable[1, 6] = this.myTable[1, 2];
                this.myTable[1, 7] = this.myTable[1, 3];
            }
            else
            {   // 2次元
                columns = 3;

                int cols = columns * 3;

                //テーブルの作成
                this.myTable = new Table(2, cols);

                // テーブルの幅
                for (var i = 0; i < cols; i++)
                    this.myTable.ColWidth[i] = 60;
                this.myTable.ColWidth[0] = 45; // 格点No
                this.myTable.ColWidth[3] = this.myTable.ColWidth[0];
                this.myTable.ColWidth[6] = this.myTable.ColWidth[0];

                // ヘッダー
                switch (data.language)
                {
                    case "en":
                        this.title = "Node Data";
                        this.myTable[0, 0] = "Node";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "X";
                        this.myTable[1, 2] = "Y";
                        break;

                    case "cn":
                        this.title = "节点";
                        this.myTable[0, 0] = "节点";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "X";
                        this.myTable[1, 2] = "Y";
                        break;

                    default:
                        this.title = "格点データ";
                        this.myTable[0, 0] = "格点";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "X";
                        this.myTable[1, 2] = "Y";
                        break;
                }
                this.myTable[0, 3] = this.myTable[0, 0];
                this.myTable[1, 3] = this.myTable[1, 0];
                this.myTable[1, 4] = this.myTable[1, 1];
                this.myTable[1, 5] = this.myTable[1, 2];
                this.myTable[0, 6] = this.myTable[0, 0];
                this.myTable[1, 6] = this.myTable[1, 0];
                this.myTable[1, 7] = this.myTable[1, 1];
                this.myTable[1, 8] = this.myTable[1, 2];
            }

            return columns;
        }

        
        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <param name="rows">行数</param>
        /// <returns>印刷する用の配列</returns>
        private List<string[]> getPageContents(Dictionary<string, Vector3> target, int rows, int columns)
        {
            int count = this.myTable.Columns;
            int c = count / columns;

            // 行コンテンツを生成
            var table = new List<string[]>();

            for (var i = 0; i < rows; i++)
            {
                var lines = new string[count];

                for (var j = 0; j < columns; j++)
                {
                    int index = i + (rows * j);

                    if (target.Count <= index)
                        continue;

                    string No = target.ElementAt(index).Key;
                    Vector3 item = target.ElementAt(index).Value;

                    lines[0 + c * j] = No;
                    lines[1 + c * j] = printManager.toString(item.x, 3);
                    lines[2 + c * j] = printManager.toString(item.y, 3);
                    if (this.dimension == 3)
                        lines[3 + c * j] = printManager.toString(item.z, 3);
                }
                table.Add(lines);
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
            int columns = this.printInit(mc, data);

            // 印刷可能な行数
            double H1 = printManager.FontHeight + printManager.LineSpacing2; // タイトルの印字高さ + 改行高
            var printRows = myTable.getPrintRowCount(mc, H1);

            // 行コンテンツを生成
            var page = new List<List<string[]>>();

            // 1ページ目に入る行数
            int rows = printRows[0];

            // 集計開始
            var tmp1 = new Dictionary<string, Vector3>(this.nodes); // clone
            while (true)
            {
                // 1ページに納まる分のデータをコピー
                var tmp2 = new Dictionary<string, Vector3>();
                for (int i=0; i<rows * columns; i++)
                {
                    if (tmp1.Count <= 0)
                        break;
                    tmp2.Add(tmp1.First().Key, tmp1.First().Value);
                    tmp1.Remove(tmp1.First().Key);
                }
                if (tmp2.Count > 0)
                {
                    // 全データを 1ページに印刷したら 何行になるか
                    int rs = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tmp2.Count) / columns));
                    rows = Math.Min(rows, rs);

                    var table = this.getPageContents(tmp2, rows, columns);
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
            this.printContent(mc, page, this.title, myTable);

        }


        /// <summary>
        /// 印刷を行う
        /// </summary>
        /// <param name="mc">キャンパス</param>
        /// <param name="page">印字内容</param>
        /// <param name="title">タイトル</param>
        /// <param name="tbl">表</param>
        public void printContent(PdfDocument mc, List<List<string[]>> page, string title, Table HeaderTable)
        {
            // 表の印刷
            for(var i=0; i<page.Count; i++)
            {
                var table = page[i];
                if (0 < i)
                    mc.NewPage(); // 2ページ目以降は改ページする

                // タイトルの印字
                mc.setCurrentX(printManager.H1PosX);
                Text.PrtText(mc, title);
                mc.addCurrentY(printManager.FontHeight + printManager.LineSpacing2);

                // 表の作成
                var tbl = HeaderTable.Clone();
                tbl.ReDim(tbl.Rows + table.Count, tbl.Columns);

                int r = HeaderTable.Rows;
                foreach (var line in table)
                {
                    for (int c = 0; c < line.Length; c++)
                    {
                        var str = line[c];
                        if (str == null)
                            continue;
                        if (str.Length <= 0)
                            continue;
                        tbl[r, c] = str;
                        tbl.AlignX[r, c] = "R";
                    }
                    r++;
                }

                tbl.RowHeight[2] = printManager.LineSpacing2;

                // 表の印刷
                tbl.PrintTable(mc);
            }

            // 最後の改行
            mc.addCurrentY(printManager.LineSpacing1);
        }
        #endregion


        #region 他のモジュールのヘルパー関数
        /// <summary>
        /// 節点座標を返す
        /// </summary>
        /// <param name="nodeNo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        public Vector3 GetNodePos(string nodeNo)
        {
            if (this.nodes.Count <= 0)
                return null;

            if (!this.nodes.ContainsKey(nodeNo))
                return null;

            var target = this.nodes[nodeNo];

            var result = new Vector3();

            result.x = double.IsNaN(target.x) ? 0 : target.x;
            result.y = double.IsNaN(target.y) ? 0 : target.y;
            result.z = double.IsNaN(target.z) ? 0 : target.z;

            return result;
        }
        
        #endregion

    }

}

