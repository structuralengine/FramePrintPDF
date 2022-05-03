using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class ResultReacPickUp : ResultReacCombine
    {
        public ResultReacPickUp(PrintData pd, Dictionary<string, object> value) : base(pd, value, "reacPickUp")
        {

        }
    }
}
