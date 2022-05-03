using Newtonsoft.Json.Linq;
using PDF_Manager.Comon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PDF_Manager.Printing
{
    public class DisgCombine
    {
        public List<Disg> dx_max = new List<Disg>();
        public List<Disg> dx_min = new List<Disg>();
        public List<Disg> dy_max = new List<Disg>();
        public List<Disg> dy_min = new List<Disg>();
        public List<Disg> dz_max = new List<Disg>();
        public List<Disg> dz_min = new List<Disg>();
        public List<Disg> rx_max = new List<Disg>();
        public List<Disg> rx_min = new List<Disg>();
        public List<Disg> ry_max = new List<Disg>();
        public List<Disg> ry_min = new List<Disg>();
        public List<Disg> rz_max = new List<Disg>();
        public List<Disg> rz_min = new List<Disg>();

        public void Add(string key, Disg value)
        {
            // key と同じ名前の変数を取得する
            Type type = this.GetType();
            FieldInfo field = type.GetField(key);
            if(field == null)
            {
                throw new Exception(String.Format("DisgCombineクラスの変数{0} に値{1}を登録しようとしてエラーが発生しました", key, value));
            }
            var val = (List<Disg>)field.GetValue(this);

            // 変数に値を追加する
            val.Add(value);

            // 変数を更新する
            field.SetValue(this, val);
        }
    }

    internal class ResultDisgCombine
    {
        public const string KEY = "disgCombine";

        private Dictionary<string, DisgCombine> disgs = new Dictionary<string, DisgCombine>();

        public ResultDisgCombine(Dictionary<string, object> value, string key = ResultDisgCombine.KEY)
        {
            if (!value.ContainsKey(KEY))
                return;

            // データを取得する．
            var target = JObject.FromObject(value[key]).ToObject<Dictionary<string, object>>();


            // データを抽出する
            for (var i = 0; i < target.Count; i++)
            {
                var No = dataManager.toString(target.ElementAt(i).Key);  // ケース番号
                var val = JToken.FromObject(target.ElementAt(i).Value);

                var Dis = ((JObject)val).ToObject<Dictionary<string, object>>(); ;
                var _disg = ResultDisgCombine.getDisgCombine(Dis);

                this.disgs.Add(No, _disg);
            }
        }

        public static DisgCombine getDisgCombine(Dictionary<string, object> Dis)
        {
            var _disg = new DisgCombine();

            for (int j = 0; j < Dis.Count; j++)
            {
                var item = JToken.FromObject(Dis.ElementAt(j).Value);
                var k = Dis.ElementAt(j).Key;

                var ds = new Disg();

                ds.n = Dis.ElementAt(j).Key;
                ds.dx = dataManager.parseDouble(item["dx"]);
                ds.dy = dataManager.parseDouble(item["dy"]);
                ds.dz = dataManager.parseDouble(item["dz"]);
                ds.rx = dataManager.parseDouble(item["rx"]);
                ds.ry = dataManager.parseDouble(item["ry"]);
                ds.rz = dataManager.parseDouble(item["rz"]);
                ds.caseStr = dataManager.toString(item["case"]);
                ds.comb = dataManager.toString(item["comb"]);

                _disg.Add(k, ds);

            }
            return _disg;
        }

    }
}
