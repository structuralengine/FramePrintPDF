using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing.PrintResult
{
    internal class ResulDisgPickUp : ResultDisgCombine
    {
        public ResulDisgPickUp(PrintData pd, Dictionary<string, object> value) : base(pd, value, "disgPickUp")
        {

        }
    }
}
