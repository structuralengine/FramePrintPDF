using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    internal class ResultDisgPickUp : ResultDisgCombine
    {
        public ResultDisgPickUp(PrintData pd, Dictionary<string, object> value) : base(pd, value, "disgPickUp")
        {

        }
    }
}
