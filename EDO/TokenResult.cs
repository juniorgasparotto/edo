using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace EDO
{
    public class TokenResult
    {
        public EDObject EdoObject { get; private set; }
        public Dictionary<EDObject, List<Token>> Tokens { get; private set; }
        public TokenResult(EDObject edoObject, Dictionary<EDObject, List<Token>> tokens)
        {
            this.EdoObject = edoObject;
            this.Tokens = tokens;
        }
    }
}
