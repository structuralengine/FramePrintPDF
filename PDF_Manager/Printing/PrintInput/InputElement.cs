﻿using Newtonsoft.Json.Linq;
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
        // テーブル
        private Table myTable;

        /// <summary>
        /// 印刷前の初期化処理
        /// </summary>
        private void printInit(PdfDocument mc, PrintData data)
        {
            var X1 = printManager.H1PosX; //表題を印字するX位置  px ピクセル

            this.dimension = data.dimension;
            if (this.dimension == 3)
            {   // 3次元

                //テーブルの作成
                this.myTable = new Table(2, 8);

                // テーブルの幅
                this.myTable.ColWidth[0] = 45.0; // 材料番号 
                for(int i=1; i<8; ++i)
                    this.myTable.ColWidth[i] = 60.0;

                switch (data.language)
                {
                    case "en":
                        this.title = "Material Data";
                        this.myTable[0, 0] = "No";
                        this.myTable[0, 1] = "Area";
                        this.myTable[1, 1] = "A(m2)";
                        this.myTable[0, 2] = "Elastic";
                        this.myTable[1, 2] = "E(kN/m2)";
                        this.myTable[0, 3] = "Shear Elastic";
                        this.myTable[1, 3] = "G(kN/m2)";
                        this.myTable[0, 4] = "CTE";
                        this.myTable[0, 5] = "　　Inertia";
                        this.myTable[1, 5] = "IY(m4)";
                        this.myTable[1, 6] = "IZ(m4)";
                        this.myTable[0, 7] = "Torsion Constant";
                        break;

                    case "cn":
                        this.title = "材料";
                        this.myTable[0, 0] = "No";
                        this.myTable[0, 1] = "截面面积";
                        this.myTable[1, 1] = "A(m2)";
                        this.myTable[0, 2] = "弹性系数";
                        this.myTable[1, 2] = "E(kN/m2)";
                        this.myTable[0, 3] = "剪力弹性系数";
                        this.myTable[1, 3] = "G(kN/m2)";
                        this.myTable[0, 4] = "膨胀系数";
                        this.myTable[0, 5] = "　　截面二次力矩";
                        this.myTable[1, 5] = "IY(m4)";
                        this.myTable[1, 6] = "IZ(m4)";
                        this.myTable[0, 7] = "扭转常数";
                        break;

                    default:
                        this.title = "材料データ";
                        this.myTable[0, 0] = "No";
                        this.myTable[0, 1] = "断面積";
                        this.myTable[1, 1] = "A(m2)";
                        this.myTable[0, 2] = "弾性係数";
                        this.myTable[1, 2] = "E(kN/m2)";
                        this.myTable[0, 3] = "せん断弾性係数";
                        this.myTable[1, 3] = "G(kN/m2)";
                        this.myTable[0, 4] = "膨張係数";
                        this.myTable[0, 5] = "　　断面二次モーメント";
                        this.myTable[1, 5] = "IY(m4)";
                        this.myTable[1, 6] = "IZ(m4)";
                        this.myTable[0, 7] = "ねじり剛性";
                        break;
                }

                // 表題の文字位置
                this.myTable.AlignX[0, 5] = "L";    // 左寄せ

            }
            else
            {   // 2次元

                //テーブルの作成
                this.myTable = new Table(2, 6);

                // テーブルの幅
                this.myTable.ColWidth[0] = 45.0; // 材料番号 
                this.myTable.ColWidth[1] = 70.0;
                this.myTable.ColWidth[2] = 70.0;
                this.myTable.ColWidth[3] = 70.0;
                this.myTable.ColWidth[4] = 100.0;

                switch (data.language)
                {
                    case "en":
                        this.title = "Material Data";
                        this.myTable[0, 0] = "No";
                        this.myTable[0, 1] = "Area";
                        this.myTable[1, 1] = "A(m2)";
                        this.myTable[0, 2] = "Elastic";
                        this.myTable[1, 2] = "E(kN/m2)";
                        this.myTable[0, 3] = "CTE";
                        this.myTable[0, 4] = "Inertia";
                        this.myTable[1, 4] = "I(m4)";
                        this.myTable[0, 5] = "    Name of Material";
                        break;

                    case "cn":
                        this.title = "材料";
                        this.myTable[0, 0] = "编码";
                        this.myTable[0, 1] = "截面面积";
                        this.myTable[1, 1] = "A(m2)";
                        this.myTable[0, 2] = "弹性系数";
                        this.myTable[1, 2] = "E(kN/m2)";
                        this.myTable[0, 3] = "膨胀系数";
                        this.myTable[0, 4] = "截面二次力矩";
                        this.myTable[1, 4] = "I(m4)";
                        this.myTable[0, 5] = "    材料名称";
                        break;

                    default:
                        this.title = "材料データ";
                        this.myTable[0, 0] = "No";
                        this.myTable[0, 1] = "断面積";
                        this.myTable[1, 1] = "A(m2)";
                        this.myTable[0, 2] = "弾性係数";
                        this.myTable[1, 2] = "E(kN/m2)";
                        this.myTable[0, 3] = "膨張係数";
                        this.myTable[0, 4] = "断面二次ﾓｰﾒﾝﾄ";
                        this.myTable[1, 4] = "I(m4)";
                        this.myTable[0, 5] = "    材料名称";
                        break;
                }

                // 表題の文字位置
                this.myTable.AlignX[0, 5] = "L";    // 左寄せ

            }

        }


        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <returns>印刷する用の配列</returns>
        private Table getPageContents(Dictionary<string, Element> target)
        {
            int r = this.myTable.Rows;
            int rows = target.Count;

            // 行コンテンツを生成
            var table = this.myTable.Clone();

            if (this.dimension == 3)
            {
                table.ReDim(row: r + rows * 2);

                for (var i = 0; i < rows; i++)
                {
                    string No = target.ElementAt(i).Key;
                    Element item = target.ElementAt(i).Value;

                    // 1行目
                    table[r, 0] = No;
                    table.AlignX[r, 0] = "R";

                    table[r, 1] = "　" + printManager.toString(this.GetElementName(No)); // 材料名称
                    table.AlignX[r, 1] = "L";
                    r++;

                    // 2行目
                    table[r, 1] = (item.A < 999) ? printManager.toString(item.A, 4) : printManager.toString(item.A);
                    table.AlignX[r, 1] = "R";

                    table[r, 2] = printManager.toString(item.E, 2, "E");
                    table.AlignX[r, 2] = "R";

                    table[r, 3] = printManager.toString(item.G, 2, "E");
                    table.AlignX[r, 3] = "R";

                    table[r, 4] = printManager.toString(item.Xp, 2, "E");
                    table.AlignX[r, 4] = "R";

                    table[r, 5] = (item.Iy < 999) ? printManager.toString(item.Iy, 6) : printManager.toString(item.Iy);
                    table.AlignX[r, 5] = "R";

                    table[r, 6] = (item.Iz < 999) ? printManager.toString(item.Iz, 6) : printManager.toString(item.Iz);
                    table.AlignX[r, 6] = "R";

                    table[r, 7] = (item.J < 999) ? printManager.toString(item.J, 6) : printManager.toString(item.J);
                    table.AlignX[r, 7] = "R";

                    r++;
                }
            }
            else
            {
                table.ReDim(row: r + rows);
                for (var i = 0; i < rows; i++)
                {
                    string No = target.ElementAt(i).Key;
                    Element item = target.ElementAt(i).Value;

                    table[r, 0] = No;
                    table.AlignX[r, 0] = "R";

                    table[r, 1] = printManager.toString(item.A, 4);
                    table.AlignX[r, 1] = "R";

                    table[r, 2] = printManager.toString(item.E, 2, "E");
                    table.AlignX[r, 2] = "R";

                    table[r, 3] = printManager.toString(item.Xp, 2, "E");
                    table.AlignX[r, 3] = "R";

                    table[r, 4] = printManager.toString(item.Iz, 6);
                    table.AlignX[r, 4] = "R";

                    table[r, 5] = "    " + printManager.toString(this.GetElementName(No)); // 材料名称
                    table.AlignX[r, 5] = "L";

                    r++;
                }

            }

            table.RowHeight[2] = printManager.LineSpacing2; // 表題と body の間

            return table;
        }


        /// <summary>
        /// 印刷する
        /// </summary>
        /// <param name="mc"></param>
        public void printPDF(PdfDocument mc, PrintData data)
        {

            if (this.elements.Count == 0)
                return;

            // タイトル などの初期化
            this.printInit(mc, data);

            // 印刷可能な行数
            var printRows = myTable.getPrintRowCount(mc, 2);

            if(this.dimension == 3)
            {   // 3次元は、１データにつき２行なので印刷可能な行数は半分になる
                for(var i=0; i<printRows.Length; ++i)
                    printRows[i] = (int)(printRows[i] / 2);
            }

            // 集計開始
            foreach(var tmp0 in this.elements)
            {
                var typeNo = string.Format("Type{0}", tmp0.Key); // タイプ番号
                var titles = new string[] { this.title, typeNo };

                // 行コンテンツを生成
                var page = new List<Table>();

                // 1ページ目に入る行数
                int rows = printRows[0];

                var tmp1 = new Dictionary<string, Element>(tmp0.Value);
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
                        var table =  this.getPageContents(tmp2);

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
                printManager.printTableContentsOnePage(mc, page, titles);

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
