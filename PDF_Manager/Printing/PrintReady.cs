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
using PDF_Manager.Printing.PrintInput;

namespace PDF_Manager.Printing
{
    internal class PrintReady
    {
        public enum class_name
        {
            node,
            elememt,
            member,
            fix_node,
            joint,
            notice_points,
            fix_member,
            shell,
            loadname,
            load,
            define,
            combine,
            pickup,
            disg,
            disgCombine,
            disgPickup,
            fsec,
            fsecCombine,
            fsecPickup,
            reac,
            reacCombine,
            reacPickup,
            diagramLoad,
        }

        public object[] Ready(PdfDoc mc, Dictionary<string, object> data)
        {
            // 2次元か3次元かを記憶
            mc.dimension = Int32.Parse(data["dimension"].ToString());

            // 言語を記憶
            if (data.ContainsKey("language"))
            {
                mc.language = data["language"].ToString();
            }
            else
            {
                mc.language = "ja";
            }

            // classをまとめてここに代入する．
            var class_set = new object[Enum.GetNames(typeof(class_name)).Length];

            // node
            InputNode node = new InputNode();
            if (data.ContainsKey("node"))
            {
                node.init(mc, data);
                class_set[(int)class_name.node] = node;
            }

            // element
            InputElement element = new InputElement();
            if (data.ContainsKey("element"))
            {
                element.init(mc, data);
                class_set[(int)class_name.elememt] = element;
            }

            // member
            InputMember member = new InputMember();
            if (data.ContainsKey("member"))
            {
                member.init(mc,element, data);
                class_set[(int)class_name.member] = member;
            }

            // fixnode
            if (data.ContainsKey("fix_node"))
            {
                InputFixNode fixnode = new InputFixNode();
                fixnode.init(mc, data);
                class_set[(int)class_name.fix_node] = fixnode;
            }

            // joint
            if (data.ContainsKey("joint"))
            {
                InputJoint joint = new InputJoint();
                joint.init(mc, data); 
                class_set[(int)class_name.joint] = joint;
            }

            // notice_points
            if (data.ContainsKey("notice_points"))
            {
                InputNoticePoints noticepoints = new InputNoticePoints();
                noticepoints.init(mc, member,data);
                class_set[(int)class_name.notice_points] = noticepoints;
            }

            // fixmember
            if (data.ContainsKey("fix_member"))
            {
                InputFixMember fixmember = new InputFixMember();
                fixmember.init(mc, data);
                class_set[(int)class_name.fix_member] = fixmember;
            }

            // shell
            if (data.ContainsKey("shell"))
            {
                InputShell shell = new InputShell();
                shell.init(mc, data); 
                class_set[(int)class_name.shell] = shell;
            }

            // load
            if (data.ContainsKey("load"))
            {
                //基本荷重
                InputLoadName loadname = new InputLoadName();
                loadname.init(mc, data);
                class_set[(int)class_name.loadname] = loadname;

                //実荷重
                InputLoad load = new InputLoad();
                load.init(mc, data);
                class_set[(int)class_name.load] = load;
            }


            // define
            if (data.ContainsKey("define"))
            {
                InputDefine define = new InputDefine();
                define.init(mc, data);
                class_set[(int)class_name.define] = define;
            }

            // combine 
            if (data.ContainsKey("combine"))
            {
                InputCombine combine = new InputCombine();
                combine.init(mc, data);     
                class_set[(int)class_name.combine] = combine;
            }

            // pickup
            if (data.ContainsKey("pickup"))
            {
                InputPickup pickup = new InputPickup();
                pickup.init(mc, data);
                class_set[(int)class_name.pickup] = pickup;
            }


            // disg
            ResultDisgAnnexing disgAnnexing = new ResultDisgAnnexing();
            if (data.ContainsKey("disg"))
            {
                ResultDisg disg = new ResultDisg();
                disg.init(mc, data,disgAnnexing);
                class_set[(int)class_name.disg] = disg;
            }

            // disgcombine
            if (data.ContainsKey("disgCombine"))
            {
                disgAnnexing.init(mc, data, "Combine");
                class_set[(int)class_name.disgCombine] = disgAnnexing;
            }

            // disgPickup
            if (data.ContainsKey("disgPickup"))
            {
                disgAnnexing.init(mc, data, "Pickup");
                class_set[(int)class_name.disgPickup] = disgAnnexing;
            }

            // fsec
            ResultFsecAnnexing fsecAnnexing = new ResultFsecAnnexing();
            if (data.ContainsKey("fsec"))
            {
                ResultFsec fsec = new ResultFsec();
                fsec.init(mc, data, fsecAnnexing);
                class_set[(int)class_name.fsec] = fsec;
            }

            // fseccombine
            if (data.ContainsKey("fsecCombine"))
            {
                fsecAnnexing.init(mc, data, "Combine");
                class_set[(int)class_name.fsecCombine] = fsecAnnexing;
            }

            // fsecPickup
            if (data.ContainsKey("fsecPickup"))
            {
                fsecAnnexing.init(mc, data, "Pickup");
                class_set[(int)class_name.fsecPickup] = fsecAnnexing;
            }


            ResultReacAnnexing reacAnnexing = new ResultReacAnnexing();
            // reac
            if (data.ContainsKey("reac"))
            {
                ResultReac reac = new ResultReac();
                reac.init(mc, data, reacAnnexing);
                class_set[(int)class_name.reac] = reac;
            }

            // reaccombine
            if (data.ContainsKey("reacCombine"))
            {
                reacAnnexing.init(mc, data, "Combine");
                class_set[(int)class_name.reacCombine] = reacAnnexing;
            }

            // reacPickup
            if (data.ContainsKey("reacPickup"))
            {
                reacAnnexing.init(mc, data, "Pickup");
                class_set[(int)class_name.reacPickup] = reacAnnexing;
            }

            // 荷重図
            if (data.ContainsKey("diagramLoad"))
            {
                InputDiagramLoad diagram_load = new InputDiagramLoad();
                diagram_load.init(mc, data);
                class_set[(int)class_name.diagramLoad] = diagram_load;
            }


            return class_set;
        }

    }
}