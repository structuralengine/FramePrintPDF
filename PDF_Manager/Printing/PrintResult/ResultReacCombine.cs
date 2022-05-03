using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using System.Collections.Generic;
using System.Linq;

namespace PDF_Manager.Printing.PrintResult
{
    public class ReacCombine
    {
        public List<Reac> tx_max = new List<Reac>();
        public List<Reac> tx_min = new List<Reac>();
        public List<Reac> ty_max = new List<Reac>();
        public List<Reac> ty_min = new List<Reac>();
        public List<Reac> tz_max = new List<Reac>();
        public List<Reac> tz_min = new List<Reac>();
        public List<Reac> mx_max = new List<Reac>();
        public List<Reac> mx_min = new List<Reac>();
        public List<Reac> my_max = new List<Reac>();
        public List<Reac> my_min = new List<Reac>();
        public List<Reac> mz_max = new List<Reac>();
        public List<Reac> mz_min = new List<Reac>();
    }


    class ResultReacCombine
    {
        private Dictionary<string, ReacCombine> reacs = new Dictionary<string, ReacCombine>();

        public ResultReacCombine(PrintData pd, Dictionary<string, object> value, string key = "reacCombine")
        {
            // データを取得する．
            var target = JObject.FromObject(value[key]).ToObject<Dictionary<string, object>>();


            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var No = dataManager.TypeChange(target.ElementAt(i).Key);  // ケース番号
                var val = JToken.FromObject(target.ElementAt(i).Value);

                var Dis = ((JObject)val).ToObject<Dictionary<string, object>>(); ;
                var _reac = ResultReacCombine.getReacCombine(Dis);

                this.reacs.Add(No, _reac);
            }
        }

        public static ReacCombine getReacCombine(Dictionary<string, object> Dis)
        {
            var _reac = new ReacCombine();
            for (int j = 0; j < Dis.Count; j++)
            {
                var item = JToken.FromObject(Dis.ElementAt(j).Value);
                var k = Dis.ElementAt(j).Key;

                var ds = new Reac();

                ds.n = Dis.ElementAt(j).Key;
                ds.tx = dataManager.parseDouble(item["tx"]);
                ds.ty = dataManager.parseDouble(item["ty"]);
                ds.tz = dataManager.parseDouble(item["tz"]);
                ds.mx = dataManager.parseDouble(item["mx"]);
                ds.my = dataManager.parseDouble(item["my"]);
                ds.mz = dataManager.parseDouble(item["mz"]);
                ds.caseStr = dataManager.TypeChange(item["case"]);
                ds.comb = dataManager.TypeChange(item["comb"]);

                _reac[k].Add(ds);

            }
            return _reac;
        }

    }
}
