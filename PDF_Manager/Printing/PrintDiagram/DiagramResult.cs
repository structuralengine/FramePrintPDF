using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections;
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
        //private InputLoadName LoadName = null;
        //// 断面力情報
        //private ResultFsec Fsec = null;
        //// 組合せ断面力情報
        //private ResultFsecCombine combFsec = null;
        //// ピックアップ断面力情報
        //private ResultFsecPickup pickFsec = null;

        // 出力情報
        private List<string> output = new List<string>() { "mz", "fy", "fx", "disg" };

        public DiagramResult(Dictionary<string, object> value) 
        {
            if (!value.ContainsKey(KEY))
                return;

            //荷重図の設定データを取得する
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // 骨組の描画クラスを生成する
            this.Frame = new DiagramFrame(target);

            // 出力する内容を決定する
            if (target.ContainsKey("output")) {
                var obj = target["output"];
                IEnumerable enumerable = obj as IEnumerable;
                if (enumerable != null)
                {
                    this.output = new List<string>();
                    foreach (var element in enumerable)
                    {
                        this.output.Add(element.ToString());
                    }
                }
            }
            if (this.output.Contains("disg")) {
                if (!value.ContainsKey(ResultDisg.KEY)) {
                    this.output.Remove("disg");
                } 
            }

        }


        /// <summary>
        /// 荷重図の作成
        /// </summary>
        /// <param name="_mc">キャンパス</param>
        /// <param name="class_set">入力データ</param>
        public void printPDF(PdfDocument mc, PrintData data)
        {
            this.Frame.printInit(mc, data);

            // 断面力図を描く
            var Fsec = (ResultFsec)data.printDatas[ResultFsec.KEY];             // 断面力を取得
            var LoadName = (InputLoadName)data.printDatas[InputLoadName.KEY];   // 荷重名称を取得
            this.printFsec(mc, Fsec, LoadName);

            // 組合せ断面力図を描く
            var combFsec = (ResultFsecCombine)data.printDatas[ResultFsecCombine.KEY];   // 組合せ断面力を取得
            var combName = (InputCombine)data.printDatas[InputCombine.KEY];             // 組合せ名称を取得
            this.printCombFsec(mc, combFsec, combName);

            // ピックアップ断面力図を描く
            var pickFsec = (ResultFsecPickup)data.printDatas[ResultFsecPickup.KEY];     // ピックアップ断面力を取得
            var pickbName = (InputPickup)data.printDatas[InputPickup.KEY];              // ピックアップ名称を取得
            // this.printPicFsec(mc, pickFsec, pickbName);
        }

        /// <summary>
        /// 組合せ断面力図を描く
        /// </summary>
        /// <param name="mc">キャンパス</param>
        /// <param name="Fsec">断面力クラス</param>
        private void printCombFsec(PdfDocument mc, ResultFsecCombine combFsec, InputCombine combName)
        {
            // 断面力図を描く
            foreach (var fsec in combFsec.Fsecs)
            {
                // LoadName から同じキーの情報を
                string combNo = fsec.Key;
                var cfs = (FsecCombine)fsec.Value;
                int index = Convert.ToInt32(combNo);

                // ケース番号を印刷
                if (combName.combines.ContainsKey(index))
                {
                    var ln = combName.combines[index];
                    if (ln.name != null)
                        Text.PrtText(mc, string.Format("CASE : {0}  {1}", combNo, ln.name));
                }

                // 何の図を出すのか決めている
                int k = 1;
                switch (this.Frame.mode)
                {
                    case Layout.SplitHorizontal:
                    case Layout.SplitVertical:
                        k = 2;
                        break;
                }
                for (int i = 0; i < this.output.Count; i += k)
                {
                    for (int j = 0; j < k; ++j)
                    {
                        if (this.output.Count <= i + j)
                            continue;
                        var key = this.output[i + j];
                        // タイトルを印刷
                        this.printTitle(mc, key, j);
                        // 骨組の描写
                        this.Frame.printFrame(j, true);

                        List<Fsec> fs;
                        if (key == "mz")
                        {
                            fs = cfs.mz_min;
                            fs.AddRange(cfs.mz_max);
                        }
                        else if (key == "fy")
                        {
                            fs = cfs.fy_min;
                            fs.AddRange(cfs.fy_max);
                        }
                        else if (key == "fx")
                        {
                            fs = cfs.fx_min;
                            fs.AddRange(cfs.fx_max);
                        }
                        else
                        {
                            continue;
                        }
                        // 骨組みを印字する
                        this.printFrame(fs, key);
                    }
                    // 改ページ
                    mc.NewPage();
                }

            }
        }



        /// <summary>
        /// 基本ケース断面力図を描く
        /// </summary>
        /// <param name="mc">キャンパス</param>
        /// <param name="Fsec">断面力クラス</param>
        private void printFsec(PdfDocument mc, ResultFsec Fsec, InputLoadName LoadName)
        {
            // 断面力図を描く
            foreach (var fsec in Fsec.fsecs)
            {
                // LoadName から同じキーの情報を
                string caseNo = fsec.Key;
                var fs = (List<Fsec>)fsec.Value;
                int index = Convert.ToInt32(caseNo);

                // ケース番号を印刷
                LoadName ln = null;
                if (LoadName.loadnames.TryGetValue(index, out ln))
                    if (ln != null)
                        Text.PrtText(mc, string.Format("CASE : {0}  {1} :{2}", caseNo, ln.name, ln.symbol));

                // 何の図を出すのか決めている
                int k = 1;
                switch (this.Frame.mode)
                {
                    case Layout.SplitHorizontal:
                    case Layout.SplitVertical:
                        k = 2;
                        break;
                }
                for (int i = 0; i < this.output.Count; i += k)
                {
                    for (int j = 0; j < k; ++j)
                    {
                        if (this.output.Count <= i + j)
                            continue;
                        var key = this.output[i + j];
                        // タイトルを印刷
                        this.printTitle(mc, key, j);
                        // 骨組の描写
                        this.Frame.printFrame(j, true);
                        // 骨組みを印字する
                        this.printFrame(fs, key);
                    }
                    // 改ページ
                    mc.NewPage();
                }

            }
        }

        /// <summary>
        /// 骨組みを印字する
        /// </summary>
        /// <param name="fsec"></param>
        private void printFrame(List<Fsec> fsec, string key)
        {
            // 断面力の描写用のペン設定
            this.Frame.canvas.mc.xpen = new XPen(XColors.Blue, 0.2);

            // 断面力の縮尺を計算する
            var max_value = 0.0;
            foreach (Fsec f in fsec)
                max_value = Math.Max(Math.Abs(f.getValue2D(key)), max_value);

            // 断面力の最大値を 50pt とする
            var margin = Math.Min(printManager.padding.Left, Math.Min(
                                  printManager.padding.Right, Math.Min(
                                  printManager.padding.Bottom, 
                                  printManager.padding_Top)));
            var fsecScale = margin / max_value;

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
                if (pos == null)
                    continue;   


                //荷重の大きさを取得
                var Value = f.getValue2D(key); 

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

                    this.Frame.canvas.printText(x2, y2, string.Format("{0:0.00}", Value), radian, this.Frame.canvas.mc.font_got);
                }

                xx = x2;
                yy = y2;

            }
            //}
        }


        /// <summary>
        /// 曲げモーメント図、せん断力図などのタイトルを印刷する
        /// </summary>
        private void printTitle(PdfDocument mc, string key, int index)
        {
            // タイトルを印字する位置の設定
            var center = this.Frame.canvas.Center(index);
            var Area = this.Frame.canvas.areaSize;

            mc.currentPos.Y = center.Y - Area.Height / 2;
            mc.currentPos.Y -= printManager.padding_Top;
            mc.currentPos.Y += printManager.FontHeight + printManager.LineSpacing2;
            mc.currentPos.X = center.X - Area.Width / 2;

            // タイトルを印字する
            if (key == "mz")
                Text.PrtText(mc, "曲げモーメント図");
            if (key == "fy")
                Text.PrtText(mc, "せん断力図");
            if (key == "fx")
                Text.PrtText(mc, "軸方向力図");
            if (key == "disg")
                Text.PrtText(mc, "変 位 図");

        }
    }
}
