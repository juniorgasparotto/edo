using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO.Converter
{
    public class ConverterFromExpressionToEDObject : IConverterFromExpression
    {
        public void Convert(string expression, EDObjectCollection collectionToAppy)
        {
            if (collectionToAppy == null)
                throw new Exception("Parameter 'collection' can't be null'");
            
            var e = this.PrepareExpression(expression);
            
            e.EvaluateParameter += delegate(string name, ParameterArgs args)
            {
                // FIX to back params name
                name = name.Replace('_', '.');
                var objectAdd = collectionToAppy.GetObjectByName(name);
                if (objectAdd == null)
                {
                    objectAdd = new EDObject(name);
                    collectionToAppy.Add(objectAdd);
                }

                var param = new ObjectParamExpression(objectAdd);
                args.Result = param;
            };

            e.Evaluate();
        }

        public void Convert(string expression, EDObject edoObjectToApply)
        {
            if (edoObjectToApply == null)
                throw new Exception("Parameter 'edoObject' can't be null'");

            var e = this.PrepareExpression(expression);
            
            e.EvaluateParameter += delegate(string name, ParameterArgs args)
            {
                // FIX to back params name
                name = name.Replace('_', '.');
                var objectAdd = edoObjectToApply.References.FirstOrDefault(f=>f.Name == name);
                if (objectAdd == null)
                {
                    objectAdd = new EDObject(name);
                    edoObjectToApply.References.Add(objectAdd);
                }

                var param = new ObjectParamExpression(objectAdd);
                args.Result = param;
            };

            e.Evaluate();
        }

        public Expression PrepareExpression(string expression)
        {
            // FIX to resolve params with name contains ".", ex: "Namespace.ObjectName"
            expression = expression.Replace('.', '_');
            var e = new Expression(expression, EvaluateOptions.NoCache);

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                var value = args.Parameters[0].Evaluate();
                args.Result = value;
            };

            return e;
        }
    }
}
