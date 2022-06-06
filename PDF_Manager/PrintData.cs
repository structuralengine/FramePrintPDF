using PDF_Manager.Printing;
using System;
using System.Collections.Generic;

namespace PDF_Manager
{
    class PrintData
    {
        // classをまとめてここに代入する．
        public Dictionary<string, object> printDatas = new Dictionary<string, object>();
        
        /// <summary>
        /// コンストラクタ　印刷するためのデータを集計する 
        /// </summary>
        /// <param name="data"></param>
        public PrintData(Dictionary<string, object> data)
        {
            // 2次元か3次元かを記憶
            if (data.ContainsKey("dimension"))
                this.printDatas.Add("dimension", Int32.Parse(data["dimension"].ToString()));
            else
                this.printDatas.Add("dimension", 3);

            // 言語を記憶
            if (data.ContainsKey("language"))
                this.printDatas.Add("language", data["language"].ToString());
            else
                this.printDatas.Add("language", "ja");

            // ペーパサイズ
            if (data.ContainsKey("pageSize"))
                this.printDatas.Add("pageSize", data["pageSize"].ToString());
            else
                this.printDatas.Add("pageSize", "A4");

            // ペーパ向き
            if (data.ContainsKey("pageOrientation"))
                this.printDatas.Add("pageOrientation", data["pageOrientation"].ToString());
            else
                this.printDatas.Add("pageOrientation", "Vertical"); // or Horizontal

            // タイトル
            if (data.ContainsKey("title"))
                this.printDatas.Add("title", data["title"].ToString());
            else
                this.printDatas.Add("title", null);

            // node
            this.printDatas.Add(InputNode.KEY, new InputNode(data));
            // element
            this.printDatas.Add(InputElement.KEY, new InputElement(data));
            // member
            this.printDatas.Add(InputMember.KEY, new InputMember(data));
            // fixnode
            this.printDatas.Add(InputFixNode.KEY, new InputFixNode(data));
            // joint
            this.printDatas.Add(InputJoint.KEY, new InputJoint(data));
            // notice_points
            this.printDatas.Add(InputNoticePoints.KEY, new InputNoticePoints(data));
            // fixmember
            this.printDatas.Add(InputFixMember.KEY, new InputFixMember(data));
            // shell
            this.printDatas.Add(InputShell.KEY, new InputShell(data));
            // load
            //基本荷重
            this.printDatas.Add(InputLoadName.KEY, new InputLoadName(data));
            //実荷重
            this.printDatas.Add(InputLoad.KEY, new InputLoad(data));
            // define
            this.printDatas.Add(InputDefine.KEY, new InputDefine(data));
            // combine 
            this.printDatas.Add(InputCombine.KEY, new InputCombine(data));
            // pickup
            this.printDatas.Add(InputPickup.KEY, new InputPickup(data));

            // disg
            this.printDatas.Add(ResultDisg.KEY, new ResultDisg(data));
            // disgcombine
            this.printDatas.Add(ResultDisgCombine.KEY, new ResultDisgCombine(data));
            // disgPickup
            this.printDatas.Add(ResultDisgPickup.KEY, new ResultDisgPickup(data));
            // fsec
            this.printDatas.Add(ResultFsec.KEY, new ResultFsec(data));
            // fseccombine
            this.printDatas.Add(ResultFsecCombine.KEY, new ResultFsecCombine(data));
            // fsecPickup
            this.printDatas.Add(ResultFsecPickup.KEY, new ResultFsecPickup(data));
            // reac
            this.printDatas.Add(ResultReac.KEY, new ResultReac(data));
            // reaccombine
            this.printDatas.Add(ResultReacCombine.KEY, new ResultReacCombine(data));
            // reacPickup
            this.printDatas.Add(ResultReacPickup.KEY, new ResultReacPickup(data));

            // 荷重図
            if (data.ContainsKey(InputDiagramLoad.KEY))
                this.printDatas.Add(InputDiagramLoad.KEY, new InputDiagramLoad(data));
            // 断面力図
            if (data.ContainsKey(InputDiagramFsec.KEY))
                this.printDatas.Add(InputDiagramFsec.KEY, new InputDiagramFsec(data));

        }

        #region 他のモジュールのヘルパー関数


        /// <summary>
        /// 2次元モードか3次元モードか？
        /// </summary>
        public int dimension
        {
            get
            {
                int re = (int)this.printDatas["dimension"];
                return re;
            }
        }

        public string language
        {
            get
            {
                string re = (string)this.printDatas["language"];
                return re;
            }
        }

        public string pageSize
        {
            get
            {
                string re = (string)this.printDatas["pageSize"];
                return re;
            }
        }

        public string pageOrientation
        {
            get
            {
                string re = (string)this.printDatas["pageOrientation"];
                return re;
            }

        }

        public string title
        {
            get
            {
                string re = (string)this.printDatas["title"];
                return re;
            }

        }
        #endregion
    }
}
