using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EDO.Converter;
using EDO.Tests;

namespace EDO.Unit
{
    [TestClass]
    public class Tests
    {
        private List<TestExpression> testsExpressions;
        private DatabaseTests database;
        private string delimiterCollection = "\r\n-----\r\n";
        private string delimiterReferences = "\r\n";

        [TestInitialize]
        public void Setup()
        {
            this.database = new DatabaseTests();
            this.UpdateByCode();
            this.testsExpressions = database.TestExpressions.ToList();
        }

        [TestMethod]
        public void TestHierarchy()
        {
            var type = TokenizeType.Normal;
            var expressionInput = "A + (B + (C + D)) + C";
            var collection = CreateObjectCollection(expressionInput);
            var converterToken = new EdoObjectToToken(type);
            var tokenCollection = converterToken.Convert(collection);

            var converterToDebug = new TokenToDebug();
            var debug = converterToDebug.Convert(tokenCollection, delimiterCollection);

            var converterHierarchy = new TokenToHierarchy();
            var output = converterHierarchy.Convert(tokenCollection, delimiterCollection);
        }

        [TestMethod]
        public void TestHierarchyInverse()
        {
            var type = TokenizeType.Normal;
            var expressionInput = "A + (B + C) + (D+C) + (E+C)";
            var collection = CreateObjectCollection(expressionInput);
            var converterToken = new EdoObjectToToken(type);
            var tokenCollection = converterToken.Convert(collection);

            var converterToDebug = new TokenToDebug();
            var debug = converterToDebug.Convert(tokenCollection, delimiterCollection);

            var converterHierarchyInverse = new TokenToHierarchyInverse();
            var output = converterHierarchyInverse.Convert(tokenCollection, delimiterCollection);
            var output2 = converterHierarchyInverse.Convert(tokenCollection[collection.GetObjectByName("A")], delimiterCollection);
            var output3 = converterHierarchyInverse.Convert(tokenCollection[collection.GetObjectByName("A")], collection.GetObjectByName("C"));
        }

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
            var edo = collection.GetObjectByName("A");
            var expressionOutput = GetExpressionEdoObject(edo, type);
            Assert.IsTrue(expressionOutput == "A + (B + C) + C", "Teste 1");

            //Test2
            convertToEdo.Convert(collection, "B-C");
            expressionOutput = GetExpressionEdoObject(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C", "Teste 2");

            //Test3
            convertToEdo.Convert(collection, "A+Y+Z+J; A-Y; ".Split(';'));
            expressionOutput = GetExpressionEdoObject(edo, type);
            Assert.IsTrue(expressionOutput == "A + B + C + Z + J", "Teste 3");
        }

        [TestMethod]
        public void TestMultiplesExpressionsFAILURE()
        {
            var init = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
            foreach (var test in testsExpressions)
            {
                var collection = CreateObjectCollectionSpliting(test.OutputNormal);
                this.ValidateCenaries(test, collection);
                collection = CreateObjectCollectionSpliting(test.OutputAwaysRepeatDefinedToken);
                this.ValidateCenaries(test, collection);
                collection = CreateObjectCollectionSpliting(test.OutputNeverRepeatDefinedTokenIfAlreadyParsed);
                this.ValidateCenaries(test, collection);
            }

            var end = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
        }

        [TestMethod]
        public void TestParseTokens()
        {
            var expressionInput = "A + (B + (C + D)) + C";
            var collection = CreateObjectCollection(expressionInput);
            var converterToken = new EdoObjectToToken(TokenizeType.Normal);
            var tokenResult = converterToken.Convert(collection.GetObjectByName("A"));
            var tokens = tokenResult.Tokens[tokenResult.EdoObject];

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
            var converterExpression = new TokenToExpression();
            var tokensCollection = converterToken.Convert(collection);

            var edoMain = collection.GetObjectByName("A");
            var expressionOutput = converterExpression.Convert(tokensCollection[edoMain].Tokens[edoMain]);
            Assert.IsTrue(expressionInput == expressionOutput);

            // EDObjectDirectly
            var tokensResult = converterToken.Convert(collection.GetObjectByName("A"));
            var expressionOutput2 = converterExpression.Convert(tokensResult.Tokens[tokensResult.EdoObject]);
            Assert.IsTrue(expressionInput == expressionOutput2);
        }

        [TestMethod]
        public void TestMultiplesExpressions()
        {
            var init = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
            foreach (var test in testsExpressions)
            {
                var collection = CreateObjectCollection(test.Input);
                this.ValidateCenaries(test, collection);
                this.ValidateExpressionArray(test, TokenizeType.Normal, test.OutputNormal);
                this.ValidateExpressionArray(test, TokenizeType.AwaysRepeatDefinedToken, test.OutputAwaysRepeatDefinedToken);
                this.ValidateExpressionArray(test, TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed, test.OutputNeverRepeatDefinedTokenIfAlreadyParsed);
            }

            var end = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
        }

        #region Helpers

        public void UpdateByCode()
        {
            var tests = database.TestExpressions.Where(f => f.HasUpdatedByCode != 2333331).ToList();
            foreach (var test in tests)
            {
                var collection = CreateObjectCollection(test.Input);
                var convertToToken = new EdoObjectToToken(TokenizeType.Normal);
                test.OutputNormal = (new TokenToExpression()).Convert(convertToToken.Convert(collection), delimiterCollection);

                var convertToToken2 = new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken);
                test.OutputAwaysRepeatDefinedToken = (new TokenToExpression()).Convert(convertToToken2.Convert(collection), delimiterCollection);

                var convertToToken3 = new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed);
                test.OutputNeverRepeatDefinedTokenIfAlreadyParsed = (new TokenToExpression()).Convert(convertToToken3.Convert(collection), delimiterCollection);

                test.OutputHierarchyNormal = new TokenToHierarchy().Convert(new EdoObjectToToken(TokenizeType.Normal).Convert(collection), delimiterCollection);
                test.OutputHierarchyAwaysRepeatDefinedToken = new TokenToHierarchy().Convert(new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken).Convert(collection), delimiterCollection);
                test.OutputHierarchyNeverRepeatDefinedTokenIfAlreadyParsed = (new TokenToHierarchy()).Convert(new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(collection), delimiterCollection);

                test.OutputHierarchyInverseNormal = new TokenToHierarchyInverse().Convert(new EdoObjectToToken(TokenizeType.Normal).Convert(collection), delimiterCollection);
                test.OutputHierarchyInverseAwaysRepeatDefinedToken = new TokenToHierarchyInverse().Convert(new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken).Convert(collection), delimiterCollection);
                test.OutputHierarchyInverseNeverRepeatDefinedTokenIfAlreadyParsed = new TokenToHierarchyInverse().Convert(new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(collection), delimiterCollection);

                test.OutputDebugNormal = new TokenToDebug().Convert(new EdoObjectToToken(TokenizeType.Normal).Convert(collection), delimiterCollection);
                test.OutputDebugAwaysRepeatDefinedToken = new TokenToDebug().Convert(new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken).Convert(collection), delimiterCollection);
                test.OutputDebugNeverRepeatDefinedTokenIfAlreadyParsed = new TokenToDebug().Convert(new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(collection), delimiterCollection);
                
                test.HasUpdatedByCode = 1;
            }

            database.SaveChanges();
        }

        public EDObjectCollection CreateObjectCollection(string exp)
        {
            var collection = new EDObjectCollection();
            var writer = new ExpressionToEdoObject();
            writer.Convert(collection, exp);
            return collection;
        }

        public EDObjectCollection CreateObjectCollectionSpliting(string exp)
        {
            var converter = new ExpressionToEdoObject();
            exp = exp.Replace(delimiterCollection, delimiterReferences);
            var split = exp.Split(new string[] { delimiterReferences }, StringSplitOptions.None);
            return converter.Convert(split);
        }

        public string GetExpressionEdoObject(EDObject edo, TokenizeType type)
        {
            var converterToken = new EdoObjectToToken(type);
            var tokens = converterToken.Convert(edo);
            var converterExpression = new TokenToExpression();
            return converterExpression.Convert(tokens.Tokens[edo]);
        }

        public void ValidateExpressionArray(TestExpression test, TokenizeType type, string strInputTest)
        {
            var converterToken = new EdoObjectToToken(type);
            var converterExpression = new TokenToExpression();

            var collection = CreateObjectCollection(test.Input);
            var tokensCollection = converterToken.Convert(collection);
            var edoMain = collection.GetObjectByName(test.ObjectMain);
            var expressionOutput = converterExpression.Convert(tokensCollection[edoMain].Tokens[edoMain]);

            var collection2 = CreateObjectCollectionSpliting(strInputTest);
            var edoMain2 = collection2.GetObjectByName(test.ObjectMain);
            var tokensCollection2 = converterToken.Convert(collection2);
            var expressionOutput2 = converterExpression.Convert(tokensCollection2[edoMain2].Tokens[edoMain2]);
            Assert.IsTrue(expressionOutput == expressionOutput2, "Testing creation by array of string :" + test.Description);
        }

        public void ValidateCenaries(TestExpression test, EDObjectCollection collection)
        {
            if (string.IsNullOrWhiteSpace(test.OutputNormal))
                throw new Exception("Output is null");

            if (string.IsNullOrWhiteSpace(test.OutputNeverRepeatDefinedTokenIfAlreadyParsed))
                throw new Exception("OutputNeverRepeatDefinedExpressionIfAlreadyParsed is null");

            if (string.IsNullOrWhiteSpace(test.OutputAwaysRepeatDefinedToken))
                throw new Exception("OutputAwaysRepeatDefinedExpression is null");

            //output = output.Replace(" ", "");
            //output = output.Replace("+", " + ");
            //output = output.Replace("-", " - ");

            var converterNormal = new EdoObjectToToken(TokenizeType.Normal);
            var tokensNormal = converterNormal.Convert(collection);
            var normal = (new TokenToExpression()).Convert(tokensNormal, delimiterCollection);

            var converterAwaysRepeat = new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken);
            var tokensAwaysRepeat = converterAwaysRepeat.Convert(collection);
            var awaysRepeat = (new TokenToExpression()).Convert(tokensAwaysRepeat, delimiterCollection);

            var converterNeverRepeat = new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed);
            var tokensNeverRepeat = converterNeverRepeat.Convert(collection);
            var neverRepeat = (new TokenToExpression()).Convert(tokensNeverRepeat, delimiterCollection);

            Assert.IsTrue(test.OutputNormal == normal, "Test expression: compare output normal - " + test.Description);
            this.ValidateDebug(test, tokensNormal, normal);
            this.ValidateTokens(test, tokensNormal, normal);

            Assert.IsTrue(test.OutputAwaysRepeatDefinedToken == awaysRepeat, "Test expression: compare output aways repeat - " + test.Description);
            this.ValidateDebug(test, tokensAwaysRepeat, awaysRepeat);
            this.ValidateTokens(test, tokensAwaysRepeat, awaysRepeat);

            Assert.IsTrue(test.OutputNeverRepeatDefinedTokenIfAlreadyParsed == neverRepeat, "Test expression: compare output never repeat - " + test.Description);
            this.ValidateDebug(test, tokensNeverRepeat, neverRepeat);
            this.ValidateTokens(test, tokensNeverRepeat, neverRepeat);
        }

        private void ValidateDebug(TestExpression test, Dictionary<EDObject, TokenResult> tokens, string output)
        {
            var convert = new TokenToDebug();
            var delimiterDebugRef = "*******";
            var debug = convert.Convert(tokens, delimiterCollection, delimiterDebugRef);

            var outputSplitCollection = output.Split(new string[] { delimiterCollection }, StringSplitOptions.None);
            var debugSplitCollection = debug.Split(new string[] { delimiterCollection }, StringSplitOptions.None);
            for (var i = 0; i < outputSplitCollection.Length; i++)
            {
                var outputSplitRefs = outputSplitCollection[i].Split(new string[] { delimiterReferences }, StringSplitOptions.None);
                var debugSplitRefs = debugSplitCollection[i].Split(new string[] { delimiterDebugRef }, StringSplitOptions.None);
                for (var iRef = 0; iRef < outputSplitRefs.Length; iRef++)
                {
                    var debugSplit = debugSplitRefs[iRef].Split(new char[] { '\r', '\n' });
                    debugSplit = debugSplit.Where(f => f != "").ToArray();

                    var regEx = @"[a-zA-Z\._]+|\(|\)|\+|\-";

                    // Testing debug
                    string[] outputExpectedSplit = Regex.Matches(outputSplitRefs[iRef], regEx).Cast<Match>().Select(m => m.Value).ToArray();
                    for (var iDebug = 0; iDebug < outputExpectedSplit.Length; iDebug++)
                    {
                        string[] debugSplit2 = Regex.Matches(debugSplit[iDebug], regEx).Cast<Match>().Select(m => m.Value).ToArray();
                        Assert.IsTrue(outputExpectedSplit[iDebug] == debugSplit2[0], "Test debug: compare output ´- " + test.Description);
                    }
                }
            }
        }

        private void ValidateTokens(TestExpression test, Dictionary<EDObject, TokenResult> tokensResult, string output)
        {
            var outputSplitCollection = output.Split(new string[] { delimiterCollection }, StringSplitOptions.None);
            for (var i = 0; i < outputSplitCollection.Length; i++)
            {
                var outputSplitRefs = outputSplitCollection[i].Split(new string[] { delimiterReferences }, StringSplitOptions.None);
                for (var iRef = 0; iRef < outputSplitRefs.Length; iRef++)
                {
                    var regEx = @"[a-zA-Z\._]+|\(|\)|\+|\-";
                    var tokens = tokensResult.ElementAt(i).Value.Tokens.ElementAt(iRef).Value;

                    // Testing debug
                    string[] outputExpectedSplit = Regex.Matches(outputSplitRefs[iRef], regEx).Cast<Match>().Select(m => m.Value).ToArray();
                    for (var iOut = 0; iOut < outputExpectedSplit.Length; iOut++)
                    {
                        if (tokens[iOut].TokenValue is TokenValuePlus)
                            Assert.IsTrue(tokens[iOut].TokenValue == TokenValuePlus.Instance, "Test get token: plus instance - " + test.Description);

                        if (tokens[iOut].TokenValue is TokenValueOpenParenthesis)
                            Assert.IsTrue(tokens[iOut].TokenValue == TokenValueOpenParenthesis.Instance, "Test get token: open parenthesis instance - " + test.Description);

                        if (tokens[iOut].TokenValue is TokenValueCloseParenthesis)
                            Assert.IsTrue(tokens[iOut].TokenValue == TokenValueCloseParenthesis.Instance, "Test get token: close parenthesis instance - " + test.Description);

                        // Trim() because the token plus coming with space " + "
                        Assert.IsTrue(outputExpectedSplit[iOut] == tokens[iOut].TokenValue.ToString().Trim(), "Test get token: compare ouput - " + test.Description);
                    }
                }
            }
        }

        #endregion
    }
}