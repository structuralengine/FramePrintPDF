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

        public (List<string>, List<List<string[]>>) FixNode(PdfDoc mc,Dictionary<string, object> value_)
        {
            value = value_;
            // elementデータを取得する．
            var target = JObject.FromObject(value["fix_node"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはすべてここに格納する
            List<string> fixnode_title = new List<string>();
            List<List<string[]>> fixnode_data = new List<List<string[]>>();


            for (int i = 0; i < target.Count; i++)
            {
                JArray Elem = JArray.FromObject(target.ElementAt(i).Value);

                // タイトルを入れる．
                fixnode_title.Add("タイプ" + target.ElementAt(i).Key);

                List<string[]> table = new List<string[]>();

                for (int j = 0; j < Elem.Count; j++)
                {
                    JToken item = Elem[j];
                   
                    string[] line = new String[7];

                    line[0] = mc.TypeChange(item["n"], true);
                    line[1] = mc.TypeChange(item["tx"]);
                    line[2] = mc.TypeChange(item["ty"]); ;
                    line[3] = mc.TypeChange(item["tz"]); ;
                    line[4] = mc.TypeChange(item["rx"]); ;
                    line[5] = mc.TypeChange(item["ry"]); ;
                    line[6] = mc.TypeChange(item["rz"]); ;
                    
                    table.Add(line);
                }
                fixnode_data.Add(table);
            }
            return (
                fixnode_title,
                fixnode_data
            );
        }

        public void FixNodePDF(PdfDoc mc, List<string> fixnodeTitle, List<List<string[]>> fixnodeData)
        {
            // 全行の取得
            int count = 2;
            for (int i = 0; i < fixnodeTitle.Count; i++)
            {
                count += (fixnodeData[i].Count + 5) * mc.single_Yrow + 1;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content = {
                { "格点", "", "", "", "", "", "" },
                { "No", "TX", "TY", "TZ", "RX", "RY", "RZ" },
                {"","(kN/m)","(kN/m)","(kN/m)","","y軸周り","z軸周り" }
            };
            // ヘッダーのx方向の余白
            int[,] header_Xspacing ={
                { 0, 40, 80, 120, 160, 200, 240 },
                { 0, 40, 80, 120, 160, 200, 240 },
                { 0, 40, 80, 120, 160, 200, 240 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing = {
                { 0, 40, 80, 120, 160, 200, 240 }
            };

            // タイトルの印刷
            mc.PrintContent("支点データ", 0);
            mc.CurrentRow(2);


            int k = 0;

            for (int i = 0; i < fixnodeData.Count; i++)
            {
                //  1タイプ内でページをまたぐかどうか
                mc.TypeCount(i, 6, fixnodeData[i].Count, fixnodeTitle[i]);

                // タイプの印刷
                mc.CurrentColumn(0);
                mc.PrintContent(fixnodeTitle[i], 0);
                mc.CurrentRow(2);


                // ヘッダーの印刷
                mc.Header(header_content, header_Xspacing);

                for (int j = 0; j < fixnodeData[i].Count; j++)
                {
                    for (int l = 0; l < fixnodeData[i][j].Length; l++)
                    {
                        mc.CurrentColumn(body_Xspacing[k, l]); //x方向移動
                        mc.PrintContent(fixnodeData[i][j][l]); // print
                    }
                    mc.CurrentRow(1); // y方向移動
                }
            }

        }
    }

}
