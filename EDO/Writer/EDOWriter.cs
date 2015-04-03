using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO.Writer
{
    public class EDOWriter
    {
        private EDObjectCollection ObjectCollection;
        private EDObject Object;

        public EDOWriter(EDObjectCollection collection)
        {
            this.ObjectCollection = collection;
        }

        public EDOWriter(EDObject Object)
        {
            if (Object == null)
                throw new Exception("Object 'Object' can't be null'");

            this.Object = Object;
        }

        public void Writer(string expression)
        {
            // FIX to resolve params with name contains ".", ex: "Namespace.ObjectName"
            expression = expression.Replace('.', '_');
            Expression e = new Expression(expression, EvaluateOptions.NoCache);

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                var value = args.Parameters[0].Evaluate();
                args.Result = value;
            };

            e.EvaluateParameter += delegate(string name, ParameterArgs args)
            {
                // FIX to back params name
                name = name.Replace('_', '.');
                var objectAdd = this.ObjectCollection.GetObjectByName(name);
                if (objectAdd == null)
                {
                    objectAdd = new EDObject(name);
                    this.ObjectCollection.Add(objectAdd);
                }

                var param = new ObjectParamExpression(objectAdd);
                args.Result = param;
            };

            e.Evaluate();
        }
    }
}
