using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EDO.Converter;
using EDO.Tests;
using System.Text;

namespace EDO.Unit
{
    [TestClass]
    public class TestsInverse
    {
        Func<HierarchicalEntity, string> viewFunc = f => f.Identity.ToString();

        [TestMethod]
        public void TestTraverse()
        {
            var list = Utils.FromExpression("A+(B+(C+D))");
            var allItemsFromA = list[0].DescendantsAndSelf();
            var res = allItemsFromA.Traverse(f => f.Children, RecursiveExtensions.SelectAllWithChildren).ToListOfHierarchicalEntity();
            Assert.IsTrue(res[0].Identity.Equals("A"));
            Assert.IsTrue(res[1].Identity.Equals("B"));
            Assert.IsTrue(res[2].Identity.Equals("C"));

            res = allItemsFromA.Traverse(f => f.Children, RecursiveExtensions.SelectAllWithoutChildren).ToListOfHierarchicalEntity();
            Assert.IsTrue(res[0].Identity.Equals("D"));

            res = list[0].DescendantsAndSelf().Traverse(f => f.Children, RecursiveExtensions.SelectAllOrphans).ToListOfHierarchicalEntity();
            Assert.IsTrue(res[0].Identity.Equals("A"));
        }

        [TestMethod]
        public void TestPaths()
        {
            var list = Utils.FromExpression("A+(B+C+D)+A+(I+(J+C)+A)");
            //list = Utils.FromExpression("A+B+C+D+(K+E)", "E+J+I+P", "M+N", "C+K+E");
            //list = Utils.FromExpression("A+B", "B+C+I", "B+I+(J+I)", "K+I");
            list = Utils.FromExpression("A + (B + (C + D))", "D + (C + (B + A))");
            list = Utils.FromExpression("B + (C + (D + C) + B) + (A + B)");
            list = Utils.FromExpression("A + (B + (C + D)) + (I+B) + (J+B)");
            list = Utils.FromExpression("A + (I+B) + (B + (C + D)) + (J+B)");

            var a = Utils.ToExpression(list, viewFunc);
            //var list2 = new List<HierarchicalEntity>();
            //list2.Add(list.GetByIdentity("B"));
            //list2.Add(list.GetByIdentity("C"));
            //list2.Add(list.GetByIdentity("N"));
            //list2.Add(list.GetByIdentity("M"));
            //list2.Add(list.GetByIdentity("D"));
            //list2.Add(list.GetByIdentity("P"));
            //list2.Add(list.GetByIdentity("I"));
            //list2.Add(list.GetByIdentity("J"));
            //list2.Add(list.GetByIdentity("E"));
            //list2.Add(list.GetByIdentity("A"));
            var list2 = new List<HierarchicalEntity>();
            list2.Add(list.GetByIdentity("J"));
            list2.Add(list.GetByIdentity("B"));
            list2.Add(list.GetByIdentity("I"));
            list2.Add(list.GetByIdentity("A"));
            list2.Add(list.GetByIdentity("C"));
            list2.Add(list.GetByIdentity("D"));

            var ret = "";
            var ret2 = "";

            var branches = list2.ToPaths(f => f.Children).ToList();
            var teste = branches.FirstOrDefault().Root.Get().ToList();
            var roots = branches.RemoveCoexistent().ToList();
            var entities = roots.Roots().ToList();
            var group1 = branches.Group1();

            foreach (var item in branches)
            {
                ret += item.Debug();
                ret += "\r\n";
            }

            foreach (var item in roots)
            {
                ret2 += item.Debug();
                ret2 += "\r\n";
            }
        }

        [TestMethod]
        public void TestInverse1()
        {
            var list = Utils.FromExpression("L");
            list = Utils.FromExpression(list, "A+(L+A)");
            list = Utils.FromExpression(list, "B+(J+L)");
            list = Utils.FromExpression(list, "C+(D+A)");
            list = Utils.FromExpression(list, "D");
            list = Utils.FromExpression(list, "I+B");
            list = Utils.FromExpression(list, "O+(K+I)");
            list = Utils.FromExpression(list, "K+B");
            var output = Utils.ToExpression(list, viewFunc);
            var roots = list.Traverse(f => f.Parents).ToList();

            list = Utils.FromExpression("A+(B+(C+D))", "I+B");
            output = Utils.ToExpression(list, viewFunc);

            var parentsList = new ListOfHierarchicalEntity();
            var childrenList = new ListOfHierarchicalEntity();
            var all = new ListOfHierarchicalEntity();
            parentsList = list[0].DescendantsAndSelf().Traverse(f => f.Children, RecursiveExtensions.SelectAllWithChildren).ToListOfHierarchicalEntity();
            parentsList = list[0].DescendantsAndSelf().Traverse(f => f.Children, RecursiveExtensions.SelectAllWithoutChildren).ToListOfHierarchicalEntity();
            parentsList = list[0].DescendantsAndSelf().Traverse(f => f.Children, RecursiveExtensions.SelectAllOrphans).ToListOfHierarchicalEntity();
            parentsList = list.Traverse(f => f.Parents, RecursiveExtensions.SelectAllWithChildren).ToListOfHierarchicalEntity();

            list = Utils.FromExpression("a + (b+(d+a)) + (c+(f+d)) + (h+g)");

            roots = list.Traverse(f => f.Parents, RecursiveExtensions.SelectAllOrphans).ToListOfHierarchicalEntity();
            var a = list.GetByIdentity("a");
            var aH = Utils.ToHierarchy(a, viewFunc);
            var aHI = Utils.ToHierarchyInverse(list, viewFunc);
            var parentsA = a.Ancestors();
        }


        #region Helpers

        public ListOfHierarchicalEntity ExecuteExpression(string exp)
        {
            var writer = new ExpressionToHierarchicalEntity();
            return writer.Convert(exp);
        }

        public string ExecuteExpression(HierarchicalEntity edo, TokenizeType type)
        {
            var converterToken = new HierarchicalEntityToToken(viewFunc, type);
            var tokens = converterToken.Convert(edo.DescendantsAndSelf());
            var converterExpression = new TokenToExpression(viewFunc);
            return converterExpression.Convert(tokens[edo].MainTokens);
        }

        #endregion
    }
}