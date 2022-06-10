using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class InputDiagramFsec
    {
        public const string KEY = "diagramFsec";

        // 軸線スケール
        private double scaleX;
        private double scaleY;

        // 位置補正
        private double posX;
        private double posY;

        // 文字サイズ
        private double fontSize;

        // 描画情報
        private Layout mode;


        // 軸線を作成するのに必要な情報

        // 節点情報
        private Dictionary<string, Vector3> Node = null;
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

            // 描画情報
            string mode = target.ContainsKey("layout") ? target["layout"].ToString() : "Default";
            switch (mode) {
                case "SpritHorizontal":
                    this.mode = Layout.SplitHorizontal;
                    break;
                case "SpritVertical":
                    this.mode = Layout.SplitVertical;
                    break;
                default:
                    this.mode = Layout.Default;
                    break;
            }

        }


        // 描画するために必要なパラメータ
        private diagramManager canvas;   // 図を描くためのモジュール
        private XPoint CenterPos;       // 骨組の中心座標


        /// <summary>
        /// 荷重図の作成
        /// </summary>
        /// <param name="_mc">キャンパス</param>
        /// <param name="class_set">入力データ</param>
        public void printPDF(PdfDocument mc, PrintData data)
        {

            // 部材長を取得できる状態にする
            this.Node = ((InputNode)data.printDatas[InputNode.KEY]).Nodes;

            // 要素を取得できる状態にする
            this.Member = (InputMember)data.printDatas[InputMember.KEY];

            // 材料名称を取得できる状態にする
            this.Element = (InputElement)data.printDatas[InputElement.KEY];

            // 描画領域を
            this.canvas = new diagramManager(mc, this.mode);

            // 印刷の前処理
            this.printInit();

            // 骨組みを印字する
            this.printFrame();
        }


        /// <summary>
        /// 印刷の前処理
        /// </summary>
        private void printInit()
        {
            // 格点の中心座標を求める
            var LeftTop = new XPoint(double.MaxValue, double.MaxValue);     // 節点の最も左上
            var RightBottom = new XPoint(double.MinValue, double.MinValue); // 節点の最も右下
            foreach (var n in this.Node.Values)
            {
                if (n.x < LeftTop.X)
                    LeftTop.X = n.x;
                if (n.x > RightBottom.X)
                    RightBottom.X = n.x;
                if (n.y < LeftTop.Y)
                    LeftTop.Y = n.y;
                if (n.y > RightBottom.Y)
                    RightBottom.Y = n.y;
            }
            this.CenterPos = new XPoint((LeftTop.X + RightBottom.X) / 2, (LeftTop.Y + RightBottom.Y) / 2);

            // スケールを決める
            if(double.IsNaN(this.scaleX))
            {
                var frameWidth = Math.Abs(LeftTop.X - RightBottom.X);
                var paperWidth = this.canvas.areaSize.Width;
                this.scaleX = paperWidth / frameWidth;
            }
            if (double.IsNaN(this.scaleY))
            {
                var frameHeight = Math.Abs(LeftTop.Y - RightBottom.Y);
                var paperHeight = this.canvas.areaSize.Height;
                this.scaleY = paperHeight / frameHeight;
            }


        }

        private void printFrame()
        {
            // 節点データ
            foreach(var n in this.Node)
            {
                var id = n.Key;
                var p = n.Value;
                var x = (p.x - this.CenterPos.X) * this.scaleX;
                var y = -(p.y - this.CenterPos.Y) * this.scaleY;

                canvas.printNode(x, y);
            }
        }

    }
}
