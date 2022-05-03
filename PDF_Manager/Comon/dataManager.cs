using Newtonsoft.Json.Linq;
using PDF_Manager.Printing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PDF_Manager.Comon
{
    internal static class dataManager
    {

        // データの精査と変換
        // (data,四捨五入する時の桁数,指数形式の表示など)
        public static string toString(JToken data)
        {
            string result = "";

            if (data == null)
                return result;

            if (data.Type == JTokenType.Null)
                return result;

            try
            {
                result = data.ToString();
            }
            catch
            {
                result = "";
            }

            return result;
        }

        public static string TypeChange(JToken data, int round = 0, string style = "none")
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
                double newDataDouble = dataManager.parseDouble(data);
                if (double.IsNaN(newDataDouble))
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

        /// <summary>
        /// double型に変換する
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public double parseDouble(JToken data)
        {
            double result = double.NaN;

            if (data == null) return result;
            if (data.Type == JTokenType.Null) return result;

            if (!double.TryParse(data.ToString(), out result))
            {
                return double.NaN;
            }
            return result;

        }
        static public double parseDouble(object data)
        {
            JToken a = JToken.FromObject(data);
            return dataManager.parseDouble(a);

        }
        static public double parseDouble(Dictionary<string,object> dict, string key)
        {
            if (dict.ContainsKey("key"))
            {
                object data = dict[key];
                return dataManager.parseDouble(data);
            }
            return double.NaN;
        }
        /// <summary>
        /// int型に変換する
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public int parseInt(JToken data)
        {
            int result = 0;

            if (data == null) return result;
            if (data.Type == JTokenType.Null) return result;

            result = int.Parse(data.ToString());

            return result;
        }

        static public int parseInt(object data)
        {
            JToken a = JToken.FromObject(data);
            return dataManager.parseInt(a);

        }
    }
}
