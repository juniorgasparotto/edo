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
        [TestMethod]
        public void TestMinus()
        {
            var type = TokenizeType.Normal;
            var convertToEdo = new ExpressionToHierarchicalEntity();
            var converterToken = new HierarchicalEntityToToken(type);

            var expressionInput = "A + (B + (C + D)) + (C - D)";
            var root = convertToEdo.Convert(expressionInput);
            var tokensCollection = converterToken.Convert(root.Descendants());

            //Test1
            var edo = root.FindHierarchically("A");
            var expressionOutput = GetExpressionWithThisInclude(edo, type);
            Assert.IsTrue(expressionOutput == "A + (B + C) + C", "Teste 1");

            //Test2
            convertToEdo.Convert(root, "B-C");
            expressionOutput = GetExpressionWithThisInclude(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C", "Teste 2");

            //Test3 using EdoUtils
            //convertToEdo.Convert(root, "A+Y+Z+J; A-Y; ".Split(';'));
            EdoUtils.ApplyExpression(edo, "A+Y+Z+J; A-Y; ".Split(';'));
            expressionOutput = GetExpressionWithThisInclude(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C + Z + J", "Teste 3 using EdoUtils");
        }

        [TestMethod]
        public void TestIntegrityCollections()
        {
            var type = TokenizeType.Normal;
            var convertToEdo = new ExpressionToHierarchicalEntity();
            var converterToken = new HierarchicalEntityToToken(type);

            // Test create root by expression
            var expressionInput = "A + (B + (C + D)) + C";
            var root = convertToEdo.Convert(expressionInput);

            // Basic test object 'A'
            var edoA = root.FindHierarchically("A");            
            var edoB = root.FindHierarchically("B");
            var edoC = root.FindHierarchically("C");
            var edoD = root.FindHierarchically("D");

            var expressionOutput = GetExpressionWithThisInclude(edoA, type);
            Assert.IsTrue(expressionOutput == "A + (B + (C + D)) + C", "Teste 1");
            Assert.IsTrue(EdoUtils.ToExpression(edoB.DescendantsAndThis()) == "B + (C + D)");
            Assert.IsTrue(EdoUtils.ToExpression(edoC.DescendantsAndThis()) == "C + D");
            Assert.IsTrue(EdoUtils.ToExpression(edoD.DescendantsAndThis()) == "D");

            // Test add exists object "D"
            EdoUtils.ApplyExpression(edoA, "A+D; ".Split(';'));
            expressionOutput = GetExpressionWithThisInclude(edoA, type).Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];
            Assert.IsTrue(expressionOutput == "A + (B + (C + D)) + C + D", "Test new root deriveted the object A");

            // Test remove 'A'
            root.RemoveChild(edoA);
            expressionOutput = GetExpressionWithThisInclude(root, type).Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];
            Assert.IsTrue(expressionOutput == "B + (C + D)", "Teste remove A of root");

            // Test add again
            root.AddChild(edoA);
            expressionOutput = GetExpressionWithThisInclude(root, type);
            Assert.IsTrue(expressionOutput == "B + (C + D)\r\nC + D\r\nD\r\nA + (B + (C + D)) + C + D", "Teste add again");
            
            try
            {
                edoA.AddChild(new HierarchicalEntity("D"));
            }
            catch(EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'D' already exists directly or indirectly.", "Expected error");
            }

            var y = new HierarchicalEntity("Y");
            edoA.AddChild(y);
            EdoUtils.ApplyExpression(y, "Y+Z+J");

            expressionInput = EdoUtils.ToExpression(edoA.DescendantsAndThis());
            Assert.IsTrue(expressionInput == "A + (B + (C + D)) + C + D + (Y + Z + J)");

            EdoUtils.ApplyExpression(edoA, "A+Z");
            expressionInput = EdoUtils.ToExpression(edoA.DescendantsAndThis());
            Assert.IsTrue(expressionInput == "A + (B + (C + D)) + C + D + (Y + Z + J) + Z");

            // New 'J' directly o.AddReference            
            try
            {
                var j = new HierarchicalEntity("J");
                edoA.AddChild(j);
            }
            catch (EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'J' already exists directly or indirectly.", "Expected error 2");
            }

            // Danify collectin because is add directly in object of 3 level
            EdoUtils.ApplyExpression(y, "Y+B");

            try
            {
                expressionInput = EdoUtils.ToExpression(edoA.Descendants());
            }
            catch (EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'B' already exists in the list", "Expected error 3");
            }

            try
            {
                expressionInput = EdoUtils.ToExpression(root.Descendants());
            }
            catch (EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'B' already exists in the list", "Expected error 4");
            }

            // Saving root
            y.RemoveChild(y.FindHierarchically("B"));
            expressionInput = EdoUtils.ToExpression(root.Descendants());
            Assert.IsTrue(expressionInput == "B + (C + D)\r\nC + D\r\nD\r\nA + (B + (C + D)) + C + D + (Y + Z + J) + Z\r\nY + Z + J\r\nZ\r\nJ", "Test");
            
            EdoUtils.ApplyExpression(y, "Y + L");
            expressionInput = EdoUtils.ToExpression(root.Descendants());
            Assert.IsTrue(expressionInput == "B + (C + D)\r\nC + D\r\nD\r\nA + (B + (C + D)) + C + D + (Y + Z + J + L) + Z\r\nL\r\nY + Z + J + L\r\nZ\r\nJ", "Test");
        }

        [TestMethod]
        public void TestParseTokens()
        {
            var expressionInput = "A + (B + (C + D)) + C";
            var root = CreateRoot(expressionInput);
            var converterToken = new HierarchicalEntityToToken(TokenizeType.Normal);
            var edoTest = root.FindHierarchically("A");
            var tokenResult = converterToken.Convert(edoTest.DescendantsAndThis());
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
            var root = CreateRoot(expressionInput);
            var converterToken = new HierarchicalEntityToToken(TokenizeType.Normal);

            // Tanto faz se ira ignorar ou não os subtokens, pois abaixo trabalha diretament com os tokens
            var converterExpression = new TokenToExpression();
            var tokensCollection = converterToken.Convert(root.Descendants());

            var edoMain = root.FindHierarchically("A");
            var expressionOutput = converterExpression.Convert(tokensCollection[edoMain].MainTokens);
            Assert.IsTrue(expressionInput == expressionOutput);

            // Other method override and the same instances
            tokensCollection = converterToken.Convert(edoMain.DescendantsAndThis());

            var expressionOutput2 = converterExpression.Convert(tokensCollection[edoMain].MainTokens);
            Assert.IsTrue(expressionInput == expressionOutput2);
        }

        #region Helpers

        public HierarchicalEntity CreateRoot(string exp)
        {
            var root = new HierarchicalEntity("root");
            var writer = new ExpressionToHierarchicalEntity();
            writer.Convert(root, exp);
            return root;
        }

        public string GetExpressionWithThisInclude(HierarchicalEntity edo, TokenizeType type)
        {
            var converterToken = new HierarchicalEntityToToken(type);
            var tokens = converterToken.Convert(edo.DescendantsAndThis());
            var converterExpression = new TokenToExpression();
            return converterExpression.Convert(tokens[edo].MainTokens);
        }

        #endregion
    }
}