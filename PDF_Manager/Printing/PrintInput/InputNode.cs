using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.Comon;
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
        // 項目タイトル
        private string[,] header_content;
        // ヘッダーのx方向の余白
        private int[,] header_Xspacing;
        // ボディーのx方向の余白
        private int[,] body_Xspacing;
        // 列数で３次元の場合は2列、2次元の場合は3列
        private int columns;

        /// <summary>
        /// 印刷前の初期化処理
        /// </summary>
        private void printInit(PdfDocument mc, PrintData data)
        {
            this.dimension = data.dimension;
            if (this.dimension == 3)
            {   // 3次元
                this.columns = 2;
                this.header_Xspacing = new int[,] {
                    { 10, 60, 120, 180, 250, 300, 360, 420 },
                    { 10, 60, 120, 180, 250, 300, 360, 420 },
                };
                this.body_Xspacing = new int[,] {
                    { 17, 77, 137, 197, 257, 317, 377, 437 }
                };
                switch (data.language)
                {
                    case "en":
                        this.title = "Node Data";
                        this.header_content = new string[,] {
                            { "Node", "", "", "", "Node", "", "", "", },
                            { "No", "X", "Y", "Z", "No", "X", "Y", "Z" }
                        };
                        break;

                    case "cn":
                        this.title = "节点";
                        this.header_content = new string[,] {
                            { "节点", "", "", "", "节点", "", "", "", },
                            { "No", "X", "Y", "Z", "No", "X", "Y", "Z" }
                        };
                        break;

                    default:
                        this.title = "格点データ";
                        this.header_content = new string[,] {
                            { "格点", "", "", "", "格点", "", "", "", },
                            { "No", "X", "Y", "Z", "No", "X", "Y", "Z" }
                        };
                        break;
                }
            }
            else
            {   // 2次元
                this.columns = 3;
                this.header_Xspacing = new int[,] {
                    { 10, 60, 120, 120, 190, 240, 300, 420, 420 },
                    { 10, 60, 120, 120, 190, 240, 300, 420, 420 },
                };
                this.body_Xspacing = new int[,] {
                    { 17, 77, 137, 137, 197, 257, 317, 437, 437 }
                };
                switch (data.language)
                {
                    case "en":
                        this.title = "Node Data";
                        this.header_content = new string[,] {
                            { "Node", "", "", "Node", "", "", "Node", "", "" },
                            { "No", "X", "Y", "No", "X", "Y", "No", "X", "Y" }
                        };
                        break;

                    case "cn":
                        this.title = "节点";
                        this.header_content = new string[,] {
                            { "节点", "", "", "节点", "", "", "节点", "", "" },
                            { "No", "X", "Y", "No", "X", "Y", "No", "X", "Y" }
                        };
                        break;

                    default:
                        this.title = "格点データ";
                        this.header_content = new string[,] {
                            { "格点", "", "", "格点", "", "", "格点", "", "" },
                            { "No", "X", "Y", "No", "X", "Y", "No", "X", "Y" }
                        };
                        break;
                }
            }
        }

        /// <summary>
        /// 何行印刷できるか調べる
        /// </summary>
        /// <returns>
        /// return[0] = 1ページ目の印刷可能行数, 
        /// return[1] = 2ページ目以降の印刷可能行数
        /// </returns>
        private int[] getPrintRowCount(PdfDocument mc)
        {
            // タイトルの印字高さ + 改行高
            double H1 = printManager.FontHeight + printManager.LineSpacing1;

            // 表題の印字高さ + 改行高
            double H2 = this.header_content.GetLength(0) * printManager.FontHeight + printManager.LineSpacing2;

            // 1行当りの高さ + 改行高
            double H3 = printManager.FontHeight + printManager.LineSpacing3;

            // 2ページ目以降（ページ全体を使ってよい場合）の行数
            double Hx = mc.currentPageSize.Height;
            Hx -= H1;
            Hx -= H2;
            int rows2 = (int)(Hx / H3); // 切り捨て

            // 1ページ目（現在位置から）の行数
            Hx -= mc.currentY;
            int rows1 = (int)(Hx / H3); // 切り捨て

            return new int[] { rows1, rows2 };
        }


        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の</param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        /// <returns></returns>
        private List<string[]> getPageContents(Dictionary<string, Vector3> target, int rows)
        {
            int count = this.header_content.GetLength(1);

            // 行コンテンツを生成
            var table = new List<string[]>();

            for (var i = 0; i < rows; i++)
            {
                var lines = new string[count];

                for (var j = 0; j < this.columns; j++)
                {
                    int index = i + (rows * j);

                    if (target.Count <= index)
                        break;

                    string No = target.ElementAt(index).Key;
                    Vector3 XYZ = target.ElementAt(index).Value;

                    lines[0 + this.columns * j] = No;
                    lines[1 + this.columns * j] = printManager.toString(XYZ.x, 3);
                    lines[2 + this.columns * j] = printManager.toString(XYZ.y, 3);
                    if (this.dimension == 3)
                        lines[3 + this.columns * j] = printManager.toString(XYZ.z, 3);
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
            this.printInit(mc, data);

            // 印刷可能な行数
            var printRows = this.getPrintRowCount(mc);

            // 全データを 1ページに印刷したら 何行になるか
            int counter = this.nodes.Count;
            int rs = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(counter) / this.columns));

            // 行コンテンツを生成
            var page = new List<List<string[]>>();


            // ページに入る行数
            int rows = printRows[0];

            var table = getPageContents(this.nodes, rs);
            page.Add(table);

            // 集計開始
            while (0 < counter)
            {
                if (rs < rows)
                {   // 全データが1ページに入る場合
                    for(var i = 0; i < rs; i++)
                    {
                        var columns = new string[this.header_content.GetLength(1)];
                        for (var j=0; j< this.columns; j++)
                        {
                            int index = i + (rs * j);
                            
                            if(this.nodes.Count <= index)
                                break;

                            string No = this.nodes.ElementAt(index).Key;
                            var XYZ = this.nodes.ElementAt(index).Value;

                            columns[0 + this.columns * j] = No;
                            columns[1 + this.columns * j] = printManager.toString(XYZ.x, 3);
                            columns[2 + this.columns * j] = printManager.toString(XYZ.y, 3);
                            if (data.dimension == 3)
                                columns[3 + this.columns * j] = printManager.toString(XYZ.z, 3);
                        }
                        page.Add(columns);
                        columns = null;
                    }
                    break;
                }
                else
                {
                    var columns = new string[this.header_content.GetLength(1)];


                    page.Add(columns);

                    // 残りのデータ数を更新する
                    counter -= 100000;
                    rs = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(counter) / this.columns));
                }
                rows = printRows[1];
            }
                /*
                // 2ページ目以降（ページ全体を使ってよい場合）の行数

                var currentPos = mc.currentY; // 現在のポジション
                int counter = 0;
                while (counter < this.nodes.Count)
                {
                    if(mc.currentPageSize.Height < currentPos)
                    {   // 改ページが必要ならば
                        currentPos = H1 + H2; //
                    }
                    double curY = H1 + H2;
                    // 現在のポジション

                }

                // 何行印刷できるか調べる
                double rowHeight = printManager.;

                // 印刷可能な領域
                var page = mc.currentPageSize;
                var pageHeight = page.Height;   // 用紙の印刷可能範囲の高さ
                var pageWidth = page.Width;     // 用紙の印刷可能範囲の幅


                // 何行印刷できるか調べる
                int dataCount = this.nodes.Count;

                // タイトルとヘッダーの印字で必要な高さ
                int titleHeigth = 



                while (true)
                {
                    // 何行印刷できるか調べる
                    var height = mc.currentY;       // 現在の高さ



                    if (row > bottomCell)
                    {
                        List<string[]> body = new List<string[]>();
                        var half = bottomCell / 2;
                        for (var i = 0; i < half; i++)
                        {
                            //　各行の配列開始位置を取得する（左段/右段)
                            var j = bottomCell * page + i;
                            var k = bottomCell * page + bottomCell / 2 + i;

                            string[] line = Enumerable.Repeat<String>("", 8).ToArray();

                            //　各行のデータを取得する（左段/右段)
                            Vector3 targetValue_l = this.nodes.ElementAt(j).Value;
                            line[0] = this.nodes.ElementAt(j).Key;
                            line[1] = dataManager.TypeChange(targetValue_l.x, 3);
                            line[2] = dataManager.TypeChange(targetValue_l.y, 3);
                            if(this.helper.dimension == 3)
                                line[3] = dataManager.TypeChange(targetValue_l.z, 3);

                            var targetValue_r = this.nodes.ElementAt(k).Value;
                            line[4] = this.nodes.ElementAt(k).Key;
                            line[5] = dataManager.TypeChange(targetValue_r.x, 3);
                            line[6] = dataManager.TypeChange(targetValue_r.y, 3);
                            if (this.helper.dimension == 3)
                                line[7] = dataManager.TypeChange(targetValue_r.z, 3);

                            body.Add(line);
                        }
                        data.Add(body);
                        row -= bottomCell;
                        page++;
                    }
                    else
                    {
                        List<string[]> body = new List<string[]>();

                        row = row % 2 == 0 ? row / 2 : row / 2 + 1;

                        for (var i = 0; i < row; i++)
                        {
                            //　各行の配列開始位置を取得する（左段/右段)
                            var j = bottomCell * page + i;
                            var k = j + row;

                            string[] line = Enumerable.Repeat<String>("", 8).ToArray();

                            //　各行のデータを取得する（左段)
                            var targetValue_l = this.nodes.ElementAt(j).Value;
                            line[0] = this.nodes.ElementAt(j).Key;
                            line[1] = dataManager.TypeChange(targetValue_l.x, 3);
                            line[2] = dataManager.TypeChange(targetValue_l.y, 3);
                            if (this.helper.dimension == 3)
                                line[3] = dataManager.TypeChange(targetValue_l.z, 3);

                            try
                            {
                                //　各行のデータを取得する（右段)
                                var targetValue_r = this.nodes.ElementAtOrDefault(k).Value;
                                line[4] = this.nodes.ElementAtOrDefault(k).Key;
                                line[5] = dataManager.TypeChange(targetValue_r.x, 3);
                                line[6] = dataManager.TypeChange(targetValue_r.y, 3);
                                if (this.helper.dimension == 3)
                                    line[7] = dataManager.TypeChange(targetValue_r.z, 3);
                            }
                            catch
                            {
                                line[4] = "";
                                line[5] = "";
                                line[6] = "";
                                line[7] = "";
                            }
                            body.Add(line);
                        }
                        data.Add(body);
                        break;
                    }
                }
                #endregion

                #region 印刷する
                mc.PrintContent(title, 0);
                mc.CurrentRow(2);
                mc.CurrentColumn(0);

                mc.Header(header_content, header_Xspacing);


                for (int i = 0; i < data.Count; i++)
                {
                    var pages = data[i];
                    for (int j = 0; j < pages.Count; j++)
                    {
                        var line = pages[j];
                        for (int k = 0; k < line.Length; k++)
                        {
                            mc.CurrentColumn(body_Xspacing[0, k]); //x方向移動
                            mc.PrintContent(line[k], 3); // print
                        }
                        if (!(i == data.Count - 1 && j == data[i].Count - 1))
                        {
                            mc.CurrentRow(1); // y方向移動
                        }
                    }
                }

                */
                #endregion
            }


        #region 他のモジュールのヘルパー関数
        /*
        /// <summary>
        /// 節点座標を返す
        /// </summary>
        /// <param name="nodeNo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double[] GetNodePos(string nodeNo, Dictionary<string, object> value)
        {
            //var nodeList = JObject.FromObject(value["node"]).ToObject<Dictionary<string, object>>();

            if (this.nodes.Count <= 0)
                return null;

            if (!this.nodes.ContainsKey(nodeNo))
                return null;

            var targetValue = this.nodes[nodeNo];

            double[] node = new double[3];
            node[0] = double.IsNaN(targetValue.x) ? 0 : targetValue.x;
            node[1] = double.IsNaN(targetValue.y) ? 0 : targetValue.y;
            node[2] = double.IsNaN(targetValue.z) ? 0 : targetValue.z;

            return node;
        }
        */
        #endregion

    }

}

