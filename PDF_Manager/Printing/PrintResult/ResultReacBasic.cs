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
    internal class ResultReacBasic
    {
        //反力データの基本形のデータはここに入る
        public List<List<string[]>> data = new List<List<string[]>>();

        /// <summary>
        /// 基本形のデータを取得する
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="Elem">case1つぶんのデータ</param>
        public void ReacBasic(PdfDoc mc, JArray Elem)
        {
            List<string[]> table = new List<string[]>();

            for (int j = 0; j < Elem.Count; j++)
            {
                JToken item = Elem[j];

                string[] line = new String[7];

                line[0] = mc.TypeChange(item["id"]);
                line[1] = mc.TypeChange(item["tx"], 2);
                line[2] = mc.TypeChange(item["ty"], 2);
                line[3] = mc.TypeChange(item["tz"], 2);
                line[4] = mc.TypeChange(item["mx"], 2);
                line[5] = mc.TypeChange(item["my"], 2);
                line[6] = mc.TypeChange(item["mz"], 2);

                table.Add(line);
            }
            data.Add(table);
        }

        /// <summary>
        ///　反力データの基本形PDF書き込み
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="LL">LLがList何番目のデータか</param>
        public void ReacBasicPDF(PdfDoc mc, int LL = 0)
        {
            //　ヘッダー
            string[,] header_content3D = {
                { "SUPPORT", "TX", "TY", "TZ", "MX", "MY","MZ" },
                { "",  "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)" },
            };
            string[,] header_content2D = {
                { "SUPPORT", "TX", "TY", "TZ", "MX", "MY","MZ" },
                { "",  "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)" },
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 18, 70, 140, 210, 280, 350, 420 },
                { 18, 70, 140, 210, 280, 350, 420 },
            };
            int[,] header_Xspacing2D = {
                { 18, 70, 140, 210, 280, 350, 420 },
                { 18, 70, 140, 210, 280, 350, 420 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 23, 85, 155, 225, 295, 365,435 }
            };
            int[,] body_Xspacing2D = {
                { 23, 85, 155, 225, 295, 365,435 }
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

