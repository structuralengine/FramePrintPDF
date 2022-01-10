﻿using Newtonsoft.Json.Linq;
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

                for (int j = 0; j < Elem.Count; j++)
                {
                    JToken item = Elem[j];
                   
                    string[] line = new String[7];

                    line[0] = mc.TypeChange(item["n"]);
                    line[1] = mc.TypeChange(item["tx"]);
                    line[2] = mc.TypeChange(item["ty"]); ;
                    line[3] = mc.Dimension(mc.TypeChange(item["tz"]));
                    line[4] = mc.Dimension(mc.TypeChange(item["rx"]));
                    line[5] = mc.Dimension(mc.TypeChange(item["ry"]));
                    line[6] = mc.TypeChange(item["rz"]); ;
                    
                    table.Add(line);
                }
                data.Add(table);
            }
        }

        public void FixNodePDF(PdfDoc mc)
        {
            // 全行の取得
            int count = 2;
            for (int i = 0; i < title.Count; i++)
            {
                count += (data[i].Count + 5) * mc.single_Yrow + 1;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content3D = {
                { "格点", "", "", "", "", "", "" },
                { "No", "TX", "TY", "TZ", "RX", "RY", "RZ" },
                {"","(kN/m)","(kN/m)","(kN/m)","","y軸周り","z軸周り" }
            };

            string[,] header_content2D = {
                { "格点", "", "", "", "", "", "" },
                { "No", "TX", "TY", "", "", "", "RZ" },
                {"","(kN/m)","(kN/m)","","","","z軸周り" }
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D ={
                { 10, 65, 130, 195, 260, 325, 390 },
                { 10, 65, 130, 195, 260, 325, 390 },
                { 10, 65, 130, 195, 260, 325, 390 },
            };

            int[,] header_Xspacing2D ={
                { 10, 90, 180, 0, 0, 0, 270 },
                { 10, 90, 180, 0, 0, 0, 270 },
                { 10, 90, 180, 0, 0, 0, 270 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 82, 147, 212, 277, 342, 407 }
            };

            int[,] body_Xspacing2D = {
                { 17, 102, 197, 0, 0, 0, 287 }
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
