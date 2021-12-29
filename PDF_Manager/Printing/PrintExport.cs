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
    internal class PrintExport
    {
        public ArrayList Export(PdfDoc mc,ArrayList dataset)
        {
            // node
            InputNode node = new InputNode();
            // gfx登録
            node.NodePDF(mc, (List<List<string[]>>)dataset[0]);

            // member
            InputMember member = new InputMember();
            // gfx登録
            member.MemberPDF(mc, (List<string[]>)dataset[3]);

            // element
            InputElement element = new InputElement();
            // gfx登録 (mc,tltle,data)
            element.ElementPDF(mc ,(List<string>)dataset[1],(List<List<string[]>>)dataset[2]);

            // fixnode
            InputFixNode fixnode = new InputFixNode();
            // gfx登録 (mc,tltle,data)
            fixnode.FixNodePDF(mc, (List<string>)dataset[4], (List<List<string[]>>)dataset[5]);

            // joint
            InputJoint joint = new InputJoint();
            // gfx登録 (mc,tltle,data)
            joint.JointPDF(mc, (List<string>)dataset[6], (List<List<string[]>>)dataset[7]);

            // noticepoints
            InputNoticePoints noticepoints = new InputNoticePoints();
            // gfx登録 (mc,tltle,data)
            noticepoints.NoticePointsPDF(mc, (List<List<string[]>>)dataset[8]);

            // fixmember
            InputFixMember fixmember = new InputFixMember();
            // gfx登録 (mc,tltle,data)
            fixmember.FixMemberPDF(mc, (List<string>)dataset[9], (List<List<string[]>>)dataset[10]);

            // shell
            InputShell shell = new InputShell();
            // gfx登録 (mc,tltle,data)
            shell.ShellPDF(mc, (List<List<string[]>>)dataset[11]);


            return dataset;
        }
    }
}
