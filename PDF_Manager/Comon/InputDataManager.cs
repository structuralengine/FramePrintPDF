using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PDF_Manager.Comon
{
    public class Vector3
    {
        double x;
        double y;
        double z;

        public Vector3(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }
    }

    internal class InputDataManager
    {
        // データの精査と変換
        // (data,四捨五入する時の桁数,指数形式の表示など)
        static public string TypeChange(JToken data, int round = 0, string style = "none")
        {
            string newDataString = "";

            if (data == null)
            {
                newDataString = "";
            }
            // すぐにstringにする
            else if (data.Type == JTokenType.String)
            {
                if (data.Type == JTokenType.Null) data = "";
                newDataString = data.ToString();

            }
            // 四捨五入等の処理を行う
            else
            {
                double newDataDouble = InputDataManager.getNumeric(data);
                if (Double.IsNaN(newDataDouble))
                {
                    newDataString = "";
                }
                else if (style == "none")
                {
                    var digit = "F" + round.ToString();
                    newDataString = Double.IsNaN(Math.Round(newDataDouble, round, MidpointRounding.AwayFromZero)) ? "" : newDataDouble.ToString(digit);
                    if (StringInfo.ParseCombiningCharacters(newDataString).Length > round + 5)
                    {
                        newDataString = newDataDouble.ToString("E2", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                }
                else if (style == "E")
                {
                    newDataString = Double.IsNaN(Math.Round(newDataDouble, round, MidpointRounding.AwayFromZero)) ? "" : newDataDouble.ToString("E2", CultureInfo.CreateSpecificCulture("en-US"));
                }
            }
            return newDataString;
        }


        static public double getNumeric(JToken data)
        {
            double result = double.NaN;

            if (data == null) return result;
            if (data.Type == JTokenType.Null) return result;

            result = double.Parse(data.ToString());

            return result;
        }
    }
}
