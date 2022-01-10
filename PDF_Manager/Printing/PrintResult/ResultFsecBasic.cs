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
    internal class ResultFsecBasic
    {
        //変位量データの基本形のデータはここに入る
        public List<List<string[]>> data = new List<List<string[]>>();

        /// <summary>
        /// 基本形のデータを取得する
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="Elem">case1つぶんのデータ</param>
        public void FsecBasic(PdfDoc mc, JArray Elem)
        {
            List<string[]> table = new List<string[]>();

            for (int j = 0; j < Elem.Count; j++)
            {
                JToken item = Elem[j];

                string[] line = new String[9];

                line[0] = mc.TypeChange(item["m"]);
                line[1] = mc.TypeChange(item["n"]);
                line[2] = mc.TypeChange(item["l"], 3);
                line[3] = mc.TypeChange(item["fx"], 2);
                line[4] = mc.TypeChange(item["fy"], 2);
                line[5] = mc.Dimension(mc.TypeChange(item["fz"], 2));
                line[6] = mc.Dimension(mc.TypeChange(item["mx"], 2));
                line[7] = mc.Dimension(mc.TypeChange(item["my"], 2));
                line[8] = mc.TypeChange(item["mz"], 2);

                table.Add(line);
            }
            data.Add(table);
        }

        /// <summary>
        ///　断面力データの基本形PDF書き込み
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="LL">LLがList何番目のデータか</param>
        public void FsecBasicPDF(PdfDoc mc, int LL = 0)
        {   //　ヘッダー
            string[,] header_content3D = {
                { "部材", "節点", "着目", "", "y軸方向の", "z軸方向の", "ねじり","y軸回りの","z軸回りの" },
                { "No","No","位置", "軸方向力", "せん断力", "せん断力", "ﾓｰﾒﾝﾄ", "曲げﾓｰﾒﾝﾄ", "曲げﾓｰﾒﾝﾄ" },
                { "","","(m)", "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)" },

            };
            string[,] header_content2D = {
                { "部材", "節点", "着目位置", "軸方向力", "せん断力", "", "","","曲げﾓｰﾒﾝﾄ" },
                { "No","No","(m)", "(kN)", "(kN)", "", "", "", "(kN・m)" },
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 10, 35, 77, 130, 185, 240, 295,350,415 },
                { 10, 35, 77, 130, 185, 240, 295,350,415 },
                { 10, 35, 77, 130, 185, 240, 295,350,415 },
            };
            int[,] header_Xspacing2D = {
                { 10, 45, 87, 150, 220, 0, 0,0,290 },
                { 10, 45, 87, 150, 220, 0, 0,0,290 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 52, 105, 150, 205, 260, 315,370,435 }
            };
            int[,] body_Xspacing2D = {
                { 17, 52, 105, 170, 240, 0, 0,0,310 }
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;

            mc.header_content = header_content;
            mc.header_Xspacing = header_Xspacing;
            mc.body_Xspacing = body_Xspacing;

            // 印刷
            mc.PrintResultBasic(data[LL], LL);

        }
    }
}

