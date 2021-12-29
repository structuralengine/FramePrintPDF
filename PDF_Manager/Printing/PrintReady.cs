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

        public ArrayList ready(PdfDoc mc, Dictionary<string, object> data)
        {
            // データをまとめてここに代入する．
            ArrayList dataAll = new ArrayList();

            // nodeを処理するクラスを呼び出す
            InputNode node_call = new InputNode();
            //nodeデータの整理
            dataAll.Add(node_call.node(data));

            // memberを処理するクラスを呼び出す
            InputMember member_call = new InputMember();
            // memberデータの整理
            dataAll.Add(member_call.member(data));

            // elementを処理するクラスを呼び出す
            InputElement element_call = new InputElement();
            // memberデータの整理
            List<List<string[]>> x;
            List<string> y;
            (x,y)=(element_call.element(data));
            dataAll.Add(x);
            dataAll.Add(y);

            return dataAll;
        }

    }
}