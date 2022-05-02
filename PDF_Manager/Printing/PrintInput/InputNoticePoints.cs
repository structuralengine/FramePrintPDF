using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
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
        private List<NoticePoint> noticepoints = new List<NoticePoint>();
        private InputMember member;

        public InputNoticePoints(PrintData pd, Dictionary<string, object> value)
        {
            this.member = (InputMember)pd.printDatas["member"];

            //nodeデータを取得する
            JArray target = JArray.FromObject(value["notice_points"]);

            for (int i = 0; i < target.Count; i++)
            {
                JToken item = target[i];

                var np = new NoticePoint();

                np.m = dataManager.TypeChange(item["m"]);

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
    }
}

