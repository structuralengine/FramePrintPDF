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
        private InputElement element_call;
        public object[] Ready(PdfDoc mc, Dictionary<string, object> data)
        {
            // データをまとめてここに代入する．
            object[] dataAll = new object[12];

            // node
            InputNode node_call = new InputNode();
            if (data.ContainsKey("node"))
            {
                dataAll[0] = node_call.Node(mc, data);
            }

            // element
            InputElement element_call = new InputElement();
            if (data.ContainsKey("element"))
            {
                List<string> elememt_title;
                List<List<string[]>> elememt_data;
                (elememt_title, elememt_data) = (element_call.Element(mc, data));
                dataAll[1] = elememt_title;
                dataAll[2] = elememt_data;
            }

            // member
            InputMember member_call = new InputMember();
            if (data.ContainsKey("member"))
            {
                dataAll[3] = member_call.Member(mc, element_call, data);
            }

            // fixnode
            if (data.ContainsKey("fix_node"))
            {
                InputFixNode fixnode_call = new InputFixNode();
                List<string> fixnode_title;
                List<List<string[]>> fixnode_data;
                (fixnode_title, fixnode_data) = (fixnode_call.FixNode(mc, data));
                dataAll[4] = fixnode_title;
                dataAll[5] = fixnode_data;
            }

            // joint
            if (data.ContainsKey("joint"))
            {
                InputJoint joint_call = new InputJoint();
                List<string> joint_title;
                List<List<string[]>> joint_data;
                (joint_title, joint_data) = (joint_call.Joint(mc, data));
                dataAll[6] = (joint_title);
                dataAll[7] = (joint_data);
            }

            // notice_points
            if (data.ContainsKey("notice_points"))
            {
                InputNoticePoints noticepoints_call = new InputNoticePoints();
                dataAll[8] = (noticepoints_call.NoticePoints(mc, member_call, data));
            }

            // fixmember
            if (data.ContainsKey("fix_member"))
            {
                InputFixMember fixmember_call = new InputFixMember();
                List<string> fixmember_title;
                List<List<string[]>> fixmember_data;
                (fixmember_title, fixmember_data) = (fixmember_call.FixMember(mc, data));
                dataAll[9] = (fixmember_title);
                dataAll[10] = (fixmember_data);
            }

            // shell
            if (data.ContainsKey("shell"))
            {
                InputShell shell_call = new InputShell();
                dataAll[11] = (shell_call.Shell(mc, data));
            }

            return dataAll;
        }

    }
}