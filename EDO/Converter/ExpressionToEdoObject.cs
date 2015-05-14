using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO.Converter
{
    public class ExpressionToEdoObject : IConverterFromExpression
    {
        public HorizontalCollection Convert(params string[] expressions)
        {
            var collection = new HorizontalCollection();
            this.Convert(collection, expressions);
            return collection;
        }

        public void Convert(HorizontalCollection collectionToAppy, params string[] expressions)
        {
            if (collectionToAppy == null)
                throw new Exception("Parameter 'collectionToAppy' can't be null'");

            expressions = expressions.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();

            this.ApplyInList(collectionToAppy, expressions);
        }

        public HorizontalCollection Convert(HierarchicalEntity edoToApply, params string[] expressions)
        {
            if (edoToApply == null)
                throw new Exception("Parameter 'edoToApply' can't be null'");

            expressions = expressions.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
            var ret = edoToApply.ToHorizontalCollection();
            this.ApplyInList(ret, expressions);
            return ret;
        }

        private void ApplyInList(HorizontalCollection collection, string[] expressions)
        {
            foreach (var expression in expressions)
            {
                var e = this.Prepare(expression);

                e.EvaluateParameter += delegate(string name, ParameterArgs args)
                {
                    // FIX to back params name
                    name = name.Replace('_', '.');
                    var objectAdd = collection.FirstOrDefault(f => f.Name == name);
                    if (objectAdd == null)
                    {
                        objectAdd = new HierarchicalEntity(name);
                        collection.Add(objectAdd);
                    }

                    var param = new ObjectParamExpression(objectAdd);
                    args.Result = param;
                };

                e.Evaluate();
            }
        }

        public Expression Prepare(string expression)
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
