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
            // nodeを処理するクラスを呼び出す
            InputNode node = new InputNode();
            // gfx登録
            node.NodePDF(mc, (List<List<string[]>>)dataset[0]);

            // memberを処理するクラスを呼び出す
            //InputMember member = new InputMember();
            // gfx登録
            //member.MemberPDF(mc, (List<string[]>)dataset[3]);

            // elementを処理するクラスを呼び出す
            //InputElement element = new InputElement();
            // gfx登録 (mc,element_tltle,element_data)
            //element.ElementPDF(mc ,(List<string>)dataset[1],(List<List<string[]>>)dataset[2]);


            return dataset;
        }
    }
}
