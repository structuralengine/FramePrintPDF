using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class ResultFsecPickUp : ResultFsecCombine
    {
        public ResultFsecPickUp(PrintData pd, Dictionary<string, object> value) : base(pd, value, "fsecPickUp")
        {

        }
    }
}
