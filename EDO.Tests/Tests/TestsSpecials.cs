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
    public class TestsSpecials
    {
        Func<HierarchicalEntity, string> viewFunc = f => f.Identity.ToString();

        [TestMethod]
        public void TestMinus()
        {
            var type = TokenizeType.Normal;
            var convertToEdo = new ExpressionToHierarchicalEntity();
            var converterToken = new HierarchicalEntityToToken(viewFunc, type);

            var expressionInput = "A + (B + (C + D)) + (C - D)";
            var result = convertToEdo.Convert(expressionInput);
            var tokensCollection = converterToken.Convert(result);

            //Test1
            var edo = result.GetByIdentity("A");
            var expressionOutput = ExecuteExpression(edo, type);
            Assert.IsTrue(expressionOutput == "A + (B + C) + C", "Teste 1");

            //Test2
            convertToEdo.Convert(result, "B-C");
            expressionOutput = ExecuteExpression(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C", "Teste 2");

            //Test3 using EdoUtils
            //convertToEdo.Convert(root, "A+Y+Z+J; A-Y; ".Split(';'));
            Utils.FromExpression(edo.DescendantsAndSelf(), "A+Y+Z+J; A-Y; ".Split(';'));
            expressionOutput = ExecuteExpression(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C + Z + J", "Teste 3 using EdoUtils");
        }

        [TestMethod]
        public void TestUsingCSharpExpression()
        {
            var a = new HierarchicalEntity("a");
            var b = new HierarchicalEntity("b");
            var c = new HierarchicalEntity("c");
            var d = new HierarchicalEntity("d");

            a += b + c + d;
            var output = Utils.ToExpression(a, viewFunc);
            Assert.IsTrue(output == "a + (b + c + d)", "Test add with +=");

            a -= b - c - d;
            output = Utils.ToExpression(a, viewFunc);
            Assert.IsTrue(output == "a", "Test remove all with -=");

            a = a + b + c + d;
            output = Utils.ToExpression(a, viewFunc);
            Assert.IsTrue(output == "a + b + c + d", "Test add with =, is different the +=");

            b = b + c + d;
            output = Utils.ToExpression(b, viewFunc);
            Assert.IsTrue(output == "b + c + d", "Test add 2");

            b = b + c + d;
            output = Utils.ToExpression(b, viewFunc);
            Assert.IsTrue(output == "b + c + d", "Test tentative duplicate");

            b = b - c - d;
            output = Utils.ToExpression(b, viewFunc);
            Assert.IsTrue(output == "b", "Test remove all 'b'");

            b = b + c + d - d - c;
            output = Utils.ToExpression(b, viewFunc);
            Assert.IsTrue(output == "b", "Test add and remove together");
            
            a = a + (b + c + b + a) + (c + d);
            output = Utils.ToExpression(a, viewFunc);
            Assert.IsTrue(output == "a + (b + (c + d) + b + a) + c + d", "Test add complex");
            Assert.IsTrue(Utils.ToExpression(a.Descendants(), viewFunc) == "b + (c + d) + b + (a + b + c + d)\r\nc + d\r\nd\r\na + (b + (c + d) + b + a) + c + d", "Test 'Descendants'");
            Assert.IsTrue(Utils.ToExpression(a.DescendantsAndSelf(), viewFunc) == "a + (b + (c + d) + b + a) + c + d\r\nb + (c + d) + b + (a + b + c + d)\r\nc + d\r\nd", "Test 'DescendantsAndThis'");
        }

        [TestMethod]
        public void TestIdentityWithSpacesOrIntFAILURE()
        {
            // This failure because exists spaces
            var failure = Utils.FromExpression("\"p1 teste\" + \"p2 teste 2\" + \"p3 teste 4\" + \"p4 teste 5\"");

            // This failure because ncalc process by int
            var failure2 = Utils.FromExpression("1 + 2 + 3");

            // This failure because ncalc process by int too
            var failure3 = Utils.FromExpression("\"1\" + \"2\" + \"3\"");

            // This failure because ncalc lost
            var p1 = Utils.FromExpression("\"p1\" + \"p2\" + \"p3\" + \"p4\"");
        }

        [TestMethod]
        public void TestTwoRootsNotRelationalFAILURE()
        {
            var resultNatural = Utils.FromExpression("A + B", "C + D");
            var resultLogical = resultNatural.DescendantsOfAll();

            var output = Utils.ToExpression(resultNatural, viewFunc);
            Assert.IsTrue(output == "", "Test natural order");

            output = Utils.ToExpression(resultLogical, viewFunc);
            Assert.IsTrue(output == "", "Test logical order");
        }

        [TestMethod]
        public void TestNaturalAndLogicalOrder()
        {
            var resultNatural = Utils.FromExpression("A + B", "C + A", "D + E + J", "B+D");
            var resultLogical = resultNatural.DescendantsOfAll();

            var output = Utils.ToExpression(resultNatural, viewFunc);
            Assert.IsTrue(output == "A + (B + (D + E + J))\r\nB + (D + E + J)\r\nC + (A + (B + (D + E + J)))\r\nD + E + J\r\nE\r\nJ", "Test natural order");

            output = Utils.ToExpression(resultLogical, viewFunc);
            Assert.IsTrue(output == "A + (B + (D + E + J))\r\nB + (D + E + J)\r\nD + E + J\r\nE\r\nJ\r\nC + (A + (B + (D + E + J)))", "Test logical order");
        }

        [TestMethod]
        public void TestExecuteExpressionNormal()
        {
            var result = Utils.FromExpression("p1 + p2 + p3 + p4");
            var p1 = result.GetByIdentity("p1");

            var output = Utils.ToExpression(p1, viewFunc);
            Assert.IsTrue(output == "p1 + p2 + p3 + p4", "Test root");

            Utils.FromExpression(p1.DescendantsAndSelf(), "p1+(D+C)");
            output = Utils.ToExpression(p1, viewFunc);
            Assert.IsTrue(output == "p1 + p2 + p3 + p4 + (D + C)", "Test withot specify 'p1' in expression");

            Utils.FromExpression(p1.DescendantsAndSelf(), "p1+(a+b+c)");
            output = Utils.ToExpression(p1, viewFunc);
            Assert.IsTrue(output == "p1 + p2 + p3 + p4 + (D + C) + (a + b + c)", "Test specify 'p1' in expression");

            Utils.FromExpression(p1.DescendantsAndSelf(), "p1+h+j");
            output = Utils.ToExpression(p1, viewFunc);
            Assert.IsTrue(output == "p1 + p2 + p3 + p4 + (D + C) + (a + b + c) + h + j", "Test specify 'p1' in expression 2");

            Utils.FromExpression(p1.DescendantsAndSelf(), "p1+p1");
            output = Utils.ToExpression(p1, viewFunc);
            Assert.IsTrue(output == "p1 + p2 + p3 + p4 + (D + C) + (a + b + c) + h + j + p1", "Test specify 'p1' and add 'p1' again");
        }

        [TestMethod]
        public void TestExecuteExpressionNeverRepeat()
        {
            var result = Utils.FromExpression("p1 + p2 + p3 + (p4 + p5)");
            var output = Utils.ToExpression(result.GetByIdentity("p1"), viewFunc, TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed);
        }

        [TestMethod]
        public void TestViewAsHashCode()
        {
            var result = Utils.FromExpression("p1 + p2 + p3 + p4");
            var output = Utils.ToExpression(result, f => f.GetHashCode().ToString());
            foreach (var item in result)
                Assert.IsTrue(output.Contains(item.GetHashCode().ToString()), "Test hashcode with view in expression");
        }

        [TestMethod]
        public void TestIntegrityCollections()
        {
            var type = TokenizeType.Normal;
            var convertToEdo = new ExpressionToHierarchicalEntity();
            var converterToken = new HierarchicalEntityToToken(viewFunc, type);

            // Test create root by expression
            var expressionInput = "A + (B + (C + D)) + C";
            var result = convertToEdo.Convert(expressionInput);

            // Basic test object 'A'
            var edoA = result.GetByIdentity("A");            
            var edoB = result.GetByIdentity("B");
            var edoC = result.GetByIdentity("C");
            var edoD = result.GetByIdentity("D");

            var expressionOutput = ExecuteExpression(edoA, type);
            Assert.IsTrue(expressionOutput == "A + (B + (C + D)) + C", "Teste 1");
            Assert.IsTrue(Utils.ToExpression(edoB.DescendantsAndSelf(), viewFunc) == "B + (C + D)");
            Assert.IsTrue(Utils.ToExpression(edoC.DescendantsAndSelf(), viewFunc) == "C + D");
            Assert.IsTrue(Utils.ToExpression(edoD.DescendantsAndSelf(), viewFunc) == "D");

            // Test add exists object "D"
            Utils.FromExpression(edoA.DescendantsAndSelf(), "A+D; ".Split(';'));
            expressionOutput = ExecuteExpression(edoA, type).Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];
            Assert.IsTrue(expressionOutput == "A + (B + (C + D)) + C + D", "Test new root deriveted the object A");

            // Test remove 'A'
            result.Remove(edoA);
            expressionOutput = ExecuteExpression(edoA, type).Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];
            Assert.IsTrue(expressionOutput == "B + (C + D)", "Teste remove A of root");

            // Test add again
            result.Add(edoA);
            expressionOutput = ExecuteExpression(edoA, type);
            Assert.IsTrue(expressionOutput == "B + (C + D)\r\nC + D\r\nD\r\nA + (B + (C + D)) + C + D", "Teste add again");
            
            try
            {
                edoA.Add(new HierarchicalEntity("D"));
            }
            catch(EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'D' already exists directly or indirectly.", "Expected error");
            }

            var y = new HierarchicalEntity("Y");
            edoA.Add(y);
            Utils.FromExpression(y.DescendantsAndSelf(), "Y+Z+J");

            expressionInput = Utils.ToExpression(edoA.DescendantsAndSelf(), viewFunc);
            Assert.IsTrue(expressionInput == "A + (B + (C + D)) + C + D + (Y + Z + J)");

            Utils.FromExpression(edoA.DescendantsAndSelf(), "A+Z");
            expressionInput = Utils.ToExpression(edoA.DescendantsAndSelf(), viewFunc);
            Assert.IsTrue(expressionInput == "A + (B + (C + D)) + C + D + (Y + Z + J) + Z");

            // New 'J' directly o.AddReference            
            try
            {
                var j = new HierarchicalEntity("J");
                edoA.Add(j);
            }
            catch (EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'J' already exists directly or indirectly.", "Expected error 2");
            }

            // Danify collectin because is add directly in object of 3 level
            Utils.FromExpression(y.DescendantsAndSelf(), "Y+B");

            try
            {
                expressionInput = Utils.ToExpression(edoA.Descendants(), viewFunc);
            }
            catch (EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'B' already exists in the list", "Expected error 3");
            }

            try
            {
                expressionInput = Utils.ToExpression(result, viewFunc);
            }
            catch (EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'B' already exists in the list", "Expected error 4");
            }

            // Saving root
            y.Remove(y.Descendants().GetByIdentity("B"));
            expressionInput = Utils.ToExpression(result, viewFunc);
            Assert.IsTrue(expressionInput == "B + (C + D)\r\nC + D\r\nD\r\nA + (B + (C + D)) + C + D + (Y + Z + J) + Z\r\nY + Z + J\r\nZ\r\nJ", "Test");

            Utils.FromExpression(y.DescendantsAndSelf(), "Y + L");
            expressionInput = Utils.ToExpression(result, viewFunc);
            Assert.IsTrue(expressionInput == "B + (C + D)\r\nC + D\r\nD\r\nA + (B + (C + D)) + C + D + (Y + Z + J + L) + Z\r\nL\r\nY + Z + J + L\r\nZ\r\nJ", "Test");
        }

        [TestMethod]
        public void TestParseTokens()
        {
            var expressionInput = "A + (B + (C + D)) + C";
            var result = ExecuteExpression(expressionInput);
            var converterToken = new HierarchicalEntityToToken(viewFunc, TokenizeType.Normal);
            var edoTest = result.GetByIdentity("A");
            var tokenResult = converterToken.Convert(edoTest.DescendantsAndSelf());
            var tokens = tokenResult[edoTest].MainTokens.ToList();

            Assert.IsTrue(tokens[0].TokenValue.ToString() == "A");
            Assert.IsTrue(tokens[0].Level == 1);
            Assert.IsTrue(tokens[0].Parent == null);

            Assert.IsTrue(tokens[1].TokenValue.ToString() == " + ");
            Assert.IsTrue(tokens[1].Level == 2);
            Assert.IsTrue(tokens[1].Parent == tokens[0]);
            Assert.IsTrue(tokens[1].TokenValue == TokenValuePlus.Instance);

            Assert.IsTrue(tokens[2].TokenValue.ToString() == "(");
            Assert.IsTrue(tokens[2].Level == 2);
            Assert.IsTrue(tokens[2].Parent == tokens[0]);

            Assert.IsTrue(tokens[3].TokenValue.ToString() == "B");
            Assert.IsTrue(tokens[3].Level == 2);
            Assert.IsTrue(tokens[3].Parent == tokens[0]);

            Assert.IsTrue(tokens[4].TokenValue.ToString() == " + ");
            Assert.IsTrue(tokens[4].Level == 3);
            Assert.IsTrue(tokens[4].Parent == tokens[3]);
            Assert.IsTrue(tokens[4].TokenValue == TokenValuePlus.Instance);

            Assert.IsTrue(tokens[5].TokenValue.ToString() == "(");
            Assert.IsTrue(tokens[5].Level == 3);
            Assert.IsTrue(tokens[5].Parent == tokens[3]);
            Assert.IsTrue(tokens[5].TokenValue == TokenValueOpenParenthesis.Instance);

            Assert.IsTrue(tokens[6].TokenValue.ToString() == "C");
            Assert.IsTrue(tokens[6].Level == 3);
            Assert.IsTrue(tokens[6].Parent == tokens[3]);

            Assert.IsTrue(tokens[7].TokenValue.ToString() == " + ");
            Assert.IsTrue(tokens[7].Level == 4);
            Assert.IsTrue(tokens[7].Parent == tokens[6]);
            Assert.IsTrue(tokens[7].TokenValue == TokenValuePlus.Instance);

            Assert.IsTrue(tokens[8].TokenValue.ToString() == "D");
            Assert.IsTrue(tokens[8].Level == 4);
            Assert.IsTrue(tokens[8].Parent == tokens[6]);

            Assert.IsTrue(tokens[9].TokenValue.ToString() == ")");
            Assert.IsTrue(tokens[9].Level == 3);
            Assert.IsTrue(tokens[9].Parent == tokens[3]);
            Assert.IsTrue(tokens[9].TokenValue == TokenValueCloseParenthesis.Instance);

            Assert.IsTrue(tokens[10].TokenValue.ToString() == ")");
            Assert.IsTrue(tokens[10].Level == 2);
            Assert.IsTrue(tokens[10].Parent == tokens[0]);
            Assert.IsTrue(tokens[10].TokenValue == TokenValueCloseParenthesis.Instance);

            Assert.IsTrue(tokens[11].TokenValue.ToString() == " + ");
            Assert.IsTrue(tokens[11].Level == 2);
            Assert.IsTrue(tokens[11].Parent == tokens[0]);
            Assert.IsTrue(tokens[11].TokenValue == TokenValuePlus.Instance);

            Assert.IsTrue(tokens[12].TokenValue.ToString() == "C");
            Assert.IsTrue(tokens[12].Level == 2);
            Assert.IsTrue(tokens[12].Parent == tokens[0]);
            Assert.IsTrue(tokens[12].TokenValue == tokens[6].TokenValue);
        }

        [TestMethod]
        public void TestUniqueExpression()
        {
            var expressionInput = "A + (B + (C + D)) + C";
            var result = ExecuteExpression(expressionInput);
            var converterToken = new HierarchicalEntityToToken(viewFunc, TokenizeType.Normal);

            // Tanto faz se ira ignorar ou não os subtokens, pois abaixo trabalha diretament com os tokens
            var converterExpression = new TokenToExpression(viewFunc);
            var tokensCollection = converterToken.Convert(result);

            var edoMain = result.GetByIdentity("A");
            var expressionOutput = converterExpression.Convert(tokensCollection[edoMain].MainTokens);
            Assert.IsTrue(expressionInput == expressionOutput);

            // Other method override and the same instances
            tokensCollection = converterToken.Convert(edoMain.DescendantsAndSelf());

            var expressionOutput2 = converterExpression.Convert(tokensCollection[edoMain].MainTokens);
            Assert.IsTrue(expressionInput == expressionOutput2);
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