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
            var expressionOutput = GetExpression(edo, type);
            Assert.IsTrue(expressionOutput == "A + (B + C) + C", "Teste 1");

            //Test2
            convertToEdo.Convert(collection, "B-C");
            expressionOutput = GetExpression(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C", "Teste 2");

            //Test3 using EdoUtils
            //convertToEdo.Convert(collection, "A+Y+Z+J; A-Y; ".Split(';'));
            EdoUtils.ApplyExpression(edo, "A+Y+Z+J; A-Y; ".Split(';'));
            expressionOutput = GetExpression(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C + Z + J", "Teste 3 using EdoUtils");
        }

        [TestMethod]
        public void TestIntegrityCollections()
        {
            var type = TokenizeType.Normal;
            var convertToEdo = new ExpressionToEdoObject();
            var converterToken = new EdoObjectToToken(type);

            // Test create collection by expression
            var expressionInput = "A + (B + (C + D)) + C";
            var collection = convertToEdo.Convert(expressionInput);

            // Basic test object 'A'
            var edoA = collection.Contains("A");            
            var edoB = collection.Contains("B");
            var edoC = collection.Contains("C");
            var edoD = collection.Contains("D");

            var expressionOutput = GetExpression(edoA, type);
            Assert.IsTrue(expressionOutput == "A + (B + (C + D)) + C", "Teste 1");
            Assert.IsTrue(EdoUtils.ToExpression(edoB) == "B + (C + D)");
            Assert.IsTrue(EdoUtils.ToExpression(edoC) == "C + D");
            Assert.IsTrue(EdoUtils.ToExpression(edoD) == "D");

            // Test new collection and add exists object "D"
            var newCollection = EdoUtils.ApplyExpression(edoA, "A+D; ".Split(';'));
            expressionOutput = GetExpression(newCollection, type).Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];
            Assert.IsTrue(expressionOutput == "A + (B + (C + D)) + C + D", "Test new collection deriveted the object A");

            // Test remove 'A'
            collection.Remove(edoA);
            expressionOutput = GetExpression(collection, type).Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];
            Assert.IsTrue(expressionOutput == "B + (C + D)", "Teste remove collection");

            // Test add again
            collection.Add(edoA);
            expressionOutput = GetExpression(collection, type);
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
            EdoUtils.ApplyExpression(y, "Y+Z+J");

            expressionInput = EdoUtils.ToExpression(edoA);
            Assert.IsTrue(expressionInput == "A + (B + (C + D)) + C + D + (Y + Z + J)");

            EdoUtils.ApplyExpression(edoA, "A+Z");
            expressionInput = EdoUtils.ToExpression(edoA);
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
            EdoUtils.ApplyExpression(y, "Y+B");

            try
            {
                expressionInput = EdoUtils.ToExpression(edoA);
            }
            catch (EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'B' already exists in collection", "Expected error 3");
            }

            try
            {
                expressionInput = EdoUtils.ToExpression(collection);
            }
            catch (EntityAlreadyExistsException ex)
            {
                Assert.IsTrue(ex.Message == "Object 'B' already exists in collection", "Expected error 4");
            }
            
            try
            {
                var a = collection.FirstOrDefault(f=>f.Name == "B");
            }
            catch (InvalidatedCollectionException ex)
            {
                Assert.IsTrue(ex.Message == "The collection is invalidated. Please corrected it or create another.", "Expected error 5");
            }

            // Saving collection
            y.Remove(y.FindHierarchically("B"));
            expressionInput = EdoUtils.ToExpression(collection);
            Assert.IsTrue(expressionInput == "B + (C + D)\r\nC + D\r\nD\r\nA + (B + (C + D)) + C + D + (Y + Z + J) + Z\r\nY + Z + J\r\nZ\r\nJ", "Test");
            
            EdoUtils.ApplyExpression(y, "Y + L");
            collection.Refresh();
            expressionInput = EdoUtils.ToExpression(collection);
            Assert.IsTrue(expressionInput == "B + (C + D)\r\nC + D\r\nD\r\nA + (B + (C + D)) + C + D + (Y + Z + J + L) + Z\r\nL\r\nY + Z + J + L\r\nZ\r\nJ", "Test");
            
            try
            {
                collection.Remove(edoB);
            }
            catch (RemoveDeniedException ex)
            {
                Assert.IsTrue(ex.Message == "Unable to remove the object 'B' because it is being used by the object 'A' directly or indirectly.", "Expected error 6");
            }
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

        public HorizontalCollection CreateObjectCollection(string exp)
        {
            var collection = new HorizontalCollection();
            var writer = new ExpressionToEdoObject();
            writer.Convert(collection, exp);
            return collection;
        }

        public string GetExpression(HierarchicalEntity edo, TokenizeType type)
        {
            var converterToken = new EdoObjectToToken(type);
            var tokens = converterToken.Convert(edo);
            var converterExpression = new TokenToExpression();
            return converterExpression.Convert(tokens[edo].MainTokens);
        }

        public string GetExpression(HorizontalCollection collection, TokenizeType type)
        {
            var converterToken = new EdoObjectToToken(type);
            var tokens = converterToken.Convert(collection);
            var converterExpression = new TokenToExpression();
            return converterExpression.Convert(tokens);
        }

        #endregion
    }
}