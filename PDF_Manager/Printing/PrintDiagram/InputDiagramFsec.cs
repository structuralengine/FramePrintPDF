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
        private double scale;

        // 位置補正
        private double posX;
        private double posY;

        // 文字サイズ
        private double fontSize;

        // 描画情報
        private Layout mode;


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
            this.Node = (InputNode)data.printDatas[InputNode.KEY];

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
            var LeftTop = new XPoint(double.MaxValue, double.MinValue);     // 節点の最も左上
            var RightBottom = new XPoint(double.MinValue, double.MaxValue); // 節点の最も右下
            foreach (var n in this.Node.Nodes.Values)
            {
                if (n.x < LeftTop.X)
                    LeftTop.X = n.x;
                if (n.x > RightBottom.X)
                    RightBottom.X = n.x;
                if (n.y < RightBottom.Y)
                    RightBottom.Y = n.y;
                if (n.y > LeftTop.Y)
                    LeftTop.Y = n.y;
            }
            this.CenterPos = new XPoint((LeftTop.X + RightBottom.X) / 2, (LeftTop.Y + RightBottom.Y) / 2);

            // スケールを決める

                    if (double.IsNaN(this.scaleX))
                    {

                        var frameWidth = Math.Abs(LeftTop.X - RightBottom.X);
                        var paperWidth = this.canvas.areaSize.Width;
                        this.scaleX = paperWidth / frameWidth;
                    }
                    if (double.IsNaN(this.scaleY))
                    {
                        var frameHeight = Math.Abs(LeftTop.Y - RightBottom.Y);
                        var paperHeight = this.canvas.areaSize.Height;
                        paperHeight -= printManager.FontHeight; // タイトル印字分高さを減らす
                        paperHeight -= printManager.LineSpacing2;
                        this.scaleY = paperHeight / frameHeight;
                    }

            if (scaleX >= scaleY)
            {
                this.scale = scaleY;
            }
            else
            {
                this.scale = scaleX;
            }
        }


        private void printFrame()
        {
            int j = 0;
            switch (this.mode)
            {
                case Layout.SplitHorizontal:
                case Layout.SplitVertical:
                    j = 1;
                    break;
            }

            for (var i = 0; i <= j; ++i)
            {
                canvas.currentArea = i;


                // 要素を取得できる状態にする
                foreach (var m in this.Member.members)
                {
                    var mm = m.Value;
                    var p1 = this.Node.GetNodePos(mm.ni);
                    var p2 = this.Node.GetNodePos(mm.nj);

                    //n スケール調整
                    var x1 = (p1.x - this.CenterPos.X) * this.scale;
                    var y1 = -(p1.y - this.CenterPos.Y) * this.scale;
                    var x2 = (p2.x - this.CenterPos.X) * this.scale;
                    var y2 = -(p2.y - this.CenterPos.Y) * this.scale;

                    canvas.printLine(x1, y1, x2, y2);
                }

                // 節点データ
                foreach (var n in this.Node.Nodes)
                {
                    var id = n.Key;
                    var p = n.Value;

                    var x = (p.x - this.CenterPos.X) * this.scale;
                    var y = -(p.y - this.CenterPos.Y) * this.scale;

                    canvas.printNode(x, y);
                }
            }



        }

    }
}
