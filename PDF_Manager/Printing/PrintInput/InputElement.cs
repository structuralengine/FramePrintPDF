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
    internal class InputElement
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();

        public (List<string>, List<List<string[]>>) Element(Dictionary<string, object> value_)
        {
            value = value_;
            // elementデータを取得する．
            var target = JObject.FromObject(value["element"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはすべてここに格納する
            List<string> elememt_title = new List<string>();
            List<List<string[]>> elememt_data = new List<List<string[]>>();

            for (int i = 0; i < target.Count; i++)
            {
                var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                // タイトルを入れる．
                elememt_title.Add("タイプ" + Elem.ElementAt(i).Key);

                List<string[]> table = new List<string[]>();

                for (int j = 0; j < Elem.Count; j++)
                {
                    var targetValue = new Dictionary<string, object>();
                    var item = JObject.FromObject(Elem.ElementAt(j).Value);
                    double A = double.Parse(item["A"].ToString());
                    double E = double.Parse(item["E"].ToString());
                    double G = double.Parse(item["G"].ToString());
                    double Xp = double.Parse(item["Xp"].ToString());
                    double Iy = double.Parse(item["Iy"].ToString());
                    double Iz = double.Parse(item["Iz"].ToString());
                    double J = double.Parse(item["J"].ToString());
                    if (item["n"].Type == JTokenType.Null) item["n"] = "";
                    var n = item["n"].ToString();
                    targetValue = item.ToObject<Dictionary<string, object>>();

                    string[] line = new String[10];

                    line[0] = Elem.ElementAt(j).Key.ToString();
                    line[1] = n;
                    line[2] = "";
                    line[3] = Math.Round(A, 4).ToString();
                    line[4] = Math.Round(E, 2).ToString("E");
                    line[5] = Math.Round(G, 2).ToString("E");
                    line[6] = Math.Round(Xp, 2).ToString("E");
                    line[7] = Math.Round(Iy, 6).ToString();
                    line[8] = Math.Round(Iz, 6).ToString();
                    line[9] = Math.Round(J, 6).ToString();
                    table.Add(line);
                }
                elememt_data.Add(table);
            }
            return (
                elememt_title,
                elememt_data
            );
        }

        public void ElementPDF(PdfDoc mc, List<string> elementTitle, List<List<string[]>> elementData)
        {
            double bottomCell = mc.bottomCell;
            int single_Yrow = mc.single_Yrow;
            int currentXposition_values = 60;

            int count = 2;
            for (int i = 0; i < elementTitle.Count; i++)
            {
               count += elementData[i].Count*2+ 6; 
            }

            bool judge = mc.DataCountKeep(count);
            if (judge == true)
            {
                mc.NewPage();
            }

            //mc.gfx.DrawString("材料データ", mc.font_got, XBrushes.Black, mc.CurrentPosHeader);
            //mc.CurrentPosHeader.Y += single_Yrow*2;
            //mc.CurrentPosBody.Y += single_Yrow*7;
            //count = 2;

            //for (int i = 0; i < elementTitle.Count; i++)
            //{
            //    mc.gfx.DrawString(elementTitle[i], mc.font_got, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.Y += single_Yrow*2;
            //    mc.gfx.DrawString("No", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 1);
            //    mc.gfx.DrawString("A", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 2);
            //    mc.gfx.DrawString("E", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 3);
            //    mc.gfx.DrawString("G", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 4);
            //    mc.gfx.DrawString("ESP", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 5);
            //    mc.gfx.DrawString("断面二次モーメント", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 7);
            //    mc.gfx.DrawString("ねじり合成", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x;
            //    mc.CurrentPosHeader.Y += single_Yrow;
            //    mc.gfx.DrawString("", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 1);
            //    mc.gfx.DrawString("(m2)", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 2);
            //    mc.gfx.DrawString("(kN/m2)", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 3);
            //    mc.gfx.DrawString("(kN/m2)", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 4);
            //    mc.gfx.DrawString("", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 5);
            //    mc.gfx.DrawString("y軸周り", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 6);
            //    mc.gfx.DrawString("z軸周り", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 7);
            //    mc.gfx.DrawString("", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //    mc.CurrentPosHeader.X = mc.x;
            //    count+=5;

            //    for (int j = 0; j < elementData[i].Count; j++)
            //    {
            //        if (mc.CurrentPosBody.Y> bottomCell* mc.single_Yrow || (elementData[i].Count+3) * mc.single_Yrow > (bottomCell * mc.single_Yrow - mc.CurrentPosBody.Y))
            //        {
            //            count = 0;
            //            mc.NewPage();
            //            mc.CurrentPosHeader.Y += single_Yrow*2;
            //            mc.CurrentPosBody.Y += single_Yrow*7;
            //            mc.gfx.DrawString("No", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 1);
            //            mc.gfx.DrawString("A", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 2);
            //            mc.gfx.DrawString("E", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 3);
            //            mc.gfx.DrawString("G", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 4);
            //            mc.gfx.DrawString("ESP", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 5);
            //            mc.gfx.DrawString("断面二次モーメント", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 7);
            //            mc.gfx.DrawString("ねじり合成", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x;
            //            mc.CurrentPosHeader.Y += single_Yrow;
            //            mc.gfx.DrawString("", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 1);
            //            mc.gfx.DrawString("(m2)", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 2);
            //            mc.gfx.DrawString("(kN/m2)", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 3);
            //            mc.gfx.DrawString("(kN/m2)", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 4);
            //            mc.gfx.DrawString("", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 5);
            //            mc.gfx.DrawString("y軸周り", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 6);
            //            mc.gfx.DrawString("z軸周り", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x + (currentXposition_values * 7);
            //            mc.gfx.DrawString("", mc.font_mic, XBrushes.Black, mc.CurrentPosHeader);
            //            mc.CurrentPosHeader.X = mc.x;
            //            count+=5;
            //        }

            //        mc.gfx.DrawString(elementData[i][j][0], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x + (currentXposition_values * 1);
            //        mc.gfx.DrawString(elementData[i][j][1], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x;
            //        mc.CurrentPosBody.Y += single_Yrow;

            //        mc.gfx.DrawString(elementData[i][j][2], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x + (currentXposition_values * 1);
            //        mc.gfx.DrawString(elementData[i][j][3], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x + (currentXposition_values * 2);
            //        mc.gfx.DrawString(elementData[i][j][4], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x + (currentXposition_values * 3);
            //        mc.gfx.DrawString(elementData[i][j][5], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x + (currentXposition_values * 4);
            //        mc.gfx.DrawString(elementData[i][j][6], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x + (currentXposition_values * 5);
            //        mc.gfx.DrawString(elementData[i][j][7], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x + (currentXposition_values * 6);
            //        mc.gfx.DrawString(elementData[i][j][8], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
            //        mc.CurrentPosBody.X = mc.x + (currentXposition_values * 7);
            //        mc.gfx.DrawString(elementData[i][j][9], mc.font_mic, XBrushes.Black, mc.CurrentPosBody);
                    
            //        mc.CurrentPosBody.X = mc.x;
            //        mc.CurrentPosBody.Y += single_Yrow;
            //    }
            //}
            //mc.DataCountKeep(count);
        }



        //public string getElementName(string e)
        //{
        //    if (e == "" || e == null)
        //    {
        //        return "";
        //    }

        //const key = Object.keys(this.element)[0];
        //const row = this.element[key];

        //const target = row.find((columns) =>
        //{
        //    return columns.id.toString() === e.toString();
        //});
        //let name: string = "";
        //if (target !== undefined)
        //{
        //    name = target.n !== undefined ? target.n : "";
        //}

        //    return name;
        //}
    }

}
