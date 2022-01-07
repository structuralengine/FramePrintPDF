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

namespace PDF_Manager.Printing
{
    internal class InputNode
    {
        private PdfDoc mc;
        private Dictionary<string, object> value = new Dictionary<string, object>();
        public List<List<string[]>> data = new List<List<string[]>>();


        public void Node(PdfDoc mc, Dictionary<string, object> value_)
        {
            int bottomCell = mc.bottomCell * 2;

            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["node"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            //ArrayList node_data = new ArrayList();

            // 全部の行数
            var row = target.Count;

            var page = 0;
            //var body = new ArrayList()

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
                        //　各行のデータを取得する（左段/右段)
                        var targetValue_l = JObject.FromObject(target.ElementAt(j).Value);

                        string[] line = new String[8];
                        line[0] = target.ElementAt(j).Key;
                        line[1] = mc.TypeChange(targetValue_l["x"], 3);
                        line[2] = mc.TypeChange(targetValue_l["y"], 3);
                        line[3] = mc.TypeChange(targetValue_l["z"], 3);

                        var targetValue_r = JObject.FromObject(target.ElementAt(k).Value);
                        line[4] = target.ElementAt(k).Key;
                        line[5] = mc.TypeChange(targetValue_r["x"], 3);
                        line[6] = mc.TypeChange(targetValue_r["y"], 3);
                        line[7] = mc.TypeChange(targetValue_r["z"], 3);
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
                        //　各行のデータを取得する（左段)
                        var targetValue_l = JObject.FromObject(target.ElementAt(j).Value);

                        string[] line = new String[8];
                        line[0] = target.ElementAt(j).Key;
                        line[1] = mc.TypeChange(targetValue_l["x"], 3);
                        line[2] = mc.TypeChange(targetValue_l["y"], 3);
                        line[3] = mc.TypeChange(targetValue_l["z"], 3);

                        try
                        {
                            //　各行のデータを取得する（右段)
                            var targetValue_r = JObject.FromObject(target.ElementAtOrDefault(k).Value);
                            line[4] = target.ElementAtOrDefault(k).Key;
                            line[5] = mc.TypeChange(targetValue_r["x"], 3);
                            line[6] = mc.TypeChange(targetValue_r["y"], 3);
                            line[7] = mc.TypeChange(targetValue_r["z"], 3);
                            body.Add(line);
                        }
                        catch
                        {
                            line[4] = "";
                            line[5] = "";
                            line[6] = "";
                            line[7] = "";
                            body.Add(line);
                        }
                    }
                    data.Add(body);
                    break;
                }
            }
        }

        public double[] GetNodePos(PdfDoc mc, string nodeNo, Dictionary<string, object> value)
        {
            var nodeList = JObject.FromObject(value["node"]).ToObject<Dictionary<string, object>>();

            if (nodeList.Count <= 0)
            {
                return null;
            }

            if (nodeList.ContainsValue(nodeNo))
            {
                return null;
            }

            var targetValue = JObject.FromObject(nodeList[nodeNo]);

            string[] node_st = new string[3];
            node_st[0] = mc.TypeChange(targetValue["x"]);
            node_st[1] = mc.TypeChange(targetValue["y"]);
            node_st[2] = mc.TypeChange(targetValue["z"]);

            double[] node = new double[3];
            for (int i = 0; i < 3; i++)
            {
                if (node_st[i] == "")
                {
                    node[i] = 0;
                }
                else
                {
                    node[i] = double.Parse(node_st[i]);
                }
            }

            return node;
        }

        public void NodePDF(PdfDoc mc)
        {
            //タイトルの印刷
            mc.PrintContent("格点データ", 0);
            mc.CurrentRow(2);
            // ヘッダー
            string[,] header_content3D = {
                { "格点", "", "", "", "格点", "", "", "", },
                { "No", "X", "Y", "Z", "No", "X", "Y", "Z" }
            };
            // ヘッダー
            string[,] header_content2D = {
                { "格点", "", "", "", "格点", "", "", "", },
                { "No", "X", "Y", "", "No", "X", "Y", "" }
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 10, 60, 120, 180, 250, 300, 360, 420 },
                { 10, 60, 120, 180, 250, 300, 360, 420 },
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing2D = {
                { 10, 60, 120, 120, 190, 240, 300, 420 },
                { 10, 60, 120, 120, 190, 240, 300, 420 },
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;

            mc.Header(header_content, header_Xspacing);

            // ボディーのx方向の余白
            int[,] body_Xspacing3D = { { 17, 77, 137, 197, 257, 317, 377, 437 } };
            int[,] body_Xspacing2D = { { 17, 77, 137, 137, 197, 257, 317, 437 } };

            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    for (int k = 0; k < data[i][j].Length; k++)
                    {
                        mc.CurrentColumn(body_Xspacing[0, k]); //x方向移動
                        mc.PrintContent(data[i][j][k], 3); // print
                    }
                    if (!(i == data.Count - 1 && j == data[i].Count - 1))
                    {
                        mc.CurrentRow(1); // y方向移動
                    }
                }

            }
        }
    }
}

