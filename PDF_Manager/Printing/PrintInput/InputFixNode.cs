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
    internal class InputFixNode
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string> title = new List<string>();
        List<List<string[]>> data = new List<List<string[]>>();

        public void FixNode(PdfDoc mc,Dictionary<string, object> value_)
        {
            value = value_;
            // データを取得する．
            var target = JObject.FromObject(value["fix_node"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはすべてここに格納する
            title = new List<string>();
            data = new List<List<string[]>>();


            for (int i = 0; i < target.Count; i++)
            {
                JArray Elem = JArray.FromObject(target.ElementAt(i).Value);

                // タイトルを入れる．
                title.Add("タイプ" + target.ElementAt(i).Key);

                List<string[]> table = new List<string[]>();

                if (mc.dimension == 3)
                {
                    for (int j = 0; j < Elem.Count; j++)
                    {
                        JToken item = Elem[j];

                        string[] line = new String[7];

                        line[0] = mc.TypeChange(item["n"]);
                        line[1] = mc.TypeChange(item["tx"]);
                        line[2] = mc.TypeChange(item["ty"]); ;
                        line[3] = mc.TypeChange(item["tz"]);
                        line[4] = mc.TypeChange(item["rx"]);
                        line[5] = mc.TypeChange(item["ry"]);
                        line[6] = mc.TypeChange(item["rz"]); ;

                        table.Add(line);
                    }
                    data.Add(table);
                }
                else if(mc.dimension == 2)
                {
                    int bottomCell = mc.bottomCell * 2;

                    // 全部の行数
                    var row = Elem.Count;

                    var page = 0;
                    //var body = new ArrayList()

                    while (true)
                    {
                        if (row > bottomCell)
                        {
                            List<string[]> body = new List<string[]>();
                            var half = bottomCell / 2;
                            for (var l = 0; l < half; l++)
                            {
                                //　各行の配列開始位置を取得する（左段/右段)
                                var j = bottomCell * page + l;
                                var k = bottomCell * page + bottomCell / 2 + l;
                                //　各行のデータを取得する（左段/右段)
                                var targetValue_l = Elem[j];

                                string[] line = new String[8];
                                line[0] = mc.TypeChange(targetValue_l["n"]);
                                line[1] = mc.TypeChange(targetValue_l["tx"], 3);
                                line[2] = mc.TypeChange(targetValue_l["ty"], 3);
                                line[3] = mc.TypeChange(targetValue_l["rz"], 3);

                                var targetValue_r = Elem[k];
                                line[4] = mc.TypeChange(targetValue_r["n"]);
                                line[5] = mc.TypeChange(targetValue_r["tx"], 3);
                                line[6] = mc.TypeChange(targetValue_r["ty"], 3);
                                line[7] = mc.TypeChange(targetValue_r["rz"], 3);
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

                            for (var l = 0; l < row; l++)
                            {
                                //　各行の配列開始位置を取得する（左段/右段)
                                var j = bottomCell * page + l;
                                var k = j + row;
                                //　各行のデータを取得する（左段)
                                var targetValue_l = Elem[j];

                                string[] line = new String[8];
                                line[0] = mc.TypeChange(targetValue_l["n"]);
                                line[1] = mc.TypeChange(targetValue_l["tx"], 3);
                                line[2] = mc.TypeChange(targetValue_l["ty"], 3);
                                line[3] = mc.TypeChange(targetValue_l["rz"], 3);

                                try
                                {
                                    //　各行のデータを取得する（右段)
                                    var targetValue_r = Elem[k];
                                    line[4] = mc.TypeChange(targetValue_r["n"]);
                                    line[5] = mc.TypeChange(targetValue_r["tx"], 3);
                                    line[6] = mc.TypeChange(targetValue_r["ty"], 3);
                                    line[7] = mc.TypeChange(targetValue_r["rz"], 3);
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
            }
        }

        public void FixNodePDF(PdfDoc mc)
        {
            // 全行の取得
            int count = 20;
            for (int i = 0; i < title.Count; i++)
            {
                count += (data[i].Count + 6) * mc.single_Yrow;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content3D = {
                { "格点", "", "変位拘束", "", "", "回転拘束", "" },
                { "No", "X方向", "Y方向", "Z方向", "X軸回り", "Y軸回り", "Z軸回り" },
                {"","(kN/m)","(kN/m)","(kN/m)","(kN・m/rad)","(kN・m/rad)","(kN・m/rad)" }
            };

            string[,] header_content2D = {
                { "格点", "変位拘束", "", "","格点", "変位拘束", "", "" },
                { "No", "X方向", "Y方向", "回転拘束","No", "X方向", "Y方向", "回転拘束" },
                {"","(kN/m)","(kN/m)","(kN・m/rad)","","(kN/m)","(kN/m)","(kN・m/rad)" }
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D ={
                { 10, 65, 130, 195, 260, 325, 390 },
                { 10, 65, 130, 195, 260, 325, 390 },
                { 10, 65, 130, 195, 260, 325, 390 },
            };

            int[,] header_Xspacing2D = {
                { 10, 100, 0, 0, 250, 330, 0,0 },
                { 10, 60, 120, 175, 250, 300, 360,415 },
                { 10, 60, 120, 175, 250, 300, 360,415 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 90, 155, 220, 285, 350, 415 }
            };

            int[,] body_Xspacing2D = {
                { 17, 80, 140, 200, 257,320,380,440 }
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;

            // タイトルの印刷
            mc.PrintContent("支点データ", 0);
            mc.CurrentRow(2);


            int k = 0;

            for (int i = 0; i < data.Count; i++)
            {
                //  1タイプ内でページをまたぐかどうか
                mc.TypeCount(i, 6, data[i].Count, title[i]);

                // タイプの印刷
                mc.CurrentColumn(0);
                mc.PrintContent(title[i], 0);
                mc.CurrentRow(2);


                // ヘッダーの印刷
                mc.Header(header_content, header_Xspacing);

                for (int j = 0; j < data[i].Count; j++)
                {
                    for (int l = 0; l < data[i][j].Length; l++)
                    {
                        mc.CurrentColumn(body_Xspacing[k, l]); //x方向移動
                        mc.PrintContent(data[i][j][l]); // print
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
