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
        public EDObjectCollection Convert(params string[] expressions)
        {
            var collection = new EDObjectCollection();
            this.Convert(collection, expressions);
            return collection;
        }

        public void Convert(EDObjectCollection collectionToAppy, params string[] expressions)
        {
            if (collectionToAppy == null)
                throw new Exception("Parameter 'collectionToAppy' can't be null'");

            expressions = expressions.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();

            this.ApplyInList(collectionToAppy, expressions);
        }

        //public void Convert(EDObject edoToApply, params string[] expressions)
        //{
        //    if (edoToApply == null)
        //        throw new Exception("Parameter 'edoToApply' can't be null'");

        //    expressions = expressions.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();

        //    // Prepare current items
        //    var list = new List<EDObject>();
        //    list.AddRange(edoToApply.References.Traverse(f => f.References));

        //    // Prevent recursion
        //    if (!list.Contains(edoToApply))
        //        list.Add(edoToApply);

        //    this.ApplyInList(list, expressions);
        //}

        private void ApplyInList(IEnumerable<EDObject> list, string[] expressions)
        {
            foreach (var expression in expressions)
            {
                var e = this.Prepare(expression);

                e.EvaluateParameter += delegate(string name, ParameterArgs args)
                {
                    // FIX to back params name
                    name = name.Replace('_', '.');
                    var objectAdd = list.FirstOrDefault(f => f.Name == name);
                    if (objectAdd == null)
                    {
                        objectAdd = new EDObject(name);
                        ((ICollection<EDObject>)list).Add(objectAdd);
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
