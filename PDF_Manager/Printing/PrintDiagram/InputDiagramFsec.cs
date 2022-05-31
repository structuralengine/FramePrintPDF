using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class InputDiagramFsec
    {
        public const string KEY = "diagramLoad";

        public InputDiagramFsec(Dictionary<string, object> value) 
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


            #region



            #endregion


        }

        
        /// <summary>
        /// 荷重図の作成
        /// </summary>
        /// <param name="_mc">キャンパス</param>
        /// <param name="class_set">入力データ</param>
        public void printPDF(PdfDocument mc, PrintData data)
        {

        }


    }
}
