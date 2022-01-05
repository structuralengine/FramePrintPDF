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
    internal class InputLoadName
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string[]> data = new List<string[]>();

        public void LoadName(PdfDoc mc, Dictionary<string, object> value_)
        {
            value = value_;

            var target = JObject.FromObject(value["load"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            data = new List<string[]>();

            for (int i = 0; i < target.Count; i++)
            {
                var item = JObject.FromObject(target.ElementAt(i).Value);

                string[] line = new String[8];
                line[0] = target.ElementAt(i).Key;
                line[1] = mc.TypeChange(item["rate"]);
                line[2] = mc.TypeChange(item["symbol"]);
                line[3] = mc.TypeChange(item["name"]);
                line[4] = mc.TypeChange(item["fix_node"]);
                line[5] = mc.TypeChange(item["element"]);
                line[6] = mc.TypeChange(item["fix_member"]);
                line[7] = mc.TypeChange(item["joint"]);
                data.Add(line);
            }
        }

        public void LoadNamePDF(PdfDoc mc)
        {
            int bottomCell = mc.bottomCell;

            // 全行数の取得
            double count = (data.Count + ((data.Count / bottomCell) + 1) * 4) * mc.single_Yrow;
            //  改ページ判定
            mc.DataCountKeep(count);// 全行の取得

            //  タイトルの印刷
            mc.PrintContent("基本荷重データ", 0);
            mc.CurrentRow(2);

            //　ヘッダー
            string[,] header_content = {
                { "Case", "割増", "", "","","構造系条件","",""},
                { "No", "係数", "記号", "荷重名称", "支点","断面","バネ","結合"}
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                 { 10, 50, 100, 205, 300,397,380,420 },
                 { 10, 50, 100, 215, 355,380,405,430 },
            };

            mc.Header(header_content, header_Xspacing);

            // ボディーのx方向の余白
            int[,] body_Xspacing = {
               { 17, 57, 85, 140, 362,387,412,437 },
            };

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Length; j++)
                {
                    mc.CurrentColumn(body_Xspacing[0, j]); //x方向移動
                    if (j == 2 || j == 3) // 記号，名称のみ左詰め
                    {
                        mc.PrintContent(data[i][j], 1);  // print
                    }
                    else
                    {
                        mc.PrintContent(data[i][j]);  // print
                    }
                }
                mc.CurrentRow(1);
            }

        }
    }
}

