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

            if (mc.dimension == 3)
            {
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
            else if (mc.dimension == 2)
            {
                int bottomCell = mc.bottomCell * 2;
                var row = Elem.Count;
                var page = 0;

                while (true)
                {
                    if (row > bottomCell)
                    {
                        List<string[]> body = new List<string[]>();
                        var half = bottomCell / 2;
                        for (var i = 0; i < half; i++)
                        {
                            //　各行の配列開始位置を取得する（左段/右段)
                            var j = bottomCell * page + i;
                            var k = bottomCell * page + bottomCell / 2 + i;
                            //　各行のデータを取得する（左段/右段)
                            var targetValue_l = Elem[j];

                            string[] line = new String[8];
                            line[0] = mc.TypeChange(targetValue_l["id"]);
                            line[1] = mc.TypeChange(targetValue_l["dx"], 4);
                            line[2] = mc.TypeChange(targetValue_l["dy"], 4);
                            line[3] = mc.TypeChange(targetValue_l["rz"], 4);

                            var targetValue_r = Elem[k];
                            line[4] = mc.TypeChange(targetValue_r["id"]);
                            line[5] = mc.TypeChange(targetValue_r["dx"], 4);
                            line[6] = mc.TypeChange(targetValue_r["dy"], 4);
                            line[7] = mc.TypeChange(targetValue_r["rz"], 4);
                            body.Add(line);
                        }
                        data.Add(body);
                        row -= bottomCell;
                        page++;
                    }
                    else
                    {
                        List<string[]> body = new List<string[]>();

                        row = row % 2 == 0 ? row / 2 : row / 2 + 1;

                        for (var i = 0; i < row; i++)
                        {
                            //　各行の配列開始位置を取得する（左段/右段)
                            var j = bottomCell * page + i;
                            var k = j + row;
                            //　各行のデータを取得する（左段)
                            var targetValue_l = Elem[j];

                            string[] line = new String[8];
                            line[0] = mc.TypeChange(targetValue_l["id"]);
                            line[1] = mc.TypeChange(targetValue_l["dx"], 4);
                            line[2] = mc.TypeChange(targetValue_l["dy"], 4);
                            line[3] = mc.TypeChange(targetValue_l["rz"], 4);

                            try
                            {
                                //　各行のデータを取得する（右段)
                                var targetValue_r = Elem[k];
                                line[4] = mc.TypeChange(targetValue_r["id"]);
                                line[5] = mc.TypeChange(targetValue_r["dx"], 4);
                                line[6] = mc.TypeChange(targetValue_r["dy"], 4);
                                line[7] = mc.TypeChange(targetValue_r["rz"], 4);
                                body.Add(line);
                            }
                            catch
                            {
                                line[4] = "";
                                line[5] = "";
                                line[6] = "";
                                line[7] = "";
                                body.Add(line);
                            }
                        }
                        data.Add(body);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///　変位データの基本形PDF書き込み
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="LL">LLがList何番目のデータか</param>
        public void DisgBasicPDF(PdfDoc mc, int LL = 0)
        {   //　ヘッダー
            string[,] header_content3D = {
                { "節点", "x方向の", "y方向の", "z方向の", "x軸回りの", "y軸回りの", "z軸回りの" },
                { "No", "移動量", "移動量", "移動量", "回転量", "回転量", "回転量" },
                { "", "(mm)", "(mm)", "(mm)", "(mmrad)", "(mmrad)", "(mmrad)" },
            };

            string[,] header_content2D = {
                 { "節点", "x方向の", "y方向の", "", "節点", "x方向の", "y方向の", "" },
                { "No", "移動量", "移動量", "回転量", "No", "移動量", "移動量", "回転量" },
                { "", "(mm)", "(mm)", "(mmrad)", "","(mmrad)", "(mmrad)", "(mmrad)" },
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 10, 65, 130, 195, 260, 325, 390 },
                { 10, 65, 130, 195, 260, 325, 390 },
                { 10, 65, 130, 195, 260, 325, 390 },
            };

            int[,] header_Xspacing2D = {
                { 10, 60, 120, 180, 250, 300, 360,420 },
                { 10, 60, 120, 180, 250, 300, 360,420 },
                { 10, 60, 120, 180, 250, 300, 360,420 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 85, 150, 215, 280, 345,410 }
            };

            int[,] body_Xspacing2D = {
                { 17, 80, 140, 200, 257,320,380,440 }
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

