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
    internal class ResultDisgAnnexing
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string> title = new List<string>();
        string[] type = {
            "x方向の移動量 最大",
            "x方向の移動量 最小",
            "y方向の移動量 最大",
            "y方向の移動量 最小",
            "z方向の移動量 最大",
            "z方向の移動量 最小",
            "x軸回りの回転角 最大",
            "x軸回りの回転角 最小",
            "y軸回りの回転角 最大",
            "y軸回りの回転角 最小",
            "z軸回りの回転角 最大",
            "Z軸回りの回転角 最小"
        };
        List<List<List<string[]>>> data = new List<List<List<string[]>>>();
        List<List<List<string[]>>> dataCombine = new List<List<List<string[]>>>();
        List<List<List<string[]>>> dataPickup = new List<List<List<string[]>>>();
        List<List<List<string[]>>> dataLL = new List<List<List<string[]>>>();


        public void DisgAnnexing(PdfDoc mc, Dictionary<string, object> value_, string key)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["disg" + key]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            title = new List<string>();
            switch (key)
            {
                case "Combine":
                    dataCombine = new List<List<List<string[]>>>();
                    break;
                case "Pickup":
                    dataPickup = new List<List<List<string[]>>>();
                    break;
                case "LL":
                    dataLL = new List<List<List<string[]>>>();
                    break;
            }

            for (int i = 0; i < target.Count; i++)
            {
                var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();

                // タイトルを入れる．
                title.Add("Case." + target.ElementAt(i).Key);

                List<List<string[]>> table = new List<List<string[]>>();

                for (int j = 0; j < Elem.Count; j++)
                {
                    Dictionary<string, object> elist = JObject.FromObject(Elem.ElementAt(j).Value).ToObject<Dictionary<string, object>>();

                    List<string[]> body = new List<string[]>();

                    for (int k = 0; k < elist.Count; k++)
                    {
                        var item = JObject.FromObject(elist.ElementAt(k).Value); ;
                        string[] line = new String[8];

                        line[0] = mc.TypeChange(elist.ElementAt(k).Key);
                        line[1] = mc.TypeChange(item["dx"], 4);
                        line[2] = mc.TypeChange(item["dy"], 4);
                        line[3] = mc.TypeChange(item["dz"], 4);
                        line[4] = mc.TypeChange(item["rx"], 4);
                        line[5] = mc.TypeChange(item["ry"], 4);
                        line[6] = mc.TypeChange(item["rz"], 4);
                        line[7] = mc.TypeChange(item["case"]);

                        body.Add(line);
                    }
                    table.Add(body);
                }
                switch (key)
                {
                    case "Combine":
                        dataCombine.Add(table);
                        break;
                    case "Pickup":
                        dataPickup.Add(table);
                        break;
                    case "LL":
                        dataLL.Add(table);
                        break;
                }
            }

        }

        public void DisgAnnexingPDF(PdfDoc mc, string key)
        {
            data = new List<List<List<string[]>>>();

            switch (key)
            {
                case "Combine":
                    data = dataCombine;
                    break;
                case "Pickup":
                    data = dataPickup;
                    break;
                case "LL":
                    data = dataLL;
                    break;
            }

            // 全行の取得
            int count = 2;
            for (int i = 0; i < title.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    for (int k = 0; k < data[i][j].Count; k++)
                    {
                        count += (data[i].Count * 5 + data[i][j].Count * 2 + data[i][j][k].Length) * mc.single_Yrow + 1;
                    }
                }
            }

            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content = {
                { "節点", "X-Disp", "Y-Disp", "Z-Disp", "X-Rotation", "Y-Rotation", "Z-Rotation","組み合わせ" },
                { "No", "(mm)", "(mm)", "(mm)", "(mmrad)", "(mmrad)", "(mmrad)","" },
            };
            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                { 10, 50, 100, 150, 200, 260, 320,400 },
                { 10, 50, 100, 150, 200, 260, 320,400 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing = {
                { 17, 65, 115, 165, 215, 275,335,435 }
            };

            // タイトルの印刷
            mc.PrintContent(key + "変位量", 0);
            mc.CurrentRow(2);

            // 印刷
            mc.PrintResultAnnexing(title, type, data, header_content, header_Xspacing, body_Xspacing,20);

        }
    }
}

