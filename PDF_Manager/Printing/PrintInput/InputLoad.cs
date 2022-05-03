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
    public class LoadMember
    {
        public string m1;
        public string m2;
        public string direction;
        public string mark;

        public string L1;
        public double L2;
        public double P1;
        public double P2;
    }

    public class LoadNode
    {
        public string n;
        public double tx;
        public double ty;
        public double tz;
        public double rx;
        public double ry;
        public double rz;
    }

    public class Load
    {
        public LoadMember[] load_member;
        public LoadNode[] load_node;
    }


    internal class InputLoad
    {
        private Dictionary<int, Load> loads = new Dictionary<int, Load>();
        private InputLoadName loadname;

        public InputLoad(PrintData pd, Dictionary<string, object> value)
        {
            this.loadname = (InputLoadName)pd.printDatas["LoadName"];


            //nodeデータを取得する
            var target = JObject.FromObject(value["load"]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = target.ElementAt(i).Key;
                int index = dataManager.parseInt(key);

                var lo = new Load();

                var item = JObject.FromObject(target.ElementAt(i).Value);
                
                if (item.ContainsKey("load_member") )
                {   // 要素荷重
                    var _loadMember = item["load_member"];
                    var LoadM = new List<LoadMember>();
                    foreach(JToken member in _loadMember)
                    {
                        var lm = new LoadMember();

                        lm.m1 = dataManager.TypeChange(member["m1"]);
                        lm.m2 = dataManager.TypeChange(member["m2"]);
                        lm.direction = dataManager.TypeChange(member["direction"]);
                        lm.mark = dataManager.TypeChange(member["mark"]);
                        lm.L1 = dataManager.TypeChange(member["L1"]);
                        lm.L2 = dataManager.parseDouble(member["L2"]);
                        lm.P1 = dataManager.parseDouble(member["P1"]);
                        lm.P2 = dataManager.parseDouble(member["P2"]);

                        LoadM.Add(lm);
                    }
                    lo.load_member = LoadM.ToArray();
                }

                if (item.ContainsKey("load_node"))
                {   // 節点荷重
                    var _loadNode = item["load_node"];
                    var LoadN = new List<LoadNode>();
                    foreach (JToken node in _loadNode)
                    {
                        var ln = new LoadNode();

                        ln.n = dataManager.TypeChange(node["n"]);
                        ln.tx = dataManager.parseDouble(node["tx"]);
                        ln.ty = dataManager.parseDouble(node["ty"]);
                        ln.tz = dataManager.parseDouble(node["tz"]);
                        ln.rx = dataManager.parseDouble(node["rx"]);
                        ln.ry = dataManager.parseDouble(node["ry"]);
                        ln.rz = dataManager.parseDouble(node["rz"]);

                        LoadN.Add(ln);
                    }
                    lo.load_node = LoadN.ToArray();
                }

                this.loads.Add(index, lo);
            }
        }

    /*
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string> title = new List<string>();
        List<List<List<string[]>>> data = new List<List<List<string[]>>>();

        public void init(PdfDoc mc, Dictionary<string, object> value_)
        {
            value = value_;
            var target = JObject.FromObject(value["load"]).ToObject<Dictionary<string, object>>();
            // 集まったデータはここに格納する
            title = new List<string>();
            data = new List<List<List<string[]>>>();

            for (int i = 0; i < target.Count; i++)
            {
                var item = JObject.FromObject(target.ElementAt(i).Value);
                var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();

                // タイトルの表示
                if (item.ContainsKey("load_member") || item.ContainsKey("load_node"))
                {
                    title.Add("Case" + target.ElementAt(i).Key + ":" + item["name"]);
                }

                List<List<string[]>> compile = new List<List<string[]>>();

                List<string[]> table1 = new List<string[]>();
                if (item.ContainsKey("load_member"))
                {
                    for (int j = 0; j < item["load_member"].Count(); j++)
                    {
                        JToken member = item["load_member"][j];
                        if (member.SelectToken("m1") != null)
                        {
                            string[] line = new string[8];
                            line[0] = dataManager.TypeChange(member["m1"]);
                            line[1] = dataManager.TypeChange(member["m2"]);
                            line[2] = dataManager.TypeChange(member["direction"]);
                            line[3] = dataManager.TypeChange(member["mark"]);

                            // line[4] = member["L1"].ToString().StartsWith("-0") ? "-0.000" : dataManager.TypeChange(double.Parse(member["L1"].ToString()), 3);
                            var a4 = member["L1"].ToString();
                            if(a4.Trim().Length == 0)
                            {
                                line[4] = "";
                            } else {
                                var b4 = double.Parse(a4);
                                if (b4 == 0 && a4.StartsWith("-"))
                                {
                                    line[4] = "-0.000"; // ゼロにマイナスがついている場合
                                }
                                else
                                {
                                    line[4] = dataManager.TypeChange(b4, 3);
                                }
                            }

                            line[5] = dataManager.TypeChange(member["L2"], 3);
                            line[6] = dataManager.TypeChange(member["P1"], 2);
                            line[7] = dataManager.TypeChange(member["P2"], 2);
                            table1.Add(line);
                        }
                    }
                    compile.Add(table1);
                }
                else
                {
                    compile.Add(null);
                }

                List<string[]> table2 = new List<string[]>();
                if (item.ContainsKey("load_node"))
                {
                    for (int j = 0; j < item["load_node"].Count(); j++)
                    {
                        string[] line = new string[8];
                        JToken node = item["load_node"][j];
                        line[0] = "";
                        line[1] = dataManager.TypeChange(node["n"]);
                        line[2] = dataManager.TypeChange(node["tx"], 2);
                        line[3] = dataManager.TypeChange(node["ty"], 2);
                        line[4] = mc.Dimension(dataManager.TypeChange(node["tz"], 2));
                        line[5] = mc.Dimension(dataManager.TypeChange(node["rx"], 2));
                        line[6] = mc.Dimension(dataManager.TypeChange(node["ry"], 2));
                        line[7] = dataManager.TypeChange(node["rz"], 2);
                        table2.Add(line);
                    }
                    compile.Add(table2);
                }
                else
                {
                    compile.Add(null);
                }

                data.Add(compile);
            }

        */

        /*
        public void LoadPDF(PdfDoc mc)
        {
            // 全行の取得
            int count = 20;
            for (int i = 0; i < title.Count; i++)
            {
                int mCount = data[i][0] != null ? data[i][0].Count : 0;
                int pCount = data[i][1] != null ? data[i][1].Count : 0;

                count += ((mCount + 5) + (pCount + 5)) * mc.single_Yrow + 10;
            }
            // 改ページ判定
            mc.DataCountKeep(count);

            //　ヘッダー(部材荷重)
            string[,] headerM_content = {
            {"部材荷重","","","","","","","" },
            {"スタート","エンド","方向","マーク","L1","L2","P1","P2" }
            };

            //　ヘッダー(節点荷重)
            string[,] headerP_content3D = {
            {"節点荷重","","","","","","","" },
            {"","節点番号","X","Y","Z","RX","RY","RZ" }
            };

            string[,] headerP_content2D = {
            {"節点荷重","","","","","","","" },
            {"","節点番号","X","Y","","","","M" }
            };

            // ヘッダーのx方向の余白（部材荷重）
            int[,] headerM_Xspacing ={
                { 20, 70, 120, 180, 240, 330, 330, 420 },
                { 20, 70, 120, 180, 240, 300, 360, 420 }
            };

            // ヘッダーのx方向の余白（節点荷重）
            int[,] headerP_Xspacing3D ={
                { 20, 70, 120, 180, 240, 300, 360, 420 },
                { 20, 70, 120, 180, 240, 300, 360, 420 }
            };

            int[,] headerP_Xspacing2D ={
                { 20, 70, 120, 180, 0, 0, 0, 240 },
                { 20, 70, 120, 180, 0, 0, 0, 240 }
            };

            // ボディーのx方向の余白（部材荷重）
            int[,] bodyM_Xspacing = {
              { 27, 80, 123, 184, 255, 315, 375, 435 }
            };

            // ボディーのx方向の余白（節点荷重）
            int[,] bodyP_Xspacing3D = {
              { 27, 80, 135, 195, 255, 315, 375, 435 }
            };

            int[,] bodyP_Xspacing2D = {
              { 27, 80, 135, 195, 0, 0, 0, 255 }
            };

            string[,] headerP_content = mc.dimension == 3 ? headerP_content3D : headerP_content2D;
            int[,] headerP_Xspacing = mc.dimension == 3 ? headerP_Xspacing3D : headerP_Xspacing2D;
            int[,] bodyP_Xspacing = mc.dimension == 3 ? bodyP_Xspacing3D : bodyP_Xspacing2D;

            // タイトルの印刷
            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("実荷重データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Loads DATA", 0);
                    //　ヘッダー
                    headerM_content[0, 0] = "Member Load";
                    headerM_content[1, 0] = "First";
                    headerM_content[1, 1] = "Last";
                    headerM_content[1, 2] = "Direction";
                    headerM_content[1, 3] = "Mark";

                    headerP_content3D[0, 0] = "Node Load";
                    headerP_content3D[1, 1] = "Node No.";

                    headerP_content2D[0, 0] = "Node Load";
                    headerP_content2D[1, 1] = "Node No.";


                    break;
            }
            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            for (int i = 0; i < title.Count; i++)
            {
                // 部材荷重の印刷
                if (data[i][0] != null)
                {
                    if (data[i][0].Count != 0)
                    {
                        // 1タイプ内でページをまたぐかどうか
                        mc.TypeCount(i, 5, data[i][0].Count, title[i]);

                        // タイプの印刷
                        mc.CurrentColumn(0);
                        mc.PrintContent(title[i], 0);
                        mc.CurrentRow(2);

                        // ヘッダーの印刷
                        mc.Header(headerM_content, headerM_Xspacing);

                        for (int k = 0; k < data[i][0].Count; k++)
                        {
                            for (int l = 0; l < data[i][0][k].Length; l++)
                            {
                                mc.CurrentColumn(bodyM_Xspacing[0, l]); //x方向移動
                                mc.PrintContent(data[i][0][k][l]); // print
                            }
                            if (!(i == data.Count - 1 && k == data[i][0].Count - 1))
                            {
                                mc.CurrentRow(1); // y方向移動
                            }
                        }
                    }
                }

                // 節点荷重の印刷
                if (data[i][1] != null)
                {
                    if (i == 0)
                    {
                        mc.CurrentPos.Y += mc.single_Yrow;
                    }
                    // 1タイプ内でページをまたぐかどうか
                    mc.TypeCount(i, 5, data[i][1].Count, title[i]);

                    // 節点荷重のみの時に，タイプ番号を表示する
                    if (data[i][0] == null)
                    {
                        // タイプの印刷
                        mc.CurrentColumn(0);
                        mc.PrintContent(title[i], 0);
                        mc.CurrentRow(2);
                    }

                    // ヘッダーの印刷
                    mc.Header(headerP_content, headerP_Xspacing);

                    for (int k = 0; k < data[i][1].Count; k++)
                    {
                        for (int l = 0; l < data[i][1][k].Length; l++)
                        {
                            mc.CurrentColumn(bodyP_Xspacing[0, l]); //x方向移動
                            mc.PrintContent(data[i][1][k][l]); // print
                        }
                        if (!(i == data.Count - 1 && k == data[i][1].Count - 1))
                        {
                            mc.CurrentRow(1); // y方向移動
                        }
                    }
                }

            }
        }
      */      
    }

}
