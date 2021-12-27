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

        public List<string[]> Member(Dictionary<string, object> value_)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["member"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            List<string[]> member_data = new List<string[]>();

            // 全部の行数
            var row = target.Count;

            for (int i = 0; i < row; i++)
            {
                string index = target.ElementAt(i).Key;
                var targetValue = new Dictionary<string, double>();

                // nullの判定
                var Elem = JObject.FromObject(target.ElementAt(i).Value);
                if (Elem["cg"].Type == JTokenType.Null) Elem["cg"] = 0;

                // jobjectに変換
                targetValue = Elem.ToObject<Dictionary<string, double>>();

                double len = this.GetMemberLength(index, targetValue, value); // 部材長さ
                                                                              //string name = this.getElementName(targetValue["e"]); // 材料名称

                string[] line = new String[7];
                line[0] = target.ElementAt(i).Key;
                line[1] = targetValue["ni"].ToString();
                line[2] = targetValue["nj"].ToString();
                line[3] = (Math.Round(len, 3, MidpointRounding.AwayFromZero)).ToString();
                line[4] = targetValue["e"].ToString();
                line[5] = targetValue["cg"].ToString() != null ? targetValue["cg"].ToString() : "";
                //line[6] = (Math.Round(targetValue["z"], 3, MidpointRounding.AwayFromZero)).ToString();
                member_data.Add(line);
            }
            return member_data;
        }

        public double GetMemberLength(string memberNo, Dictionary<string, double> target, Dictionary<string, object> value)
        {
            string ni = target["ni"].ToString();
            string nj = target["nj"].ToString();
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

        public void MemberPDF(PdfDoc mc, List<string[]> memberData)
        {
            int bottomCell = mc.bottomCell;
            int single_Yrow = mc.single_Yrow;
            int currentXposition_values = 40;

            // 全行数の取得
            int count = memberData.Count + (memberData.Count / bottomCell) * 2 + 4;
            // 改ページするかの判定
            bool judge = mc.DataCountKeep(count);
            // trueなら改ページ，そうでないなら2行空きで挿入
            if (judge == true)
            {
                mc.NewPage();
            }else
            {
                mc.CurrentPosHeader.Y += single_Yrow * 2;
                mc.CurrentPosBody.Y += single_Yrow * 2;
            };

            mc.gfx.DrawString("部材データ", mc.font_got, XBrushes.Black, mc.CurrentPosHeader);

            // 初期位置の設定
            mc.CurrentPosHeader.Y += single_Yrow * 2;
            mc.CurrentPosBody.Y += single_Yrow * 4;

            for (int i = 0; i < memberData.Count; i++)
            {
                if (i != 0 && i % bottomCell == 0)
                {
                    mc.NewPage();
                    mc.CurrentPosHeader.Y += single_Yrow * 2;
                    mc.CurrentPosBody.Y += single_Yrow * 4;
                }

                if (i == 0 || (i != 0 && i % bottomCell == 0))
                {
                    mc.gfx.DrawString("No", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 1);
                    mc.gfx.DrawString("I-TAN", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 2);
                    mc.gfx.DrawString("J-TAN", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 3);
                    mc.gfx.DrawString("L(m)", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 4);
                    mc.gfx.DrawString("材料番号", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 5);
                    mc.gfx.DrawString("コードアングル", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
                    //mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 6);
                    //mc.gfx.DrawString("材料名称", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);

                    mc.CurrentPosHeader.X = mc.x;
                }

                mc.gfx.DrawString(memberData[i][0], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                mc.CurrentPosBody.X = mc.x + (currentXposition_values * 1);
                mc.gfx.DrawString(memberData[i][1], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                mc.CurrentPosBody.X = mc.x + (currentXposition_values * 2);
                mc.gfx.DrawString(memberData[i][2], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                mc.CurrentPosBody.X = mc.x + (currentXposition_values * 3);
                mc.gfx.DrawString(memberData[i][3], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                mc.CurrentPosBody.X = mc.x + (currentXposition_values * 4);
                mc.gfx.DrawString(memberData[i][4], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                mc.CurrentPosBody.X = mc.x + (currentXposition_values * 5);
                mc.gfx.DrawString(memberData[i][5] == "0" ? "" : memberData[i][5], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                //mc.CurrentPosBody.X = mc.x + (currentXposition_values * 6);
                //mc.gfx.DrawString(memberData[i][6], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);

                mc.CurrentPosBody.X = mc.x;
                mc.CurrentPosBody.Y += single_Yrow;
            }
           mc.CurrentPosHeader.Y = mc.CurrentPosBody.Y + single_Yrow*2;
           mc.CurrentPosBody.Y = mc.CurrentPosHeader.Y + single_Yrow * 2;
           mc.DataCountKeep(mc.CurrentPosHeader.Y);
        }
    }
}

