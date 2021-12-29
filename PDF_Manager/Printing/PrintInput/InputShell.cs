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
    internal class InputShell
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();

        public List<List<string[]>> Shell(PdfDoc mc, Dictionary<string, object> value_)
        {
            value = value_;

            var target = JObject.FromObject(value["shell"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            List<List<string[]>> shell_data = new List<List<string[]>>();
            List<string[]> body = new List<string[]>();


            for (int i = 0; i < target.Count; i++)
            {
                var item = JObject.FromObject(target.ElementAt(i).Value);

                string[] line = new String[5];
                line[0] = mc.TypeChange(item["e"], true);
                
                int count = 0;
                var itemPoints = item["nodes"];

                for (int j = 0; j < itemPoints.Count(); j++)
                {
                    line[count + 1] = mc.TypeChange(itemPoints[count], true);
                    count++;
                    if (count == 4)
                    {
                        body.Add(line);
                        count = 0;
                        line = new string[5];
                        line[0] = "";
                    }
                }
                if (count > 0)
                {
                    for (int k = 1; k < 5; k++)
                    {
                        line[k] = line[k] == null ? "" : line[k];
                    }
                   
                    body.Add(line);
                }
                if (body.Count > 0)
                {
                    shell_data.Add(body);
                }
            }
            shell_data.Add(body);
            return shell_data;
        }

        public void ShellPDF(PdfDoc mc, List<List<string[]>> shellData)
        {
            int bottomCell = mc.bottomCell;

            // 全行の取得
            int count = 2;
            for (int i = 0; i < shellData.Count; i++)
            {
                count += (shellData[i].Count + 2) * mc.single_Yrow;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //  タイトルの印刷
            mc.PrintContent("パネルデータ", 0);
            mc.CurrentRow(2);
            //　ヘッダー
            string[,] header_content = {
                { "材料", "", "頂点No", "", ""},
                { "No", "1", "2", "3", "4"}
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                 { 0, 40, 90, 120, 160 },
                 { 0, 40, 80, 120, 160 } 
            };

            mc.Header(header_content, header_Xspacing);

            // ボディーのx方向の余白
            int[,] body_Xspacing = { { 0, 40, 80, 120, 160 } };

            for (int i = 0; i < shellData.Count; i++)
            {
                for (int j = 0; j < shellData[i].Count; j++)
                {
                    for (int l = 0; l < shellData[i][j].Length; l++)
                    {
                        mc.CurrentColumn(body_Xspacing[0, l]); //x方向移動
                        mc.PrintContent(shellData[i][j][l]);  // print
                    }
                    mc.CurrentRow(1);
                }
            }

        }
    }
}

