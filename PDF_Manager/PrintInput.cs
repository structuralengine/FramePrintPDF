﻿using Newtonsoft.Json.Linq;
using PDF_Manager;
using PDF_Manager.Printing;
using System;
using System.Collections.Generic;

public class PrintInput
{
    private PdfDoc mc;
    private PrintData red;
    //private int bottomCell = 88;

    public PrintInput(string jsonString)
    {
        // データを読み込む
        JObject data = (JObject.Parse(jsonString));
        var value = data.ToObject<Dictionary<string, object>>(); // JObject.FromObject(data).ToObject<Dictionary<string, object>>();
        //　準備のためのclassの呼び出し
        this.red = new PrintData(value);
    }

    /// <summary>
    /// インプットデータの印刷PDFを生成する
    /// </summary>
    public void createPDF()
    {
        //  PDF出力のためのclassの呼び出し
        //  整形したデータを送る
        this.Export(this.red);

        // PDFファイルを生成する
        mc.SavePDF();
    }

    /// <summary>
    /// PDFのBase64コードを返す
    /// </summary>
    /// <returns></returns>
    public string getPdfSource()
    {
        //  PDF出力のためのclassの呼び出し
        //  整形したデータを送る
        this.Export(this.red);

        // PDF を Byte型に変換
        var b = mc.GetPDFBytes();

        // Byte型配列をBase64文字列に変換
        string str = Convert.ToBase64String(b);

        // PDFファイルを生成する
        return str;
    }

    /// <summary>
    /// PDF を生成する
    /// </summary>
    /// <param name="red"></param>
    private void Export(PrintData red)
    {
        //　jsonから取り出した生データを送る
        object[] class_set = red.Ready(this.value);

        // PDF ページを準備する
        var mc = new PdfDoc();

        for (int i = 0; i < class_set.Length; i++)
        {
            if (class_set[i] != null)
            {
                mc.name = Enum.GetNames(typeof(class_name))[i];
                break;
            }
        }


        // 荷重図
        if (class_set[(int)class_name.diagramLoad] != null)
        {
            InputDiagramLoad cls_diagram = (InputDiagramLoad)class_set[(int)class_name.diagramLoad];
            cls_diagram.DiagramOfLoadPDF(mc, class_set);


            return; // 荷重図の指定があったらその他の出力はしない
        }

        // node
        if ((InputNode)class_set[(int)class_name.node] != null)
        {
            InputNode cls_node = (InputNode)class_set[(int)class_name.node];
            cls_node.NodePDF(mc);
        }

        // member
        if ((InputMember)class_set[(int)class_name.member] != null)
        {
            InputMember cls_member = (InputMember)class_set[(int)class_name.member];
            cls_member.MemberPDF(mc);
        }

        // element
        if ((InputElement)class_set[(int)class_name.elememt] != null)
        {
            InputElement cls_element = (InputElement)class_set[(int)class_name.elememt];
            cls_element.ElementPDF(mc);
        }

        // fixnode
        if ((InputFixNode)class_set[(int)class_name.fix_node] != null)
        {
            InputFixNode cls_fixnode = (InputFixNode)class_set[(int)class_name.fix_node];
            cls_fixnode.FixNodePDF(mc);
        }

        // joint
        if ((InputJoint)class_set[(int)class_name.joint] != null)
        {
            InputJoint cls_joint = (InputJoint)class_set[(int)class_name.joint];
            cls_joint.JointPDF(mc);
        }

        // noticepoints
        if ((InputNoticePoints)class_set[(int)class_name.notice_points] != null)
        {
            InputNoticePoints cls_noticepoints = (InputNoticePoints)class_set[(int)class_name.notice_points];
            cls_noticepoints.NoticePointsPDF(mc);
        }

        // fixmember
        if ((InputFixMember)class_set[(int)class_name.fix_member] != null)
        {
            InputFixMember cls_fixmember = (InputFixMember)class_set[(int)class_name.fix_member];
            cls_fixmember.FixMemberPDF(mc);
        }

        // shell
        if ((InputShell)class_set[(int)class_name.shell] != null)
        {
            InputShell cls_shell = (InputShell)class_set[(int)class_name.shell];
            cls_shell.ShellPDF(mc);
        }

        //loadname
        if ((InputLoadName)class_set[(int)class_name.loadname] != null)
        {
            InputLoadName cls_loadname = (InputLoadName)class_set[(int)class_name.loadname];
            cls_loadname.LoadNamePDF(mc);
        }

        //load
        if ((InputLoad)class_set[(int)class_name.load] != null)
        {
            InputLoad cls_load = (InputLoad)class_set[(int)class_name.load];
            cls_load.LoadPDF(mc);
        }


        //define
        if ((InputDefine)class_set[(int)class_name.define] != null)
        {
            InputDefine cls_define = (InputDefine)class_set[(int)class_name.define];
            cls_define.DefinePDF(mc);
        }

        //combine
        if ((InputCombine)class_set[(int)class_name.combine] != null)
        {
            InputCombine cls_combine = (InputCombine)class_set[(int)class_name.combine];
            cls_combine.CombinePDF(mc);
        }

        //pickup
        if ((InputPickup)class_set[(int)class_name.pickup] != null)
        {
            InputPickup cls_pickup = (InputPickup)class_set[(int)class_name.pickup];
            cls_pickup.PickupPDF(mc);
        }

        //disg
        if ((ResultDisg)class_set[(int)class_name.disg] != null)
        {
            ResultDisg cls_disg = (ResultDisg)class_set[(int)class_name.disg];
            cls_disg.DisgPDF(mc);
        }

        //disgCombine
        if ((ResultDisgAnnexing)class_set[(int)class_name.disgCombine] != null)
        {
            ResultDisgAnnexing cls_disgAnnexing = (ResultDisgAnnexing)class_set[(int)class_name.disgCombine];
            cls_disgAnnexing.DisgAnnexingPDF(mc, "Combine");
        }

        //disgPickup
        if ((ResultDisgAnnexing)class_set[(int)class_name.disgPickup] != null)
        {
            ResultDisgAnnexing cls_disgAnnexing = (ResultDisgAnnexing)class_set[(int)class_name.disgPickup];
            cls_disgAnnexing.DisgAnnexingPDF(mc, "Pickup");
        }

        //fsec
        if ((ResultFsec)class_set[(int)class_name.fsec] != null)
        {
            ResultFsec cls_fsec = (ResultFsec)class_set[(int)class_name.fsec];
            cls_fsec.FsecPDF(mc);
        }

        //fsecCombine
        if ((ResultFsecAnnexing)class_set[(int)class_name.fsecCombine] != null)
        {
            ResultFsecAnnexing cls_fsecAnnexing = (ResultFsecAnnexing)class_set[(int)class_name.fsecCombine];
            cls_fsecAnnexing.FsecAnnexingPDF(mc, "Combine");
        }

        //fsecPickup
        if ((ResultFsecAnnexing)class_set[(int)class_name.fsecPickup] != null)
        {
            ResultFsecAnnexing cls_fsecAnnexing = (ResultFsecAnnexing)class_set[(int)class_name.fsecPickup];
            cls_fsecAnnexing.FsecAnnexingPDF(mc, "Pickup");
        }

        //reac
        if ((ResultReac)class_set[(int)class_name.reac] != null)
        {
            ResultReac cls_reac = (ResultReac)class_set[(int)class_name.reac];
            cls_reac.ReacPDF(mc);
        }

        //reacCombine
        if ((ResultReacAnnexing)class_set[(int)class_name.reacCombine] != null)
        {
            ResultReacAnnexing cls_reacAnnexing = (ResultReacAnnexing)class_set[(int)class_name.reacCombine];
            cls_reacAnnexing.ReacAnnexingPDF(mc, "Combine");
        }

        //reacPickup
        if ((ResultReacAnnexing)class_set[(int)class_name.reacPickup] != null)
        {
            ResultReacAnnexing cls_reacAnnexing = (ResultReacAnnexing)class_set[(int)class_name.reacPickup];
            cls_reacAnnexing.ReacAnnexingPDF(mc, "Pickup");
        }
    }


}



