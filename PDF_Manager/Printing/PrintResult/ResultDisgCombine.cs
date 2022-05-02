using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing.PrintResult
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
    }


    internal class ResultDisgCombine
    {
        private Dictionary<string, DisgCombine> disgs = new Dictionary<string, DisgCombine>();

        public ResultDisgCombine(PrintData pd, Dictionary<string, object> value)
        {
            // データを取得する．
            var target = JObject.FromObject(value["disg"]).ToObject<Dictionary<string, object>>();
        }

    }
}
