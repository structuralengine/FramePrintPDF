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
        private PdfDoc mc;
        private Dictionary<string, object> value = new Dictionary<string, object>();
        private int bottomCell = 73;

        public (List<string> ,List<List<string[]>>) element(Dictionary<string, object> value_)
        {
            value = value_;
            // elementデータを取得する．
            var target = JObject.FromObject(value["element"]).ToObject<Dictionary<string, object>>();

            // 集まったデータはすべてここに格納する
            List<List<string[]>> elememt_data = new List<List<string[]>>();
            List<string> elememt_title = new List<string>();

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
                    double Xp= double.Parse(item["Xp"].ToString());
                    double Iy = double.Parse(item["Iy"].ToString());
                    double Iz = double.Parse(item["Iz"].ToString());
                    double J = double.Parse(item["J"].ToString());
                    if (item["n"].Type == JTokenType.Null) item["n"] = "";
                    var n = item["n"].ToString();
                    targetValue = item.ToObject<Dictionary<string, object>>();

                    string[] line1 = new String[2];
                    string[] line2 = new String[8];

                    line1[0] = targetValue.ElementAt(j).Value.ToString();
                    line1[1] = n;
                    line2[0] = "";
                    line2[1] = Math.Round(A, 4).ToString();
                    line2[2] = Math.Round(E, 2).ToString("E");
                    line2[3] = Math.Round(G, 2).ToString("E");
                    line2[4] = Math.Round(Xp,2).ToString("E");
                    line2[5] = Math.Round(Iy,6).ToString();
                    line2[6] = Math.Round(Iz,6).ToString();
                    line2[7] = Math.Round(J, 6).ToString();
                    table.Add(line1);
                    table.Add(line2);
                }
                elememt_data.Add(table);
            }
            return (
                title: elememt_title,
                data: elememt_data
            );
        }

        public void elementPDF(PdfDoc mc, List<string> elementTitle,List<List<string[]>> elementData)
        {

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
