using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class ResultReacPickUp : ResultReacCombine
    {
        public new const string KEY = "reacPickUp";

        public ResultReacPickUp(PrintData pd, Dictionary<string, object> value) : base(pd, value, ResultReacPickUp.KEY)
        {

        }
    }
}
