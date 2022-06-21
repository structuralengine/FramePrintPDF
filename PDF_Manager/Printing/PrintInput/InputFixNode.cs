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
using PDF_Manager.Printing.Comon;

namespace PDF_Manager.Printing
{
    public class FixNode
    {
        public string n;    // 節点番号
        public double tx;
        public double ty;
        public double tz;
        public double rx;
        public double ry;
        public double rz;
    }

    internal class InputFixNode
    {
        public const string KEY = "fix_node";

        private Dictionary<int, List<FixNode>> fixnodes = new Dictionary<int, List<FixNode>>();

        public InputFixNode(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(KEY))
                return;

            // データを取得する．
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = dataManager.parseInt(target.ElementAt(i).Key);  // タイプ番号
                JArray FixN = JArray.FromObject(target.ElementAt(i).Value);

                var _fixnode = new List<FixNode>();

                for (int j = 0; j < FixN.Count; j++)
                {
                    JToken item = FixN[j]; 

                    var fn = new FixNode();

                    fn.n = dataManager.toString(item["n"]);
                    fn.tx = dataManager.parseDouble(item["tx"]);
                    fn.ty = dataManager.parseDouble(item["ty"]);
                    fn.tz = dataManager.parseDouble(item["tz"]);
                    fn.rx = dataManager.parseDouble(item["rx"]);
                    fn.ry = dataManager.parseDouble(item["ry"]);
                    fn.rz = dataManager.parseDouble(item["rz"]);

                    _fixnode.Add(fn);

                }
                this.fixnodes.Add(key, _fixnode);
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
            this.dimension = data.dimension;

            if (this.dimension == 3)
            {   // 3次元

                //テーブルの作成
                this.myTable = new Table(3, 7);

                // テーブルの幅
                this.myTable.ColWidth[0] = 45.0; // 格点No
                this.myTable.ColWidth[1] = 70.0;
                this.myTable.ColWidth[2] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[3] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[4] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[5] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[6] = this.myTable.ColWidth[1];

                switch (data.language)
                {
                    case "en":
                        this.title = "Support DATA";
                        this.myTable[0, 0] = "Node";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "TX";
                        this.myTable[2, 1] = "(kN/m)";
                        this.myTable[0, 2] = "Displacement Restraint";
                        this.myTable[1, 2] = "TY";
                        this.myTable[2, 2] = "(kN/m)";
                        this.myTable[1, 3] = "TZ";
                        this.myTable[2, 3] = "(kN/m)";

                        this.myTable[1, 4] = "MX";
                        this.myTable[2, 4] = "(kN・m/rad)";
                        this.myTable[0, 5] = "Rotational Restraint";
                        this.myTable[1, 5] = "MY";
                        this.myTable[2, 5] = "(kN・m/rad)";
                        this.myTable[1, 6] = "MZ";
                        this.myTable[2, 6] = "(kN・m/rad)";

                        break;

                    case "cn":
                        this.title = "支点";
                        this.myTable[0, 0] = "节点";
                        this.myTable[1, 0] = "编码";
                        this.myTable[1, 1] = "X方向";
                        this.myTable[2, 1] = "(kN/m)";
                        this.myTable[0, 2] = "位移约束";
                        this.myTable[1, 2] = "Y方向";
                        this.myTable[2, 2] = "(kN/m)";
                        this.myTable[1, 3] = "Z方向";
                        this.myTable[2, 3] = "(kN/m)";

                        this.myTable[1, 4] = "围绕X轴";
                        this.myTable[2, 4] = "(kN・m/rad)";
                        this.myTable[0, 5] = "旋转约束";
                        this.myTable[1, 5] = "围绕Y轴";
                        this.myTable[2, 5] = "(kN・m/rad)";
                        this.myTable[1, 6] = "围绕Z轴";
                        this.myTable[2, 6] = "(kN・m/rad)";
                        break;

                    default:
                        this.title = "支点データ";
                        this.myTable[0, 0] = "格点";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "X方向";
                        this.myTable[2, 1] = "(kN/m)";
                        this.myTable[0, 2] = "変位拘束";
                        this.myTable[1, 2] = "Y方向";
                        this.myTable[2, 2] = "(kN/m)";
                        this.myTable[1, 3] = "Z方向";
                        this.myTable[2, 3] = "(kN/m)";

                        this.myTable[1, 4] = "X軸回り";
                        this.myTable[2, 4] = "(kN・m/rad)";
                        this.myTable[0, 5] = "回転拘束";
                        this.myTable[1, 5] = "Y軸回り";
                        this.myTable[2, 5] = "(kN・m/rad)";
                        this.myTable[1, 6] = "Z軸回り";
                        this.myTable[2, 6] = "(kN・m/rad)";
                        break;
                }

            }
            else
            {   // 2次元

                //テーブルの作成
                this.myTable = new Table(3, 8);

                // テーブルの幅
                this.myTable.ColWidth[0] = 45.0; // 格点No
                this.myTable.ColWidth[1] = 70.0;
                this.myTable.ColWidth[2] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[3] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[4] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[5] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[6] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[7] = this.myTable.ColWidth[1];


                switch (data.language)
                {
                    case "en":
                        this.title = "Support Data";
                        this.myTable[1, 0] = "Node";
                        this.myTable[2, 0] = "No";
                        this.myTable[1, 1] = "TX";
                        this.myTable[2, 1] = "(kN/m)";
                        this.myTable[1, 2] = "TY";
                        this.myTable[2, 2] = "(kN/m)";
                        this.myTable[1, 3] = "MZ";
                        this.myTable[2, 3] = "(kN・m/rad)";
                        break;

                    case "cn":
                        this.title = "支点";
                        this.myTable[1, 0] = "节点";
                        this.myTable[2, 0] = "编码";
                        this.myTable[1, 1] = "TX";
                        this.myTable[2, 1] = "(kN/m)";
                        this.myTable[1, 2] = "TY";
                        this.myTable[2, 2] = "(kN/m)";
                        this.myTable[1, 3] = "MZ";
                        this.myTable[2, 3] = "(kN・m/rad)";
                        break;

                    default:
                        this.title = "支点データ";
                        this.myTable[1, 0] = "節点";
                        this.myTable[2, 0] = "No";
                        this.myTable[1, 1] = "TX";
                        this.myTable[2, 1] = "(kN/m)";
                        this.myTable[1, 2] = "TY";
                        this.myTable[2, 2] = "(kN/m)";
                        this.myTable[1, 3] = "MZ";
                        this.myTable[2, 3] = "(kN・m/rad)";
                        break;
                }
                this.myTable[1, 4] = this.myTable[1, 0];
                this.myTable[2, 4] = this.myTable[2, 0];
                this.myTable[1, 5] = this.myTable[1, 1];
                this.myTable[2, 5] = this.myTable[2, 1];
                this.myTable[1, 6] = this.myTable[1, 2];
                this.myTable[2, 6] = this.myTable[2, 2];
                this.myTable[1, 7] = this.myTable[1, 3];
                this.myTable[2, 7] = this.myTable[2, 3];

            }
        }


        /// <summary>
        /// 1ページに入れるコンテンツを集計する 3次元の場合
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <param name="rows">行数</param>
        /// <returns>印刷する用の配列</returns>
        private Table getPageContents3D(List<FixNode> target)
        {
            int r = this.myTable.Rows;
            int rows = target.Count;

            // 行コンテンツを生成
            var table = this.myTable.Clone();
            table.ReDim(row: r + rows);

            for (var i = 0; i < rows; i++)
            {
                FixNode item = target[i];

                table[r, 0] = printManager.toString(item.n);
                table.AlignX[r, 0] = "R";

                table[r, 1] = printManager.toString(item.tx); 
                table.AlignX[r, 1] = "R";

                table[r, 2] = printManager.toString(item.ty);
                table.AlignX[r, 2] = "R";

                table[r, 3] = printManager.toString(item.tz);
                table.AlignX[r, 3] = "R";

                table[r, 4] = printManager.toString(item.rx);
                table.AlignX[r, 4] = "R";

                table[r, 5] = printManager.toString(item.ry);
                table.AlignX[r, 5] = "R";

                table[r, 6] = printManager.toString(item.rz);
                table.AlignX[r, 6] = "R";

                r++;
            }

            table.RowHeight[3] = printManager.LineSpacing2; // 表題と body の間

            return table;
        }

        /// <summary>
        /// 1ページに入れるコンテンツを集計する 2次元の場合
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <param name="rows">行数</param>
        /// <returns>印刷する用の配列</returns>
        private Table getPageContents2D(List<FixNode> item1, List<FixNode> item2)
        {
            int header_rows = this.myTable.Rows;
            int rows = Math.Max(item1.Count, item2.Count);

            // 行コンテンツを生成
            var table = this.myTable.Clone();
            table.ReDim(row: header_rows + rows);

            // 表の左側
            int r = header_rows;
            for (var i = 0; i < item1.Count; i++)
            {
                FixNode item = item1[i];

                table[r, 0] = printManager.toString(item.n);
                table.AlignX[r, 0] = "R";

                table[r, 1] = printManager.toString(item.tx);
                table.AlignX[r, 1] = "R";

                table[r, 2] = printManager.toString(item.ty);
                table.AlignX[r, 2] = "R";

                table[r, 3] = printManager.toString(item.rz);
                table.AlignX[r, 3] = "R";
                r++;
            }

            // 表の右側
            r = header_rows;
            for (var i = 0; i < item2.Count; i++)
            {
                FixNode item = item2[i];

                table[r, 4] = printManager.toString(item.n);
                table.AlignX[r, 4] = "R";

                table[r, 5] = printManager.toString(item.tx);
                table.AlignX[r, 5] = "R";

                table[r, 6] = printManager.toString(item.ty);
                table.AlignX[r, 6] = "R";

                table[r, 7] = printManager.toString(item.rz);
                table.AlignX[r, 7] = "R";
                r++;
            }

            table.RowHeight[3] = printManager.LineSpacing2; // 表題と body の間

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

            // 集計開始
            if (this.dimension == 3)
            {   // 3次元
                foreach (var tmp0 in this.fixnodes)
                {
                    var typeNo = string.Format("Type{0}", tmp0.Key); // タイプ番号
                    var titles = new string[] { this.title, typeNo };

                    var tmp1 = new List<FixNode>(tmp0.Value);

                    // 行コンテンツを生成
                    var page = new List<Table>();

                    // 印刷可能な行数
                    var printRows = this.myTable.getPrintRowCount(mc, 2);

                    // 1ページ目に入る行数
                    int rows = printRows[0];

                    if (printRows[0] < tmp1.Count / 2)
                    { // もし行の半分がページに入らなければ、改ページする
                        mc.NewPage();
                        printRows = this.myTable.getPrintRowCount(mc, 2);
                        rows = printRows[1];
                    }

                    while (true)
                    {
                        // 1ページに納まる分のデータをコピー
                        var tmp2 = new List<FixNode>();
                        for (int i = 0; i < rows; i++)
                        {
                            if (tmp1.Count <= 0)
                                break;
                            tmp2.Add(tmp1.First());
                            tmp1.Remove(tmp1.First());
                        }

                        if (tmp2.Count > 0)
                        {
                            var table = this.getPageContents3D(tmp2);

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
            else
            {   // 2次元
                var titles = new string[] { this.title };

                for (var k = 0; k < this.fixnodes.Count; k += 2)
                {
                    var tmp01 = this.fixnodes.ElementAt(k);
                    var typeNo1 = string.Format("Type{0}", tmp01.Key); // 表の左のタイプ番号

                    var tmp02 = (k < this.fixnodes.Count) ? this.fixnodes.ElementAt(k + 1) : new KeyValuePair<int, List<FixNode>>();
                    var typeNo2 = string.Format("Type{0}", tmp02.Key); // 表の右のタイプ番号

                    var tmp1 = new List<FixNode>(tmp01.Value);
                    var tmp2 = new List<FixNode>(tmp02.Value);

                    int body_rows = Math.Max(tmp1.Count, tmp2.Count);

                    // 行コンテンツを生成
                    var page = new List<Table>();

                    // 印刷可能な行数
                    var printRows = this.myTable.getPrintRowCount(mc, 2);

                    // 1ページ目に入る行数
                    int rows = printRows[0];

                    if (printRows[0] < body_rows / 2)
                    { // もし行の半分がページに入らなければ、改ページする
                        mc.NewPage();
                        printRows = this.myTable.getPrintRowCount(mc, 2);
                        rows = printRows[1];
                    }

                    while (true)
                    {
                        // 1ページに納まる分のデータをコピー
                        var tmp3 = new List<FixNode>();
                        for (int i = 0; i < rows; i++)
                        {
                            if (tmp1.Count <= 0)
                                break;
                            tmp3.Add(tmp1.First());
                            tmp1.Remove(tmp1.First());
                        }
                        var tmp4 = new List<FixNode>();
                        for (int i = 0; i < rows; i++)
                        {
                            if (tmp2.Count <= 0)
                                break;
                            tmp4.Add(tmp2.First());
                            tmp2.Remove(tmp2.First());
                        }

                        if (tmp3.Count > 0 || tmp4.Count > 0)
                        {
                            var table = this.getPageContents2D(tmp3, tmp4);
                            table[0, 2] = typeNo1; 
                            table[0, 5] = typeNo2;
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
        }
        #endregion


        /*
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string> title = new List<string>();
        List<List<string[]>> data = new List<List<string[]>>();


            string language = (string)pd.printDatas["language"];

            // 集まったデータはすべてここに格納する
            title = new List<string>();
            data = new List<List<string[]>>();


            for (int i = 0; i < target.Count; i++)
            {
                JArray Elem = JArray.FromObject(target.ElementAt(i).Value);

                // タイトルを入れる．
                switch (language)
                {
                    case "ja":
                        title.Add("タイプ" + target.ElementAt(i).Key);
                        break;
                    case "en":
                        title.Add("Type" + target.ElementAt(i).Key);
                        break;

                }

                List<string[]> table = new List<string[]>();

                if (mc.dimension == 3)
                {
                    for (int j = 0; j < Elem.Count; j++)
                    {
                        JToken item = Elem[j];

                        string[] line = new String[7];

                        line[0] = dataManager.TypeChange(item["n"]);
                        line[1] = dataManager.TypeChange(item["tx"]);
                        line[2] = dataManager.TypeChange(item["ty"]);
                        line[3] = dataManager.TypeChange(item["tz"]);
                        line[4] = dataManager.TypeChange(item["rx"]);
                        line[5] = dataManager.TypeChange(item["ry"]);
                        line[6] = dataManager.TypeChange(item["rz"]);

                        table.Add(line);
                    }
                    data.Add(table);
                }
                else if(mc.dimension == 2)
                {
                    int bottomCell = mc.bottomCell * 2;

                    // 全部の行数
                    var row = Elem.Count;

                    var page = 0;
                    //var body = new ArrayList()

                    while (true)
                    {
                        if (row > bottomCell)
                        {
                            List<string[]> body = new List<string[]>();
                            var half = bottomCell / 2;
                            for (var l = 0; l < half; l++)
                            {
                                //　各行の配列開始位置を取得する（左段/右段)
                                var j = bottomCell * page + l;
                                var k = bottomCell * page + bottomCell / 2 + l;
                                //　各行のデータを取得する（左段/右段)
                                var targetValue_l = Elem[j];

                                string[] line = new String[8];
                                line[0] = dataManager.TypeChange(targetValue_l["n"]);
                                line[1] = dataManager.TypeChange(targetValue_l["tx"], 3);
                                line[2] = dataManager.TypeChange(targetValue_l["ty"], 3);
                                line[3] = dataManager.TypeChange(targetValue_l["rz"], 3);

                                var targetValue_r = Elem[k];
                                line[4] = dataManager.TypeChange(targetValue_r["n"]);
                                line[5] = dataManager.TypeChange(targetValue_r["tx"], 3);
                                line[6] = dataManager.TypeChange(targetValue_r["ty"], 3);
                                line[7] = dataManager.TypeChange(targetValue_r["rz"], 3);
                                body.Add(line);
                            }
                            data.Add(body);
                            row -= bottomCell;
                            page++;
                        }
                        else
                        {
                            List<string[]> body = new List<string[]>();

                            row = row % 2 == 0 ? row / 2 : row / 2 + 1;

                            for (var l = 0; l < row; l++)
                            {
                                //　各行の配列開始位置を取得する（左段/右段)
                                var j = bottomCell * page + l;
                                var k = j + row;
                                //　各行のデータを取得する（左段)
                                var targetValue_l = Elem[j];

                                string[] line = new String[8];
                                line[0] = dataManager.TypeChange(targetValue_l["n"]);
                                line[1] = dataManager.TypeChange(targetValue_l["tx"], 3);
                                line[2] = dataManager.TypeChange(targetValue_l["ty"], 3);
                                line[3] = dataManager.TypeChange(targetValue_l["rz"], 3);

                                try
                                {
                                    //　各行のデータを取得する（右段)
                                    var targetValue_r = Elem[k];
                                    line[4] = dataManager.TypeChange(targetValue_r["n"]);
                                    line[5] = dataManager.TypeChange(targetValue_r["tx"], 3);
                                    line[6] = dataManager.TypeChange(targetValue_r["ty"], 3);
                                    line[7] = dataManager.TypeChange(targetValue_r["rz"], 3);
                                    body.Add(line);
                                }
                                catch
                                {
                                    line[4] = "";
                                    line[5] = "";
                                    line[6] = "";
                                    line[7] = "";
                                    body.Add(line);
                                }
                            }
                            data.Add(body);
                            break;
                        }
                    }
                }
            }
            */

        /*
        public void printPDF(PdfDoc mc)
        {
            // 全行の取得
            int count = 20;
            for (int i = 0; i < title.Count; i++)
            {
                count += (data[i].Count + 6) * mc.single_Yrow;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content3D = {
                { "格点", "", "変位拘束", "", "", "回転拘束", "" },
                { "No", "X方向", "Y方向", "Z方向", "X軸回り", "Y軸回り", "Z軸回り" },
                {"","(kN/m)","(kN/m)","(kN/m)","(kN・m/rad)","(kN・m/rad)","(kN・m/rad)" }
            };

            string[,] header_content2D = {
                { "格点", "変位拘束", "", "","格点", "変位拘束", "", "" },
                { "No", "X方向", "Y方向", "回転拘束","No", "X方向", "Y方向", "回転拘束" },
                {"","(kN/m)","(kN/m)","(kN・m/rad)","","(kN/m)","(kN/m)","(kN・m/rad)" }
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D ={
                { 10, 65, 130, 195, 260, 325, 390 },
                { 10, 65, 130, 195, 260, 325, 390 },
                { 10, 65, 130, 195, 260, 325, 390 },
            };

            int[,] header_Xspacing2D = {
                { 10, 100, 0, 0, 250, 330, 0,0 },
                { 10, 60, 120, 175, 250, 300, 360,415 },
                { 10, 60, 120, 175, 250, 300, 360,415 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 90, 155, 220, 285, 350, 415 }
            };

            int[,] body_Xspacing2D = {
                { 17, 80, 140, 200, 257,320,380,440 }
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;
          
            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("支点データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Support DATA", 0);
                    //　ヘッダー
                    header_content3D[0, 0] = "Node";
                    header_content3D[0, 2] = "Displacement Restraint";
                    header_content3D[0, 5] = "Rotational Restraint";

                    header_content3D[1, 1] = "TX";
                    header_content3D[1, 2] = "TY";
                    header_content3D[1, 3] = "TZ";
                    header_content3D[1, 4] = "MX";
                    header_content3D[1, 5] = "MY";
                    header_content3D[1, 6] = "MZ";


                    header_content2D[0, 0] = "Node";
                    header_content2D[0, 1] = "Displacement Restraint";
                    header_content2D[0, 4] = "Node";
                    header_content2D[0, 5] = "Displacement Restraint";

                    header_content2D[1, 1] = "TX";
                    header_content2D[1, 2] = "TY";
                    header_content2D[1, 3] = "MZ";
                    header_content2D[1, 5] = "TX";
                    header_content2D[1, 6] = "TY";
                    header_content2D[1, 7] = "TZ";
                    break;
            }
            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            int k = 0;

            for (int i = 0; i < data.Count; i++)
            {
                //  1タイプ内でページをまたぐかどうか
                mc.TypeCount(i, 6, data[i].Count, title[i]);

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
                        mc.PrintContent(data[i][j][l]); // print
                    }
                    if (!(i == data.Count - 1 && j == data[i].Count - 1))
                    {
                        mc.CurrentRow(1); // y方向移動
                    }
                }
            }
        }
        */
    }

}
