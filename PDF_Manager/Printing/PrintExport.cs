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
            for (int i = 0; i < class_set.Length; i ++)
            {
                if(class_set[i] != null)
                {
                    mc.name = Enum.GetNames(typeof(PrintReady.class_name))[i];
                    break;
                }
            }

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

            //disg
            if ((ResultDisg)class_set[(int)PrintReady.class_name.disg] != null)
            {
                ResultDisg cls_disg = (ResultDisg)class_set[(int)PrintReady.class_name.disg];
                cls_disg.DisgPDF(mc);
            }

            //disgCombine
            if ((ResultDisgAnnexing)class_set[(int)PrintReady.class_name.disgCombine] != null)
            {
                ResultDisgAnnexing cls_disgAnnexing = (ResultDisgAnnexing)class_set[(int)PrintReady.class_name.disgCombine];
                cls_disgAnnexing.DisgAnnexingPDF(mc, "Combine");
            }

            //disgPickup
            if ((ResultDisgAnnexing)class_set[(int)PrintReady.class_name.disgPickup] != null)
            {
                ResultDisgAnnexing cls_disgAnnexing = (ResultDisgAnnexing)class_set[(int)PrintReady.class_name.disgPickup];
                cls_disgAnnexing.DisgAnnexingPDF(mc, "Pickup");
            }

            //fsec
            if ((ResultFsec)class_set[(int)PrintReady.class_name.fsec] != null)
            {
                ResultFsec cls_fsec = (ResultFsec)class_set[(int)PrintReady.class_name.fsec];
                cls_fsec.FsecPDF(mc);
            }

            //fsecCombine
            if ((ResultFsecAnnexing)class_set[(int)PrintReady.class_name.fsecCombine] != null)
            {
                ResultFsecAnnexing cls_fsecAnnexing = (ResultFsecAnnexing)class_set[(int)PrintReady.class_name.fsecCombine];
                cls_fsecAnnexing.FsecAnnexingPDF(mc, "Combine");
            }

            //fsecPickup
            if ((ResultFsecAnnexing)class_set[(int)PrintReady.class_name.fsecPickup] != null)
            {
                ResultFsecAnnexing cls_fsecAnnexing = (ResultFsecAnnexing)class_set[(int)PrintReady.class_name.fsecPickup];
                cls_fsecAnnexing.FsecAnnexingPDF(mc, "Pickup");
            }

            //reac
            if ((ResultReac)class_set[(int)PrintReady.class_name.reac] != null)
            {
                ResultReac cls_reac = (ResultReac)class_set[(int)PrintReady.class_name.reac];
                cls_reac.ReacPDF(mc);
            }

            //reacCombine
            if ((ResultReacAnnexing)class_set[(int)PrintReady.class_name.reacCombine] != null)
            {
                ResultReacAnnexing cls_reacAnnexing = (ResultReacAnnexing)class_set[(int)PrintReady.class_name.reacCombine];
                cls_reacAnnexing.ReacAnnexingPDF(mc, "Combine");
            }

            //reacPickup
            if ((ResultReacAnnexing)class_set[(int)PrintReady.class_name.reacPickup] != null)
            {
                ResultReacAnnexing cls_reacAnnexing = (ResultReacAnnexing)class_set[(int)PrintReady.class_name.reacPickup];
                cls_reacAnnexing.ReacAnnexingPDF(mc, "Pickup");
            }
        }
    }
}
