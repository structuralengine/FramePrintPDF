using Newtonsoft.Json.Linq;
using PDF_Manager.Printing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PDF_Manager.Comon
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
        pickup,
        disg,
        disgCombine,
        disgPickup,
        fsec,
        fsecCombine,
        fsecPickup,
        reac,
        reacCombine,
        reacPickup,
        diagramLoad,
    }


    internal class dataManager
    {
        public int dimension;
        public string language;

        public object[] Ready( Dictionary<string, object> data)
        {
            // 2次元か3次元かを記憶
            this.dimension = Int32.Parse(data["dimension"].ToString());

            // 言語を記憶
            if (data.ContainsKey("language"))
            {
                this.language = data["language"].ToString();
            }
            else
            {
                this.language = "ja";
            }

            // classをまとめてここに代入する．
            var class_set = new object[Enum.GetNames(typeof(class_name)).Length];

            // node
            InputNode node = new InputNode();
            if (data.ContainsKey("node"))
            {
                node.init(this, data);
                class_set[(int)class_name.node] = node;
            }

            // element
            InputElement element = new InputElement();
            if (data.ContainsKey("element"))
            {
                element.init(this, data);
                class_set[(int)class_name.elememt] = element;
            }

            // member
            InputMember member = new InputMember();
            if (data.ContainsKey("member"))
            {
                member.init(element, data);
                class_set[(int)class_name.member] = member;
            }

            // fixnode
            if (data.ContainsKey("fix_node"))
            {
                InputFixNode fixnode = new InputFixNode();
                fixnode.init(data);
                class_set[(int)class_name.fix_node] = fixnode;
            }

            // joint
            if (data.ContainsKey("joint"))
            {
                InputJoint joint = new InputJoint();
                joint.init(data);
                class_set[(int)class_name.joint] = joint;
            }

            // notice_points
            if (data.ContainsKey("notice_points"))
            {
                InputNoticePoints noticepoints = new InputNoticePoints();
                noticepoints.init(member, data);
                class_set[(int)class_name.notice_points] = noticepoints;
            }

            // fixmember
            if (data.ContainsKey("fix_member"))
            {
                InputFixMember fixmember = new InputFixMember();
                fixmember.init(data);
                class_set[(int)class_name.fix_member] = fixmember;
            }

            // shell
            if (data.ContainsKey("shell"))
            {
                InputShell shell = new InputShell();
                shell.init(data);
                class_set[(int)class_name.shell] = shell;
            }

            // load
            if (data.ContainsKey("load"))
            {
                //基本荷重
                InputLoadName loadname = new InputLoadName();
                loadname.init(data);
                class_set[(int)class_name.loadname] = loadname;

                //実荷重
                InputLoad load = new InputLoad();
                load.init(data);
                class_set[(int)class_name.load] = load;
            }


            // define
            if (data.ContainsKey("define"))
            {
                InputDefine define = new InputDefine();
                define.init(data);
                class_set[(int)class_name.define] = define;
            }

            // combine 
            if (data.ContainsKey("combine"))
            {
                InputCombine combine = new InputCombine();
                combine.init(data);
                class_set[(int)class_name.combine] = combine;
            }

            // pickup
            if (data.ContainsKey("pickup"))
            {
                InputPickup pickup = new InputPickup();
                pickup.init(data);
                class_set[(int)class_name.pickup] = pickup;
            }


            // disg
            ResultDisgAnnexing disgAnnexing = new ResultDisgAnnexing();
            if (data.ContainsKey("disg"))
            {
                ResultDisg disg = new ResultDisg();
                disg.init(data, disgAnnexing);
                class_set[(int)class_name.disg] = disg;
            }

            // disgcombine
            if (data.ContainsKey("disgCombine"))
            {
                disgAnnexing.init(data, "Combine");
                class_set[(int)class_name.disgCombine] = disgAnnexing;
            }

            // disgPickup
            if (data.ContainsKey("disgPickup"))
            {
                disgAnnexing.init(data, "Pickup");
                class_set[(int)class_name.disgPickup] = disgAnnexing;
            }

            // fsec
            ResultFsecAnnexing fsecAnnexing = new ResultFsecAnnexing();
            if (data.ContainsKey("fsec"))
            {
                ResultFsec fsec = new ResultFsec();
                fsec.init(data, fsecAnnexing);
                class_set[(int)class_name.fsec] = fsec;
            }

            // fseccombine
            if (data.ContainsKey("fsecCombine"))
            {
                fsecAnnexing.init( data, "Combine");
                class_set[(int)class_name.fsecCombine] = fsecAnnexing;
            }

            // fsecPickup
            if (data.ContainsKey("fsecPickup"))
            {
                fsecAnnexing.init(data, "Pickup");
                class_set[(int)class_name.fsecPickup] = fsecAnnexing;
            }


            ResultReacAnnexing reacAnnexing = new ResultReacAnnexing();
            // reac
            if (data.ContainsKey("reac"))
            {
                ResultReac reac = new ResultReac();
                reac.init(data, reacAnnexing);
                class_set[(int)class_name.reac] = reac;
            }

            // reaccombine
            if (data.ContainsKey("reacCombine"))
            {
                reacAnnexing.init(data, "Combine");
                class_set[(int)class_name.reacCombine] = reacAnnexing;
            }

            // reacPickup
            if (data.ContainsKey("reacPickup"))
            {
                reacAnnexing.init(data, "Pickup");
                class_set[(int)class_name.reacPickup] = reacAnnexing;
            }

            // 荷重図
            if (data.ContainsKey("diagramLoad"))
            {
                InputDiagramLoad diagram_load = new InputDiagramLoad();
                diagram_load.init(data);
                class_set[(int)class_name.diagramLoad] = diagram_load;
            }


            return class_set;
        }


        // データの精査と変換
        // (data,四捨五入する時の桁数,指数形式の表示など)
        static public string TypeChange(JToken data, int round = 0, string style = "none")
        {
            string newDataString = "";

            if (data == null)
            {
                newDataString = "";
            }
            // すぐにstringにする
            else if (data.Type == JTokenType.String)
            {
                if (data.Type == JTokenType.Null) data = "";
                newDataString = data.ToString();

            }
            // 四捨五入等の処理を行う
            else
            {
                double newDataDouble = dataManager.getNumeric(data);
                if (Double.IsNaN(newDataDouble))
                {
                    newDataString = "";
                }
                else if (style == "none")
                {
                    var digit = "F" + round.ToString();
                    newDataString = Double.IsNaN(Math.Round(newDataDouble, round, MidpointRounding.AwayFromZero)) ? "" : newDataDouble.ToString(digit);
                    if (StringInfo.ParseCombiningCharacters(newDataString).Length > round + 5)
                    {
                        newDataString = newDataDouble.ToString("E2", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                }
                else if (style == "E")
                {
                    newDataString = Double.IsNaN(Math.Round(newDataDouble, round, MidpointRounding.AwayFromZero)) ? "" : newDataDouble.ToString("E2", CultureInfo.CreateSpecificCulture("en-US"));
                }
            }
            return newDataString;
        }

        /// <summary>
        /// 数値に変換する
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public double getNumeric(JToken data)
        {
            double result = double.NaN;

            if (data == null) return result;
            if (data.Type == JTokenType.Null) return result;

            result = double.Parse(data.ToString());

            return result;
        }

    }
}
