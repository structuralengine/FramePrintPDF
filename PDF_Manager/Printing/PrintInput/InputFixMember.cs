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

    internal class InputFixMember
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();

        public (List<string>, List<List<string[]>>) FixMember(PdfDoc mc ,Dictionary<string, object> value_)
        {
            value = value_;
            // elementデータを取得する．
            var target = JObject.FromObject(value["fix_member"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはすべてここに格納する
            List<string> fixmember_title = new List<string>();
            List<List<string[]>> fixmember_data = new List<List<string[]>>();


            for (int i = 0; i < target.Count; i++)
            {
                JArray Elem = JArray.FromObject(target.ElementAt(i).Value);

                // タイトルを入れる．
                fixmember_title.Add("タイプ" + target.ElementAt(i).Key);

                List<string[]> table = new List<string[]>();

                for (int j = 0; j < Elem.Count; j++)
                {
                    JToken item = Elem[j];
                   
                    string[] line = new String[5];

                    line[0] = mc.TypeChange(item["m"]);
                    line[1] = mc.TypeChange(item["tx"]);
                    line[2] = mc.TypeChange(item["ty"]);
                    line[3] = mc.TypeChange(item["tz"]);
                    line[4] = mc.TypeChange(item["tr"]);
                                        
                    table.Add(line);
                }
                fixmember_data.Add(table);
            }
            return (
                fixmember_title,
                fixmember_data
            );
        }

        public void FixMemberPDF(PdfDoc mc, List<string> fixmemberTitle, List<List<string[]>> fixmemberData)
        {
            // 全行の取得
            int count = 2;
            for (int i = 0; i < fixmemberTitle.Count; i++)
            {
                count += (fixmemberData[i].Count + 5) * mc.single_Yrow + 1;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content = {
                { "部材", "", "", "", "",},
                { "No", "TX", "TY", "TZ", "TR" },
                {"","(kN/m2)","(kN/m2)","(kN/m2)","(kN/rad)"}
            };
            // ヘッダーのx方向の余白
            int[,] header_Xspacing ={
                { 0, 40, 80, 120, 160 },
                { 0, 40, 80, 120, 160 },
                { 0, 40, 80, 120, 160 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing = {
                { 0, 40, 80, 120, 160 }
            };

            // タイトルの印刷
            mc.PrintContent("バネデータ", 0);
            mc.CurrentRow(2);

            int k = 0;

            for (int i = 0; i < fixmemberData.Count; i++)
            {
                //  1タイプ内でページをまたぐかどうか
                mc.TypeCount(i, 6, fixmemberData[i].Count, fixmemberTitle[i]);

                // タイプの印刷
                mc.CurrentColumn(0);
                mc.PrintContent(fixmemberTitle[i], 0);
                mc.CurrentRow(2);


                // ヘッダーの印刷
                mc.Header(header_content, header_Xspacing);

                for (int j = 0; j < fixmemberData[i].Count; j++)
                {
                    for (int l = 0; l < fixmemberData[i][j].Length; l++)
                    {
                        mc.CurrentColumn(body_Xspacing[k, l]); //x方向移動
                        mc.PrintContent(fixmemberData[i][j][l]); // print
                    }
                    mc.CurrentRow(1); // y方向移動
                }
            }

        }
    }

}
