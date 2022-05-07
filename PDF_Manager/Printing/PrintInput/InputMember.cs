using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.Comon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDF_Manager.Printing
{
    public class Member
    {
        public string ni; // 節点番号
        public string nj;
        public string e;  // 材料番号
        public double cg; // コードアングル

    }


    internal class InputMember
    {
        public const string KEY = "member";

        private Dictionary<string, Member> members = new Dictionary<string, Member>();

        public InputMember(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(KEY))
                return;

            // memberデータを取得する
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = target.ElementAt(i).Key;
                var item = JObject.FromObject(target.ElementAt(i).Value);

                var m = new Member();
                m.ni = dataManager.toString(item["ni"]);
                m.nj = dataManager.toString(item["nj"]);
                m.e = dataManager.toString(item["e"]);
                m.cg = dataManager.parseDouble(item["cg"]);
                this.members.Add(key, m);
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
                    X1, X1 + 70, X1 + 140, X1 + 210, X1 + 280, X1 + 350, X1 + 420, X1 + 490
                };
                this.body_Xspacing = Array.ConvertAll(this.header_Xspacing, (double x) => { return x + 15; });

                switch (data.language)
                {
                    case "en":
                        this.title = "Member Data";
                        this.header_content = new string[,] {
                            { "", "Node", "", "Distance", "Material No.", "Angle of Rotation" , "Name of Material"},
                            { "No", "Node-I", "Node-J", "(m)", "", "(°)" , ""}
                        };
                        break;

                    case "cn":
                        this.title = "构件";
                        this.header_content = new string[,] {
                            { "", "节点", "", "构件长", "材料编码", "转动角度" , "材料名称"},
                            { "No", "I端", "J端", "(m)", "", "(°)" , ""}
                        };
                        break;

                    default:
                        this.title = "部材データ";
                        this.header_content = new string[,] {
                            { "", "節点", "", "L", "材料番号", "コードアングル" , "材料名称"},
                            { "No", "I端", "J端", "(m)", "", "(°)" , ""}
                        };
                        break;
                }
            }
            else
            {   // 2次元
                this.header_Xspacing = new double[] {
                    X1, X1 + 60, X1 + 120, X1 + 180, X1 + 240, X1 + 300, X1 + 360, X1 + 420, X1 + 480
                };
                this.body_Xspacing = Array.ConvertAll(this.header_Xspacing, (double x) => { return x + 15; });

                switch (data.language)
                {
                    case "en":
                        this.title = "Member Data";
                        this.header_content = new string[,] {
                            { "", "Node", "", "Distance", "Material No.",  "Name of Material"},
                            { "No", "Node-I", "Node-J", "(m)", "" , ""}
                        };
                        break;

                    case "cn":
                        this.title = "构件";
                        this.header_content = new string[,] {
                            { "", "节点", "", "构件长", "材料编码" , "材料名称"},
                            { "No", "I端", "J端", "(m)", "" , ""}
                        };
                        break;

                    default:
                        this.title = "部材データ";
                        this.header_content = new string[,] {
                            { "", "節点", "", "L", "材料番号" , "材料名称"},
                            { "No", "I端", "J端", "(m)", "",  ""}
                        };
                        break;
                }
            }

        }

        /// <summary>
        /// 何行印刷できるか調べる
        /// </summary>
        /// <returns>
        /// return[0] = 1ページ目の印刷可能行数, 
        /// return[1] = 2ページ目以降の印刷可能行数
        /// </returns>
        private int[] getPrintRowCount(PdfDocument mc)
        {
            // タイトルの印字高さ + 改行高
            double H1 = printManager.FontHeight + printManager.LineSpacing1;

            // 表題の印字高さ + 改行高
            double H2 = this.header_content.GetLength(0) * printManager.FontHeight + printManager.LineSpacing2;

            // 1行当りの高さ + 改行高
            double H3 = printManager.LineSpacing3;

            // 2ページ目以降（ページ全体を使ってよい場合）の行数
            double Hx = mc.currentPageSize.Height;
            Hx -= H1;
            Hx -= H2;
            int rows2 = (int)(Hx / H3); // 切り捨て

            // 1ページ目（現在位置から）の行数
            Hx -= mc.contentY;
            int rows1 = (int)(Hx / H3); // 切り捨て

            return new int[] { rows1, rows2 };
        }


        /// <summary>
        /// 1ページに入れるコンテンツを集計する
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <param name="rows">行数</param>
        /// <returns>印刷する用の配列</returns>
        private List<string[]> getPageContents(Dictionary<string, Vector3> target, int rows, int columns)
        {
            int count = this.header_content.GetLength(1);
            int c = count / columns;

            // 行コンテンツを生成
            var table = new List<string[]>();

            for (var i = 0; i < rows; i++)
            {
                var lines = new string[count];

                for (var j = 0; j < columns; j++)
                {
                    int index = i + (rows * j);

                    if (target.Count <= index)
                        continue;

                    string No = target.ElementAt(index).Key;
                    Vector3 XYZ = target.ElementAt(index).Value;

                    lines[0 + c * j] = No;
                    lines[1 + c * j] = printManager.toString(XYZ.x, 3);
                    lines[2 + c * j] = printManager.toString(XYZ.y, 3);
                    if (this.dimension == 3)
                        lines[3 + c * j] = printManager.toString(XYZ.z, 3);
                }
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
            var printRows = this.getPrintRowCount(mc);

            // 行コンテンツを生成
            var page = new List<List<string[]>>();

            // 1ページ目に入る行数
            int rows = printRows[0];
            /*
            // 集計開始
            var tmp1 = new Dictionary<string, Vector3>(this.nodes); // clone
            while (true)
            {
                // 1ページに納まる分のデータをコピー
                var tmp2 = new Dictionary<string, Vector3>();
                for (int i = 0; i < rows * columns; i++)
                {
                    if (tmp1.Count <= 0)
                        break;
                    tmp2.Add(tmp1.First().Key, tmp1.First().Value);
                    tmp1.Remove(tmp1.First().Key);
                }
                if (tmp2.Count <= 0)
                    break;

                // 全データを 1ページに印刷したら 何行になるか
                int rs = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tmp2.Count) / columns));
                rows = Math.Min(rows, rs);

                var table = this.getPageContents(tmp2, rows, columns);
                page.Add(table);

                // 2ページ以降に入る行数
                rows = printRows[1];
            }


            // 表の印刷
            int p = 1;
            foreach (var table in page)
            {
                if (1 < p)
                    mc.NewPage();

                // タイトルの印字
                mc.setCurrentX(printManager.H1PosX);
                Text.PrtText(mc, this.title);
                mc.addCurrentY(printManager.FontHeight + printManager.LineSpacing1);

                // 表題の印字
                for (int i = 0; i < this.header_content.Rank; i++)
                {
                    for (int j = 0; j < this.header_content.GetLength(i); j++)
                    {
                        var str = this.header_content[i, j];
                        if (str.Length <= 0)
                            continue;
                        var x = this.header_Xspacing[j];
                        mc.setCurrentX(x);
                        Text.PrtText(mc, str);
                    }
                    mc.addCurrentY(printManager.FontHeight);
                }
                mc.addCurrentY(printManager.LineSpacing2); // 中くらい（タイトル後など）の改行高さ

                // 表の印刷
                foreach (var line in table)
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        var str = line[i];
                        if (str == null)
                            continue;
                        if (str.Length <= 0)
                            continue;
                        var x = this.body_Xspacing[i];
                        mc.setCurrentX(x);
                        Text.PrtText(mc, str, align: XStringFormats.BottomRight);
                    }
                    mc.addCurrentY(printManager.LineSpacing3); // 小さい（テーブル内などの）改行高さ
                }

                p++;
            }
            */

        }
        #endregion


        #region 他のモジュールのヘルパー関数
        /*
        /// <summary>
        /// 部材にの長さを取得する
        /// </summary>
        /// <param name="mc"></param>
        /// <param name="memberNo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double GetMemberLength(string memberNo)
        {
            var memb = this.GetMember(memberNo);

            string ni = memb.ni.ToString();
            string nj = memb.nj.ToString();
            if (ni == null || nj == null)
            {
                return 0;
            }

            InputNode node = new InputNode();
            double[] iPos = node.GetNodePos(ni, value);
            double[] jPos = node.GetNodePos(nj, value);
            if (iPos == null || jPos == null)
            {
                return 0;
            }

            double xi = iPos[0];
            double yi = iPos[1];
            double zi = iPos[2];
            double xj = jPos[0];
            double yj = jPos[1];
            double zj = jPos[2];

            double result = Math.Sqrt(Math.Pow(xi - xj, 2) + Math.Pow(yi - yj, 2) + Math.Pow(zi - zj, 2));
            return result;
        }

        /// <summary>
        /// 部材情報を取得する
        /// </summary>
        /// <param name="No">部材番号</param>
        /// <returns></returns>
        public Member GetMember(string No)
        {
            if (!this.members.ContainsKey(No))
            {
                return null;
            }
            return this.members[No];
        }
        */

        /*
        public void printPDF(PdfDoc mc)
        {

            int bottomCell = mc.bottomCell;

            // 全行数の取得
            double count = (data.Count + ((data.Count / bottomCell) + 1) * 5) * mc.single_Yrow;
            //  改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content3D = {
                { "", "節点", "", "L", "材料番号", "コードアングル" , "材料名称"},
                { "No", "I端", "J端", "(m)", "", "(°)" , ""}

            };

            string[,] header_content2D = {
                { "", "節点", "", "L", "材料番号", "" , "材料名称"},
                { "No", "I端", "J端", "(m)", "", "" , ""}            
            };

            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("部材データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Member Data", 0);
                    header_content3D[0, 1] = "Node";
                    header_content3D[0, 3] = "Distance";
                    header_content3D[0, 4] = "Material No.";
                    header_content3D[0, 5] = "Angle of Rotation ";
                    header_content3D[0, 6] = "Name of Material";
                    header_content3D[1, 1] = "Node-I";
                    header_content3D[1, 2] = "Node-J";

                    header_content2D[0, 1] = "Node";
                    header_content2D[0, 3] = "Distance";
                    header_content2D[0, 4] = "Material No.";
                    header_content2D[0, 6] = "Name of Material";
                    header_content2D[1, 1] = "Node-I";
                    header_content2D[1, 2] = "Node-J";
                    break;
            }

        
            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 0, 75, 100, 145, 203, 280, 360 },
                { 10, 50, 100, 145, 203, 280, 360 } 
            };
            int[,] header_Xspacing2D = {
                { 10, 90, 120, 180, 255, 280, 351 },
                { 10, 60, 120, 180, 255, 280, 351 }
            };

            // ボディーのx方向の余白
            int[,] body_Xspacing3D = { { 17, 60, 110, 157, 208, 284, 341 } };
            int[,] body_Xspacing2D = { { 17, 70, 130, 192, 260, 284, 330 } };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;

            mc.Header(header_content, header_Xspacing);


            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Length; j++)
                {
                    mc.CurrentColumn(body_Xspacing[0, j]); //x方向移動
                    if (j == 6) // 材料名称のみ左詰め
                    {
                        mc.PrintContent(data[i][j], 1); // print
                    }
                    else
                    {
                        mc.PrintContent(data[i][j]); // print
                    }
                }
                if (!(i == data.Count - 1))
                {
                    mc.CurrentRow(1); // y方向移動
                }
            }
        }
        */
        #endregion
    }
}

