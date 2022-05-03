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
            this.printDatas.Add(InputNode.KEY, new InputNode(this, data));
            // element
            this.printDatas.Add(InputElement.KEY, new InputElement(this, data));
            // member
            this.printDatas.Add(InputMember.KEY, new InputMember(this, data));
            // fixnode
            this.printDatas.Add(InputFixNode.KEY, new InputFixNode(this, data));
            // joint
            this.printDatas.Add(InputJoint.KEY, new InputJoint(this, data));
            // notice_points
            this.printDatas.Add(InputNoticePoints.KEY, new InputNoticePoints(this, data));
            // fixmember
            this.printDatas.Add(InputFixMember.KEY, new InputFixMember(this, data));
            // shell
            this.printDatas.Add(InputShell.KEY, new InputShell(this, data));
            // load
            //基本荷重
            this.printDatas.Add(InputLoadName.KEY, new InputLoadName(this, data));
            //実荷重
            this.printDatas.Add(InputLoad.KEY, new InputLoad(this, data));
            // define
            this.printDatas.Add(InputDefine.KEY, new InputDefine(this, data));
            // combine 
            this.printDatas.Add(InputCombine.KEY, new InputCombine(this, data));
            // pickup
            this.printDatas.Add(InputPickup.KEY, new InputPickup(this, data));

            // disg
            this.printDatas.Add(ResultDisg.KEY, new ResultDisg(this, data));
            // disgcombine
            this.printDatas.Add(ResultDisgCombine.KEY, new ResultDisgCombine(this, data));
            // disgPickup
            this.printDatas.Add(ResultDisgPickUp.KEY, new ResultDisgPickUp(this, data));
            // fsec
            this.printDatas.Add(ResultFsec.KEY, new ResultFsec(this, data));
            // fseccombine
            this.printDatas.Add(ResultFsecCombine.KEY, new ResultFsecCombine(this, data));
            // fsecPickup
            this.printDatas.Add(ResultFsecPickUp.KEY, new ResultFsecPickUp(this, data));
            // reac
            this.printDatas.Add(ResultReac.KEY, new ResultReac(this, data));
            // reaccombine
            this.printDatas.Add(ResultReacCombine.KEY, new ResultReacCombine(this, data));
            // reacPickup
            this.printDatas.Add(ResultReacPickUp.KEY, new ResultReacPickUp(this, data));

            // 荷重図
            this.printDatas.Add(InputDiagramLoad.KEY, new InputDiagramLoad(this, data));

        }


    }
}
