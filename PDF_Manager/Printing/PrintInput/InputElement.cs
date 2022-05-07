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
                    X1, X1 + 50, X1 + 110, X1 + 170, X1 + 230, X1 + 300, X1 + 360, X1 + 420
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
                    XStringFormats.BottomRight, XStringFormats.BottomRight, XStringFormats.BottomRight, 
                    XStringFormats.BottomRight, XStringFormats.BottomCenter, XStringFormats.BottomRight, 
                    XStringFormats.BottomRight, XStringFormats.BottomRight
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

                    if (tmp2.Count > 0)
                    {
                        var table = (this.dimension == 3) ? this.getPageContents3D(tmp2):
                                                            this.getPageContents2D(tmp2);
                        page.Add(table);
                    }
                    else if (tmp1.Count <= 0)
                    {
                        break;
                    }
                    else
                    { // 印刷するものもない
                        mc.NewPage();
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
