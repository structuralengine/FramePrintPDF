using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class DiagramResult
    {
        public const string KEY = "diagramResult";

        // 軸線を作成するのに必要な情報
        private DiagramFrame Frame = null;

        // 荷重情報
        private InputLoadName LoadName = null;
        // 断面力情報
        private ResultFsec Fsec = null;

        public DiagramResult(Dictionary<string, object> value) 
        {
            if (!value.ContainsKey(KEY))
                return;

            //荷重図の設定データを取得する
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // 骨組の描画クラスを生成する
            this.Frame = new DiagramFrame(target);

        }


        /// <summary>
        /// 荷重図の作成
        /// </summary>
        /// <param name="_mc">キャンパス</param>
        /// <param name="class_set">入力データ</param>
        public void printPDF(PdfDocument mc, PrintData data)
        {
            this.Frame.printInit(mc, data);

            // 荷重名称を取得
            this.LoadName = (InputLoadName)data.printDatas[InputLoadName.KEY];

            // 断面力を取得
            this.Fsec = (ResultFsec)data.printDatas[ResultFsec.KEY];

            // 断面力図を描く


            foreach (var fsec in this.Fsec.fsecs)
            {
                // LoadName から同じキーの情報を
                int index = Convert.ToInt32(fsec.Key);

                LoadName ln = null;
                if (!this.LoadName.loadnames.TryGetValue(index, out ln))
                    continue;

                if (ln != null)
                    Text.PrtText(mc, string.Format("CASE : {0}  {1} :{2}", fsec.Key, ln.name, ln.symbol));

                // 
                this.printTitle(mc, 0);

                string caseNo = fsec.Key;
                var fs = fsec.Value;

                // 骨組みを印字する
                this.printFrame((List<Fsec>)fsec.Value);

                // 改ページ
                mc.NewPage();
            }

        }

        /// <summary>
        /// 骨組みを印字する
        /// </summary>
        /// <param name="fsec"></param>
        private void printFrame(List<Fsec> fsec)
        {
            // 描く順番
            var Index = new int[] { 2, 1, 0 }; // 2: mz, 1: fy, 0:fx の順に描く

            // レイアウトによって１ページに描画する数
            int j = 0;  
            switch (this.Frame.mode)
            {
                case Layout.SplitHorizontal:
                case Layout.SplitVertical:
                    j = 1;
                    break;
            }

            // 1ページ内に描く
            for (var i = 0; i <= j; ++i) //<-
            {
                // 骨組の描写
                this.Frame.printFrame(i, true);

                // 断面力の描写用のペン設定
                this.Frame.canvas.mc.xpen = new XPen(XColors.Blue, 0.2);

                // 断面力の縮尺を計算する
                var max_value = 0.0;
                foreach (Fsec f in fsec)
                    max_value = Math.Max(Math.Abs(f.getValue2D(Index[i])), max_value);

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
                        m = this.Frame.Member.GetMember(f.m);//任意の要素を取得

                        if (m == null)
                            continue;   // 有効な部材じゃない

                        // 断面力の線を書かないフラグ
                        xx = double.NaN;
                        yy = double.NaN;

                        //要素の節点i,jの情報を取得
                        pi = this.Frame.Node.GetNodePos(m.ni);   // 描画中の要素のi端座標情報
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
                        pos = this.Frame.Node.GetNodePos(f.n);
                    }

                    //荷重の大きさを取得
                    var Value = f.getValue2D(Index[i]); 

                    //荷重の大きさ(線の長さ)の時の座標計算
                    var fxg = m.t[1, 0] * Value;
                    var fyg = m.t[1, 1] * Value;

                    //n スケール調整
                    var x1 = (pos.x - this.Frame.CenterPos.X) * this.Frame.scaleX;
                    var y1 = -(pos.y - this.Frame.CenterPos.Y) * this.Frame.scaleY;
                    var x2 = x1 + fxg * fsecScale;
                    var y2 = y1 + fyg * fsecScale;

                    // 2点を結ぶ直線を引く
                    if (!double.IsNaN(xx))
                        this.Frame.canvas.printLine(xx, yy, x2, y2);

                    // 部材から垂線を引く
                    if (Value != 0)
                    {
                        this.Frame.canvas.printLine(x1, y1, x2, y2);
                        // 文字
                        // 文字の角度を決定する
                        var radian = (-1  *m.radian) - Math.PI / 2;

                        this.Frame.canvas.printText(x2, y2, string.Format("{0}", Value), radian, this.Frame.canvas.mc.font_got);
                    }

                    xx = x2;
                    yy = y2;

                }
            }



        }


        /// <summary>
        /// 曲げモーメント図、せん断力図などのタイトルを印刷する
        /// </summary>
        private void printTitle(PdfDocument mc, int index)
        {
            //
            double gyap = 350;

            // タイトルを印字する位置の設定
            mc.currentPos.Y = this.Frame.canvas.Center(0).Y - gyap;
            mc.currentPos.X = printManager.titlePos.X + 40;

            switch (this.Frame.mode)
            {
                case Layout.Default:
                    if (index == 0)
                        Text.PrtText(mc, "曲げモーメント図");
                    if (index == 1)
                        Text.PrtText(mc, "せん断力図");
                    if (index == 2)
                        Text.PrtText(mc, "軸方向力図");
                    break;

                case Layout.SplitHorizontal:

                    //タイトル曲げモーメントを入力
                    if (index == 0)
                    {
                        Text.PrtText(mc, "曲げモーメント図");

                        // タイトルを印字する位置の設定
                        mc.currentPos.Y = this.Frame.canvas.Center(1).Y - gyap;

                        //タイトルせん断力図を入力
                        Text.PrtText(mc, "せん断力図");
                    }
                    break;

                case Layout.SplitVertical:

                    //タイトル曲げモーメントを入力
                    Text.PrtText(mc, string.Format("{0}", "曲げモーメント図"));

                    // タイトルを印字する位置の設定
                    mc.currentPos.X = printManager.titlePos.X;
                    mc.currentPos.X += mc.currentPage.Width / 2;

                    //タイトルせん断力図を入力
                    Text.PrtText(mc, string.Format("{0}", "せん断力図"));

                    break;

            }

        }
    }
}
