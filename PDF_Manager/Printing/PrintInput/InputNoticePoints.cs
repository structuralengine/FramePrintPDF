using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing;
using PDF_Manager.Printing.Comon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDF_Manager.Printing
{
    public class NoticePoint
    {
        public string m;   // 要素番号
        public double[] Points;
    }

    internal class InputNoticePoints
    {
        public const string KEY = "notice_points";

        private List<NoticePoint> noticepoints = new List<NoticePoint>();

        // 要素情報
        private InputMember Member = null;

        public InputNoticePoints(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(KEY))
                return;

            //nodeデータを取得する
            JArray target = JArray.FromObject(value[KEY]);

            for (int i = 0; i < target.Count; i++)
            {
                JToken item = target[i];

                var np = new NoticePoint();

                np.m = dataManager.toString(item["m"]);

                var itemPoints = item["Points"];
                var _points = new List<double>();

                for (int j = 0; j < itemPoints.Count(); j++)
                {
                    var d = dataManager.parseDouble(itemPoints[j]);
                    _points.Add(d);
                }

                np.Points = _points.ToArray();

                this.noticepoints.Add(np);
            }
        }

        #region 印刷処理
        // タイトル
        private string title;
        // 2次元か3次元か
        private int dimension;
        // テーブル
        private Table myTable;
        #region 印刷処理
        // 節点情報
        private InputNode Node = null;


        /// <summary>
        /// 印刷前の初期化処理
        /// </summary>
        private void printInit(PdfDocument mc, PrintData data)
        {
            this.dimension = data.dimension;

            if (this.dimension == 3)
            {   // 3次元

                //テーブルの作成
                this.myTable = new Table(2, 12);

                // テーブルの幅
                this.myTable.ColWidth[0] = 25.0; // 格点No
                this.myTable.ColWidth[1] = 40.0;
                this.myTable.ColWidth[2] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[3] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[4] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[5] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[6] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[7] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[8] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[9] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[10] = this.myTable.ColWidth[1];
                this.myTable.ColWidth[11] = this.myTable.ColWidth[1];


                switch (data.language)
                {
                    //case "en":
                    //    this.title = "Support DATA";
                    //    this.myTable[0, 0] = "Node";
                    //    this.myTable[1, 0] = "No";
                    //    this.myTable[1, 1] = "TX";
                    //    this.myTable[2, 1] = "(kN/m)";
                    //    this.myTable[0, 2] = "Displacement Restraint";
                    //    this.myTable[1, 2] = "TY";
                    //    this.myTable[2, 2] = "(kN/m)";
                    //    this.myTable[1, 3] = "TZ";
                    //    this.myTable[2, 3] = "(kN/m)";

                    //    this.myTable[1, 4] = "MX";
                    //    this.myTable[2, 4] = "(kN・m/rad)";
                    //    this.myTable[0, 5] = "Rotational Restraint";
                    //    this.myTable[1, 5] = "MY";
                    //    this.myTable[2, 5] = "(kN・m/rad)";
                    //    this.myTable[1, 6] = "MZ";
                    //    this.myTable[2, 6] = "(kN・m/rad)";

                    //    break;

                    //case "cn":
                    //    this.title = "支点";
                    //    this.myTable[0, 0] = "节点";
                    //    this.myTable[1, 0] = "编码";
                    //    this.myTable[1, 1] = "X方向";
                    //    this.myTable[2, 1] = "(kN/m)";
                    //    this.myTable[0, 2] = "位移约束";
                    //    this.myTable[1, 2] = "Y方向";
                    //    this.myTable[2, 2] = "(kN/m)";
                    //    this.myTable[1, 3] = "Z方向";
                    //    this.myTable[2, 3] = "(kN/m)";

                    //    this.myTable[1, 4] = "围绕X轴";
                    //    this.myTable[2, 4] = "(kN・m/rad)";
                    //    this.myTable[0, 5] = "旋转约束";
                    //    this.myTable[1, 5] = "围绕Y轴";
                    //    this.myTable[2, 5] = "(kN・m/rad)";
                    //    this.myTable[1, 6] = "围绕Z轴";
                    //    this.myTable[2, 6] = "(kN・m/rad)";
                    //    break;

                    default:
                        this.title = "着目点データ";
                        this.myTable[0, 0] = "部材";
                        this.myTable[1, 0] = "No";
                        this.myTable[1, 1] = "部材長";
                        this.myTable[1, 2] = "L1";
                        this.myTable[1, 3] = "L2";
                        this.myTable[1, 4] = "L3";
                        this.myTable[1, 5] = "L4";
                        this.myTable[1, 6] = "L5";
                        this.myTable[1, 7] = "L6";
                        this.myTable[1, 8] = "L7";
                        this.myTable[1, 9] = "L8";
                        this.myTable[1, 10] = "L9";
                        this.myTable[1, 11] = "L10";

                        break;
                }
                this.myTable.AlignX[0, 0] = "R";    // 右寄せ
                this.myTable.AlignX[0, 1] = "R";    // 右寄せ
            }
        }


        /// <summary>
        /// 1ページに入れるコンテンツを集計する 3次元の場合
        /// </summary>
        /// <param name="target">印刷対象の配列</param>
        /// <param name="rows">行数</param>
        /// <returns>印刷する用の配列</returns>
        private Table getPageContents3D(List<NoticePoint> target)
        {
            int r = this.myTable.Rows;
            int rows = target.Count;

            // 行コンテンツを生成
            var table = this.myTable.Clone();
            table.ReDim(row: r + rows);

            for (var i = 0; i < rows; i++)
            {

                NoticePoint item = target[i];

                for (var j = 0; j < 3; j++)
                {
                    table[r, 0] = printManager.toString(item.m);
                    table.AlignX[r, j] = "R";
                    j++;
                    table[r, j] = printManager.toString(this.Member.GetMemberLength(item.m), 3);
                    table.AlignX[r, j] = "R";
                    j++;

                    var count = item.Points.Count();
                    var m = 0;
                    var addrows = 0;

                    for (var k = 0; k < item.Points.Count(); k++)
                    {
                        if(item.Points.Count() > 10 && k != 0)
                        {
                            if(k % 10 == 0)
                            {
                                r++;
                                j = 2;
                                addrows++;
                            }
                            table.ReDim(row: r + rows + addrows);
                        }
                        table[r, j] = printManager.toString(item.Points[k], 3);
                        table.AlignX[r, j] = "R";
                        j++;
                    }

                }
                r++;

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
            if (this.noticepoints.Count == 0)
                return;

            // 要素を取得できる状態にする
            this.Member = (InputMember)data.printDatas[InputMember.KEY];

            // タイトル などの初期化
            this.printInit(mc, data);

            // 印刷可能な行数
            var printRows = myTable.getPrintRowCount(mc);

            // 行コンテンツを生成
            var page = new List<Table>();

            // 1ページ目に入る行数
            int rows = printRows[0];

            var tmp3 = new List<NoticePoint>();

            // 集計開始
            var tmp1 = new List<NoticePoint>(this.noticepoints); // clone
            while (true)
            {
                //// 1ページに納まる分のデータをコピー
                //var tmp2 = new List<NoticePoint>();
                //for (int i = 0; i < rows; i++)
                //{
                //    if (tmp1.Count <= 0)
                //        break;
                //    tmp2.Add(tmp1.First());
                //    tmp1.Remove(tmp1.First());
                //}
                if (tmp1.Count <= 0)
                    break;


                // 1ページに納まる分のデータをコピー
                var tmp2 = new List<NoticePoint>();

                if (tmp3.Count != 0)
                    rows = rows - tmp3.Count();
                    tmp2.AddRange(tmp3);
                    tmp3.Clear();


                for (int i = 0; i < rows; i++)
                {
                    if (tmp1.Count <= 0)
                        break;

                    int Points = tmp1[0].Points.Count();
                    var pullrows = 0;

                    for(var n=0; n<Points; n++)
                    {
                        if (n % 10 == 0)
                        {
                            pullrows++;
                        }

                    }
                    rows = rows - pullrows;


                    if (rows <= 0)
                        break;

                    //if (tmp1[0].Points.Count() % 10 + 1 > rows)
                    //{
                    //    var tmp3_ = new List<NoticePoint>();
                    //    for(var m = 0; m < rows;m++)
                    //    {
                    //        tmp3_ = tmp1.Points[m];
                    //    }
                    //}

                    while (true)
                    {
                        tmp3.Add(tmp1.First());
                        tmp1.Remove(tmp1.First());
                        
                        if (tmp1.Count <= 0)
                            break;

                        if (tmp1.First().m != "")
                        {
                            break;
                        }
                    }

                    //残りの行にこれから入れるデータが入りきるとき
                    if (rows - tmp2.Count > tmp3.Count)
                    {
                        tmp2.AddRange(tmp3);
                        tmp3.Clear();
                    }
                    //残りの行にも、次のページにも入りきらないとき
                    else if (rows - tmp2.Count < tmp3.Count && tmp3.Count > printRows[1])
                    {
                        //現在のページから連続して印刷
                        while (tmp3.Count != 0)
                        {
                            for (int l = 0; l < rows; l++)
                            {
                                tmp2.Add(tmp3.First());
                                tmp3.Remove(tmp3.First());
                                if (tmp3.Count == 0)
                                    break;
                                if (rows - tmp2.Count <= 0)
                                    break;
                            }
                            if (tmp3.Count == 0)
                                break;
                            if (rows == 0)
                                break;
                            var table = this.getPageContents3D(tmp2);
                            page.Add(table);
                            tmp2.Clear();
                        }
                    }
                    //tmp3にためたまま改ページし次のページで印刷
                    else
                    {
                        break;
                    }

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
            printManager.printTableContentsOnePage(mc, page, new string[] { this.title });

            #endregion
        }

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

            if (memb == null)
                return double.NaN;

            if (memb.ni == null || memb.nj == null)
            {
                return double.NaN;
            }

            Vector3 iPos = this.Node.GetNodePos(memb.ni);
            Vector3 jPos = this.Node.GetNodePos(memb.nj);
            if (iPos == null || jPos == null)
            {
                return double.NaN;
            }

            double result = Math.Sqrt(Math.Pow(iPos.x - jPos.x, 2) + Math.Pow(iPos.y - jPos.y, 2) + Math.Pow(iPos.z - jPos.z, 2));

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
        #endregion

        /*
        // 集まったデータはここに格納する
        data = new List<List<string[]>>();
            List<string[]> body = new List<string[]>();


            for (int i = 0; i < target.Count; i++)
            {
                JToken item = target[i];

                string m = dataManager.TypeChange(item["m"]);

                double len = member.GetMemberLength(mc,m, value); // 部材長さ

                string[] line = new String[12];
                line[0] = m;
                line[1] = len == 0 ? "" : dataManager.TypeChange(len, 3);

                int count = 0;
                var itemPoints = item["Points"];

                for (int j = 0; j < item["Points"].Count(); j++)
                {
                    line[count + 2] = dataManager.TypeChange(itemPoints[count], 3);
                    count++;
                    if (count == 10)
                    {
                        body.Add(line);
                        count = 0;
                        line = new string[12];
                        line[0] = "";
                        line[1] = "";
                    }
                }
                if (count > 0)
                {
                    for (int k = 2; k < 12; k++)
                    {
                        line[k] = line[k] == null ? "" : line[k];
                    }

                    body.Add(line);
                }
            }
            if (body.Count > 0)
            {
                data.Add(body);
            }
            */
        /*
        public void NoticePointsPDF(PdfDoc mc)
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
                { "部材", "", "", "", "", "" , "", "", "", "", "",""},
                { "No", "部材長", "L1", "L2", "L3", "L4" , "L5", "L6", "L7", "L8", "L9", "L10"}
            };

            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("着目点データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Location Data", 0);
                    header_content[0, 0] = "Member";
                    header_content[1, 1] = "Distance";
                    break;
            }

            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                 { 10, 45, 85, 120, 155, 190, 225, 260, 295, 330, 365, 400 },
                 { 10, 45, 85, 120, 155, 190, 225, 260, 295, 330, 365, 400 },
            };

            mc.Header(header_content, header_Xspacing);

            // ボディーのx方向の余白
            int[,] body_Xspacing = { { 17, 58, 96, 131, 166, 201, 236, 271, 306, 341, 376, 411 } };

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    for (int l = 0; l < data[i][j].Length; l++)
                    {
                        mc.CurrentColumn(body_Xspacing[0, l]); //x方向移動
                        mc.PrintContent(data[i][j][l]);  // print
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
#endregion
