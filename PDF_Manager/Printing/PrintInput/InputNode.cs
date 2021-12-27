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

        public List<List<string[]>> Node(PdfDoc mc,Dictionary<string, object> value_)
        {
            int bottomCell = mc.bottomCell * 2;

            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["node"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            //ArrayList node_data = new ArrayList();
            List<List<string[]>> node_data = new List<List<string[]>>();

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
                        var targetValue_l = JObject.FromObject(target.ElementAt(j).Value).ToObject<Dictionary<string, double>>();

                        string[] line = new String[8];
                        line[0] = target.ElementAt(j).Key;
                        line[1] = (Math.Round(targetValue_l["x"], 3, MidpointRounding.AwayFromZero)).ToString();
                        line[2] = (Math.Round(targetValue_l["y"], 3, MidpointRounding.AwayFromZero)).ToString();
                        line[3] = (Math.Round(targetValue_l["z"], 3, MidpointRounding.AwayFromZero)).ToString();

                        var targetValue_r = JObject.FromObject(target.ElementAt(k).Value).ToObject<Dictionary<string, double>>();
                        line[4] = target.ElementAt(k).Key;
                        line[5] = (Math.Round(targetValue_r["x"], 3, MidpointRounding.AwayFromZero)).ToString();
                        line[6] = (Math.Round(targetValue_r["y"], 3, MidpointRounding.AwayFromZero)).ToString();
                        line[7] = (Math.Round(targetValue_r["z"], 3, MidpointRounding.AwayFromZero)).ToString();
                        body.Add(line);
                    }
                    node_data.Add(body);
                    row -= bottomCell;
                    page++;
                }
                else
                {
                    List<string[]> body = new List<string[]>();
                    row = decimal.ToInt32(decimal.Ceiling(row / 2));

                    for (var i = 0; i < row; i++)
                    {
                        //　各行の配列開始位置を取得する（左段/右段)
                        var j = bottomCell * page + i;
                        var k = j + row;
                        //　各行のデータを取得する（左段)
                        var targetValue_l = JObject.FromObject(target.ElementAt(j).Value).ToObject<Dictionary<string, double>>();

                        string[] line = new String[8];
                        line[0] = target.ElementAt(j).Key;
                        line[1] = (Math.Round(targetValue_l["x"], 3, MidpointRounding.AwayFromZero)).ToString();
                        line[2] = (Math.Round(targetValue_l["y"], 3, MidpointRounding.AwayFromZero)).ToString();
                        line[3] = (Math.Round(targetValue_l["z"], 3, MidpointRounding.AwayFromZero)).ToString();

                        if (target.ElementAt(k).Key != null)
                        {
                            //　各行のデータを取得する（右段)
                            var targetValue_r = JObject.FromObject(target.ElementAt(k).Value).ToObject<Dictionary<string, double>>();
                            line[4] = target.ElementAt(k).Key;
                            line[5] = (Math.Round(targetValue_r["x"], 3, MidpointRounding.AwayFromZero)).ToString();
                            line[6] = (Math.Round(targetValue_r["y"], 3, MidpointRounding.AwayFromZero)).ToString();
                            line[7] = (Math.Round(targetValue_r["z"], 3, MidpointRounding.AwayFromZero)).ToString();
                            body.Add(line);
                        }
                    }
                    node_data.Add(body);
                    break;
                }
            }
            return node_data;
        }

        public double[] GetNodePos(string nodeNo,Dictionary<string, object> value)
        {
            var nodeList = JObject.FromObject(value["node"]).ToObject<Dictionary<string, object>>();

            if (nodeList.Count <= 0)
            {
                return null;
            }

            if (nodeList.ContainsValue(nodeNo)) {
                return null;
            }

            var targetValue = JObject.FromObject(nodeList.ElementAt(Int32.Parse(nodeNo)).Value).ToObject<Dictionary<string, double>>();
            
            double[] node = new double[3];
            node[0] = targetValue["x"];
            node[1] = targetValue["y"];
            node[2] = targetValue["z"];

            return node;
        }

        public void NodePDF(PdfDoc mc, List<List<string[]>> nodeData)
        {
            int bottomCell = mc.bottomCell*2;
            int single_Yrow = mc.single_Yrow;
            int currentXposition_values = 40;
            //int count = 2;
            mc.gfx.DrawString("格点データ", mc.font_got, XBrushes.Black, mc.CurrentPosHeader);
            // 格点データ内部の初期位置の設定
            mc.CurrentPosHeader.Y += single_Yrow*2;
            mc.CurrentPosBody.Y += single_Yrow*5;

            for (int i = 0; i < nodeData.Count; i++)
            {
                for (int j = 0; j < nodeData[i].Count; j++)
                {
                    if (j != 0 && j % ((bottomCell / 2)-1)  == 0)
                    {
                        // 2段でもページをまたぐときに改ページする
                        mc.NewPage();
                        mc.CurrentPosHeader.Y += single_Yrow * 2;
                        mc.CurrentPosBody.Y += single_Yrow*5;
                        //count = 0;
                    }

                    if (j == 0 || (j != 0 && j % ((bottomCell / 2) - 1) == 0))
                    {
                        mc.gfx.DrawString("格点", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 4);
                        mc.gfx.DrawString("格点", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x;
                        mc.CurrentPosHeader.Y += single_Yrow;
                        mc.gfx.DrawString("id", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 1);
                        mc.gfx.DrawString("x", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 2);
                        mc.gfx.DrawString("y", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 3);
                        mc.gfx.DrawString("z", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 4);
                        mc.gfx.DrawString("id", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 5);
                        mc.gfx.DrawString("x", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 6);
                        mc.gfx.DrawString("y", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 7);
                        mc.gfx.DrawString("z", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                        mc.CurrentPosHeader.X = mc.x;
                        //count+=3;
                    }

                    mc.gfx.DrawString(nodeData[i][j][0], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    mc.CurrentPosBody.X = mc.x + (currentXposition_values * 1);
                    mc.gfx.DrawString(nodeData[i][j][1], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    mc.CurrentPosBody.X = mc.x + (currentXposition_values * 2);
                    mc.gfx.DrawString(nodeData[i][j][2], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    mc.CurrentPosBody.X = mc.x + (currentXposition_values * 3);
                    mc.gfx.DrawString(nodeData[i][j][3], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    mc.CurrentPosBody.X = mc.x + (currentXposition_values * 4);
                    mc.gfx.DrawString(nodeData[i][j][4], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    mc.CurrentPosBody.X = mc.x + (currentXposition_values * 5);
                    mc.gfx.DrawString(nodeData[i][j][5], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    mc.CurrentPosBody.X = mc.x + (currentXposition_values * 6);
                    mc.gfx.DrawString(nodeData[i][j][6], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    mc.CurrentPosBody.X = mc.x + (currentXposition_values * 7);
                    mc.gfx.DrawString(nodeData[i][j][7], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    mc.CurrentPosBody.X = mc.x;
                    mc.CurrentPosBody.Y += single_Yrow;
                    //count++;
                }
            }
            mc.DataCountKeep(mc.CurrentPosBody.Y);
        }
    }
}

