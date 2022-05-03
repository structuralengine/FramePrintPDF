using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using PDF_Manager.Printing.PrintResult;
using System.Collections.Generic;
using System.Linq;

namespace PDF_Manager.Printing
{
    public class Disg
    {
        public string n;    // 節点番号
        public double dx;
        public double dy;
        public double dz;
        public double rx;
        public double ry;
        public double rz;

        // 組み合わせで使う
        public string caseStr = null;
        public string comb = null;
    }


    internal class ResultDisg
    {
        private Dictionary<string, object> disgs = new Dictionary<string, object>();

        public ResultDisg(PrintData pd, Dictionary<string, object> value)
        {
            // データを取得する．
            var target = JObject.FromObject(value["disg"]).ToObject<Dictionary<string, object>>();

            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var key = dataManager.TypeChange(target.ElementAt(i).Key);  // ケース番号
                var val = JToken.FromObject(target.ElementAt(i).Value);

                if(val.Type == JTokenType.Array)
                {
                    JArray Dis = (JArray)val;
                    var _disg = new List<Disg>();
                    for (int j = 0; j < Dis.Count; j++)
                    {
                        JToken item = Dis[j];

                        var ds = new Disg();

                        ds.n = dataManager.TypeChange(item["n"]);
                        ds.dx = dataManager.parseDouble(item["dx"]);
                        ds.dy = dataManager.parseDouble(item["dy"]);
                        ds.dz = dataManager.parseDouble(item["dz"]);
                        ds.rx = dataManager.parseDouble(item["rx"]);
                        ds.ry = dataManager.parseDouble(item["ry"]);
                        ds.rz = dataManager.parseDouble(item["rz"]);

                        _disg.Add(ds);

                    }
                    this.disgs.Add(key, _disg);

                } 
                else if (val.Type == JTokenType.Object)
                {   // LL：連行荷重の時
                    var Dis = ((JObject)val).ToObject<Dictionary<string, object>>();
                    var _disg = ResultDisgCombine.getDisgCombine(Dis);
                    this.disgs.Add(key, _disg);
                }

            }
        }


    }
}
/*
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
        public void init(PdfDoc mc, Dictionary<string, object> value_,ResultDisgAnnexing disgAnnex_)
        {
            value = value_;
            disgAnnex = disgAnnex_;
            disgBasic = new ResultDisgBasic();

            disgBasic.data = new List<List<string[]>>(); 
            disgAnnex.dataLL = new List<List<List<string[]>>>();
            title = new List<string>();

            //変位量データを取得する
            var target = JObject.FromObject(value["disg"]).ToObject<Dictionary<string, object>>();
            var targetName = JArray.FromObject(value["disgName"]);

            //LLか基本形かを判定しながら1行1行確認
            for (int i = 0; i < target.Count; i++)
            {
                // タイトルを入れる
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
            switch (mc.language)
            {
                case "ja":
                    mc.PrintContent("変位量データ", 0);
                    break;
                case "en":
                    mc.PrintContent("Displacement", 0);
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

                    for (int m = 0; m < disgAnnex.dataLL[LL_count].Count; m++)
                    {
                        count += disgAnnex.dataLL[LL_count][m].Count;
                    }

                    mc.TypeCount(i, 8, count, title[i]);

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
                    mc.TypeCount(i, 6, disgBasic.data[LL_count2].Count, title[i]);

                    // タイプの印刷　ex)case2
                    mc.CurrentColumn(0);
                    mc.PrintContent(title[i], 0);
                    mc.CurrentRow(2);

                    // header情報を取り，印刷へ
                    disgBasic.DisgBasicPDF(mc, LL_count2);
                    LL_count2++;
                }
            }
   
        }
    }
}

*/