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
        public const string KEY = "diagramFsec";

        // レイアウト
        enum Layout
        {
            Default,
            SpritVertical,
            SpritHorizontal
        }
        // 軸線スケール
        private double scaleX;
        private double scaleY;

        // 位置補正
        private double posX;
        private double posY;

        // 文字サイズ
        private double fontSize;

        // 軸線を作成するのに必要な情報

        // 節点情報
        private InputNode Node = null;
        // 要素情報
        private InputMember Member = null;
        // 材料情報
        private InputElement Element = null;
        //

        public InputDiagramFsec(Dictionary<string, object> value) 
        {
            if (!value.ContainsKey(KEY))
                return;

            //荷重図の設定データを取得する
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // 軸線スケール
            this.scaleX = dataManager.parseDouble(target, "scaleX");
            this.scaleY = dataManager.parseDouble(target, "scaleY");

            // 位置補正
            this.posX = dataManager.parseDouble(target, "posX");
            this.posY = dataManager.parseDouble(target, "posY");

            // 文字サイズ
            this.fontSize = dataManager.parseDouble(target, "fontSize");


        }

        
        /// <summary>
        /// 荷重図の作成
        /// </summary>
        /// <param name="_mc">キャンパス</param>
        /// <param name="class_set">入力データ</param>
        public void printPDF(PdfDocument mc, PrintData data)
        {

            // 部材長を取得できる状態にする
            this.Node = (InputNode)data.printDatas[InputNode.KEY];

            // 要素を取得できる状態にする
            this.Member = (InputMember)data.printDatas[InputMember.KEY];

            // 材料名称を取得できる状態にする
            this.Element = (InputElement)data.printDatas[InputElement.KEY];

            // 
            var comon = new diagramManager(mc);

            // 骨組みを印字する
            this.printFrame(comon);
        }


        private void printFrame(diagramManager comon)
        {
            foreach(var n in this.Node.Nodes)
            {
                var id = n.Key;
                var p = n.Value;
                var x = p.x * this.scaleX;
                var y = -p.y * this.scaleY;

                comon.printNode(x, y);
            }
        }

    }
}
