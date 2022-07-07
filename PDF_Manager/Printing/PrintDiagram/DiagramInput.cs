using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class DiagramInput
    {
        public const string KEY = "diagramInput";

        public DiagramInput(Dictionary<string, object> value) 
        {
            if (!value.ContainsKey(KEY))
                return;

            //荷重図の設定データを取得する
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // 軸線スケール
            double scaleX = dataManager.parseDouble(target, "scaleX");
            double scaleY = dataManager.parseDouble(target, "scaleY"); 

            // 位置補正
            double posX = dataManager.parseDouble(target, "posX");
            double posY = dataManager.parseDouble(target, "posY");

            // 文字サイズ
            double fontSize = target.ContainsKey("fontSize") ? (double)target["fontSize"] : 9;

        }

        
        /// <summary>
        /// 荷重図の作成
        /// </summary>
        /// <param name="_mc">キャンパス</param>
        /// <param name="class_set">入力データ</param>
        public void printPDF(PdfDocument mc, PrintData data)
        {

        }


        /*
        /// <summary>
        /// 節点の印字
        /// </summary>
        private void printNode()
        {
            XPoint p = new XPoint(200,300);
            XSize z = new XSize(10, 10);

            Shape.Drawcircle(this.mc, p, z);
        }

        */
    }
}
