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
    /*
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

            if (mc.dimension == 3)
            {
                for (int j = 0; j < Elem.Count; j++)
                {
                    JToken item = Elem[j];

                    string[] line = new String[7];

                    line[0] = dataManager.TypeChange(item["id"]);
                    line[1] = dataManager.TypeChange(item["tx"], 2);
                    line[2] = dataManager.TypeChange(item["ty"], 2);
                    line[3] = mc.Dimension(dataManager.TypeChange(item["tz"], 2));
                    line[4] = mc.Dimension(dataManager.TypeChange(item["mx"], 2));
                    line[5] = mc.Dimension(dataManager.TypeChange(item["my"], 2));
                    line[6] = dataManager.TypeChange(item["mz"], 2);

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
                            line[0] = dataManager.TypeChange(targetValue_l["id"]);
                            line[1] = dataManager.TypeChange(targetValue_l["tx"], 2);
                            line[2] = dataManager.TypeChange(targetValue_l["ty"], 2);
                            line[3] = dataManager.TypeChange(targetValue_l["mz"], 2);

                            var targetValue_r = Elem[k];
                            line[4] = dataManager.TypeChange(targetValue_r["id"]);
                            line[5] = dataManager.TypeChange(targetValue_r["tx"], 2);
                            line[6] = dataManager.TypeChange(targetValue_r["ty"], 2);
                            line[7] = dataManager.TypeChange(targetValue_r["mz"], 2);
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
                            line[0] = dataManager.TypeChange(targetValue_l["id"]);
                            line[1] = dataManager.TypeChange(targetValue_l["tx"], 2);
                            line[2] = dataManager.TypeChange(targetValue_l["ty"], 2);
                            line[3] = dataManager.TypeChange(targetValue_l["mz"], 2);

                            try
                            {
                                //　各行のデータを取得する（右段)
                                var targetValue_r = Elem[k];
                                line[4] = dataManager.TypeChange(targetValue_r["id"]);
                                line[5] = dataManager.TypeChange(targetValue_r["tx"], 2);
                                line[6] = dataManager.TypeChange(targetValue_r["ty"], 2);
                                line[7] = dataManager.TypeChange(targetValue_r["mz"], 2);
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
        ///　反力データの基本形PDF書き込み
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="LL">LLがList何番目のデータか</param>
        public void ReacBasicPDF(PdfDoc mc, int LL = 0)
        {
            //　ヘッダー
            string[,] header_content3D = {
                { "節点", "x方向の", "y方向の", "z方向の", "x軸回りの", "y軸回りの","z軸周りの" },
                { "No", "支点反力", "支点反力", "支点反力", "回転反力", "回転反力","回転反力" },
                { "",  "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)" },
            };
            string[,] header_content2D = {
                { "節点", "x方向の", "y方向の", "回転", "節点", "x方向の","y方向の","回転", },
                { "No", "支点反力", "支点反力", "拘束力", "","支点反力", "支点反力","拘束力" },
                { "",  "(kN)", "(kN)", "(kN・m)", "","(kN)",  "(kN)", "(kN・m)" },
            };

            // ヘッダーのx方向の余白
            int[,] header_Xspacing3D = {
                { 10, 70, 140, 210, 280, 350, 420 },
                { 10, 70, 140, 210, 280, 350, 420 },
                { 10, 70, 140, 210, 280, 350, 420 },
            };
            int[,] header_Xspacing2D = {
                { 10, 60, 120, 180, 250, 300, 360,420 },
                { 10, 60, 120, 180, 250, 300, 360,420 },
                { 10, 60, 120, 180, 250, 300, 360,420 },
            };

            // ボディーのx方向の余白　-1
            int[,] body_Xspacing3D = {
                { 17, 85, 155, 225, 295, 365,435 }
            };
            int[,] body_Xspacing2D = {
                { 17, 75, 135, 195, 257,315,375,435 }
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

    */
}

