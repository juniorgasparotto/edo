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
        public HierarchicalEntity EdoObject { get; private set; }
        public Dictionary<HierarchicalEntity, List<Token>> Tokens { get; private set; }
        public TokenResult(HierarchicalEntity edoObject, Dictionary<HierarchicalEntity, List<Token>> tokens)
        {
            this.EdoObject = edoObject;
            this.Tokens = tokens;
        }
    }
}
