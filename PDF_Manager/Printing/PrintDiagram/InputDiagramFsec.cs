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
        // 荷重情報
        private InputLoadName LoadName = null;
        // 断面力情報
        private ResultFsec Fsec = null;

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
            //
            this.LoadName = (InputLoadName)data.printDatas[InputLoadName.KEY];

            // 断面力を取得
            this.Fsec = (ResultFsec)data.printDatas[ResultFsec.KEY];

            // 描画領域を
            this.canvas = new diagramManager(mc, this.mode);


            // 印刷の前処理
            this.printInit();


            // 断面力図を描く
            foreach (var fsec in this.Fsec.fsecs)
            {
                // LoadName から同じキーの情報を

                int index =Convert.ToInt32(fsec.Key);
                LoadName ln = null;
                if (!this.LoadName.loadnames.TryGetValue(index, out ln))
                    continue;

                if (ln != null)
                {
                    Text.PrtText(mc, string.Format("CASE : {0}  {1} :{2}", fsec.Key, ln.name, ln.symbol));
                }
                mc.currentPos.Y += printManager.FontHeight;
                mc.currentPos.Y += printManager.LineSpacing2;

                switch (this.mode)
                {
                    case Layout.Default:
                    case Layout.SplitHorizontal:
                        //タイトル曲げモーメントを入力
                        if (ln != null)
                        {
                            Text.PrtText(mc, string.Format("{0}", "曲げモーメント図"));
                        }
                        mc.currentPos.Y = printManager.titlePos.Y;
                        mc.currentPos.Y += mc.currentPage.Height;
                        mc.currentPos.Y -= printManager.padding.Top;
                        mc.currentPos.Y -= printManager.padding.Bottom;
                        mc.currentPos.Y -= printManager.FontHeight * 2;
                        mc.currentPos.Y -= printManager.LineSpacing2;
                        mc.currentPos.Y /= 2;
                        mc.currentPos.Y += printManager.padding.Top;
                        mc.currentPos.Y += printManager.FontHeight * 3;
                        mc.currentPos.Y += printManager.LineSpacing2 * 2;

                        //タイトルせん断力図を入力
                        if (ln != null)
                        {
                            Text.PrtText(mc, string.Format("{0}", "せん断力図"));
                        }

                        break;
                    case Layout.SplitVertical:
                        //タイトル曲げモーメントを入力
                        if (ln != null)
                        {
                            Text.PrtText(mc, string.Format("{0}", "曲げモーメント図"));
                        }
                        mc.currentPos.X = printManager.titlePos.X;
                        mc.currentPos.X += mc.currentPage.Width / 2;

                        //タイトルせん断力図を入力
                        if (ln != null)
                        {
                            Text.PrtText(mc, string.Format("{0}", "せん断力図"));
                        }
                        mc.currentPos.Y += printManager.FontHeight;
                        mc.currentPos.Y += printManager.LineSpacing2;

                        break;
                }

                string caseNo = fsec.Key;
                var fs = fsec.Value;

                // 骨組みを印字する
                this.printFrame((List<Fsec>)fsec.Value);

                // 改ページ
                mc.NewPage();
            }

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
                        paperHeight -= printManager.FontHeight * 2; // タイトル印字分高さを減らす
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


        /// <summary>
        /// 骨組みを印字する
        /// </summary>
        /// <param name="fsec"></param>
        private void printFrame(List<Fsec> fsec)
        {
            // 描く順番
            var targetIndexList = new int[] { 2, 1, 0 }; // 2: mz, 1: fy, 0:fx の順に描く

            // レイアウトによって１ページに描画する数
            int j = 0;  
            switch (this.mode)
            {
                case Layout.SplitHorizontal:
                case Layout.SplitVertical:
                    j = 1;
                    break;
            }

            // 1ページ内に描く
            for (var i = 0; i <= j; ++i) //<-
            {
                canvas.currentArea = i;

                // 骨組の描写
                canvas.mc.xpen = new XPen(XColors.Black, 1);

                // 要素を取得できる状態にする
                foreach (var mm in this.Member.members.Values)
                {
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
                foreach (var pp in this.Node.Nodes.Values)
                {
                    var x = (pp.x - this.CenterPos.X) * this.scale;
                    var y = -(pp.y - this.CenterPos.Y) * this.scale;

                    canvas.printNode(x, y);
                }

                // 断面力の描写
                canvas.mc.xpen = new XPen(XColors.Blue, 0.2);
                int Index = targetIndexList[i];

                // 断面力の縮尺を計算する
                var max_value = 0.0;
                foreach (Fsec f in fsec)
                    max_value = Math.Max(Math.Abs(f.getValue2(Index)), max_value);

                // 断面力の最大値を 50pt とする
                var fsecScale = 50 / max_value;

                // 描画中の要素情報
                Member m = null;
                Vector3 pi = new Vector3();  // 着目点位置
                double xx = double.NaN;
                double yy = double.NaN;

                foreach (Fsec f in fsec)
                {
                    // 部材情報をセットする
                    if (f.m.Length > 0)
                    {
                        var m1 = this.Member.GetMember(f.m);//任意の要素を取得

                        if (m1 == null)
                            continue;   // 有効な部材じゃない

                        // 断面力の線を書かないフラグ
                        xx = double.NaN;
                        yy = double.NaN;

                        //要素の節点i,jの情報を取得
                        pi = this.Node.GetNodePos(m1.ni);   // 描画中の要素のi端座標情報
                        Vector3 pj = this.Node.GetNodePos(m1.nj);   // 描画中の要素のj端座標情報

                        if (double.IsNaN(m1.L))
                        {
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
                        m = m1;
                    }

                    // 断面力の位置を決定する
                    Vector3 pos = new Vector3(); 
                    if (f.n.Length <= 0)
                    {  // 部材途中の着目点位置
                       pos.x = pi.x + f.l * m.t[0, 0];
                       pos.y = pi.y + f.l * m.t[0, 1];
                    }
                    else
                    {   // 部材端点位置
                        pos = this.Node.GetNodePos(f.n);
                    }

                    //荷重の大きさを取得
                    var Value = f.getValue2(Index); 

                    //荷重の大きさ(線の長さ)の時の座標計算
                    var fxg = m.t[1, 0] * Value;
                    var fyg = m.t[1, 1] * Value;

                    //n スケール調整
                    var x1 = (pos.x - this.CenterPos.X) * this.scale;
                    var y1 = -(pos.y - this.CenterPos.Y) * this.scale;
                    var x2 = x1 + fxg * fsecScale;
                    var y2 = y1 + fyg * fsecScale;

                    // 2点を結ぶ直線を引く
                    if (!double.IsNaN(xx))
                        canvas.printLine(xx, yy, x2, y2);

                    // 部材から垂線を引く
                    if (Value != 0)
                    {
                        canvas.printLine(x1, y1, x2, y2);
                        // 文字
                        // 文字の角度を決定する
                        var radian = m.radian + Math.PI / 2;
                        if (radian > 0)
                            radian -= Math.PI;

                        canvas.printText(x2, y2, string.Format("{0}", Value), radian);
                    }

                    xx = x2;
                    yy = y2;

                }
            }



        }

    }
}
