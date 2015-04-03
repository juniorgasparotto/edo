using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace EDO.Dispatcher
{
    public class ExpressionResult
    {
        public EDObject Object { get; private set; }
        public string Value { get; private set; }
        public string ValueFactored { get; set; }
        internal ExpressionResult(EDObject Object, string valueDefault)
        {
            this.Object = Object;
            this.Value = valueDefault;
        }
    }
}
