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
        public EDObjectCollection Convert(params string[] expressions)
        {
            var collection = new EDObjectCollection();
            this.Convert(collection, expressions);
            return collection;
        }

        public void Convert(EDObjectCollection collectionToAppy, params string[] expressions)
        {
            if (collectionToAppy == null)
                throw new Exception("Parameter 'collection' can't be null'");

            expressions = expressions.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();

            foreach (var expression in expressions) 
            {
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
