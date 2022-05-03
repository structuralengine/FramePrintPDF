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
    public class Joint
    {
        public string m;   // 部材番号
        public int xi;
        public int yi;
        public int zi;
        public int xj;
        public int yj;
        public int zj;
    }


    internal class InputJoint
    {
        public const string KEY = "joint";

        private Dictionary<int, List<Joint>> joints = new Dictionary<int, List<Joint>>();

        public InputJoint(Dictionary<string, object> value)
        {
            if (!value.ContainsKey(KEY))
                return;

            // データを取得する．
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = dataManager.parseInt(target.ElementAt(i).Key);  // タイプ番号
                JArray Joi = JArray.FromObject(target.ElementAt(i).Value);

                var _joint = new List<Joint>();

                for (int j = 0; j < Joi.Count; j++)
                {
                    JToken item = Joi[j];

                    var jo = new Joint();

                    jo.m = dataManager.TypeChange(item["m"]);
                    jo.xi = dataManager.parseInt(item["xi"]);
                    jo.yi = dataManager.parseInt(item["yi"]);
                    jo.zi = dataManager.parseInt(item["zi"]);
                    jo.xj = dataManager.parseInt(item["xj"]);
                    jo.yj = dataManager.parseInt(item["yj"]);
                    jo.zj = dataManager.parseInt(item["zj"]);

                    _joint.Add(jo);

                }
                this.joints.Add(key, _joint);
            }

        }


        /*
        // 集まったデータはすべてここに格納する
        List<string> title = new List<string>();
        List<List<string[]>> data = new List<List<string[]>>();

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

            for (int j = 0; j < Elem.Count; j++)
            {
                JToken item = Elem[j];

                string[] line = new String[7];

                line[0] = dataManager.TypeChange(item["m"]);
                line[1] = mc.Dimension(dataManager.TypeChange(item["xi"]));
                line[2] = mc.Dimension(dataManager.TypeChange(item["yi"]));
                line[3] = dataManager.TypeChange(item["zi"]);
                line[4] = mc.Dimension(dataManager.TypeChange(item["xj"]));
                line[5] = mc.Dimension(dataManager.TypeChange(item["yj"]));
                line[6] = dataManager.TypeChange(item["zj"]);

                table.Add(line);
            }
            data.Add(table);
        }
        */
        /*
        public void JointPDF(PdfDoc mc)
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
                { "部材", "", "i端側", "", "", "j端側", "" },
                { "No", "X", "Y", "Z", "X", "Y", "Z" },
            };
            string[,] header_content2D = {
                { "部材", "", "i端側", "", "", "j端側", "" },
                { "No", "", "", "Z", "", "", "Z" },
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 10, 60, 120, 180, 240, 300, 360 },
                { 10, 60, 120, 180, 240, 300, 360 },
            };

            int[,] header_Xspacing2D = {
                { 10, 0, 120, 0, 0, 300, 0 },
                { 10, 0, 0, 120, 0, 0, 300 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 67, 127, 187, 247, 307, 367 }
            };

            int[,] body_Xspacing2D = {
                { 17, 0, 0, 127, 0, 0, 307 }
            };

            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("結合データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Fixity DATA", 0);
                    //　ヘッダー
                    header_content3D[0, 0] = "Member";
                    header_content3D[0, 2] = "Node-I";
                    header_content3D[0, 5] = "Node-J";

                    header_content2D[0, 0] = "Member";
                    header_content2D[0, 2] = "Node-I";
                    header_content2D[0, 5] = "Node-J";
                 
                    break;
            }
            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;

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
                    if (i == data.Count - 1 && j == data[i].Count - 1)
                    {
                        mc.CurrentRow(1); // y方向移動
                    }
                }
            }

        }
        */
    }

}
