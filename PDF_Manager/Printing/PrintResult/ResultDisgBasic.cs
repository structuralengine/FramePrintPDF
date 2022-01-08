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
    internal class ResultDisgBasic
    {
        //変位量データの基本形のデータはここに入る
        public List<List<string[]>> data = new List<List<string[]>>();

        /// <summary>
        /// 基本形のデータを取得する
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="Elem">case1つぶんのデータ</param>
        public void DisgBasic(PdfDoc mc, JArray Elem)
        {
            List<string[]> table = new List<string[]>();

            for (int j = 0; j < Elem.Count; j++)
            {
                JToken item = Elem[j];

                string[] line = new String[7];

                line[0] = mc.TypeChange(item["id"]);
                line[1] = mc.TypeChange(item["dx"], 4);
                line[2] = mc.TypeChange(item["dy"], 4);
                line[3] = mc.TypeChange(item["dz"], 4);
                line[4] = mc.TypeChange(item["rx"], 4);
                line[5] = mc.TypeChange(item["ry"], 4);
                line[6] = mc.TypeChange(item["rz"], 4);

                table.Add(line);
            }
            data.Add(table);
        }

        public void DisgBasicPDF(PdfDoc mc, int LL = 0)
        {   //　ヘッダー
            string[,] header_content3D = {
                { "節点", "X-Disp", "Y-Disp", "Z-Disp", "X-Rotation", "Y-Rotation", "Z-Rotation" },
                { "No", "(mm)", "(mm)", "(mm)", "(mmrad)", "(mmrad)", "(mmrad)" },
            };

            string[,] header_content2D = {
                { "節点", "X-Disp", "Y-Disp", "Z-Disp", "X-Rotation", "Y-Rotation", "Z-Rotation" },
                { "No", "(mm)", "(mm)", "(mm)", "(mmrad)", "(mmrad)", "(mmrad)" },
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 10, 70, 140, 210, 280, 350, 420 },
                { 10, 70, 140, 210, 280, 350, 420 },
            };

            int[,] header_Xspacing2D = {
                { 10, 70, 140, 210, 280, 350, 420 },
                { 10, 70, 140, 210, 280, 350, 420 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 85, 155, 225, 295, 365,435 }
            };

            int[,] body_Xspacing2D = {
                { 17, 85, 155, 225, 295, 365,435 }
            };

            string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;

            // 印刷
            mc.PrintResultBasic(data[LL], header_content, header_Xspacing, body_Xspacing, LL);

        }
    }
}

