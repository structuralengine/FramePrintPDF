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
    internal class InputMember
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        private JObject targetLen;
        private JToken mem;
        private InputElement element;

        // 集まったデータはここに格納する
        List<string[]> data = new List<string[]>();


        public void Member(PdfDoc mc, InputElement element, Dictionary<string, object> value_)
        {
            value = value_;
            //nodeデータを取得する
            //var target_ = JObject.FromObject(value["member"]).ToObject<List<string[]>>();
            //targetLen = target_;

            var target = JObject.FromObject(value["member"]).ToObject<Dictionary<string, object>>();
            targetLen = JObject.FromObject(value["member"]);

            // 全部の行数
            var row = target.Count;

            for (int i = 0; i < row; i++)
            {
                string index = target.ElementAt(i).Key;

                var item = JObject.FromObject(target.ElementAt(i).Value);

                double len = this.GetMemberLength(index, value); // 部材長さ

                string name = item["e"].Type == JTokenType.Null ? "" : element.GetElementName(item["e"].ToString());

                string[] line = new String[7];
                line[0] = index;
                line[1] = mc.TypeChange(item["ni"]);
                line[2] = mc.TypeChange(item["nj"]);
                line[3] = mc.TypeChange(len, 3);
                line[4] = mc.TypeChange(item["e"]);
                line[5] = mc.TypeChange(item["cg"]);
                line[6] = name;
                data.Add(line);
            }
        }

        public double GetMemberLength(string memberNo, Dictionary<string, object> value)
        {
            JToken memb = this.GetMember(memberNo);

            string ni = memb["ni"].ToString();
            string nj = memb["nj"].ToString();
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

        public JToken GetMember(string memberNo)
        {
            JToken member = targetLen[memberNo];

            return member;
        }

        public void MemberPDF(PdfDoc mc)
        {

            int bottomCell = mc.bottomCell;

            // 全行数の取得
            double count = (data.Count + ((data.Count / bottomCell) + 1) * 4) * mc.single_Yrow;
            //  改ページ判定
            mc.DataCountKeep(count);

            //  タイトルの印刷
            mc.PrintContent("部材データ", 0);
            mc.CurrentRow(2);
            //　ヘッダー
            string[,] header_content = {
                { "No", "I-TAN", "J-TAN", "L(m)", "材料番号", "コードアングル" , "材料名称"}
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing = { { 10, 50, 100, 145, 203, 280, 360 } };

            mc.Header(header_content, header_Xspacing);

            // ボディーのx方向の余白
            int[,] body_Xspacing = { { 17, 60, 110, 157, 208, 284, 341 } };

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
                mc.CurrentRow(1);
            }
        }
    }
}

