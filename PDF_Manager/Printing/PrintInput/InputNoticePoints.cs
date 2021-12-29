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
    internal class InputNoticePoints
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();

        public List<List<string[]>> NoticePoints(PdfDoc mc,InputMember member, Dictionary<string, object> value_)
        {
            value = value_;
            //nodeデータを取得する
            JArray target = JArray.FromObject(value["notice_points"]);

            // 集まったデータはここに格納する
            List<List<string[]>> noticepoints_data = new List<List<string[]>>();
            List<string[]> body = new List<string[]>();


            for (int i = 0; i < target.Count; i++)
            {
                JToken item = target[i];

                string m = mc.TypeChange(item["m"]);

                double len = member.GetMemberLength(m, value); // 部材長さ


                string[] line = new String[12];
                line[0] = m;
                line[1] = len == 0 ? "" : (Math.Round(len, 3, MidpointRounding.AwayFromZero)).ToString();

                int count = 0;
                var itemPoints = item["Points"];

                for (int j = 0; j < item["Points"].Count(); j++)
                {
                    line[count + 2] = mc.TypeChange(itemPoints[count]);
                    count++;
                    if (count == 10)
                    {
                        body.Add(line);
                        count = 0;
                        line = new string[10];
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
                if (body.Count > 0)
                {
                    noticepoints_data.Add(body);
                }
            }
            noticepoints_data.Add(body);
            return noticepoints_data;
        }

        public void NoticePointsPDF(PdfDoc mc, List<List<string[]>> noticepointsData)
        {
            int bottomCell = mc.bottomCell;

            // 全行の取得
            int count = 2;
            for (int i = 0; i < noticepointsData.Count; i++)
            {
                count += (noticepointsData[i].Count + 2) * mc.single_Yrow;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //  タイトルの印刷
            mc.PrintContent("着目点データ", 0);
            mc.CurrentRow(2);
            //　ヘッダー
            string[,] header_content = {
                { "部材", "", "", "", "", "" , "", "", "", "", "",""},
                { "No", "部材長", "L1", "L2", "L3", "L4" , "L5", "L6", "L7", "L8", "L9", "L10"}
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                 { 0, 40, 80, 120, 160, 200, 240, 280, 320, 360, 400, 440 },
                 { 0, 40, 80, 120, 160, 200, 240, 280, 320, 360, 400, 440 } 
            };

            mc.Header(header_content, header_Xspacing);

            // ボディーのx方向の余白
            int[,] body_Xspacing = { { 0, 40, 80, 120, 160, 200, 240, 280, 320, 360, 400, 440 } };

            for (int i = 0; i < noticepointsData.Count; i++)
            {
                for (int j = 0; j < noticepointsData[i].Count; j++)
                {
                    for (int l = 0; l < noticepointsData[i][j].Length; l++)
                    {
                        mc.CurrentColumn(body_Xspacing[0, l]); //x方向移動
                        mc.PrintContent(noticepointsData[i][j][l]);  // print
                    }
                    mc.CurrentRow(1);
                }
            }

        }
    }
}

