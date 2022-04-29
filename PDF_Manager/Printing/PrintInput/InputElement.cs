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
    public class Element
    {
        public double E;
        public double G;
        public double Xp;
        public double A;
        public double J;
        public double Iy;
        public double Iz;
        public string n;
    }

    internal class InputElement
    {
        private Dictionary<string, Dictionary<string, Element>> elements = new Dictionary<string, Dictionary<string, Element>>();
        private dataManager helper;

        public void init(dataManager dataManager, Dictionary<string, object> value)
        {
            this.helper = dataManager;

            // elementデータを取得する．
            var target = JObject.FromObject(value["element"]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = target.ElementAt(i).Key;
                var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();

                var _element = new Dictionary<string, Element>();

                for (int j = 0; j < Elem.Count; j++)
                {
                    var id = Elem.ElementAt(j).Key;
                    var item = JObject.FromObject(Elem.ElementAt(j).Value);

                    var e = new Element();

                    e.n = dataManager.TypeChange(item["n"]);
                    e.E = dataManager.getNumeric(item["E"]);
                    e.G = dataManager.getNumeric(item["G"]);
                    e.Xp = dataManager.getNumeric(item["Xp"]);
                    e.A = dataManager.getNumeric(item["A"]);
                    e.J = dataManager.getNumeric(item["J"]);
                    e.Iy = dataManager.getNumeric(item["Iy"]);
                    e.Iz = dataManager.getNumeric(item["Iz"]);

                    _element.Add(id, e);

                }
                this.elements.Add(key, _element);
            }
        }


        public void ElementPDF(PdfDoc mc)
        {
            #region 印刷設定

            // ヘッダーのx方向の余白
            var header_Xspacing = (this.helper.dimension == 3) ?
                new int[,] {
                    { 10, 60, 120, 180, 240, 330, 330, 415 },
                    { 10, 60, 120, 180, 240, 300, 360, 415 }
                } :
                new int[,] {
                    { 10, 85, 180, 0, 260, 360, 0, 0 },
                    { 10, 85, 180, 0, 260, 0, 360, 0 }
                };

            // ボディーのx方向の余白
            var body_Xspacing = (this.helper.dimension == 3) ?
                new int[,] {
                    { 17, 40, 143, 203, 263, 320, 380, 440 },
                    { 17, 75, 143, 203, 263, 320, 380, 440 }
                } :
                new int[,] {
                    { 17, 60, 203, 0, 283, 0, 380, 0 },
                    { 17, 105, 203, 0, 283, 0, 380, 0 }
                };

            //　ヘッダー
            string title;
            string[,] header_content;

            switch (this.helper.language)
            {
                case "en":
                    title = "Material DATA";
                    header_content = (this.helper.dimension == 3) ?
                        new string[,] {
                            {"No","Area","Elastic","Shear Elastic","CTE","Inertia","","Torsion Constant" },
                            {"","A(m2)","E(kN/m2)","G(kN/m2)","","IY(m4)","IZ(m4)","" }
                        } :
                        new string[,] {
                            {"No","Area","Elastic","","CTE","Inertia","","" },
                            {"","A(m2)","E(kN/m2)","","","","(m4)","" }
                        };
                    break;

                case "cn":
                    title = "材料";
                    header_content = (this.helper.dimension == 3) ?
                        new string[,] {
                            {"No","截面面积","弹性系数","剪力弹性系数","膨胀系数","截面二次力矩","","扭转常数" },
                            {"","A(m2)","E(kN/m2)","G(kN/m2)","","IY(m4)","IZ(m4)","" }
                        } :
                        new string[,] {
                            {"No","截面面积","弹性系数","","膨胀系数","截面二次力矩","","" },
                            {"","A(m2)","E(kN/m2)","","","","(m4)","" }
                        };
                    break;

                default:
                    title = "材料データ";
                    header_content = (this.helper.dimension == 3) ? 
                        new string[,] {
                            {"No","断面積","弾性係数","せん断弾性係数","膨張係数","断面二次モーメント","","ねじり剛性" },
                            {"","A(m2)","E(kN/m2)","G(kN/m2)","","y軸周り(m4)","z軸周り(m4)","" }
                        }:
                        new string[,] {
                            {"No","断面積","弾性係数","","膨張係数","断面二次モーメント","","" },
                            {"","A(m2)","E(kN/m2)","","","","(m4)","" }
                        };
                    break;
            }

            #endregion

            #region 印刷する内容を集計する

            List<List<string[]>> element_data = new List<List<string[]>>();
            List<List<string[]>> data = new List<List<string[]>>();

            for (var i = 0; i < elements.Count; i++)
            {
                var key = this.elements.ElementAt(i).Key;
                var Elem = this.elements.ElementAt(i).Value;

                List<string[]> table1 = new List<string[]>();
                List<string[]> table2 = new List<string[]>();

                for (var j = 0; j < Elem.Count; j++)
                {
                    var id = Elem.ElementAt(j).Key;
                    var item = Elem.ElementAt(j).Value;

                    string[] line = Enumerable.Repeat<String>("", 2).ToArray();
                    string[] line1 = Enumerable.Repeat<String>("", 8).ToArray();
                    string[] line2 = Enumerable.Repeat<String>("", 8).ToArray();

                    string name = dataManager.TypeChange(item.n);

                    line[0] = id;
                    line[1] = name;
                    table1.Add(line);

                    line1[0] = id;

                    if (name == "")
                    {
                        line1[1] = dataManager.TypeChange(item.A, 4);
                        line1[2] = dataManager.TypeChange(item.E, 0, "E");
                        if (this.helper.dimension == 3)
                            line1[3] = dataManager.TypeChange(item.G, 0, "E");
                        line1[4] = dataManager.TypeChange(item.Xp, 0, "E");
                        if (this.helper.dimension == 3)
                            line1[5] = dataManager.TypeChange(item.Iy, 6);
                        line1[6] = dataManager.TypeChange(item.Iz, 6);
                        if (this.helper.dimension == 3)
                            line1[7] = dataManager.TypeChange(item.J, 6);
                        table2.Add(line1);

                    } else {
                        line1[1] = dataManager.TypeChange(item.n);
                        table2.Add(line1);

                        line2[0] = "";
                        line2[1] = dataManager.TypeChange(item.A, 4);
                        line2[2] = dataManager.TypeChange(item.E, 0, "E");
                        if (this.helper.dimension == 3)
                            line2[3] = dataManager.TypeChange(item.G, 0, "E");
                        line2[4] = dataManager.TypeChange(item.Xp, 0, "E");
                        if (this.helper.dimension == 3)
                            line2[5] = dataManager.TypeChange(item.Iy, 6);
                        line2[6] = dataManager.TypeChange(item.Iz, 6);
                        if (this.helper.dimension == 3)
                            line2[7] = dataManager.TypeChange(item.J, 6);
                        table2.Add(line2);
                    }

                    element_data.Add(table1);
                    data.Add(table2);

                }
            }

            /*
            // 全行の取得
            int count = 20;
            for (int i = 0; i < title.Count; i++)
            {
                count += (data[i].Count + 5) * mc.single_Yrow + 1;
            }
            // 改ページ判定
            mc.DataCountKeep(count);
            */

            #endregion

            #region 印刷する

            mc.PrintContent(title, 0);
            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            int k = 0;

            for (int i = 0; i < data.Count; i++)
            {

                //  1タイプ内でページをまたぐかどうか
                mc.TypeCount(i, 5, data[i].Count, title);

                // タイプの印刷
                mc.CurrentColumn(0);
                mc.PrintContent(title, 0);
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
            #endregion
        }

        /// <summary>
        /// 材料名を取得する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public string GetElementName(string e)
        {
            string name = "";

            if (e == "" || e == null)
            {
                return name;
            }

            var Elem = this.elements.First().Value;

            if (Elem.ContainsKey(e))
            {
                var item = Elem[e];
                name = item.n;
            }

            return name;
        }

    }

}
