using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PDF_Manager.Printing
{
    public class FsecCombine
    {
        public List<Fsec> dx_max = new List<Fsec>();
        public List<Fsec> dx_min = new List<Fsec>();
        public List<Fsec> dy_max = new List<Fsec>();
        public List<Fsec> dy_min = new List<Fsec>();
        public List<Fsec> dz_max = new List<Fsec>();
        public List<Fsec> dz_min = new List<Fsec>();
        public List<Fsec> rx_max = new List<Fsec>();
        public List<Fsec> rx_min = new List<Fsec>();
        public List<Fsec> ry_max = new List<Fsec>();
        public List<Fsec> ry_min = new List<Fsec>();
        public List<Fsec> rz_max = new List<Fsec>();
        public List<Fsec> rz_min = new List<Fsec>();

        public void Add(string key, Fsec value)
        {
            // key と同じ名前の変数を取得する
            Type type = this.GetType();
            FieldInfo field = type.GetField(key);
            if (field == null)
            {
                throw new Exception(String.Format("FsecCombineクラスの変数{0} に値{1}を登録しようとしてエラーが発生しました", key, value));
            }
            var val = (List<Fsec>)field.GetValue(this);

            // 変数に値を追加する
            val.Add(value);

            // 変数を更新する
            field.SetValue(this, val);
        }
    }


    class ResultFsecCombine
    {
        public const string KEY = "FsecCombine";

        private Dictionary<string, FsecCombine> Fsecs = new Dictionary<string, FsecCombine>();

        public ResultFsecCombine(Dictionary<string, object> value, string key = ResultFsecCombine.KEY)
        {
            if (!value.ContainsKey(KEY))
                return;

            // データを取得する．
            var target = JObject.FromObject(value[key]).ToObject<Dictionary<string, object>>();


            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var No = dataManager.TypeChange(target.ElementAt(i).Key);  // ケース番号
                var val = JToken.FromObject(target.ElementAt(i).Value);

                var Fsc = ((JObject)val).ToObject<Dictionary<string, object>>(); ;
                var _Fsec = ResultFsecCombine.getFsecCombine(Fsc);

                this.Fsecs.Add(No, _Fsec);
            }
        }

        public static FsecCombine getFsecCombine(Dictionary<string, object> Fsc)
        {
            var _Fsec = new FsecCombine();

            for (int j = 0; j < Fsc.Count; j++)
            {
                var item = JToken.FromObject(Fsc.ElementAt(j).Value);
                var k = Fsc.ElementAt(j).Key;

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
                fs.caseStr = dataManager.TypeChange(item["case"]);
                fs.comb = dataManager.TypeChange(item["comb"]);

                _Fsec.Add(k, fs);

            }
            return _Fsec;
        }

    }
}
