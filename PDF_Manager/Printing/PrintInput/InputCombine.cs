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
using PDF_Manager.Comon;

namespace PDF_Manager.Printing
{
    public class Combine
    {
        public string name;
        public Dictionary<string, double> coef = new Dictionary<string, double>();
    }

    internal class InputCombine
    {
        private Dictionary<int, Combine> conbines = new Dictionary<int, Combine>();

        public InputCombine(PrintData pd, Dictionary<string, object> value)
        {
            // データを取得する．
            var target = JObject.FromObject(value["combine"]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                // conbineNo
                var key = target.ElementAt(i).Key;
                int index = dataManager.parseInt(key);

                // define を構成する 基本荷重No群
                var item = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();

                var _combine = new Combine();
                for (int j = 0; j < item.Count; j++)
                {
                    var id = item.ElementAt(j).Key;  // "C1", "C2"...
                    var val = (double)item.ElementAt(j).Value;

                    if (id.Contains("name"))
                    {
                        _combine.name = val.ToString();
                    }
                    else if (id.Contains("C"))
                    {
                        double coef = (double)val;
                        _combine.coef.Add(id, coef);
                    }
                }
                this.conbines.Add(index, _combine);
            }
        }

    }
}
    /*
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<List<string[]>> data = new List<List<string[]>>();

        public void init(PdfDoc mc, Dictionary<string, object> value_)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["combine"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            data = new List<List<string[]>>();
            List<string[]> body = new List<string[]>();


            for (int i = 0; i < target.Count; i++)
            {
                string[] line1 = new String[10];
                string[] line2 = new String[10];
                line1[0] = target.ElementAt(i).Key;
                line2[0] = "";

                var item = JObject.FromObject(target.ElementAt(i).Value);

                // 荷重名称
                if (item.ContainsKey("name"))
                {
                    line1[1] = dataManager.TypeChange(item["name"]);
                }
                else
                {
                    line1[1] = "";
                }
                line2[1] = "";

                //Keyをsortするため
                var itemDic = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                string[] kk = itemDic.Keys.ToArray();
                Array.Sort(kk);

                int count = 0;

                for (int j = 0; j < kk.Length - 2; j++)
                {
                    line1[count + 2] = kk[j].Replace("C", "");
                    line2[count + 2] = dataManager.TypeChange(item[kk[j]], 2);
                    count++;

                    if (count == 8)
                    {
                        body.Add(line1);
                        body.Add(line2);
                        count = 0;
                        line1 = new String[10];
                        line2 = new String[10];
                        line1[0] = "";
                        line1[1] = "";
                        line2[0] = "";
                        line2[1] = "";
                    }
                }
                if (count > 0)
                {
                    for (int k = 2; k < 10; k++)
                    {
                        line1[k] = line1[k] == null ? "" : line1[k];
                        line2[k] = line2[k] == null ? "" : line2[k];
                    }

                    body.Add(line1);
                    body.Add(line2);
                }
            }
            if (body.Count > 0)
            {
                data.Add(body);
            }
        }

        public void CombinePDF(PdfDoc mc)
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
                { "CombNo","荷重名称", "C1", "C2", "C3", "C4" , "C5", "C6", "C7", "C8"}
            };

            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("Combineデータ", 0);
                    break;
                case "en":
                    mc.PrintContent("Combine DATA", 0);
                    header_content[0, 1] = "Name of Load";
                    break ;
            }

            mc.CurrentRow(2);

            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                 { 16, 100,203, 233, 263, 293, 323, 353, 383, 413},
            };

            mc.Header(header_content, header_Xspacing);

            // ボディーのx方向の余白
            int[,] body_Xspacing = {
                 { 23, 42,210, 240, 270, 300, 330, 360, 390, 420},
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

    */