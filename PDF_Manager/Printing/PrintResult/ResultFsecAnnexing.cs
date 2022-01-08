﻿using Newtonsoft.Json.Linq;
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
    internal class ResultFsecAnnexing
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        List<string> title = new List<string>();
        string[] type = {
            "軸方向力 最大",
            "軸方向力 最小",
            "y方向のせん断力 最大",
            "y方向のせん断力 最小",
            "z方向のせん断力 最大",
            "z方向のせん断力 最小",
            "ねじりモーメント 最大",
            "ねじりモーメント 最小",
            "y軸回りの曲げモーメント 最大",
            "y軸回りの曲げモーメント力 最小",
            "z軸回りの曲げモーメント 最大",
            "z軸回りの曲げモーメント 最小",
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
        public void FsecAnnexing(PdfDoc mc, Dictionary<string, object> value_, string key)
        {
            value = value_;
            //nodeデータを取得する
            var target = JObject.FromObject(value["fsec" + key]).ToObject<Dictionary<string, object>>();

            // 集まったデータはここに格納する
            title = new List<string>();
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

                // タイトルを入れる．
                title.Add("Case." + target.ElementAt(i).Key);

                dataTreat(mc,Elem, key);

                //List<List<string[]>> table = new List<List<string[]>>();

                //for (int j = 0; j < Elem.Count; j++)
                //{
                //    var elist = JArray.FromObject(Elem.ElementAt(j).Value);

                //    List<string[]> body = new List<string[]>();

                //    for (int k = 0; k < elist.Count; k++)
                //    {
                //        var item = elist[k]; 
                //        string[] line = new String[10];

                //        line[0] = mc.TypeChange(item["m"]);
                //        line[1] = mc.TypeChange(item["n"]);
                //        line[2] = mc.TypeChange(item["l"], 3);
                //        line[3] = mc.TypeChange(item["fx"], 2);
                //        line[4] = mc.TypeChange(item["fy"], 2);
                //        line[5] = mc.TypeChange(item["fz"], 2);
                //        line[6] = mc.TypeChange(item["mx"], 2);
                //        line[7] = mc.TypeChange(item["my"], 2);
                //        line[8] = mc.TypeChange(item["mz"], 2);
                //        line[9] = mc.TypeChange(item["case"]);

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

                List<string[]> body = new List<string[]>();

                for (int k = 0; k < elist.Count; k++)
                {
                    var item = elist[k] ;
                    string[] line = new String[10];

                    line[0] = mc.TypeChange(item["m"]);
                    line[1] = mc.TypeChange(item["n"]);
                    line[2] = mc.TypeChange(item["l"], 3);
                    line[3] = mc.TypeChange(item["fx"], 2);
                    line[4] = mc.TypeChange(item["fy"], 2);
                    line[5] = mc.TypeChange(item["fz"], 2);
                    line[6] = mc.TypeChange(item["mx"], 2);
                    line[7] = mc.TypeChange(item["my"], 2);
                    line[8] = mc.TypeChange(item["mz"], 2);
                    line[9] = mc.TypeChange(item["case"]);


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
            string[,] header_content = {
                { "部材", "節点", "着目位置", "FX", "FY", "FZ", "MX","MY","MZ","組合せ" },
                { "No","No","DIST", "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)","" },
            };
            // ヘッダーのx方向の余白
            int[,] header_Xspacing = {
                { 10, 40, 85, 135, 185, 235, 285,335,385,425},
                { 10, 40, 85, 135, 185, 235, 285,335,385,425},
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing = {
                { 17, 50, 97, 150, 200, 250,300,350,400,440 }
            };

            mc.header_content = header_content;
            mc.header_Xspacing = header_Xspacing;
            mc.body_Xspacing = body_Xspacing;

            switch (key)
            {
                case "Combine":
                    mc.PrintResultAnnexingReady("fsec", key, title, type, dataCombine, 14);
                    break;

                case "Pickup":
                    mc.PrintResultAnnexingReady("fsec", key, title, type, dataPickup, 14);
                    break;

                case "LL":
                    mc.PrintResultAnnexing(title_LL, type, dataLL[LL_count], 14);
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

