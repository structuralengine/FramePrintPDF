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
    internal class ResultDisg
    {
        private Dictionary<string, object> value = new Dictionary<string, object>();
        public ResultDisgBasic disgBasic;
        public ResultDisgAnnexing disgAnnex;
        public List<string> title = new List<string>();
        public List<int> LL_list = new List<int>();


        /// <summary>
        /// 変位量データの読み取り（LLと基本形の分離）
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        /// <param name="value_">全データが入っている</param>
        /// <param name="disgAnnex_">基本データ以外の変位量読み取り，書き込みを行う</param>
        public void Disg(PdfDoc mc, Dictionary<string, object> value_,ResultDisgAnnexing disgAnnex_)
        {
            value = value_;
            disgAnnex = disgAnnex_;
            disgBasic = new ResultDisgBasic();

            disgBasic.data = new List<List<string[]>>(); 
            disgAnnex.dataLL = new List<List<List<string[]>>>();

            //変位量データを取得する
            var target = JObject.FromObject(value["disg"]).ToObject<Dictionary<string, object>>();

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる．
                title.Add("Case." + target.ElementAt(i).Key);

                //LLのとき
                try
                {
                    var Elem = JObject.FromObject(target.ElementAt(i).Value).ToObject<Dictionary<string, object>>();
                    //LLの存在する配列番号を記録しておく
                    LL_list.Add(i);
                    //データを取得する
                    disgAnnex.dataTreat(mc, Elem, "LL");
                }

                //基本形の時
                catch
                { 
                    JArray Elem = JArray.FromObject(target.ElementAt(i).Value);
                    //データを取得する
                    disgBasic.DisgBasic(mc, Elem);
                }
            }
        }

        /// <summary>
        /// 変位量データのPDF書き込み
        /// </summary>
        /// <param name="mc">PdfDoc</param>
        public void DisgPDF(PdfDoc mc)
        { 
            int LL_count = 0;
            int LL_count2 = 0;

            // タイトルの印刷
            mc.PrintContent("変位量", 0);
            mc.CurrentRow(2);

            // 印刷
            for (int i = 0; i < title.Count; i++)
            {
                //LLの時
                if (i == LL_list.IndexOf(i))
                {
                    //  1ケースでページをまたぐかどうか
                    int count = 0;

                    for (int m = 0; m < disgAnnex.dataLL[LL_count].Count; m++)
                    {
                        count += disgAnnex.dataLL[LL_count][m].Count;
                    }

                    mc.TypeCount(i, 7, count, title[i]);

                    // タイトルの印刷 ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    disgAnnex.DisgAnnexingPDF(mc, "LL", title[i], LL_count);
                    LL_count++;
                }
                //基本形の時
                else
                {
                    //  1タイプ内でページをまたぐかどうか
                    mc.TypeCount(i, 5, disgBasic.data[LL_count2].Count, title[i]);

                    // タイプの印刷　ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    disgBasic.DisgBasicPDF(mc, LL_count2);
                    LL_count2++;
                }
            }
   
            //mc.PrintResultBasic(title, data, header_content, header_Xspacing, body_Xspacing);

            //// 全行の取得
            //int count = 2;
            //for (int i = 0; i < disgSub.title.Count; i++)
            //{
            //    count += (data[i].Count + 5) * mc.single_Yrow + 1;
            //}
            //// 改ページ判定
            //mc.DataCountKeep(count,"disg");

            ////　ヘッダー
            //string[,] header_content3D = {
            //    { "節点", "X-Disp", "Y-Disp", "Z-Disp", "X-Rotation", "Y-Rotation", "Z-Rotation" },
            //    { "No", "(mm)", "(mm)", "(mm)", "(mmrad)", "(mmrad)", "(mmrad)" },
            //};

            //string[,] header_content2D = {
            //    { "節点", "X-Disp", "Y-Disp", "Z-Disp", "X-Rotation", "Y-Rotation", "Z-Rotation" },
            //    { "No", "(mm)", "(mm)", "(mm)", "(mmrad)", "(mmrad)", "(mmrad)" },
            //};

            //// ヘッダーのx方向の余白
            //int[,] header_Xspacing3D = {
            //    { 10, 70, 140, 210, 280, 350, 420 },
            //    { 10, 70, 140, 210, 280, 350, 420 },
            //};

            //int[,] header_Xspacing2D = {
            //    { 10, 70, 140, 210, 280, 350, 420 },
            //    { 10, 70, 140, 210, 280, 350, 420 },
            //};

            //// ボディーのx方向の余白　-1
            //int[,] body_Xspacing3D = {
            //    { 17, 85, 155, 225, 295, 365,435 }
            //};

            //int[,] body_Xspacing2D = {
            //    { 17, 85, 155, 225, 295, 365,435 }
            //};

            //string[,] header_content = mc.dimension == 3 ? header_content3D : header_content2D;
            //int[,] header_Xspacing = mc.dimension == 3 ? header_Xspacing3D : header_Xspacing2D;
            //int[,] body_Xspacing = mc.dimension == 3 ? body_Xspacing3D : body_Xspacing2D;

            // タイトルの印刷
            //mc.PrintContent("変位量", 0);
            //mc.CurrentRow(2);

        }
    }
}

