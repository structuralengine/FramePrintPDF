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

namespace PDF_Manager.Printing
{
    public class FixMember
    {
        public string m;   // 部材番号
        public double tx;
        public double ty;
        public double tz;
        public double tr;
    }

    internal class InputFixMember
    {
        public const string KEY = "fix_member";

        private Dictionary<int, List<FixMember>> fixmembers = new Dictionary<int, List<FixMember>>();

        public InputFixMember(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(KEY))
                return;

            // データを取得する．
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = dataManager.parseInt(target.ElementAt(i).Key);  // タイプ番号
                JArray FixM = JArray.FromObject(target.ElementAt(i).Value);

                var _fixnode = new List<FixMember>();

                for (int j = 0; j < FixM.Count; j++)
                {
                    JToken item = FixM[j];

                    var fm = new FixMember();

                    fm.m = dataManager.TypeChange(item["m"]);
                    fm.tx = dataManager.parseDouble(item["tx"]);
                    fm.ty = dataManager.parseDouble(item["ty"]);
                    fm.tz = dataManager.parseDouble(item["tz"]);
                    fm.tr = dataManager.parseDouble(item["tr"]);

                    _fixnode.Add(fm);

                }
                this.fixmembers.Add(key, _fixnode);
            }
        }
        /*
            private Dictionary<string, object> value = new Dictionary<string, object>();
            List<string> title = new List<string>();
            List<List<string[]>> data = new List<List<string[]>>();


            value = value_;
            // elementデータを取得する．
            var target = JObject.FromObject(value["fix_member"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはすべてここに格納する
            title = new List<string>();
            data = new List<List<string[]>>();


            for (int i = 0; i < target.Count; i++)
            {
                JArray Elem = JArray.FromObject(target.ElementAt(i).Value);

                // タイトルを入れる．
                switch (mc.language)
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

                        string[] line = new String[5];

                        line[0] = dataManager.TypeChange(item["m"]);
                        line[1] = dataManager.TypeChange(item["tx"]);
                        line[2] = dataManager.TypeChange(item["ty"]);
                        line[3] = dataManager.TypeChange(item["tz"]);
                        line[4] = dataManager.TypeChange(item["tr"]);

                        table.Add(line);
                    }
                    data.Add(table);
                }
                else if (mc.dimension == 2)
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

                                string[] line = new String[6];
                                line[0] = dataManager.TypeChange(targetValue_l["m"]);
                                line[1] = dataManager.TypeChange(targetValue_l["tx"], 3);
                                line[2] = dataManager.TypeChange(targetValue_l["ty"], 3);                

                                var targetValue_r = Elem[k];
                                line[3] = dataManager.TypeChange(targetValue_r["m"]);
                                line[4] = dataManager.TypeChange(targetValue_r["tx"], 3);
                                line[5] = dataManager.TypeChange(targetValue_r["ty"], 3);
                          
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

                                string[] line = new String[6];
                                line[0] = dataManager.TypeChange(targetValue_l["m"]);
                                line[1] = dataManager.TypeChange(targetValue_l["tx"], 3);
                                line[2] = dataManager.TypeChange(targetValue_l["ty"], 3);

                                try
                                {
                                    //　各行のデータを取得する（右段)
                                    var targetValue_r = Elem[k];
                                    line[3] = dataManager.TypeChange(targetValue_r["m"]);
                                    line[4] = dataManager.TypeChange(targetValue_r["tx"], 3);
                                    line[5] = dataManager.TypeChange(targetValue_r["ty"], 3);

                                    body.Add(line);
                                }
                                catch
                                {
                                    line[3] = "";
                                    line[4] = "";
                                    line[5] = "";
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
        public void FixMemberPDF(PdfDoc mc)
        {
            // 全行の取得
            int count = 20;
            for (int i = 0; i < title.Count; i++)
            {
                count += (data[i].Count + 7) * mc.single_Yrow ;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー
            string[,] header_content3D = {
                { "部材", "", "変位拘束", "", "回転拘束",},
                { "No", "部材軸方向", "部材Y軸", "部材Z軸", "" },
                {"","(kN/m/m)","(kN/m/m)","(kN/m/m)","(kN・m/rad/m)"}
            };
            string[,] header_content2D = {
                { "部材", "変位拘束", "","部材", "変位拘束", "" },
                { "No", "部材軸方向", "部材Y軸","No", "部材軸方向", "部材Y軸"},
                {"","(kN/m/m)","(kN/m/m)","","(kN/m/m)","(kN/m/m)"}
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D ={
                { 10, 105, 140, 210, 280},
                { 10, 70, 140, 210, 280},
                { 10, 70, 140, 210, 280},
            };
            int[,] header_Xspacing2D ={
                { 10, 105, 140, 210, 305, 340},
                { 10, 70, 140, 210, 270, 340},
                { 10, 70, 140, 210, 270, 340},
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 90, 160, 230, 300 }
            };
            int[,] body_Xspacing2D = {
                { 17, 90, 160, 230, 290, 360 }
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;
            mc.header_content = header_content;
            mc.header_Xspacing = header_Xspacing;
            mc.body_Xspacing = body_Xspacing;
       
            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("バネデータ", 0);
                    break;
                case "en":
                    mc.PrintContent("MemberSpring DATA", 0);
                    //　ヘッダー
                    header_content3D[0, 0] = "Member";
                    header_content3D[0, 2] = "Displacement Restraint";
                    header_content3D[0, 4] = "Rotational Restraint";
     
                    header_content3D[1, 1] = "Kv";
                    header_content3D[1, 2] = "Ky";
                    header_content3D[1, 3] = "Kz";
                    header_content3D[1, 4] = "Kr";


                    header_content2D[0, 0] = "Member";
                    header_content2D[0, 1] = "Displacement Restraint";
                    header_content2D[0, 3] = "Member";
                    header_content2D[0, 4] = "Displacement Restraint";

                    header_content2D[1, 1] = "Kv";
                    header_content2D[1, 2] = "Ky";
                    header_content2D[1, 4] = "Kv";
                    header_content2D[1, 5] = "Ky";
                    break;
            }
            mc.CurrentRow(2);
            mc.CurrentColumn(0);


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
                        mc.CurrentColumn(body_Xspacing[0, l]); //x方向移動
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
