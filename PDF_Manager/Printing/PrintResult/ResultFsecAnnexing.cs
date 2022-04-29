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
    internal class ResultFsecAnnexing
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string> title = new List<string>();
        List<string> type = new List<string>();
        Dictionary<string, string> typeList_ja = new Dictionary<string, string>(){
           { "fx_max", "軸方向力 最大"},
           { "fx_min", "軸方向力 最小" },
           { "fy_max", "Y方向のせん断力 最大" },
           { "fy_min", "Y方向のせん断力 最小" },
           { "fz_max", "Z方向のせん断力 最大" },
           { "fz_min", "Z方向のせん断力 最小" },
           { "mx_max", "ねじりモーメント 最大" },
           { "mx_min", "ねじりモーメント 最小" },
           { "my_max", "Y軸回りの曲げモーメント 最大" },
           { "my_min", "Y軸回りの曲げモーメント 最小" },
           { "mz_max", "Z軸回りの曲げモーメント 最大" },
           { "mz_min", "Z軸回りの曲げモーメント 最小" },
        };
        Dictionary<string, string> typeList_en = new Dictionary<string, string>(){
           { "fx_max", "Axial Force Max"},
           { "fx_min", "Axial Force Min" },
           { "fy_max", "Y Shear Force Max" },
           { "fy_min", "Y Shear Force Min" },
           { "fz_max", "Z Shear Force Max" },
           { "fz_min", "Z Shear Force Min" },
           { "mx_max", "X Torsion Max" },
           { "mx_min", "X Torsion Min" },
           { "my_max", "Y Bending Moment Max" },
           { "my_min", "Y Bending Moment Min" },
           { "mz_max", "Z Bending Moment Max" },
           { "mz_min", "Z Bending Moment Min" },
        };

        List<List<List<string[]>>> dataCombine = new List<List<List<string[]>>>();
        List<List<List<string[]>>> dataPickup = new List<List<List<string[]>>>();
        public List<List<List<string[]>>> dataLL = new List<List<List<string[]>>>();


        /// <summary>
        /// Combine/Pickup断面力データの読み取り
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="value_">全データ</param>
        /// <param name="key">combine,pickupのいずれか</param>
        public void init(PdfDoc mc, Dictionary<string, object> value_, string key)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["fsec" + key]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            title = new List<string>();
            type = new List<string>();

            switch (key)
            {
                case "Combine":
                    dataCombine = new List<List<List<string[]>>>();
                    break;
                case "Pickup":
                    dataPickup = new List<List<List<string[]>>>();
                    break;
            }

            for (int i = 0; i < target.Count; i++)
            {
                var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                var targetName = JArray.FromObject(value["fsec" + key + "Name"]);
                // タイトルを入れる．
                var load = targetName[i];
                string[] loadNew = new String[2];

                loadNew[0] = load[0].ToString();
                loadNew[1] = load[1].ToString();

                title.Add(loadNew[0] + loadNew[1].PadLeft(loadNew[1].Length + 2));
                Elem.Remove("name");

                dataTreat(mc, Elem, key);

                //List<List<string[]>> table = new List<List<string[]>>();

                //for (int j = 0; j < Elem.Count; j++)
                //{
                //    var elist = JArray.FromObject(Elem.ElementAt(j).Value);

                //    List<string[]> body = new List<string[]>();

                //    for (int k = 0; k < elist.Count; k++)
                //    {
                //        var item = elist[k]; 
                //        string[] line = new String[10];

                //        line[0] = dataManager.TypeChange(item["m"]);
                //        line[1] = dataManager.TypeChange(item["n"]);
                //        line[2] = dataManager.TypeChange(item["l"], 3);
                //        line[3] = dataManager.TypeChange(item["fx"], 2);
                //        line[4] = dataManager.TypeChange(item["fy"], 2);
                //        line[5] = dataManager.TypeChange(item["fz"], 2);
                //        line[6] = dataManager.TypeChange(item["mx"], 2);
                //        line[7] = dataManager.TypeChange(item["my"], 2);
                //        line[8] = dataManager.TypeChange(item["mz"], 2);
                //        line[9] = dataManager.TypeChange(item["case"]);

                //        body.Add(line);
                //    }
                //    table.Add(body);
                //}
                //switch (key)
                //{
                //    case "Combine":
                //        dataCombine.Add(table);
                //        break;
                //    case "Pickup":
                //        dataPickup.Add(table);
                //        break;
                //}
            }

        }

        /// <summary>
        /// 基本形以外のデータを取得する（ResultFsec.csの判定でLLであった場合もここで読み取る）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="Elem">1caseぶんのデータ</param>
        /// <param name="key">combine,pickup,LLのいずれか</param>
        public void dataTreat(PdfDoc mc, Dictionary<string, object> Elem, string key)
        {
            List<List<string[]>> table = new List<List<string[]>>();
            for (int j = 0; j < Elem.Count; j++)
            {
                JArray elist = JArray.FromObject(Elem.ElementAt(j).Value);

                switch (mc.language)
                {
                    case "ja":
                        type.Add(typeList_ja[Elem.ElementAt(j).Key]);
                        break;
                    case "en":
                        type.Add(typeList_en[Elem.ElementAt(j).Key]);
                        break;
                }

                List<string[]> body = new List<string[]>();

                for (int k = 0; k < elist.Count; k++)
                {
                    var item = elist[k];
                    string[] line = new String[10];

                    line[0] = dataManager.TypeChange(item["m"]);
                    line[1] = dataManager.TypeChange(item["n"]);
                    line[2] = dataManager.TypeChange(item["l"], 3);
                    line[3] = dataManager.TypeChange(item["fx"], 2);
                    line[4] = dataManager.TypeChange(item["fy"], 2);
                    line[5] = mc.Dimension(dataManager.TypeChange(item["fz"], 2));
                    line[6] = mc.Dimension(dataManager.TypeChange(item["mx"], 2));
                    line[7] = mc.Dimension(dataManager.TypeChange(item["my"], 2));
                    line[8] = dataManager.TypeChange(item["mz"], 2);
                    line[9] = dataManager.TypeChange(item["case"]);


                    body.Add(line);
                }
                table.Add(body);
            }

            //keyに応じたListに挿入する
            switch (key)
            {
                case "Combine":
                    dataCombine.Add(table);
                    break;
                case "Pickup":
                    dataPickup.Add(table);
                    break;
                case "LL":
                    dataLL.Add(table);
                    break;
            }
        }

        /// <summary>
        /// Combine/Pickup/LL断面力データのPDF書き込み（LLのみcase1つ当たりの処理）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="key">combine,pickup,LLのいずれか</param>
        /// <param name="title_LL">LLにかぎりケース番号を取得 ex)case2</param>
        /// <param name="LL_count">dataLLの何番目に必要なデータがあるか</param>
        public void FsecAnnexingPDF(PdfDoc mc, string key, string title_LL = "", int LL_count = 0)
        {
            //　ヘッダー
            string[,] header_content3D = {
                { "部材", "節点", "着目", "", "y軸方向の", "z軸方向の", "ねじり","y軸回りの","z軸回りの","組合せ" },
                { "No","No","位置", "軸方向力", "せん断力", "せん断力", "ﾓｰﾒﾝﾄ", "曲げﾓｰﾒﾝﾄ", "曲げﾓｰﾒﾝﾄ","" },
                { "","","(m)", "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)","" },

            };
            string[,] header_content2D = {
                { "部材", "節点", "着目位置", "軸方向力", "せん断力", "", "","","曲げﾓｰﾒﾝﾄ","組合せ" },
                { "No","No","(m)", "(kN)", "(kN)", "", "", "", "(kN・m)","" },
            };

            switch (mc.language)
            {
                case "en":
                    header_content3D[0, 0] = "Member";
                    header_content3D[0, 1] = "Node";
                    header_content3D[0, 2] = "Station";
                    header_content3D[0, 3] = "Axial";
                    header_content3D[0, 4] = "Y";
                    header_content3D[0, 5] = "Z";
                    header_content3D[0, 6] = "X";
                    header_content3D[0, 7] = "Y";
                    header_content3D[0, 8] = "Z";
                    header_content3D[0, 9] = "Comb.";
                    header_content3D[1, 2] = "Location";
                    header_content3D[1, 3] = "Force";
                    header_content3D[1, 4] = "Shear";
                    header_content3D[1, 5] = "Shear";
                    header_content3D[1, 6] = "Torsion";
                    header_content3D[1, 7] = "Moment";
                    header_content3D[1, 8] = "Moment";

                    header_content2D[0, 0] = "Node";
                    header_content2D[0, 1] = "Member";
                    header_content2D[0, 2] = "Station-Location";
                    header_content2D[0, 3] = "Axial-Force";
                    header_content2D[0, 4] = "Shear";
                    header_content2D[0, 8] = "Momemt";
                    header_content2D[0, 9] = "Comb";
                    break;
            }


            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 10, 35, 72, 125, 175, 225, 275,325,375,425 },
                { 10, 35, 72, 125, 175, 225, 275,325,375,425 },
                { 10, 35, 72, 125, 175, 225, 275,325,375,425 },
            };
            int[,] header_Xspacing2D = {
                { 10, 35, 72, 125, 175, 0, 0,0,225,370 },
                { 10, 35, 72, 125, 175, 0, 0,0,225,370 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 42, 90, 145, 195, 245, 295,345,395,440},
            };
            int[,] body_Xspacing2D = {
                { 17, 42, 90, 145, 195, 0, 0, 0,245,440},
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;
            mc.header_content = header_content;
            mc.header_Xspacing = header_Xspacing;
            mc.body_Xspacing = body_Xspacing;

            // 組合せ　一行にはいる文字数
            int textLen = mc.dimension == 3 ? 6 : 40;

            switch (key)
            {
                case "Combine":
                    mc.PrintResultAnnexingReady("fsec", key, title, type, dataCombine, textLen);
                    break;

                case "Pickup":
                    mc.PrintResultAnnexingReady("fsec", key, title, type, dataPickup, textLen);
                    break;

                case "LL":
                    mc.PrintResultAnnexing(title_LL, type, dataLL[LL_count], textLen ,"fsec");
                    break;
            }

            //// 全行の取得
            //int count = 2;
            //for (int i = 0; i < title.Count; i++)
            //{
            //    for (int j = 0; j < data[i].Count; j++)
            //    {
            //        for (int k = 0; k < data[i][j].Count; k++)
            //        {
            //            count += (data[i].Count * 5 + data[i][j].Count * 2 + data[i][j][k].Length) * mc.single_Yrow + 1;
            //        }
            //    }
            //}

            //// 改ページ判定
            //mc.DataCountKeep(count, "fsec" + key);

            // 印刷
            //mc.PrintResultAnnexing(title, type, data, header_content, header_Xspacing, body_Xspacing,6);

        }
    }
}

