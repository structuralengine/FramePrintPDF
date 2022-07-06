using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class DiagramFrame
    {

        // 軸線スケール
        private double _scaleX;
        public double scaleX { get { return this._scaleX; } }
        private double _scaleY;
        public double scaleY { get { return this._scaleY; } }

        // 位置補正
        private double _posX;
        public double posX { get { return this._posX; } }
        private double _posY;
        public double posY { get { return this._posY; } }

        // 文字サイズ
        private double fontSize;

        // 描画情報
        private Layout _mode;
        public Layout mode { get { return this._mode;  } }


        // 軸線を作成するのに必要な情報

        // 節点情報
        public InputNode Node;
        // 要素情報
        public InputMember Member;
        // 材料情報
        public InputElement Element;


        public DiagramFrame(Dictionary<string, object> target)
        {

            // 軸線スケール
            this._scaleX = dataManager.parseDouble(target, "scaleX");
            this._scaleY = dataManager.parseDouble(target, "scaleY");

            // 位置補正
            this._posX = dataManager.parseDouble(target, "posX");
            this._posY = dataManager.parseDouble(target, "posY");

            // 文字サイズ
            this.fontSize = dataManager.parseDouble(target, "fontSize");

            // 描画情報
            string mode = target.ContainsKey("layout") ? target["layout"].ToString() : "Default";
            switch (mode)
            {
                case "SpritHorizontal":
                    this._mode = Layout.SplitHorizontal;
                    break;
                case "SpritVertical":
                    this._mode = Layout.SplitVertical;
                    break;
                default:
                    this._mode = Layout.Default;
                    break;
            }

        }


        // 図を描くためのモジュール
        public diagramManager canvas;
        // 骨組の中心座標
        private XPoint _CenterPos;    
        public XPoint CenterPos { get { return this._CenterPos;  } }


        /// <summary>
        /// 骨組図の作成
        /// </summary>
        /// <param name="_mc">キャンパス</param>
        /// <param name="class_set">入力データ</param>
        public void printInit(PdfDocument mc, PrintData data)
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
            this._CenterPos = new XPoint((LeftTop.X + RightBottom.X) / 2, (LeftTop.Y + RightBottom.Y) / 2);

            // スケールを決める
            if (double.IsNaN(this.scaleX) && double.IsNaN(this.scaleY))
            {
                this.setScaleX(LeftTop, RightBottom);
                this.setScaleY(LeftTop, RightBottom);
                if (this.scaleX >= this.scaleY)
                {
                    this._scaleX = this.scaleY;
                }
                else
                {
                    this._scaleY = this.scaleX;
                }
            }
            else if (double.IsNaN(this.scaleX))
            {
                this.setScaleX(LeftTop, RightBottom);
            }
            else if (double.IsNaN(this.scaleY))
            {
                this.setScaleY(LeftTop, RightBottom);
            }

            // 要素を取得できる状態にする
            foreach (var m1 in this.Member.members.Values)
            {
                //要素の節点i,jの情報を取得
                Vector3 pi = this.Node.GetNodePos(m1.ni);   // 描画中の要素のi端座標情報
                Vector3 pj = this.Node.GetNodePos(m1.nj);   // 描画中の要素のj端座標情報

                // 部材長さ
                m1.L = Math.Sqrt(Math.Pow(pj.x - pi.x, 2) + Math.Pow(pj.y - pi.y, 2));

                //節点情報の座標を取得
                var xL = (pj.x - pi.x) / m1.L;
                var yL = (pj.y - pi.y) / m1.L;

                // 座標変換マトリックス
                m1.t = new double[2, 2] { { xL, yL }, { -yL, xL } };

                // 角度
                m1.radian = Math.Atan2(yL, xL);
            }

        }

        /// <summary>
        /// 横の縮尺をセットする
        /// </summary>
        /// <param name="LeftTop"></param>
        /// <param name="RightBottom"></param>
        private void setScaleX(XPoint LeftTop, XPoint RightBottom)
        {
            var frameWidth = Math.Abs(LeftTop.X - RightBottom.X);
            var paperWidth = this.canvas.areaSize.Width;
            this._scaleX = paperWidth / frameWidth;
        }

        /// <summary>
        /// 縦の縮尺をセットする
        /// </summary>
        /// <param name="LeftTop"></param>
        /// <param name="RightBottom"></param>
        private void setScaleY(XPoint LeftTop, XPoint RightBottom)
        {
            var frameHeight = Math.Abs(LeftTop.Y - RightBottom.Y);
            var paperHeight = this.canvas.areaSize.Height;
            paperHeight -= printManager.FontHeight * 2; // タイトル印字分高さを減らす
            paperHeight -= printManager.LineSpacing2;
            this._scaleY = paperHeight / frameHeight;
        }

        /// <summary>
        /// 骨組みを印字する
        /// </summary>
        /// <param name="currentArea">骨組を描く位置</param>
        /// <param name="isNode">節点を描くか？</param>
        public void printFrame(int currentArea = 1, bool isNode = true)
        {
            canvas.currentArea = currentArea;

            // 骨組の描写
            canvas.mc.xpen = new XPen(XColors.Black, 1);

            // 要素を取得できる状態にする
            foreach (var mm in this.Member.members.Values)
            {
                var p1 = this.Node.GetNodePos(mm.ni);
                var p2 = this.Node.GetNodePos(mm.nj);

                //n スケール調整
                var x1 = (p1.x - this._CenterPos.X) * this.scaleX;
                var y1 = -(p1.y - this._CenterPos.Y) * this.scaleY;
                var x2 = (p2.x - this._CenterPos.X) * this.scaleX;
                var y2 = -(p2.y - this._CenterPos.Y) * this.scaleY;

                canvas.printLine(x1, y1, x2, y2);

            }

            if(isNode == true)
            {
                // 節点データ
                foreach (var pp in this.Node.Nodes.Values)
                {
                    var x = (pp.x - this._CenterPos.X) * this.scaleX;
                    var y = -(pp.y - this._CenterPos.Y) * this.scaleY;

                    canvas.printNode(x, y);
                }

            }

        }
    }
}
