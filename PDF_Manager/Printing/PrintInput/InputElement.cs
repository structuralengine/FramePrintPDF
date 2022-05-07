using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public string name;
    }

    internal class InputElement
    {
        public const string KEY = "element";

        private Dictionary<int, Dictionary<string, Element>> elements = new Dictionary<int, Dictionary<string, Element>>();

        /// <summary>
        /// データを読み込む
        /// </summary>
        /// <param name="dataManager"></param>
        /// <param name="value"></param>
        public InputElement(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(KEY))
                return;

            // elementデータを取得する．
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = dataManager.parseInt(target.ElementAt(i).Key);
                var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();

                var _element = new Dictionary<string, Element>();

                for (int j = 0; j < Elem.Count; j++)
                {
                    var id = Elem.ElementAt(j).Key;
                    var item = JObject.FromObject(Elem.ElementAt(j).Value);

                    var e = new Element();

                    e.name = dataManager.toString(item["n"]);
                    e.E = dataManager.parseDouble(item["E"]);
                    e.G = dataManager.parseDouble(item["G"]);
                    e.Xp = dataManager.parseDouble(item["Xp"]);
                    e.A = dataManager.parseDouble(item["A"]);
                    e.J = dataManager.parseDouble(item["J"]);
                    e.Iy = dataManager.parseDouble(item["Iy"]);
                    e.Iz = dataManager.parseDouble(item["Iz"]);

                    _element.Add(id, e);

                }
                this.elements.Add(key, _element);
            }
        }



        #region 印刷処理

        // タイトル
        private string title;
        // 2次元か3次元か
        private int dimension;
        // 項目タイトル
        private string[,] header_content;
        // ヘッダーのx方向の余白
        private double[] header_Xspacing;
        // ボディーのx方向の余白
        private double[] body_Xspacing;
        // ボディーの文字位置
        private XStringFormat[] body_align;

        /// <summary>
        /// 印刷前の初期化処理
        /// </summary>
        private void printInit(PdfDocument mc, PrintData data)
        {
            var X1 = printManager.H1PosX; //表題を印字するX位置  px ピクセル

            this.dimension = data.dimension;
            if (this.dimension == 3)
            {   // 3次元
                this.header_Xspacing = new double[] {
                    X1, X1 + 50, X1 + 110, X1 + 170, X1 + 230, X1 + 320, X1 + 405
                };
                this.body_Xspacing = Array.ConvertAll(this.header_Xspacing, (double x) => { return x + 15; });

                switch (data.language)
                {
                    case "en":
                        this.title = "Material Data";
                        this.header_content = new string[,] {
                            {"No","Area","Elastic","Shear Elastic","CTE","Inertia","","Torsion Constant" },
                            {"","A(m2)","E(kN/m2)","G(kN/m2)","","IY(m4)","IZ(m4)","" }
                        };
                        break;

                    case "cn":
                        this.title = "材料";
                        this.header_content = new string[,] {
                            {"No","截面面积","弹性系数","剪力弹性系数","膨胀系数","截面二次力矩","","扭转常数" },
                            {"","A(m2)","E(kN/m2)","G(kN/m2)","","IY(m4)","IZ(m4)","" }
                        };
                        break;

                    default:
                        this.title = "材料データ";
                        this.header_content = new string[,] {
                            {"No","断面積","弾性係数","せん断弾性係数","膨張係数","断面二次モーメント","","ねじり剛性" },
                            {"","A(m2)","E(kN/m2)","G(kN/m2)","","y軸周り(m4)","z軸周り(m4)","" }
                        };
                        break;
                }
                this.body_align = new XStringFormat[] {
                    XStringFormats.BottomRight, XStringFormats.BottomRight, XStringFormats.BottomRight, XStringFormats.BottomRight, XStringFormats.BottomCenter, XStringFormats.BottomRight, XStringFormats.BottomRight
                };

            }
            else
            {   // 2次元
                this.header_Xspacing = new double[] {
                    X1, X1 + 75, X1 + 170, X1 + 250, X1 + 350, X1 + 300
                };
                this.body_Xspacing = Array.ConvertAll(this.header_Xspacing, (double x) => { return x + 15; });

                switch (data.language)
                {
                    case "en":
                        this.title = "Material Data";
                        this.header_content = new string[,] {
                            {"No","Area","Elastic","CTE","Inertia","Name of Material" },
                            {"","A(m2)","E(kN/m2)","","(m4)","" }
                        };
                        break;

                    case "cn":
                        this.title = "材料";
                        this.header_content = new string[,] {
                            {"No","截面面积","弹性系数","膨胀系数","截面二次力矩","材料名称" },
                            {"","A(m2)","E(kN/m2)","","(m4)","" }
                        };
                        break;

                    default:
                        this.title = "材料データ";
                        this.header_content = new string[,] {
                            {"No","断面積","弾性係数","膨張係数","断面二次モーメント","材料名称", },
                            {"","A(m2)","E(kN/m2)","","(m4)","" }
                        };
                        break;
                }
                this.body_align = new XStringFormat[] {
                    XStringFormats.BottomRight, XStringFormats.BottomRight, XStringFormats.BottomRight, XStringFormats.BottomRight, XStringFormats.BottomRight, XStringFormats.BottomLeft
                };

            }

        }


        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <returns>印刷する用の配列</returns>
        private List<string[]> getPageContents3D(Dictionary<string, Element> target)
        {
            int count = this.header_content.GetLength(1);

            // 行コンテンツを生成
            var table = new List<string[]>();

            for (var i = 0; i < target.Count; i++)
            {
                string No = target.ElementAt(i).Key;
                Element item = target.ElementAt(i).Value;

                // 1行目
                var lines = new string[count];
                lines[0] = No;
                lines[1] = printManager.toString(this.GetElementName(No));
                table.Add(lines);

                // 2行目
                lines = new string[count];
                int j = 1;
                lines[j] = printManager.toString(item.A, 4);
                j++;
                lines[j] = printManager.toString(item.E, 2, "E");
                j++;
                lines[j] = printManager.toString(item.G, 2, "E");
                j++;
                lines[j] = printManager.toString(item.Xp, 2, "E");
                j++;
                lines[j] = printManager.toString(item.Iy, 6);
                j++;
                lines[j] = printManager.toString(item.Iz, 6);
                j++;
                lines[j] = printManager.toString(item.J, 6);
                j++;
                table.Add(lines);

            }
            return table;
        }
        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <returns>印刷する用の配列</returns>
        private List<string[]> getPageContents2D(Dictionary<string, Element> target)
        {
            int count = this.header_content.GetLength(1);

            // 行コンテンツを生成
            var table = new List<string[]>();

            for (var i = 0; i < target.Count; i++)
            {
                string No = target.ElementAt(i).Key;
                Element item = target.ElementAt(i).Value;

                // 1行目
                var lines = new string[count];
                int j = 0;
                lines[j] = No;
                j++;
                lines[j] = printManager.toString(item.A, 4);
                j++;
                lines[j] = printManager.toString(item.E, 2, "E");
                j++;
                lines[j] = printManager.toString(item.Xp, 2, "E");
                j++;
                lines[j] = printManager.toString(item.Iz, 6);
                j++;
                lines[j] = printManager.toString(this.GetElementName(No));
                j++;

                table.Add(lines);

            }
            return table;
        }

        /// <summary>
        /// 印刷する
        /// </summary>
        /// <param name="mc"></param>
        public void printPDF(PdfDocument mc, PrintData data)
        {

            // タイトル などの初期化
            this.printInit(mc, data);

            // 印刷可能な行数
            var printRows = printManager.getPrintRowCount(mc, this.header_content);


            // 集計開始
            foreach(var tmp0 in new Dictionary<int, Dictionary<string, Element>>(this.elements))
            {
                var typeNo = string.Format("Type{0}", tmp0.Key); // タイプ番号
                var titles = new string[] { this.title, typeNo };

                // 行コンテンツを生成
                var page = new List<List<string[]>>();

                // 1ページ目に入る行数
                int rows = printRows[0];

                var tmp1 = tmp0.Value;
                while (true)
                {
                    // 1ページに納まる分のデータをコピー
                    var tmp2 = new Dictionary<string, Element>();
                    for (int i = 0; i < rows; i++)
                    {
                        if (tmp1.Count <= 0)
                            break;
                        tmp2.Add(tmp1.First().Key, tmp1.First().Value);
                        tmp1.Remove(tmp1.First().Key);
                    }
                    if (tmp2.Count <= 0)
                        break;
                    if (this.dimension == 3)
                    {
                        var table = this.getPageContents3D(tmp2);
                        page.Add(table);
                    }
                    else
                    {
                        var table = this.getPageContents2D(tmp2);
                        page.Add(table);
                    }

                    // 2ページ以降に入る行数
                    rows = printRows[1];
                }

                // 表の印刷
                printManager.printContent(mc, page, titles,
                                          this.header_content, this.header_Xspacing,
                                          this.body_Xspacing, this.body_align);
            }

        }

        #endregion



        /*
        /// <summary>
        /// 印刷する
        /// </summary>
        /// <param name="mc"></param>
        public void printPDF(PdfDoc mc)
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

                    string name = dataManager.TypeChange(item.name);

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
                        line1[1] = dataManager.TypeChange(item.name);
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

            // 全行の取得
            int count = 20;
            for (int i = 0; i < title.Count; i++)
            {
                count += (data[i].Count + 5) * mc.single_Yrow + 1;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

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
        */


        /// <summary>
        /// 材料名を取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetElementName(string id)
        {
            string name = "";

            if (id == "" || id == null)
                return name;

            foreach(var _element in this.elements)
            {
                var Elem = _element.Value;
                if (Elem.ContainsKey(id))
                {
                    var item = Elem[id];
                    name = item.name;

                    if (name != "" || name != null)
                        break;  // 有効な名前が見つかったら forループを抜ける
                }
            }

            return name;
        }

    }

}
