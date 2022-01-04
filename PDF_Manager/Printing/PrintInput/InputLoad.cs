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
    internal class InputLoad
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();

        public (List<string>, List<List<List<string[]>>>) Load(PdfDoc mc, Dictionary<string, object> value_)
        {
            value = value_;
            var target = JObject.FromObject(value["load"]).ToObject<Dictionary<string, object>>();
            // 集まったデータはここに格納する
            List<string> load_title = new List<string>();
            List<List<List<string[]>>> load_data = new List<List<List<string[]>>>();

            for (int i = 0; i < target.Count; i++)
            {
                var item = JObject.FromObject(target.ElementAt(i).Value);
                var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();

                // タイトルの表示
                if (item.ContainsKey("load_member") || item.ContainsKey("load_node"))
                {
                    load_title.Add("case" + target.ElementAt(i).Key + ":" + item["name"]);
                }

                List<List<string[]>> compile = new List<List<string[]>>();

                //if (i == 5)
                //{
                //    Console.WriteLine("hek");
                //}

                List<string[]> table1 = new List<string[]>();
                if (item.ContainsKey("load_member"))
                {
                    for (int j = 0; j < item["load_member"].Count(); j++)
                    {
                        JToken member = item["load_member"][j];
                        if (member.SelectToken("m1") != null)
                        {
                            string[] line = new string[8];
                            line[0] = mc.TypeChange(member["m1"]);
                            line[1] = mc.TypeChange(member["m2"]);
                            line[2] = mc.TypeChange(member["direction"]);
                            line[3] = mc.TypeChange(member["mark"]);
                            line[4] = mc.TypeChange(member["L1"]);
                            line[5] = mc.TypeChange(member["L2"]);
                            line[6] = mc.TypeChange(member["P1"], 2);
                            line[7] = mc.TypeChange(member["P2"], 2);
                            table1.Add(line);
                        }
                    }
                    compile.Add(table1);
                }
                else
                {
                    compile.Add(null);
                }

                List<string[]> table2 = new List<string[]>();
                if (item.ContainsKey("load_node"))
                {
                    for (int j = 0; j < item["load_node"].Count(); j++)
                    {
                        string[] line = new string[8];
                        JToken node = item["load_node"][j];
                        line[0] = "";
                        line[1] = mc.TypeChange(node["n"]);
                        line[2] = mc.TypeChange(node["tx"], 2);
                        line[3] = mc.TypeChange(node["ty"], 2);
                        line[4] = mc.TypeChange(node["tz"], 2);
                        line[5] = mc.TypeChange(node["rx"], 2);
                        line[6] = mc.TypeChange(node["ry"], 2);
                        line[7] = mc.TypeChange(node["rz"], 2);
                        table2.Add(line);
                    }
                    compile.Add(table2);
                }
                else
                {
                    compile.Add(null);
                }

                load_data.Add(compile);
            }
            return (
               load_title,
               load_data
           );
        }

        public void LoadPDF(PdfDoc mc, List<string> loadTitle, List<List<List<string[]>>> loadData)
        {
            // 全行の取得
            int count = 2;
            for (int i = 0; i < loadTitle.Count; i++)
            {
                int mCount = loadData[i][0] != null ? loadData[i][0].Count : 0;
                int pCount = loadData[i][1] != null ? loadData[i][1].Count : 0;

                count += ((mCount + 5) + (pCount + 5)) * mc.single_Yrow + 1;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー(部材荷重)
            string[,] headerM_content = {
            {"部材荷重","","","","","","","" },
            {"スタート","エンド","方向","マーク","L1","L2","P1","P2" }
            };

            //　ヘッダー(節点荷重)
            string[,] headerP_content = {
            {"節点荷重","","","","","","","" },
            {"","節点番号","Fx","Fy","Fz","Mx","My","Mz" }
            };

            // ヘッダーのx方向の余白（部材荷重）
            int[,] headerM_Xspacing ={
                { 20, 70, 120, 180, 240, 330, 330, 420 },
                { 20, 70, 120, 180, 240, 300, 360, 420 }
            };

            // ヘッダーのx方向の余白（節点荷重）
            int[,] headerP_Xspacing ={
                { 20, 70, 120, 180, 240, 300, 360, 420 },
                { 20, 70, 120, 180, 240, 300, 360, 420 }
            };

            // ボディーのx方向の余白（部材荷重）
            int[,] bodyM_Xspacing = {
              { 27, 80, 123, 184, 255, 315, 375, 435 }
            };

            // ボディーのx方向の余白（節点荷重）
            int[,] bodyP_Xspacing = {
              { 27, 80, 135, 195, 255, 315, 375, 435 }
            };

            // タイトルの印刷
            mc.PrintContent("実荷重データ", 0);
            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            for (int i = 0; i < loadTitle.Count; i++)
            {
                // 部材荷重の印刷
                if (loadData[i][0] != null)
                {
                    if (loadData[i][0].Count != 0)
                    {
                        // 1タイプ内でページをまたぐかどうか
                        mc.TypeCount(i, 5, loadData[i][0].Count, loadTitle[i]);

                        // タイプの印刷
                        mc.CurrentColumn(0);
                        mc.PrintContent(loadTitle[i], 0);
                        mc.CurrentRow(2);

                        // ヘッダーの印刷
                        mc.Header(headerM_content, headerM_Xspacing);

                        for (int k = 0; k < loadData[i][0].Count; k++)
                        {
                            for (int l = 0; l < loadData[i][0][k].Length; l++)
                            {
                                mc.CurrentColumn(bodyM_Xspacing[0, l]); //x方向移動
                                mc.PrintContent(loadData[i][0][k][l]); // print
                            }
                            mc.CurrentRow(1);
                        }
                    }
                }

                // 節点荷重の印刷
                if (loadData[i][1] != null)
                {
                    if (i == 0)
                    {
                        mc.CurrentPos.Y += mc.single_Yrow;
                    }
                    // 1タイプ内でページをまたぐかどうか
                    mc.TypeCount(i, 5, loadData[i][1].Count, loadTitle[i]);

                    // 節点荷重のみの時に，タイプ番号を表示する
                    if (loadData[i][0] == null)
                    {
                        // タイプの印刷
                        mc.CurrentColumn(0);
                        mc.PrintContent(loadTitle[i], 0);
                        mc.CurrentRow(2);
                    }

                    // ヘッダーの印刷
                    mc.Header(headerP_content, headerP_Xspacing);

                    for (int k = 0; k < loadData[i][1].Count; k++)
                    {
                        for (int l = 0; l < loadData[i][1][k].Length; l++)
                        {
                            mc.CurrentColumn(bodyP_Xspacing[0, l]); //x方向移動
                            mc.PrintContent(loadData[i][1][k][l]); // print
                        }
                        mc.CurrentRow(1);
                    }
                }

            }
        }
    }
}
