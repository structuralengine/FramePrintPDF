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
        // 項目タイトル
        private string[,] header_content;
        // ヘッダーのx方向の余白
        private double[] header_Xspacing;
        // ボディーのx方向の余白
        private double[] body_Xspacing;


        /// <summary>
        /// 印刷前の初期化処理
        /// </summary>
        private int printInit(PdfDocument mc, PrintData data)
        {
            var X1 = printManager.H1PosX; //表題を印字するX位置  px ピクセル

            int columns = 0; // 列数で３次元の場合は2列、2次元の場合は3列

            this.dimension = data.dimension;
            if (this.dimension == 3)
            {   // 3次元
                columns = 2;
                this.header_Xspacing = new double[] {
                    X1, X1 + 70, X1 + 140, X1 + 210, X1 + 280, X1 + 350, X1 + 420, X1 + 490
                };
                this.body_Xspacing = Array.ConvertAll(this.header_Xspacing, (double x) => { return x + 15; });

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
                columns = 3;
                this.header_Xspacing = new double[] {
                    X1, X1 + 60, X1 + 120, X1 + 180, X1 + 240, X1 + 300, X1 + 360, X1 + 420, X1 + 480 
                };
                this.body_Xspacing = Array.ConvertAll(this.header_Xspacing, (double x) => { return x + 15; } );

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

            return columns;
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
            double H3 = printManager.LineSpacing3;

            // 2ページ目以降（ページ全体を使ってよい場合）の行数
            double Hx = mc.currentPageSize.Height;
            Hx -= H1;
            Hx -= H2;
            int rows2 = (int)(Hx / H3); // 切り捨て

            // 1ページ目（現在位置から）の行数
            Hx -= mc.contentY;
            int rows1 = (int)(Hx / H3); // 切り捨て

            return new int[] { rows1, rows2 };
        }


        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <param name="rows">行数</param>
        /// <returns>印刷する用の配列</returns>
        private List<string[]> getPageContents(Dictionary<string, Vector3> target, int rows, int columns)
        {
            int count = this.header_content.GetLength(1);
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
                    Vector3 XYZ = target.ElementAt(index).Value;

                    lines[0 + c * j] = No;
                    lines[1 + c * j] = printManager.toString(XYZ.x, 3);
                    lines[2 + c * j] = printManager.toString(XYZ.y, 3);
                    if (this.dimension == 3)
                        lines[3 + c * j] = printManager.toString(XYZ.z, 3);
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
            var printRows = this.getPrintRowCount(mc);

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
                if (tmp2.Count <= 0)
                    break;

                // 全データを 1ページに印刷したら 何行になるか
                int rs = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tmp2.Count) / columns));
                rows = Math.Min(rows, rs);

                var table = this.getPageContents(tmp2, rows, columns);
                page.Add(table);

                // 2ページ以降に入る行数
                rows = printRows[1];
            }


            // 表の印刷
            int p = 1;
            foreach (var table in page)
            {
                if(1 < p)
                    mc.NewPage();

                // タイトルの印字
                mc.setCurrentX(printManager.H1PosX);
                Text.PrtText(mc, this.title);
                mc.addCurrentY(printManager.FontHeight + printManager.LineSpacing1);

                // 表題の印字
                for(int i=0; i<this.header_content.Rank; i++)
                {
                    for(int j=0; j<this.header_content.GetLength(i); j++)
                    {
                        var str = this.header_content[i, j];
                        if (str.Length <= 0)
                            continue;
                        var x = this.header_Xspacing[j];
                        mc.setCurrentX(x);
                        Text.PrtText(mc, str);
                    }
                    mc.addCurrentY(printManager.FontHeight);
                }
                mc.addCurrentY(printManager.LineSpacing2); // 中くらい（タイトル後など）の改行高さ

                // 表の印刷
                foreach (var line in table)
                {
                    for(int i=0; i< line.Length; i++)
                    {
                        var str = line[i];
                        if (str == null)
                            continue;
                        if (str.Length <= 0)
                            continue;
                        var x = this.body_Xspacing[i];
                        mc.setCurrentX(x);
                        Text.PrtText(mc, str, align: XStringFormats.BottomRight);
                    }
                    mc.addCurrentY(printManager.LineSpacing3); // 小さい（テーブル内などの）改行高さ
                }

                p++;
            }
           

        }
        #endregion


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

