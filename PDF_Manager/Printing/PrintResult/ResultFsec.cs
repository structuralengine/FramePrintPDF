using Newtonsoft.Json.Linq;
using PDF_Manager.Printing;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Utils;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using Newtonsoft.Json;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PDF_Manager.Comon;

namespace PDF_Manager.Printing
{

    public class Fsec
    {
        public string m;    // 要素番号
        public string n;    // 節点番号
        public double l;    // 着目点距離
        public double fx;
        public double fy;
        public double fz;
        public double mx;
        public double my;
        public double mz;

        // 組み合わせで使う
        public string caseStr = null;
        public string comb = null;
    }


    internal class ResultFsec
    {
        public const string KEY = "fsec";

        private Dictionary<string, object> fsecs = new Dictionary<string, object>();

        public ResultFsec(PrintData pd, Dictionary<string, object> value)
        {
            if (value.ContainsKey(KEY))
                return;

            // データを取得する．
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = dataManager.TypeChange(target.ElementAt(i).Key);  // ケース番号
                var val = JToken.FromObject(target.ElementAt(i).Value);

                if (val.Type == JTokenType.Array)
                {
                    JArray Fsc = (JArray)val;
                    var _fsec = new List<Fsec>();
                    for (int j = 0; j < Fsc.Count; j++)
                    {
                        JToken item = Fsc[j];

                        var fs = new Fsec();

                        fs.n = dataManager.TypeChange(item["n"]);
                        fs.m = dataManager.TypeChange(item["m"]);
                        fs.l = dataManager.parseDouble(item["m"]);
                        fs.fx = dataManager.parseDouble(item["fx"]);
                        fs.fy = dataManager.parseDouble(item["fy"]);
                        fs.fz = dataManager.parseDouble(item["fz"]);
                        fs.mx = dataManager.parseDouble(item["mx"]);
                        fs.my = dataManager.parseDouble(item["my"]);
                        fs.mz = dataManager.parseDouble(item["mz"]);

                        _fsec.Add(fs);

                    }
                    this.fsecs.Add(key, _fsec);

                }
                else if (val.Type == JTokenType.Object)
                {   // LL：連行荷重の時
                    var Fsc = ((JObject)val).ToObject<Dictionary<string, object>>();
                    var _fsec = ResultFsecCombine.getFsecCombine(Fsc);
                    this.fsecs.Add(key, _fsec);
                }

            }
        }


    }
}
/*
        private Dictionary<string, object> value = new Dictionary<string, object>();
        public ResultFsecBasic fsecBasic;
        public ResultFsecAnnexing fsecAnnex;
        public List<string> title = new List<string>();
        public List<int> LL_list = new List<int>();

        /// <summary>
        /// 断面力データの読み取り（LLと基本形の分離）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="value_">全データが入っている</param>
        /// <param name="disgAnnex_">基本データ以外の断面力読み取り，書き込みを行う</param>
        public void init(PdfDoc mc, Dictionary<string, object> value_, ResultFsecAnnexing fsecAnnex_)
        {
            value = value_;
            fsecAnnex = fsecAnnex_;
            fsecBasic = new ResultFsecBasic();

            fsecBasic.data = new List<List<string[]>>();
            fsecAnnex.dataLL = new List<List<List<string[]>>>();
            title = new List<string>();

            //断面力データを取得する
            var target = JObject.FromObject(value["fsec"]).ToObject<Dictionary<string, object>>();
            var targetName = JArray.FromObject(value["fsecName"]);

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる．
                var load = targetName[i];
                string[] loadNew = new String[2];

                loadNew[0] = load[0].ToString();
                loadNew[1] = load[1].ToString();

                title.Add(loadNew[0] + loadNew[1].PadLeft(loadNew[1].Length + 2));
                target.Remove("name");

                //LLのとき
                try
                {
                    var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                    //LLの存在する配列番号を記録しておく
                    LL_list.Add(i);
                    //データを取得する
                    fsecAnnex.dataTreat(mc, Elem, "LL");
                }

                //基本形の時
                catch
                {
                    JArray Elem = JArray.FromObject(target.ElementAt(i).Value);
                    //データを取得する
                    fsecBasic.FsecBasic(mc, Elem);
                }
            }

        }

        /// <summary>
        /// 断面力データのPDF書き込み
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        public void FsecPDF(PdfDoc mc)
        {
            int LL_count = 0;
            int LL_count2 = 0;

            // タイトルの印刷
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("断面力データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Internal Member Forces and Momemts", 0);
                    break;
            }

            mc.CurrentRow(2);
            mc.CurrentColumn(0);

            // 印刷
            for (int i = 0; i < title.Count; i++)
            {
                //LLの時
                if (LL_list.Contains(i))
                {
                    //  1ケースでページをまたぐかどうか
                    int count = 0;

                    for (int m = 0; m < fsecAnnex.dataLL[LL_count].Count; m++)
                    {
                        count += fsecAnnex.dataLL[LL_count][m].Count;
                    }

                    mc.TypeCount(i, 8, count, title[i]);

                    // タイトルの印刷 ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    fsecAnnex.FsecAnnexingPDF(mc, "LL", title[i], LL_count);
                    LL_count++;
                }
                //基本形の時
                else
                {
                    //  1タイプ内でページをまたぐかどうか
                    mc.TypeCount(i, 6, fsecBasic.data[LL_count2].Count, title[i]);

                    // タイプの印刷　ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    fsecBasic.FsecBasicPDF(mc, LL_count2);
                    LL_count2++;
                }
            }


            //// 全行の取得
            ////int count = 2;
            //for (int i = 0; i < title.Count; i++)
            //{
            //    count += (data[i].Count + 5) * mc.single_Yrow + 1;
            //}
            //// 改ページ判定
            //mc.DataCountKeep(count,"fsec");

            ////　ヘッダー
            //string[,] header_content = {
            //    { "部材", "節点","", "FX", "FY", "FZ", "MX", "MY","MZ" },
            //    { "No", "No", "DIST", "(kN)", "(kN)", "(kN)", "(kN・m)", "(kN・m)", "(kN・m)" },
            //};
            //// ヘッダーのx方向の余白
            //int[,] header_Xspacing = {
            //    { 10, 50, 105, 160, 210, 260, 310,360,410 },
            //    { 10, 50, 105, 160, 210, 260, 310,360,410 },
            //};

            //// ボディーのx方向の余白　-1
            //int[,] body_Xspacing = {
            //    { 17, 57, 118, 174, 224, 274, 324,374,424 }
            //};

            //// タイトルの印刷
            //mc.PrintContent("断面力", 0);
            //mc.CurrentRow(2);

            // 印刷
            //mc.PrintResultBasic(title, data, header_content, header_Xspacing, body_Xspacing);
        }
    }
}

*/