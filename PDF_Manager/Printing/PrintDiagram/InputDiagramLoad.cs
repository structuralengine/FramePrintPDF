using Newtonsoft.Json.Linq;
using PDF_Manager.Printing.PrintDiagram;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class InputDiagramLoad
    {
        public const string KEY = "diagramLoad";

        public InputDiagramLoad(PrintData pd, Dictionary<string, object> value) 
        {
            if (!value.ContainsKey(KEY))
                return;


            //荷重図の設定データを取得する
            var target = JObject.FromObject(value[KEY]).ToObject<Dictionary<string, object>>();

            // ペーパサイズ
            string pageSize = target.ContainsKey("pageSize") ? (string)target["pageSize"] : "A4";

            // ペーパ向き
            string pageOrientation = target.ContainsKey("pageOrientation") ? (string)target["pageOrientation"] : "Vertical";   // Horizontal

            // 軸線スケール
            double scaleX = target.ContainsKey("scaleX") ? (double)target["scaleX"] : double.NaN;
            double scaleY = target.ContainsKey("scaleY") ? (double)target["scaleY"] : double.NaN;

            // 位置補正
            double posX = target.ContainsKey("scaleX") ? (double)target["scaleX"] : double.NaN;
            double posY = target.ContainsKey("scaleY") ? (double)target["scaleY"] : double.NaN;

            // 文字サイズ
            double fontSize = target.ContainsKey("fontSize") ? (double)target["fontSize"] : 9;


            #region
            /* 
            public void K荷重図INIT_Frame_23(ByRef fname1 As String, ByRef fname2 As String, Optional fKozo As Integer)
            //
            var MaxI                    As Integer
            var fNum                    As Integer
            var str                     As String
            var Pos                     As Integer
            var i                       As Long
            var j                       As Long
            double var RRR
            double var OT1
            double var OT2
            double var OT3
            double var OT4
            double var OT5
            double var OT6
            double var OT7
            double var OT8
            double var OT9
            double var OT10
            var TEMP1
            //    var NAME1
            //    var OF1
            //
            NAME1 = "plot2"
            OF1 = strTmpDir & "fmplot1.plt"
            strUN = ""
            FLG記号 = 1 //////////////FLG記号=0:記号なし
            if ( FLG記号 = 1 ) {
                if ( fMainForm.SICheck = True ) { //SI単位チェック
                    strUN = "kN"
                } else {
                    strUN = "tf"
                }
            }
            TEMP1 = DXFParameterString(1)
            if ( TEMP1 != "" ) { //////////////////////////////////////////////////////////////////////////////////// nagase
                DXFPRINTMODE = val(Mid(TEMP1, 4, Len(TEMP1) - 4 + 1))
                CreateDxfFlag = DXFPRINTMODE
                TEMP1 = DXFParameterString(2)
                DXFDIRECTORY = Mid(TEMP1, 4, Len(TEMP1) - 4 + 1)
                if ( Mid(DXFDIRECTORY, Len(DXFDIRECTORY), 1) != "\" ) {
                   DXFDIRECTORY = DXFDIRECTORY & "\"
                }
                TEMP1 = DXFParameterString(3)
                DXFRETSU = Mid(TEMP1, 4, Len(TEMP1) - 4 + 1) //////////////////列数
                TEMP1 = DXFParameterString(4)
                DXFDANSUU = Mid(TEMP1, 4, Len(TEMP1) - 4 + 1)
                TEMP1 = DXFParameterString(5)
                DXFMODE = Mid(TEMP1, 4, Len(TEMP1) - 4 + 1)
                TEMP1 = DXFParameterString(6)
                CADFLAG1 = Mid(TEMP1, 4, Len(TEMP1) - 4 + 1)
                if ( DXFRETSU > 0 $$ DXFRETSU< 11 ) {
                } else {
                    DXFRETSU = 1
                }
                if ( DXFDANSUU > 0 $$ DXFDANSUU < 11 ) {
                } else {
                    DXFDANSUU = 1
                }
                if ( DXFDANSUU = 1 $$ DXFRETSU = 1 ) {
                    DXFMODE = 0
                } else {
                    if ( (DXFDANSUU = 2 $$ DXFRETSU = 1) Or(DXFDANSUU = 1 $$ DXFRETSU = 2) ) {
                       DXFMODE = 0
                    } else {
                        DXFMODE = 1
                    }
                }
                this.FlagParameterFILEXXX3(1, OT1, OT2, OT3, OT4, OT5, OT6, OT7, OT8, OT9, OT10, RRR)
                OndoHenkaHyoujiFlag = OT1
                OndoHenkaHyoujiYajirusiFlag = OT2
                NoBuzaiKajuuJuuhukuFlag = OT3
                if ( OndoHenkaHyoujiFlag = 1 ) {
                    OndoHenkaHyoujiYajirusiFlag = 0
                }
            } ////////////////////////////////////////////////////////////////////////////////////////////////////// nagase


            this.FlagParameterFILEXXX2(1, OT1, OT2, OT3, OT4, OT5, OT6, OT7, OT8, OT9, OT10, RRR)
            SuiheiBaneEqual = OT1
            π = 3.141592
            Twip = 56.7 //csngLogic1mmByTwips

            iKozo = fKozo
            //////    iKozo = 1 //高架橋
            //////    iKozo = 2 //橋 脚
            //////    iKozo = 3 //カルバートボックス
            //////    iKozo = 4 //カルバートボックス
            g連続Mod = False
            if ( iKozo = 4 ) {
                g連続Mod = True ////////////////////連続梁  06/01/29
                iKozo = 3
            }
            if ( iKozo = 2 ) {
                FR橋脚 = 2
                RR橋脚 = 2
            } else {
                FR橋脚 = 1
                RR橋脚 = 1
            }
            gOut部材番号 = True
            // リスト以外のControlへのロード
            fNum = FreeFile
            if ( Dir(fname1) = "" ) {
                return;    //   *** return;
            }
            Open fname1 For( Input As #fNum
            Line Input #fNum, str
            if ( CInt(str) = 0 ) {
                Close
                return;    //   *** return;
            }
            // 作図ケース
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            For( i = 0 To 0
                Pos = InStr(str, vbTab)
                //骨組図チェックのデータが存在しないときは自動的に０にする
                if ( Pos = 0 ) {
                    荷重図フォーマット.int作図指定(0) = CInt(str)
                    str = ""
                } else {
                    荷重図フォーマット.int作図指定(0) = CInt(Left(str, Pos - 1))
                }
                str = Mid(str, Pos + 1)
            }
            // ペーパサイズ
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            荷重図フォーマット.int用紙サイズ = CInt(str)
            DXFPaperSize = CInt(str)
            // 書式１
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            荷重図フォーマット.int用紙方向 = CInt(str)
            // 書式２
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            荷重図フォーマット.intレイアウト = CInt(Int(val(str))) + 1 //////////////09/11/30
            if ( 荷重図フォーマット.intレイアウト = 1 ) { ////////////////////////////////09/11/30
                シングルCASE = 0
            } else {
                シングルCASE = (val(str) - Int(val(str))) * 10 ////////// 05/03/25
            }

            // 骨組図
            // 軸線スケール
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            //自動ならcsngNullが入る
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng骨組軸線スケール縦 = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng骨組軸線スケール縦 = csngNull
            }
            if ( 荷重図フォーマット.sng骨組軸線スケール縦 = 0 ) {
                荷重図フォーマット.sng骨組軸線スケール縦 = csngNull
            }
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng骨組軸線スケール横 = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng骨組軸線スケール横 = csngNull
            }
            if ( 荷重図フォーマット.sng骨組軸線スケール横 = 0 ) {
                荷重図フォーマット.sng骨組軸線スケール横 = csngNull
            }
            // 位置補正
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng骨組位置補正X = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng骨組位置補正X = csngNull
            }
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng骨組位置補正Y = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng骨組位置補正Y = csngNull
            }
            //--------------------------------------------
            // 荷重図
            // 軸線スケール
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            //自動ならcsngNullが入る
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重軸線スケール縦 = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重軸線スケール縦 = csngNull
            }
            if ( 荷重図フォーマット.sng荷重軸線スケール縦 = 0 ) {
                荷重図フォーマット.sng荷重軸線スケール縦 = csngNull
            }
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重軸線スケール横 = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重軸線スケール横 = csngNull
            }
            if ( 荷重図フォーマット.sng荷重軸線スケール横 = 0 ) {
                荷重図フォーマット.sng荷重軸線スケール横 = csngNull
            }
            // 位置補正
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重位置補正X = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重位置補正X = csngNull
            }
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重位置補正Y = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重位置補正Y = csngNull
            }
            // sng荷重スケール
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重スケール分布荷重 = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重スケール分布荷重 = csngNull
            }
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重スケール集中荷重 = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重スケール集中荷重 = csngNull
            }
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重スケールモーメント半径 = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重スケールモーメント半径 = csngNull
            }
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重スケール節点荷重 = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重スケール節点荷重 = csngNull
            }
            str = Mid(str, Pos + 1)
            Pos = InStr(str, vbTab)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重図フォーマット.sng荷重スケール節点モーメント = CSng(Left(str, Pos - 1))
            } else {
                荷重図フォーマット.sng荷重スケール節点モーメント = csngNull
            }
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 2)
            if ( IsNumeric(Left(str, Pos - 1)) = True ) {
                荷重文字サイズ = CSng(str) * Twip / 15 //csngMmToPoint
                if ( 荷重文字サイズ = 0 ) {
                    荷重文字サイズ = 9 //////////////14/11/177
                }
            } else {
                荷重文字サイズ = 9
            }
            Line Input #fNum, str
            Pos = InStr(str, vbTab)
            str = Mid(str, Pos + 1)
            荷重図フォーマット.int文字フォント = CInt(str)
            //
            Close
            // リストへのロード(%の部分のみ）
            fNum = FreeFile
            if ( Dir(fname2) = "" ) {
                return;
            }
            Open fname2 For( Input As #fNum
            Line Input #fNum, str
            lng部材数 = CLng(str)
            this.BuzaiBai(lng部材数 + 1)
            this.ZZBuzaiBai(lng部材数 + 1)
            this.WWBuzaiBai(lng部材数 + 1)
            this.GGBuzaiBai(lng部材数 + 1) ////////////08/06/24
            this.部材縮小率(lng部材数)
            this.一期施工Flag(lng部材数 + 1)
            MaxI = 0
            if ( lng部材数 > 0 ) {
                For( i = 0 To lng部材数 - 1
                    Line Input #fNum, str
                    For( j = 0 To 4
                        Pos = InStr(str, vbTab)
                        Select Case j
                        Case 0
                            部材縮小率(i).intIdx = val(Left(str, Pos - 1))
                        Case 1
                            部材縮小率(i).int部材番号 = val(Left(str, Pos - 1))
                        Case 2
                            部材縮小率(i).str部材名 = Left(str, Pos - 1)
                        Case 3
                            部材縮小率(i).int骨組縮小率 = val(Left(str, Pos - 1))
                        Case 4
                            部材縮小率(i).int荷重縮小率 = val(Left(str, Pos - 1))
            //                    ZZBuzaiBai(i + 1) = val(Left(str, pos - 1)) / 100
                            BuzaiBai(i + 1) = 1
                            MaxI = MaxI + 1
                            if ( 部材縮小率(i).int荷重縮小率 > 10 And(部材縮小率(i).int荷重縮小率 - _
                                                Int(部材縮小率(i).int荷重縮小率 / 10) * 10) = 1 ) {
                                一期施工Flag(i + 1) = 1 //////09/03/13
                                部材縮小率(i).int荷重縮小率 = Int(部材縮小率(i).int荷重縮小率 / 10) * 10
                            } else {
                                一期施工Flag(i + 1) = 0
                            }


                        End Select
                        str = Mid(str, Pos + 1)
                    }
                }
            }
            Close
            For( i = 1 To MaxI
                if ( MaxI!= 1 ) {
                    PointChangeFlag = 1
                    Exit For
                }
            }
            //表示色 = ""
            FOLU = gstrFrmDir
            Select Case 荷重図フォーマット.int用紙サイズ //   0-A4 1-A3 2-B4 3-B5
            Case 0 //0-A4
                A4Flag = 1
                A3FLAG = 0
                Select Case 荷重図フォーマット.int用紙方向
                Case 1 //横
                    左開き = 10
                    上開き = 18   //mm
                    枠横幅 = 260
                    枠縦幅 = 176  //mm
                    図横幅 = 190
                    図縦幅 = 110  //mm
                Case } else { //縦
                    左開き = 18
                    上開き = 18 //18   //mm
                    枠横幅 = 176
                    枠縦幅 = 260  //mm
                    図横幅 = 120
                    図縦幅 = 160  //mm
                End Select
            Case 1 //0-A3
                A4Flag = 0
                A3FLAG = 1
                Select Case 荷重図フォーマット.int用紙方向
                Case 1 //横
                    左開き = 23
                    上開き = 23   //mm
                    枠横幅 = Round(265 * Sqr(2), 0)
                    枠縦幅 = Round(178 * Sqr(2), 0) //mm
                    図横幅 = Round(224 * Sqr(2), 0)
                    図縦幅 = Round(110 * Sqr(2), 0) //mm
                Case } else { //縦
                    左開き = 24
                    上開き = 23   //mm
                    枠横幅 = Round(176 * Sqr(2), 0)
                    枠縦幅 = Round(265 * Sqr(2), 0) //mm
                    図横幅 = Round(120 * Sqr(2), 0)
                    図縦幅 = Round(180 * Sqr(2), 0) //mm
                End Select
            End Select
            //    Select Case 表示色
            //    Case "黒"
                枠Pen色 = 0
                部材Pen色 = 0
                節点荷重Pen色 = 0
                部材荷重Pen色 = 0
                部材荷重寸法Pen色 = 0
                節点Pen色 = 0
            //    Case } else {
            //        枠Pen色 = 0           //黒
            //        部材Pen色 = 12        //明るい赤
            //        節点荷重Pen色 = 3     //ｼｱﾝ
            //        部材荷重Pen色 = 1     //青
            //        部材荷重寸法Pen色 = 1 //青
            //        節点Pen色 = 0         //黒
            //    End Select
            //------------------------------------------------------------------------------
            枠Pen幅 = 20
            //------------------------------------------------------------------------------
            節点番号Fontsize = 6      // ﾎﾟｲﾝﾄ
            //------------------------------------------------------------------------------
            部材番号Fontsize = 6
            部材Pen幅 = 20
            Pen幅 = 6 //軸方向分布荷重
            //------------------------------------------------------------------------------
            // 節点荷重
            //------------------------------------------------------------------------------
            節点荷重Pen幅 = 16
            節点荷重Scale = Twip / 4 //3 //5  //節点荷重Scale  Twip/n   n が大きくなると図が小さくなる
            節点荷重角度 = 10         //部材が有る場合の節点荷重角度  ﾄﾞ
            節点荷重初期値 = 8        //節点荷重図大きさの初期値  mm             = csngNull
            if ( 荷重図フォーマット.sng荷重スケール節点荷重<csngNull ) {
                節点荷重長さmax = 荷重図フォーマット.sng荷重スケール節点荷重
            } else {
                節点荷重長さmax = 12        //節点荷重図化最大長さ  mm
            }
            if ( 荷重図フォーマット.sng荷重スケール節点モーメント<csngNull ) {
                節点M荷重長さmax = 荷重図フォーマット.sng荷重スケール節点モーメント
            } else {
                if ( FR橋脚 = 1 ) {
                    節点M荷重長さmax = 4.2 - 0.4 ////////07/01/078    //節点M荷重図化最大長さ  mm
                } else {
                    節点M荷重長さmax = 4.2
                }
            }
            //------------------------------------------------------------------------------
            // 部材荷重
            //------------------------------------------------------------------------------
            部材荷重Pen幅 = 12
            部材荷重矢Pen幅 = 2
            部材荷重寸法Pen幅 = 8 //////09/09/16
            //    荷重図上盛上げ初期値 = 1.2 //mm
            if ( gbolBuZai = 0 ) {
                荷重図上盛上げ初期値 = 1.2 * 2 //mm  08/02/04
            } else {
                荷重図上盛上げ初期値 = 1.5 //mm
            }
            荷重図下盛上げ初期値 = 2.4   //3mm  08/02/04
            荷重図盛上げpit(1) = 20 //18 部材荷重     //mm
            荷重図盛上げpit(2) = 17 //16 //集中荷重
            荷重寸法線位置(1) = 荷重図盛上げpit(1) - 5   //mm
            荷重寸法線位置(2) = 荷重図盛上げpit(2) - 5   //mm
            荷重寸法補助線 = 3.5 //5                     //mm
            部材荷重矢長さmin = 0.8               //mm
            部材荷重Scale = Twip / 7 //5              //部材荷重Scale Twip/n n が大きくなると図が小さくなる
            集中荷重Scale = Twip / 7 //5              //部材荷重Scale Twip/n n が大きくなると図が小さくなる
            部材荷重初期値 = 3                    //部材荷重図大きさの初期値  mm
            集中荷重初期値 = 6                    //部材荷重図大きさの初期値  mm
            if ( FR橋脚 = 2 ) {
                荷重図盛上げpit(1) = 22 - 2 //////////////06/02/22
                荷重図盛上げpit(2) = 18
                荷重寸法線位置(1) = 荷重図盛上げpit(1) - 8 //  mm
                荷重寸法線位置(2) = 荷重図盛上げpit(2) - 6.5 //mm
                荷重文字サイズ = 9
            }
            if ( 荷重図フォーマット.sng荷重スケール分布荷重<csngNull ) {
                部材荷重長さmax = 荷重図フォーマット.sng荷重スケール分布荷重
            } else {
                部材荷重長さmax = 荷重寸法線位置(2) - 8 //6  //部材荷重図化最大長さ mm 10/10
            }
            if ( 荷重図フォーマット.sng荷重スケール集中荷重<csngNull ) {
                集中荷重長さmax = 荷重図フォーマット.sng荷重スケール集中荷重
            } else {
                集中荷重長さmax = 荷重寸法線位置(1) + 15 //5 //2  //集中荷重図化最大長さ  mm
            }
            if ( 荷重図フォーマット.sng荷重スケールモーメント半径<csngNull ) {
                部材M荷重長さmax = 荷重図フォーマット.sng荷重スケールモーメント半径
            } else {
                if ( FR橋脚 = 1 ) {
                    部材M荷重長さmax = 4 - 0.4 //4.6   //部材M荷重図化最大長さ mm
                } else {
                    部材M荷重長さmax = 4
                }
            }
            固定部材M荷重長さ = 5 - 0.4 ////////07/01/078                //固定部材M荷重図化長さ  mm
            固定部材PURE1荷重長さ = 5             //固定部材ﾌﾟﾚｽﾄﾚｽﾄ1荷重図化長さ  mm
            固定部材PURE2荷重長さ = 5             //固定部材ﾌﾟﾚｽﾄﾚｽﾄ1荷重図化長さ  mm
            //------------------------------------------------------------------------------
            荷重文字Fontsize = 8
            荷重文字pit = 2.5           //mm
            Select Case 荷重TEXT位置
            Case "左"
                荷重座標X = 25          //mm
            Case "右"
                荷重座標X = 140         //mm
            Case } else {
                荷重座標X = 0
            End Select
            荷重座標Y初期値 = 30        //mm
            //    if ( FR橋脚 = 1 ) {
            //        節点荷重Fontsize = 8 //7//////09/11/06
            //        部材荷重Fontsize = 8 //7
            //        荷重文字Fontsize = 8 //7
            //        荷重寸法Fontsize = 8 //7
            //    } else { //                    FR橋脚 = 2
            //        節点荷重Fontsize = max(荷重文字サイズ, 8.5) //8//////09/11/06
            //        部材荷重Fontsize = 8.5 //8
            //        荷重文字Fontsize = max(荷重文字サイズ, 8.5) //8
            //        荷重寸法Fontsize = 8
            //    }
            //
                節点荷重Fontsize = max((荷重文字サイズ * 0.8), 8.5) //////14/11/177
                部材荷重Fontsize = max((荷重文字サイズ * 0.8), 8.5)
                荷重文字Fontsize = max((荷重文字サイズ * 0.8), 8.5)
                荷重寸法Fontsize = max((荷重文字サイズ * 0.8), 8.5)


                節点番号Fontsize = max((荷重文字サイズ * 0.6), 6)
                部材番号Fontsize = max((荷重文字サイズ * 0.6), 6)
            //
            sYscale = 1
            mHONE = 0
            //
            }
            */
            #endregion


        }

        /*
        /// <summary>
        /// 荷重図の作成
        /// </summary>
        /// <param name="_mc">キャンパス</param>
        /// <param name="class_set">入力データ</param>
        public static void DiagramOfLoadPDF(PdfDoc _mc, object[] class_set)
        {

            // 入力データの取得
            // 節点
            this.node = (InputNode)class_set[(int)class_name.node];
            // 部材
            this.member = (InputMember)class_set[(int)class_name.member];
            // 材料
            this.element = (InputElement)class_set[(int)class_name.elememt];
            // 支点
            this.fixnode = (InputFixNode)class_set[(int)class_name.fix_node];
            // 結合
            this.joint = (InputJoint)class_set[(int)class_name.joint];
            // バネ
            this.fixmember = (InputFixMember)class_set[(int)class_name.fix_member];
            // 荷重名
            this.loadname = (InputLoadName)class_set[(int)class_name.loadname];
            // 荷重強度
            this.load = (InputLoad)class_set[(int)class_name.load];

            this.printNode();


        }

        /// <summary>
        /// 節点の印字
        /// </summary>
        private void printNode()
        {
            XPoint p = new XPoint(200,300);
            XSize z = new XSize(10, 10);

            Shape.Drawcircle(this.mc, p, z);
        }

        */
    }
}
