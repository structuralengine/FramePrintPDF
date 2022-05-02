using PDF_Manager.Printing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager
{

    class PrintData
    {
        // classをまとめてここに代入する．
        public Dictionary<string, object> class_set = new Dictionary<string, object>();
        
        /// <summary>
        /// コンストラクタ　印刷するためのデータを集計する 
        /// </summary>
        /// <param name="data"></param>
        public PrintData(Dictionary<string, object> data)
        {
            // 2次元か3次元かを記憶
            this.class_set.Add("dimension", Int32.Parse(data["dimension"].ToString()));

            // 言語を記憶
            if (data.ContainsKey("language"))
                this.class_set.Add("language", data["language"].ToString());
            else
                this.class_set.Add("language", "ja");

            // node
            this.class_set.Add("node", new InputNode(this, data));
            // element
            this.class_set.Add("element", new InputElement(this, data));
            // member
            this.class_set.Add("member", new InputMember(this, data));
            // fixnode
            this.class_set.Add("fix_node", new InputFixNode(this, data));
            // joint
            this.class_set.Add("joint", new InputJoint(this, data));
            // notice_points
            this.class_set.Add("notice_points", new InputNoticePoints(this, data));
            // fixmember
            this.class_set.Add("fix_member", new InputFixMember(this, data));
            // shell
            this.class_set.Add("shell", new InputShell(this, data));

            // load
            //基本荷重
            this.class_set.Add("loadname", new InputLoadName(this, data));
            //実荷重
            this.class_set.Add("load", new InputLoad(this, data));
            // define
            this.class_set.Add("define", new InputDefine(this, data));
            // combine 
            this.class_set.Add("combine", new InputCombine(this, data));
            // pickup
            this.class_set.Add("pickup", new InputPickup(this, data));
            // disg
            this.class_set.Add("disg", new ResultDisg(this, data));
            // disgcombine
            this.class_set.Add("disgCombine", new ResultDisgCombine(this, data));
            // disgPickup
            this.class_set.Add("disgPickup", new ResultDisgPickUp(this, data));
            // fsec
            this.class_set.Add("fsec", new ResultFsec(this, data));
            // fseccombine
            this.class_set.Add("fsecCombine", new ResultFsecCombine(this, data));
            // fsecPickup
            this.class_set.Add("fsecPickup", new ResultFsecPickUp(this, data));
            // reac
            this.class_set.Add("reac", new ResultReac(this, data));
            // reaccombine
            this.class_set.Add("reacCombine", new ResultReacCombine(this, data));
            // reacPickup
            this.class_set.Add("reacPickup", new ResultReacPickUp(this, data));
            // 荷重図
            this.class_set.Add("diagramLoad", new InputDiagramLoad(this, data));

        }


    }
}
