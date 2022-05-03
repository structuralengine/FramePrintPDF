using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class ResultFsecPickUp : ResultFsecCombine
    {
        public new const string KEY = "fsecPickUp";

        public ResultFsecPickUp(Dictionary<string, object> value) : base(value, ResultFsecCombine.KEY)
        {

        }
    }
}
