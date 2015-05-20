using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO.Converter
{
    public class ExpressionToHierarchicalEntity
    {
        public HierarchicalEntity Convert(params string[] expressions)
        {
            var root = new HierarchicalEntity("root");
            this.Convert(root, expressions);
            return root;
        }

        public void Convert(HierarchicalEntity rootToAppy, params string[] expressions)
        {
            if (rootToAppy == null)
                throw new Exception("Parameter 'rootToAppy' can't be null'");

            expressions = expressions.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();

            this.ApplyInList(rootToAppy, expressions);
        }

        //public HierarchicalEntity Convert(HierarchicalEntity edoToApply, params string[] expressions)
        //{
        //    if (edoToApply == null)
        //        throw new Exception("Parameter 'edoToApply' can't be null'");

        //    expressions = expressions.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
        //    var ret = edoToApply.ToHierarchicalEntity();
        //    this.ApplyInList(ret, expressions);
        //    return ret;
        //}

        private void ApplyInList(HierarchicalEntity rootToAppy, string[] expressions)
        {
            foreach (var expression in expressions)
            {
                var e = this.Prepare(expression);

                e.EvaluateParameter += delegate(string name, ParameterArgs args)
                {
                    // FIX to back params name
                    name = name.Replace('_', '.');
                    var listToVerify = rootToAppy.DescendantsAndThis().ToList();
                    var objectAdd = listToVerify.FirstOrDefault(f => f.IdentityIsEquals(name));
                    if (objectAdd == null)
                    {
                        objectAdd = new HierarchicalEntity(name);
                        rootToAppy.AddChild(objectAdd);
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
