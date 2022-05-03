using System;
using System.Collections.Generic;
using System.Text;

namespace PDF_Manager.Printing
{
    class ResultReacPickUp : ResultReacCombine
    {
        public new const string KEY = "reacPickUp";

        public ResultReacPickUp(Dictionary<string, object> value) : base(value, ResultReacPickUp.KEY)
        {

        }
    }
}
