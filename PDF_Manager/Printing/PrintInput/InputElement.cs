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
        List<string> title = new List<string>();


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
                switch (mc.language)
                {
                    case "ja":
                        title.Add("タイプ" + Elem.ElementAt(i).Key);
                        break;
                    case "en":
                        title.Add("Type" + Elem.ElementAt(i).Key);
                        break;

                }

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

                    var name = mc.TypeChange(item["n"]);

                    line1[0] = Elem.ElementAt(j).Key.ToString();
                    line1[1] = name == "" ? mc.TypeChange(item["A"], 4) : mc.TypeChange(item["n"]);
                    line1[2] = name == "" ? mc.TypeChange(item["E"], 0, "E") : "";
                    line1[3] = mc.Dimension(name == "" ? mc.TypeChange(item["G"], 0, "E") : "");
                    line1[4] = name == "" ? mc.TypeChange(item["Xp"], 0, "E") : "";
                    line1[5] = mc.Dimension(name == "" ? mc.TypeChange(item["Iy"], 6) : "");
                    line1[6] = name == "" ? mc.TypeChange(item["Iz"], 6) : "";
                    line1[7] = mc.Dimension(name == "" ? mc.TypeChange(item["J"], 6) : "");
                    table2.Add(line1);

                    if (name != "")
                    {
                        line2[0] = "";
                        line2[1] = mc.TypeChange(item["A"], 4);
                        line2[2] = mc.TypeChange(item["E"], 0, "E");
                        line2[3] = mc.Dimension(mc.TypeChange(item["G"], 0, "E"));
                        line2[4] = mc.TypeChange(item["Xp"], 0, "E");
                        line2[5] = mc.Dimension(mc.TypeChange(item["Iy"], 6));
                        line2[6] = mc.TypeChange(item["Iz"], 6);
                        line2[7] = mc.Dimension(mc.TypeChange(item["J"], 6));
                        table2.Add(line2);
                    }
                }
                element_data.Add(table1);
                data.Add(table2);
            }
        }

        public void ElementPDF(PdfDoc mc)
        {
            // 全行の取得
            int count = 20;
            for (int i = 0; i < title.Count; i++)
            {
                count += (data[i].Count + 5) * mc.single_Yrow + 1;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            string[,] header_content3D = {
            {"No","断面積","弾性係数","せん断弾性係数","膨張係数","断面二次モーメント","","ねじり剛性" },
            {"","A(m2)","E(kN/m2)","G(kN/m2)","","y軸周り(m4)","z軸周り(m4)","" }
            };

            string[,] header_content2D = {
            {"No","断面積","弾性係数","","膨張係数","断面二次モーメント","","" },
            {"","A(m2)","E(kN/m2)","","","","(m4)","" }
            };


            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D ={
                { 10, 60, 120, 180, 240, 330, 330, 415 },
                { 10, 60, 120, 180, 240, 300, 360, 415 }
            };

            int[,] header_Xspacing2D ={
                { 10, 85, 180, 0, 260, 360, 0, 0 },
                { 10, 85, 180, 0, 260, 0, 360, 0 }
            };

            // ボディーのx方向の余白　
            int[,] body_Xspacing3D = {
                { 17, 40, 143, 203, 263, 320, 380, 440 },
                { 17, 75, 143, 203, 263, 320, 380, 440 }
            };

            int[,] body_Xspacing2D = {
                { 17, 60, 203, 0, 283, 0, 380, 0 },
                { 17, 105, 203, 0, 283, 0, 380, 0 }
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;

            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("材料データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Material DATA", 0);
                    //　ヘッダー
                    header_content3D[0, 1] = "Area";
                    header_content3D[0, 2] = "Elastic";
                    header_content3D[0, 3] = "Shear Elastic";
                    header_content3D[0, 4] = "CTE";
                    header_content3D[0, 5] = "Inertia";
                    header_content3D[0, 7] = "Torsion Constant";
                    header_content3D[1, 5] = "IY";
                    header_content3D[1, 6] = "IZ";

                    header_content2D[0, 1] = "Area";
                    header_content2D[0, 2] = "Elastic";
                    header_content2D[0, 4] = "CTE";
                    header_content2D[0, 5] = "Inertia";
                    break;
            }
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
                    try
                    {
                        // 名称が入っているかAが入っているか．
                        double.Parse(data[i][j][1]);
                    }
                    catch //2段になるとき
                    {
                        double y = mc.CurrentPos.Y + mc.single_Yrow * 2;
                        // 跨ぎそうなら1行あきらめて，次ページへ．
                        if (y > mc.single_Yrow * mc.bottomCell + mc.Margine.Y)
                        {
                            mc.CurrentRow(1);
                        }
                    }

                    for (int l = 0; l < data[i][j].Length; l++)
                    {
                        if (l == 1)
                        {
                            try //材料名称が存在しない→Aなどのパラメータを段下げせずに表示
                            {
                                double.Parse(data[i][j][l]);
                                k = 1;
                                mc.CurrentColumn(body_Xspacing[k, l]); //x方向移動
                                mc.PrintContent(data[i][j][l]); // print
                            }
                            catch //材料名称が存在する
                            {
                                k = 0;
                                mc.CurrentColumn(body_Xspacing[k, l]); //x方向移動
                                mc.PrintContent(data[i][j][l], 1); // print　材料名称：左詰め
                            }
                        }
                        else
                        {
                            mc.CurrentColumn(body_Xspacing[k, l]); //x方向移動
                            mc.PrintContent(data[i][j][l]); // print
                        }
                    }
                    if (!(i == data.Count - 1 && j == data[i].Count - 1))
                    {
                        mc.CurrentRow(1); // y方向移動
                    }
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
