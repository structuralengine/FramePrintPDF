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
    internal class ResultFsec
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string> title = new List<string>();
        List<List<string[]>> data = new List<List<string[]>>();

        public void Fsec(PdfDoc mc, Dictionary<string, object> value_)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["fsec"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            title = new List<string>();
            data = new List<List<string[]>>();

            for (int i = 0; i < target.Count; i++)
            {
                JArray Elem = JArray.FromObject(target.ElementAt(i).Value);

                // タイトルを入れる．
                title.Add("Case." + target.ElementAt(i).Key);

                List<string[]> table = new List<string[]>();

                for (int j = 0; j < Elem.Count; j++)
                {
                    JToken item = Elem[j];

                    string[] line = new String[9];

                    line[0] = mc.TypeChange(item["m"]);
                    line[1] = mc.TypeChange(item["n"]);
                    line[2] = mc.TypeChange(item["l"], 3);
                    line[3] = mc.TypeChange(item["fx"], 2);
                    line[4] = mc.TypeChange(item["fy"], 2);
                    line[5] = mc.TypeChange(item["fz"], 2);
                    line[6] = mc.TypeChange(item["mx"], 2);
                    line[7] = mc.TypeChange(item["my"], 2);
                    line[8] = mc.TypeChange(item["mz"], 2);

                    table.Add(line);
                }
                data.Add(table);
            }

        }

        public void FsecPDF(PdfDoc mc)
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
            string[,] header_content = {
                { "部材", "節点","", "FX", "FY", "FZ", "MX", "MY","MZ" },
                { "No", "No", "DIST", "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)" },
            };
            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                { 10, 50, 105, 160, 210, 260, 310,360,410 },
                { 10, 50, 105, 160, 210, 260, 310,360,410 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing = {
                { 17, 57, 118, 174, 224, 274, 324,374,424 }
            };

            // タイトルの印刷
            mc.PrintContent("断面力", 0);
            mc.CurrentRow(2);

            // 印刷
            mc.PrintResultBasic(title, data, header_content, header_Xspacing, body_Xspacing);
        }
    }
}

