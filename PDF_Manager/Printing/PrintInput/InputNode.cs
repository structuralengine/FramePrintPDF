using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
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

        public Vector3(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }
    }

    internal class InputNode
    {
        private Dictionary<string, Vector3> nodes = new Dictionary<string, Vector3>();
        private dataManager helper;

        /// <summary>
        /// データを読み込む
        /// </summary>
        /// <param name="dataManager"></param>
        /// <param name="value"></param>
        public void init(dataManager dataManager, Dictionary<string, object> value)
        {
            this.helper = dataManager;

            //nodeデータを取得する
            var target = JObject.FromObject(value["node"]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for(var i=0; i< target.Count; i++)
            {
                var key = target.ElementAt(i).Key;
                var targetValue = JObject.FromObject(target.ElementAt(i).Value);
                var x = dataManager.getNumeric(targetValue["x"]);
                var y = dataManager.getNumeric(targetValue["x"]);
                var z = dataManager.getNumeric(targetValue["x"]);
                var pos = new Vector3(x, y, z);
                this.nodes.Add(key, pos);
            }
        }

        /// <summary>
        /// 印刷する
        /// </summary>
        /// <param name="mc"></param>
        public void NodePDF(PdfDoc mc)
        {
            #region 印刷設定
            // ヘッダーのx方向の余白
            var header_Xspacing = (this.helper.dimension == 3) ?
                new int[,] {
                    { 10, 60, 120, 180, 250, 300, 360, 420 },
                    { 10, 60, 120, 180, 250, 300, 360, 420 },
                } :
                new int[,] {
                    { 10, 60, 120, 120, 190, 240, 300, 420 },
                    { 10, 60, 120, 120, 190, 240, 300, 420 },
                };

            // ボディーのx方向の余白
            var body_Xspacing = (this.helper.dimension == 3) ?
                new int[,] {
                    { 17, 77, 137, 197, 257, 317, 377, 437 }
                } :
                new int[,] {
                    { 17, 77, 137, 137, 197, 257, 317, 437 }
                };

            //　ヘッダー
            string title;
            string[,] header_content;
            switch (this.helper.language)
            {
                case "en":
                    title = "Node Data";
                    header_content = (this.helper.dimension == 3) ?
                        new string[,] {
                            { "Node", "", "", "", "Node", "", "", "", },
                            { "No", "X", "Y", "Z", "No", "X", "Y", "Z" }
                        } :
                        new string[,] {
                            { "Node", "", "", "", "Node", "", "", "", },
                            { "No", "X", "Y", "", "No", "X", "Y", "" }
                        };
                    break;

                case "cn":
                    title = "节点";
                    header_content = (this.helper.dimension == 3) ?
                        new string[,] {
                            { "节点", "", "", "", "节点", "", "", "", },
                            { "No", "X", "Y", "Z", "No", "X", "Y", "Z" }
                        } :
                        new string[,] {
                            { "节点", "", "", "", "节点", "", "", "", },
                            { "No", "X", "Y", "", "No", "X", "Y", "" }
                        };
                    break;

                default:
                    title = "格点データ";
                    header_content = (this.helper.dimension == 3) ?
                        new string[,] {
                            { "格点", "", "", "", "格点", "", "", "", },
                            { "No", "X", "Y", "Z", "No", "X", "Y", "Z" }
                        } :
                        new string[,] {
                            { "格点", "", "", "", "格点", "", "", "", },
                            { "No", "X", "Y", "", "No", "X", "Y", "" }
                        };
                    break;
            }

            #endregion

            #region 印刷する内容を集計する

            List<List<string[]>> data = new List<List<string[]>>();

            int bottomCell = mc.bottomCell * 2;

            var row = this.nodes.Count;
            var page = 0;

            while (true)
            {
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
            #endregion
        }

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
    }

}

