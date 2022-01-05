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
        public void Export(PdfDoc mc, object[] class_set)
        {
            // node
            if ((InputNode)class_set[(int)PrintReady.class_name.node] != null)
            {
                InputNode cls_node = (InputNode)class_set[(int)PrintReady.class_name.node];
                cls_node.NodePDF(mc);
            }

            // member
            if ((InputMember)class_set[(int)PrintReady.class_name.member] != null)
            {
                InputMember cls_member = (InputMember)class_set[(int)PrintReady.class_name.member];
                cls_member.MemberPDF(mc);
            }

            // element
            if ((InputElement)class_set[(int)PrintReady.class_name.elememt] != null)
            {
                InputElement cls_element = (InputElement)class_set[(int)PrintReady.class_name.elememt];
                cls_element.ElementPDF(mc);
            }

            // fixnode
            if ((InputFixNode)class_set[(int)PrintReady.class_name.fix_node] != null)
            {
                InputFixNode cls_fixnode = (InputFixNode)class_set[(int)PrintReady.class_name.fix_node];
                cls_fixnode.FixNodePDF(mc);
            }

            // joint
            if ((InputJoint)class_set[(int)PrintReady.class_name.joint] != null)
            {
                InputJoint cls_joint = (InputJoint)class_set[(int)PrintReady.class_name.joint];
                cls_joint.JointPDF(mc);
            }

            // noticepoints
            if ((InputNoticePoints)class_set[(int)PrintReady.class_name.notice_points] != null)
            {
                InputNoticePoints cls_noticepoints = (InputNoticePoints)class_set[(int)PrintReady.class_name.notice_points];
                cls_noticepoints.NoticePointsPDF(mc);
            }

            // fixmember
            if ((InputFixMember)class_set[(int)PrintReady.class_name.fix_member] != null)
            {
                InputFixMember cls_fixmember = (InputFixMember)class_set[(int)PrintReady.class_name.fix_member];
                cls_fixmember.FixMemberPDF(mc);
            }

            // shell
            if ((InputShell)class_set[(int)PrintReady.class_name.shell] != null)
            {
                InputShell cls_shell = (InputShell)class_set[(int)PrintReady.class_name.shell];
                cls_shell.ShellPDF(mc);
            }

            //loadname
            if ((InputLoadName)class_set[(int)PrintReady.class_name.loadname] != null)
            {
                InputLoadName cls_loadname = (InputLoadName)class_set[(int)PrintReady.class_name.loadname];
                cls_loadname.LoadNamePDF(mc);
            }

            //load
            if ((InputLoad)class_set[(int)PrintReady.class_name.load] != null)
            {
                InputLoad cls_load = (InputLoad)class_set[(int)PrintReady.class_name.load];
                cls_load.LoadPDF(mc);
            }

            //define
            if ((InputDefine)class_set[(int)PrintReady.class_name.define] != null)
            {
                InputDefine cls_define = (InputDefine)class_set[(int)PrintReady.class_name.define];
                cls_define.DefinePDF(mc);
            }

            //combine
            if ((InputCombine)class_set[(int)PrintReady.class_name.combine] != null)
            {
                InputCombine cls_combine = (InputCombine)class_set[(int)PrintReady.class_name.combine];
                cls_combine.CombinePDF(mc);
            }

            //pickup
            if ((InputPickup)class_set[(int)PrintReady.class_name.pickup] != null)
            {
                InputPickup cls_pickup = (InputPickup)class_set[(int)PrintReady.class_name.pickup];
                cls_pickup.PickupPDF(mc);
            }
        }
    }
}
