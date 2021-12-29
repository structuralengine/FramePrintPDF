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
        public ArrayList Ready(PdfDoc mc, Dictionary<string, object> data)
        {
            // データをまとめてここに代入する．
            ArrayList dataAll = new ArrayList();

            // node
            InputNode node_call = new InputNode();
            dataAll.Add(node_call.Node(mc, data));

            // element
            InputElement element_call = new InputElement();
            List<string> elememt_title;
            List<List<string[]>> elememt_data;
            (elememt_title, elememt_data) = (element_call.Element(mc, data));
            dataAll.Add(elememt_title);
            dataAll.Add(elememt_data);

            // member
            InputMember member_call = new InputMember();
            dataAll.Add(member_call.Member(mc,element_call, data));

            // fixnode
            InputFixNode fixnode_call = new InputFixNode();
            List<string> fixnode_title;
            List<List<string[]>> fixnode_data;
            (fixnode_title, fixnode_data) = (fixnode_call.FixNode(mc, data));
            dataAll.Add(fixnode_title);
            dataAll.Add(fixnode_data);

            // joint
            InputJoint joint_call = new InputJoint();
            List<string> joint_title;
            List<List<string[]>> joint_data;
            (joint_title, joint_data) = (joint_call.Joint(mc,data));
            dataAll.Add(joint_title);
            dataAll.Add(joint_data);

            // notice_points
            InputNoticePoints noticepoints_call = new InputNoticePoints();
            dataAll.Add(noticepoints_call.NoticePoints(mc,member_call, data));

            // fixmember
            InputFixMember fixmember_call = new InputFixMember();
            List<string> fixmember_title;
            List<List<string[]>> fixmember_data;
            (fixmember_title, fixmember_data) = (fixmember_call.FixMember(mc, data));
            dataAll.Add(fixmember_title);
            dataAll.Add(fixmember_data);

            // shell
            InputShell shell_call = new InputShell();
            dataAll.Add(shell_call.Shell(mc,data));

            return dataAll;
        }

    }
}