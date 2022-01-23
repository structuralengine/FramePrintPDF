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
    internal class ResultDisgAnnexing
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        public List<string> title = new List<string>();
        List<string> type = new List<string>();
        Dictionary<string, string> typeList_ja = new Dictionary<string, string>(){
           { "dx_max","X方向の移動量 最大"},
           { "dx_min","X方向の移動量 最小" },
           { "dy_max", "Y方向の移動量 最大" },
           { "dy_min","Y方向の移動量 最小" },
           { "dz_max","Z方向の移動量 最大" },
           { "dz_min", "Z方向の移動量 最小" },
           { "rx_max", "X軸回りの回転角 最大" },
           { "rx_min", "X軸回りの回転角 最小" },
           { "ry_max", "Y軸回りの回転角 最大" },
           { "ry_min", "Y軸回りの回転角 最小" },
           { "rz_max", "Z軸回りの回転角 最大" },
           { "rz_min", "Z軸回りの回転角 最小" },
        };
        Dictionary<string, string> typeList_en = new Dictionary<string, string>(){
           { "dx_max","X Displacement Max"},
           { "dx_min","X Displacement Min" },
           { "dy_max", "Y Displacement Max" },
           { "dy_min","Y Displacement Min" },
           { "dz_max","Z Displacement Max" },
           { "dz_min", "Z Displacement Min" },
           { "rx_max", "X Rotation Max" },
           { "rx_min", "X Rotation Min" },
           { "ry_max", "Y Rotation Max" },
           { "ry_min", "Y Rotation Min" },
           { "rz_max", "Z Rotation Max" },
           { "rz_min", "Z Rotation Min" },
        };

        List<List<List<string[]>>> dataCombine = new List<List<List<string[]>>>();
        List<List<List<string[]>>> dataPickup = new List<List<List<string[]>>>();
        public List<List<List<string[]>>> dataLL = new List<List<List<string[]>>>();

        /// <summary>
        /// Combine/Pickup変位量データの読み取り
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="value_">全データ</param>
        /// <param name="key">combine,pickupのいずれか</param>
        public void DisgAnnexing(PdfDoc mc, Dictionary<string, object> value_, string key)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["disg" + key]).ToObject<Dictionary<string, object>>();

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
                var targetName = JArray.FromObject(value["disg" + key + "Name"]);

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
        /// 基本形以外のデータを取得する（ResultDisg.csの判定でLLであった場合もここで読み取る）
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
                    var item = JObject.FromObject(elist.ElementAt(k).Value);
                    string[] line = new String[8];

                    line[0] = mc.TypeChange(elist.ElementAt(k).Key);
                    line[1] = mc.TypeChange(item["dx"], 4);
                    line[2] = mc.TypeChange(item["dy"], 4);
                    line[3] = mc.Dimension(mc.TypeChange(item["dz"], 4));
                    line[4] = mc.Dimension(mc.TypeChange(item["rx"], 4));
                    line[5] = mc.Dimension(mc.TypeChange(item["ry"], 4));
                    line[6] = mc.TypeChange(item["rz"], 4);
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
        /// Combine/Pickup/LL変位量データのPDF書き込み（LLのみcase1つ当たりの処理）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="key">combine,pickup,LLのいずれか</param>
        /// <param name="title_LL">LLにかぎりケース番号を取得 ex)case2</param>
        /// <param name="LL_count">dataLLの何番目に必要なデータがあるか</param>
        public void DisgAnnexingPDF(PdfDoc mc, string key, string title_LL = "", int LL_count = 0)
        {
            //　ヘッダー
            string[,] header_content3D = {
                { "節点", "X方向の", "Y方向の", "Z方向の", "X軸回りの", "Y軸回りの", "Z軸回りの","組合せ" },
                { "No", "移動量", "移動量", "移動量", "回転量", "回転量", "回転量","" },
                { "", "(mm)", "(mm)", "(mm)", "(mmrad)", "(mmrad)", "(mmrad)","" },
            };

            string[,] header_content2D = {
                { "節点", "X方向の", "Y方向の", "", "", "", "","組合せ" },
                { "No", "移動量", "移動量", "", "", "", "回転量","" },
                { "", "(mm)", "(mm)", "", "", "", "(mmrad)","" },
            };

            switch (mc.language)
            {
                case "en":
                    header_content3D[0, 0] = "Node";
                    header_content3D[0, 1] = "X";
                    header_content3D[0, 2] = "Y";
                    header_content3D[0, 3] = "Z";
                    header_content3D[0, 4] = "X";
                    header_content3D[0, 5] = "Y";
                    header_content3D[0, 6] = "Z";
                    header_content3D[0, 7] = "Comb.";
                    header_content3D[1, 1] = "Displacement";
                    header_content3D[1, 2] = "Displacement";
                    header_content3D[1, 3] = "Displacement";
                    header_content3D[1, 4] = "Rotation";
                    header_content3D[1, 5] = "Rotation";
                    header_content3D[1, 6] = "Rotation";

                    header_content2D[0, 0] = "Node";
                    header_content2D[0, 1] = "X";
                    header_content2D[0, 2] = "Y";
                    header_content2D[0, 7] = "Comb.";
                    header_content2D[1, 1] = "Displacement";
                    header_content2D[1, 2] = "Displacement";
                    header_content2D[1, 6] = "Rotation";
                    break;
            }

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 10, 60, 120, 180, 240, 300, 360,417 },
                { 10, 60, 120, 180, 240, 300, 360,417 },
                { 10, 60, 120, 180, 240, 300, 360,417 },
            };

            int[,] header_Xspacing2D = {
                { 10, 60, 120, 0, 0, 0, 180,350 },
                { 10, 60, 120, 0, 0, 0, 180,350 },
                { 10, 60, 120, 0, 0, 0, 180,350 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 80, 140, 200, 260,320,380,435 }
            };

            int[,] body_Xspacing2D = {
                { 17, 80, 140, 0, 0,0,200,435 }
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
                    mc.PrintResultAnnexingReady("disg", key, title, type, dataCombine, textLen);
                    break;

                case "Pickup":
                    mc.PrintResultAnnexingReady("disg", key, title, type, dataPickup, textLen);
                    break;

                case "LL":
                    mc.PrintResultAnnexing(title_LL, type, dataLL[LL_count], textLen);
                    break;
            }

            // 全行の取得
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
            //mc.DataCountKeep(count, "disg" + key);


            //// タイトルの印刷
            //mc.PrintContent(key + "変位量", 0);
            //mc.CurrentRow(2);

            // 印刷

        }
    }
}

