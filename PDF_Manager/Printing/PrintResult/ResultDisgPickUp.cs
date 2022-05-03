using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    internal class ResultDisgPickUp : ResultDisgCombine
    {
        public new const string KEY = "disgPickUp";

        public ResultDisgPickUp(PrintData pd, Dictionary<string, object> value) : base(pd, value, ResultDisgPickUp.KEY)
        {

        }
    }
}
