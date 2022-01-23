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
    internal class ResultReacAnnexing
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string> title = new List<string>();
        List<string> type = new List<string>();
        Dictionary<string,string> typeList_ja = new Dictionary<string, string>(){
           { "tx_max","X方向の支点反力 最大"},
           { "tx_min","X方向の支点反力 最小" },
           { "ty_max", "Y方向の支点反力 最大" },
           { "ty_min","Y方向の支点反力 最小" },
           { "tz_max","Z方向の支点反力 最大" },
           { "tz_min", "Z方向の支点反力 最小" },
           { "mx_max", "X軸回りの回転反力 最大" },
           { "mx_min", "X軸回りの回転反力 最小" },
           { "my_max", "Y軸回りの回転反力 最大" },
           { "my_min", "Y軸回りの回転反力 最小" },
           { "mz_max", "Z軸回りの回転反力 最大" },
           { "mz_min", "Z軸回りの回転反力 最小" },
        };
        Dictionary<string, string> typeList_en = new Dictionary<string, string>(){
           { "tx_max","X Reaction Max"},
           { "tx_min","X Reaction Min" },
           { "ty_max","Y Reaction Max" },
           { "ty_min","Y Reaction Min" },
           { "tz_max","Z Reaction Max" },
           { "tz_min", "Z Reaction Min" },
           { "mx_max", "X Moment Reaction Max" },
           { "mx_min", "X Moment Reaction Min" },
           { "my_max", "Y Moment Reaction Max" },
           { "my_min", "Y Moment Reaction Min" },
           { "mz_max", "Z Moment Reaction Max" },
           { "mz_min", "Z Moment Reaction Min" },
        };

        List<List<List<string[]>>> dataCombine = new List<List<List<string[]>>>();
        List<List<List<string[]>>> dataPickup = new List<List<List<string[]>>>();
        public List<List<List<string[]>>> dataLL = new List<List<List<string[]>>>();

        /// <summary>
        /// Combine/Pickup反力データの読み取り
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="value_">全データ</param>
        /// <param name="key">combine,pickupのいずれか</param>
        public void ReacAnnexing(PdfDoc mc, Dictionary<string, object> value_, string key)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["reac" + key]).ToObject<Dictionary<string, object>>();

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
                var targetName = JArray.FromObject(value["reac" + key + "Name"]);

                // タイトルを入れる．
                var load = targetName[i];
                string[] loadNew = new String[2];

                loadNew[0] = load[0].ToString();
                loadNew[1] = load[1].ToString();

                title.Add(loadNew[0] + loadNew[1].PadLeft(loadNew[1].Length + 2));
                Elem.Remove("name");

                dataTreat(mc, Elem, key);
            }

        }

        /// <summary>
        /// 基本形以外のデータを取得する（ResultReac.csの判定でLLであった場合もここで読み取る）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="Elem">1caseぶんのデータ</param>
        /// <param name="key">combine,pickup,LLのいずれか</param>
        public void dataTreat(PdfDoc mc, Dictionary<string, object> Elem, string key)
        {
            List<List<string[]>> table = new List<List<string[]>>();
            for (int j = 0; j < Elem.Count; j++)
            {
                Dictionary<string, object> elist = JObject.FromObject(Elem.ElementAt(j).Value).ToObject<Dictionary<string, object>>();
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
                    var item = JObject.FromObject(elist.ElementAt(k).Value); ;
                    string[] line = new String[8];

                    line[0] = mc.TypeChange(elist.ElementAt(k).Key);
                    line[1] = mc.TypeChange(item["tx"], 2);
                    line[2] = mc.TypeChange(item["ty"], 2);
                    line[3] = mc.Dimension(mc.TypeChange(item["tz"], 2));
                    line[4] = mc.Dimension(mc.TypeChange(item["mx"], 2));
                    line[5] = mc.Dimension(mc.TypeChange(item["my"], 2));
                    line[6] = mc.TypeChange(item["mz"], 2);
                    line[7] = mc.TypeChange(item["case"]);

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
        /// Combine/Pickup/LL反力データのPDF書き込み（LLのみcase1つ当たりの処理）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="key">combine,pickup,LLのいずれか</param>
        /// <param name="title_LL">LLにかぎりケース番号を取得 ex)case2</param>
        /// <param name="LL_count">dataLLの何番目に必要なデータがあるか</param>
        public void ReacAnnexingPDF(PdfDoc mc, string key, string title_LL = "", int LL_count = 0)
        {
            //　ヘッダー
            string[,] header_content3D = {
                { "節点", "x方向の", "y方向の", "z方向の", "x軸回りの", "y軸回りの","z軸回りの","組合せ" },
                { "No", "支点反力", "支点反力", "支点反力", "回転反力", "回転反力","回転反力","" },
                { "",  "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)","" },
            };
            string[,] header_content2D = {
                { "節点", "x方向の", "y方向の", "", "", "","回転","組合せ" },
                { "No", "支点反力", "支点反力", "", "", "","拘束力","" },
                { "",  "(kN)", "(kN)", "", "", "", "(kN・m)","" },
            };

            switch (mc.language)
            {
                case "en":
                    header_content3D[0, 0] = "Node";
                    header_content3D[0, 1] = "X";
                    header_content3D[0, 2] = "Y";
                    header_content3D[0, 3] = "Z";
                    header_content3D[0, 4] = "X-Moment";
                    header_content3D[0, 5] = "Y-Moment";
                    header_content3D[0, 6] = "Z-Moment";
                    header_content3D[0, 7] = "Comb.";
                    header_content3D[1, 1] = "Reaction";
                    header_content3D[1, 2] = "Reaction";
                    header_content3D[1, 3] = "Reaction";
                    header_content3D[1, 4] = "Reaction";
                    header_content3D[1, 5] = "Reaction";
                    header_content3D[1, 6] = "Reaction";
                    header_content3D[1, 7] = "Reaction";

                    header_content2D[0, 0] = "Node";
                    header_content2D[0, 1] = "X";
                    header_content2D[0, 2] = "Y";
                    header_content2D[0, 6] = "Moment";
                    header_content2D[0, 4] = "Reaction";
                    header_content2D[0, 8] = "Reaction";
                    header_content2D[0, 9] = "Reaction";
                    break;
            }

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 18, 70, 130, 190, 250, 310, 370,420 },
                { 18, 70, 130, 190, 250, 310, 370,420 },
                { 18, 70, 130, 190, 250, 310, 370,420 },
            };
            int[,] header_Xspacing2D = {
                { 10, 70, 130, 0, 0, 0, 190,350 },
                { 10, 70, 130, 0, 0, 0, 190,350 },
                { 10, 70, 130, 0, 0, 0, 190,350 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 23, 85, 145, 215, 265, 325,385,435 }
            };
            int[,] body_Xspacing2D = {
                { 23, 85, 145, 0, 0, 0,205,435 }
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;
            mc.header_content = header_content;
            mc.header_Xspacing = header_Xspacing;
            mc.body_Xspacing = body_Xspacing;

            // 組合せ　一行にはいる文字数
            int textLen = mc.dimension == 3 ? 8 : 50;

            switch (key)
            {
                case "Combine":
                    mc.PrintResultAnnexingReady("reac", key, title, type, dataCombine, textLen);
                    break;

                case "Pickup":
                    mc.PrintResultAnnexingReady("reac", key, title, type, dataPickup, textLen);
                    break;

                case "LL":
                    mc.PrintResultAnnexing(title_LL, type, dataLL[LL_count], textLen);
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
            //mc.DataCountKeep(count, "reac" + key);

            // 印刷
            //mc.PrintResultAnnexing(title, type, data, header_content, header_Xspacing, body_Xspacing,10);

        }
    }
}

