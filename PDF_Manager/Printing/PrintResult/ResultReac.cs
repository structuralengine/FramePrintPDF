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

namespace PDF_Manager.Printing
{
    internal class ResultReac
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        public ResultReacBasic reacBasic;
        public ResultReacAnnexing reacAnnex;
        public List<string> title = new List<string>();
        public List<int> LL_list = new List<int>();


        public void Reac(PdfDoc mc, Dictionary<string, object> value_, ResultReacAnnexing reacAnnex_)
        {
            value = value_;
            reacAnnex = reacAnnex_;
            reacBasic = new ResultReacBasic();

            reacBasic.data = new List<List<string[]>>();
            reacAnnex.dataLL = new List<List<List<string[]>>>();
            title = new List<string>();    
            
            //reacデータを取得する
            var target = JObject.FromObject(value["reac"]).ToObject<Dictionary<string, object>>();

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる．
                var load = InputLoadName.data[i][3] == null ? "" : InputLoadName.data[i][3];
                title.Add("Case." + target.ElementAt(i).Key + load.PadLeft(10));

                //LLのとき
                try
                {
                    var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                    //LLの存在する配列番号を記録しておく
                    LL_list.Add(i);
                    //データを取得する
                    reacAnnex.dataTreat(mc, Elem, "LL");
                }

                //基本形の時
                catch
                {
                    JArray Elem = JArray.FromObject(target.ElementAt(i).Value);
                    //データを取得する
                    reacBasic.ReacBasic(mc, Elem);
                }
            }

        }

        public void ReacPDF(PdfDoc mc)
        {
            int LL_count = 0;
            int LL_count2 = 0;

            // タイトルの印刷
            mc.PrintContent("反力", 0);
            mc.CurrentRow(2);

            // 印刷
            for (int i = 0; i < title.Count; i++)
            {
                //LLの時
                if (i == LL_list.IndexOf(i))
                {
                    //  1ケースでページをまたぐかどうか
                    int count = 0;

                    for (int m = 0; m < reacAnnex.dataLL[LL_count].Count; m++)
                    {
                        count += reacAnnex.dataLL[LL_count][m].Count;
                    }

                    mc.TypeCount(i, 8, count, title[i]);

                    // タイトルの印刷 ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    reacAnnex.ReacAnnexingPDF(mc, "LL", title[i], LL_count);
                    LL_count++;
                }
                //基本形の時
                else
                {
                    //  1タイプ内でページをまたぐかどうか
                    mc.TypeCount(i, 6, reacBasic.data[LL_count2].Count, title[i]);

                    // タイプの印刷　ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    reacBasic.ReacBasicPDF(mc, LL_count2);
                    LL_count2++;
                }
            }

            //// 全行の取得
            //int count = 2;
            //for (int i = 0; i < title.Count; i++)
            //{
            //    count += (data[i].Count + 5) * mc.single_Yrow + 1;
            //}
            //// 改ページ判定
            //mc.DataCountKeep(count, "reac");

            // 印刷
            //mc.PrintResultBasic(title, data, header_content, header_Xspacing, body_Xspacing);
        }
    }
}

