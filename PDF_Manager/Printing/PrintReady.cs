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
            pickup
        }

        public object[] Ready(PdfDoc mc, Dictionary<string, object> data)
        {
            // classをまとめてここに代入する．
            var class_set = new object[Enum.GetNames(typeof(class_name)).Length];

            // node
            InputNode node_call = new InputNode();
            if (data.ContainsKey("node"))
            {
                node_call.Node(mc, data);
                class_set[(int)class_name.node] = node_call;
            }

            // element
            InputElement element_call = new InputElement();
            if (data.ContainsKey("element"))
            {
                element_call.Element(mc, data);
                class_set[(int)class_name.elememt] = element_call;
            }

            // member
            InputMember member_call = new InputMember();
            if (data.ContainsKey("member"))
            {
                member_call.Member(mc,element_call, data);
                class_set[(int)class_name.member] = member_call;
            }

            // fixnode
            if (data.ContainsKey("fix_node"))
            {
                InputFixNode fixnode_call = new InputFixNode();
                fixnode_call.FixNode(mc, data);
                class_set[(int)class_name.fix_node] = fixnode_call;
            }

            // joint
            if (data.ContainsKey("joint"))
            {
                InputJoint joint_call = new InputJoint();
                joint_call.Joint(mc, data); 
                class_set[(int)class_name.joint] = joint_call;
            }

            // notice_points
            if (data.ContainsKey("notice_points"))
            {
                InputNoticePoints noticepoints_call = new InputNoticePoints();
                noticepoints_call.NoticePoints(mc, member_call,data);
                class_set[(int)class_name.notice_points] = noticepoints_call;
            }

            // fixmember
            if (data.ContainsKey("fix_member"))
            {
                InputFixMember fixmember_call = new InputFixMember();
                fixmember_call.FixMember(mc, data);
                class_set[(int)class_name.fix_member] = fixmember_call;
            }

            // shell
            if (data.ContainsKey("shell"))
            {
                InputShell shell_call = new InputShell();
                shell_call.Shell(mc, data); 
                class_set[(int)class_name.shell] = shell_call;
            }

            // load
            if (data.ContainsKey("load"))
            {
                //基本荷重
                InputLoadName loadname_call = new InputLoadName();
                loadname_call.LoadName(mc, data);
                class_set[(int)class_name.loadname] = loadname_call;

                //実荷重
                InputLoad load_call = new InputLoad();
                load_call.Load(mc, data);
                class_set[(int)class_name.load] = load_call;
            }

            // define
            if (data.ContainsKey("define"))
            {
                InputDefine define_call = new InputDefine();
                define_call.Define(mc, data);
                class_set[(int)class_name.define] = define_call;
            }

            // combine 
            if (data.ContainsKey("combine"))
            {
                InputCombine combine_call = new InputCombine();
                combine_call.Combine(mc, data);     
                class_set[(int)class_name.combine] = combine_call;
            }

            // pickup
            if (data.ContainsKey("pickup"))
            {
                InputPickup pickup_call = new InputPickup();
                pickup_call.Pickup(mc, data);
                class_set[(int)class_name.pickup] = pickup_call;
            }

            return class_set;
        }

    }
}