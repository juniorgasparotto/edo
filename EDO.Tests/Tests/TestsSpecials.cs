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
            var convertToEdo = new ExpressionToEdoObject();
            var converterToken = new EdoObjectToToken(type);

            var expressionInput = "A + (B + (C + D)) + (C - D)";
            var collection = convertToEdo.Convert(expressionInput);
            var tokensCollection = converterToken.Convert(collection);

            //Test1
            var edo = collection.Contains("A");
            var expressionOutput = GetExpressionEdoObject(edo, type);
            Assert.IsTrue(expressionOutput == "A + (B + C) + C", "Teste 1");

            //Test2
            convertToEdo.Convert(collection, "B-C");
            expressionOutput = GetExpressionEdoObject(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C", "Teste 2");

            //Test3 using EdoUtils
            //convertToEdo.Convert(collection, "A+Y+Z+J; A-Y; ".Split(';'));
            EdoUtils.ApplyExpression(collection, "A+Y+Z+J; A-Y; ".Split(';'));
            expressionOutput = GetExpressionEdoObject(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C + Z + J", "Teste 3 using EdoUtils");

            //EdoUtils.ApplyExpression(collection.GetObjectByName("J"), "J+Y+Z+J;".Split(';'));
            //expressionOutput = GetExpressionEdoObject(edo, type);
            //Assert.IsTrue(expressionOutput == "A + B + C + Z + J", "Teste 3 using EdoUtils");
        }

        [TestMethod]
        public void TestParseTokens()
        {
            var expressionInput = "A + (B + (C + D)) + C";
            var collection = CreateObjectCollection(expressionInput);
            var converterToken = new EdoObjectToToken(TokenizeType.Normal);
            var edoTest = collection.Contains("A");
            var tokenResult = converterToken.Convert(edoTest);
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
            var collection = CreateObjectCollection(expressionInput);
            var converterToken = new EdoObjectToToken(TokenizeType.Normal);

            // Tanto faz se ira ignorar ou não os subtokens, pois abaixo trabalha diretament com os tokens
            var converterExpression = new TokenToExpression();
            var tokensCollection = converterToken.Convert(collection);

            var edoMain = collection.Contains("A");
            var expressionOutput = converterExpression.Convert(tokensCollection[edoMain].MainTokens);
            Assert.IsTrue(expressionInput == expressionOutput);

            // Other method override and the same instances
            tokensCollection = converterToken.Convert(edoMain);

            var expressionOutput2 = converterExpression.Convert(tokensCollection[edoMain].MainTokens);
            Assert.IsTrue(expressionInput == expressionOutput2);
        }

        #region Helpers

        public EDObjectCollection CreateObjectCollection(string exp)
        {
            var collection = new EDObjectCollection();
            var writer = new ExpressionToEdoObject();
            writer.Convert(collection, exp);
            return collection;
        }

        public string GetExpressionEdoObject(EDObject edo, TokenizeType type)
        {
            var converterToken = new EdoObjectToToken(type);
            var tokens = converterToken.Convert(edo);
            var converterExpression = new TokenToExpression();
            return converterExpression.Convert(tokens[edo].MainTokens);
        }

        #endregion
    }
}