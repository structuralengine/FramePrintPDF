using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDF_Manager.Printing
{
    public class Member
    {
        public double L;
        public double cg;
        public int ni;
        public int nj;
        public int e;
        public string n;
    }


    internal class InputMember
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        private JObject targetLen;
        private JToken mem;
        private InputElement element;

        // 集まったデータはここに格納する
        List<string[]> data = new List<string[]>();


        public void init(PdfDoc mc, InputElement element, Dictionary<string, object> value_)
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

                double len = this.GetMemberLength(mc, index, value); // 部材長さ

                string name = item["e"].Type == JTokenType.Null ? "" : element.GetElementName(item["e"].ToString());

                string[] line = new String[7];
                line[0] = index;
                line[1] = dataManager.TypeChange(item["ni"]);
                line[2] = dataManager.TypeChange(item["nj"]);
                line[3] = dataManager.TypeChange(len, 3);
                line[4] = dataManager.TypeChange(item["e"]);
                line[5] = mc.Dimension(dataManager.TypeChange(item["cg"]));
                line[6] = name;
                data.Add(line);
            }
        }

        public double GetMemberLength(PdfDoc mc, string memberNo, Dictionary<string, object> value)
        {
            JToken memb = this.GetMember(memberNo);

            string ni = memb["ni"].ToString();
            string nj = memb["nj"].ToString();
            if (ni == null || nj == null)
            {
                return 0;
            }

            InputNode node = new InputNode();
            double[] iPos = node.GetNodePos( ni, value);
            double[] jPos = node.GetNodePos( nj, value);
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
    }
}

