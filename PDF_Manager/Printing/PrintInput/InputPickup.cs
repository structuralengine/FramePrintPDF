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
    internal class InputPickup
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<List<string[]>> data = new List<List<string[]>>();

        public void Pickup(PdfDoc mc, Dictionary<string, object> value_)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["pickup"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            data = new List<List<string[]>>();
            List<string[]> body = new List<string[]>();

            for (int i = 0; i < target.Count; i++)
            {
                string[] line = new String[12];
                line[0] = target.ElementAt(i).Key;

                var item = JObject.FromObject(target.ElementAt(i).Value);

                // 荷重名称
                if (item.ContainsKey("name"))
                {
                    line[1] = mc.TypeChange(item["name"]);
                }
                else
                {
                    line[1] = "";
                }

                //Keyをsortするため
                var itemDic = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                string[] kk = itemDic.Keys.ToArray();
                Array.Sort(kk);

                int count = 0;

                foreach (string key in kk)
                {
                    if (!key.StartsWith("C"))
                    {
                        continue;   
                    }
                    line[count + 2] = mc.TypeChange(item[key]);
                    count++;

                    if (count == 8)
                    {
                        body.Add(line);
                        count = 0;
                        line = new String[12];
                        line[0] = "";
                        line[1] = "";
                      
                    }
                }
                if (count > 0)
                {
                    for (int k = 2; k < 12; k++)
                    {
                        line[k] = line[k] == null ? "" : line[k];
                      
                    }

                    body.Add(line);
                }
            }
            if (body.Count > 0)
            {
                data.Add(body);
            }
        }

        public void PickupPDF(PdfDoc mc)
        {
            int bottomCell = mc.bottomCell;

            // 全行の取得
            int count = 20;
            for (int i = 0; i < data.Count; i++)
            {
                count += (data[i].Count + 2) * mc.single_Yrow;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content = {
                { "No","荷重名称", "C1", "C2", "C3", "C4" , "C5", "C6", "C7", "C8","C9","C10"}
            };

            switch(mc.language)
            {
                case "ja":
                    mc.PrintContent("PickUpデータ", 0);
                break;
                case "en":
                    mc.PrintContent("PickUp Data", 0);
                header_content[0, 1] = "Name of Load";
                break;
            }

            mc.CurrentRow(2);
            mc.CurrentColumn(0);


            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                 { 17, 100,203, 233, 263, 293, 323, 353, 383, 413,443,473},
            };

            mc.Header(header_content, header_Xspacing);

            // ボディーのx方向の余白
            int[,] body_Xspacing = {
                 { 24, 42,208, 238, 268, 298, 328, 358, 388, 418,448,478},
            };

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    for (int l = 0; l < data[i][j].Length; l++)
                    {
                        mc.CurrentColumn(body_Xspacing[0, l]); //x方向移動
                        if (l == 1)
                        {
                            mc.PrintContent(data[i][j][l], 1);  // print
                        }
                        else
                        {
                            mc.PrintContent(data[i][j][l]);  // print
                        }
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

