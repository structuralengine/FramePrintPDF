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

            // node
            this.printDatas.Add("node", new InputNode(this, data));
            // element
            this.printDatas.Add("element", new InputElement(this, data));
            // member
            this.printDatas.Add("member", new InputMember(this, data));
            // fixnode
            this.printDatas.Add("fix_node", new InputFixNode(this, data));
            // joint
            this.printDatas.Add("joint", new InputJoint(this, data));
            // notice_points
            this.printDatas.Add("notice_points", new InputNoticePoints(this, data));
            // fixmember
            this.printDatas.Add("fix_member", new InputFixMember(this, data));
            // shell
            this.printDatas.Add("shell", new InputShell(this, data));
            // load
            //基本荷重
            this.printDatas.Add("loadname", new InputLoadName(this, data));
            //実荷重
            this.printDatas.Add("load", new InputLoad(this, data));
            // define
            this.printDatas.Add("define", new InputDefine(this, data));
            // combine 
            this.printDatas.Add("combine", new InputCombine(this, data));
            // pickup
            this.printDatas.Add("pickup", new InputPickup(this, data));

            // disg
            this.printDatas.Add("disg", new ResultDisg(this, data));
            // disgcombine
            this.printDatas.Add("disgCombine", new ResultDisgCombine(this, data));
            // disgPickup
            this.printDatas.Add("disgPickup", new ResultDisgPickUp(this, data));
            // fsec
            this.printDatas.Add("fsec", new ResultFsec(this, data));
            // fseccombine
            this.printDatas.Add("fsecCombine", new ResultFsecCombine(this, data));
            // fsecPickup
            this.printDatas.Add("fsecPickup", new ResultFsecPickUp(this, data));
            // reac
            this.printDatas.Add("reac", new ResultReac(this, data));
            // reaccombine
            this.printDatas.Add("reacCombine", new ResultReacCombine(this, data));
            // reacPickup
            this.printDatas.Add("reacPickup", new ResultReacPickUp(this, data));

            // 荷重図
            this.printDatas.Add("diagramLoad", new InputDiagramLoad(this, data));

        }


    }
}
