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
    internal class InputElement
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        private List<List<string[]>> element_data = new List<List<string[]>>();
        List<List<string[]>> data = new List<List<string[]>>();
        List<string>title = new List<string>();


        public void Element(PdfDoc mc, Dictionary<string, object> value_)
        {
            value = value_;
            // elementデータを取得する．
            var target = JObject.FromObject(value["element"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはすべてここに格納する
            title = new List<string>();
            element_data = new List<List<string[]>>();
            data = new List<List<string[]>>();


            for (int i = 0; i < target.Count; i++)
            {
                var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                // タイトルを入れる．
                title.Add("タイプ" + Elem.ElementAt(i).Key);

                List<string[]> table1 = new List<string[]>();
                List<string[]> table2 = new List<string[]>();

                for (int j = 0; j < Elem.Count; j++)
                {
                    var item = JObject.FromObject(Elem.ElementAt(j).Value);

                    string[] line = new String[2];
                    string[] line1 = new String[8];
                    string[] line2 = new String[8];

                    line[0] = Elem.ElementAt(j).Key.ToString();
                    line[1] = mc.TypeChange(item["n"]);
                    table1.Add(line);

                    line1[0] = Elem.ElementAt(j).Key.ToString();
                    line1[1] = mc.TypeChange(item["n"]);
                    line1[2] = "";
                    line1[3] = "";
                    line1[4] = "";
                    line1[5] = "";
                    line1[6] = "";
                    line1[7] = "";
                    table2.Add(line1);

                    line2[0] = "";
                    line2[1] = mc.TypeChange(item["A"], 4);
                    line2[2] = mc.TypeChange(item["E"], 0, "E");
                    line2[3] = mc.TypeChange(item["G"], 0, "E"); ;
                    line2[4] = mc.TypeChange(item["Xp"], 0, "E"); ;
                    line2[5] = mc.TypeChange(item["Iy"], 6);
                    line2[6] = mc.TypeChange(item["Iz"], 6);
                    line2[7] = mc.TypeChange(item["J"], 6);
                    table2.Add(line2);
                }
                element_data.Add(table1);
                data.Add(table2);
            }
        }

        public void ElementPDF(PdfDoc mc)
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
            {"No","A","E","G","ESP","断面二次モーメント","","ねじり剛性" },
            {"","(m2)","(kN/m2)","(kN/m2)","","y軸周り","z軸周り","" }
            };
            // ヘッダーのx方向の余白
            int[,] header_Xspacing ={
                { 10, 60, 120, 180, 240, 330, 330, 420 },
                { 10, 60, 120, 180, 240, 300, 360, 420 }
            };

            // ボディーのx方向の余白　
            int[,] body_Xspacing = {
                { 17, 40, 0, 0, 0, 0, 0, 0 },
                { 17, 75, 143, 203, 263, 320, 380, 440 }
            };

            // タイトルの印刷
            mc.PrintContent("材料データ", 0);
            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            int k = 0;

            for (int i = 0; i < data.Count; i++)
            {
                //  1タイプ内でページをまたぐかどうか
                mc.TypeCount(i, 5, data[i].Count, title[i]);

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
                        if (l == 1 && k == 0) // 材料名称のみ左詰め
                        {
                            mc.PrintContent(data[i][j][l], 1); // print
                        }
                        else
                        {
                            mc.PrintContent(data[i][j][l]); // print
                        }
                    }
                    mc.CurrentRow(1); // y方向移動
                    k = (j + 1) % 2 == 0 ? 0 : 1; //　x方向余白の切り替え
                }
            }

        }

        public string GetElementName(string e)
        {
            if (e == "" || e == null)
            {
                return "";
            }

            var row = element_data[0];
            string[] target = row.Find(n =>
            {
                return n[0].ToString() == e;
            }

            );
            string name = "";
            if (target != null)
            {
                name = target[1].ToString() != null ? target[1] : "";
            }

            return name;
        }


    }

}
