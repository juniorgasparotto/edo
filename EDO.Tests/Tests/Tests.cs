using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EDO.Converter;
using EDO.Tests;
using EDO.Tests.Tests;

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

        public void UpdateByCode()
        {
            var tests = database.TestExpressions.Where(f=>f.HasUpdatedByCode != 1).ToList();
            foreach (var test in tests)
            {
                var collection = CreateObjectCollection(test.Input);
                var convertToToken = new ConverterFromEDObjectToToken(TokenizeType.Normal);
                test.OutputNormal = (new ConverterFromTokenToExpression()).Convert(convertToToken.Convert(collection), delimiterCollection);

                var convertToToken2 = new ConverterFromEDObjectToToken(TokenizeType.AwaysRepeatDefinedExpression);
                test.OutputAwaysRepeatDefinedExpression = (new ConverterFromTokenToExpression()).Convert(convertToToken2.Convert(collection), delimiterCollection);

                var convertToToken3 = new ConverterFromEDObjectToToken(TokenizeType.NeverRepeatDefinedExpressionIfAlreadyParsed);
                test.OutputNeverRepeatDefinedExpressionIfAlreadyParsed = (new ConverterFromTokenToExpression()).Convert(convertToToken3.Convert(collection), delimiterCollection);
                test.HasUpdatedByCode = 1;
            }

            database.SaveChanges();
        }

        public EDObjectCollection CreateObjectCollection(string exp)
        {
            var collection = new EDObjectCollection();
            var writer = new ConverterFromExpressionToEDObject();
            writer.Convert(exp, collection);
            return collection;
        }

        [TestMethod]
        public void TestParseTokens()
        {
            var expressionInput = "A + (B + (C + D)) + C";
            var collection = CreateObjectCollection(expressionInput);
            var converterToken = new ConverterFromEDObjectToToken(TokenizeType.Normal);
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
            var converterToken = new ConverterFromEDObjectToToken(TokenizeType.Normal);
            var converterExpression = new ConverterFromTokenToExpression();
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
            }

            var end = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
        }

        public void ValidateCenaries(TestExpression test, EDObjectCollection collection)
        {
            if (string.IsNullOrWhiteSpace(test.OutputNormal))
                throw new Exception("Output is null");

            if (string.IsNullOrWhiteSpace(test.OutputNeverRepeatDefinedExpressionIfAlreadyParsed))
                throw new Exception("OutputNeverRepeatDefinedExpressionIfAlreadyParsed is null");

            if (string.IsNullOrWhiteSpace(test.OutputAwaysRepeatDefinedExpression))
                throw new Exception("OutputAwaysRepeatDefinedExpression is null");

            //output = output.Replace(" ", "");
            //output = output.Replace("+", " + ");
            //output = output.Replace("-", " - ");

            var converterNormal = new ConverterFromEDObjectToToken(TokenizeType.Normal);
            var tokensNormal = converterNormal.Convert(collection);
            var normal = (new ConverterFromTokenToExpression()).Convert(tokensNormal, delimiterCollection);

            var converterAwaysRepeat = new ConverterFromEDObjectToToken(TokenizeType.AwaysRepeatDefinedExpression);
            var tokensAwaysRepeat = converterAwaysRepeat.Convert(collection);
            var awaysRepeat = (new ConverterFromTokenToExpression()).Convert(tokensAwaysRepeat, delimiterCollection);

            var converterNeverRepeat = new ConverterFromEDObjectToToken(TokenizeType.NeverRepeatDefinedExpressionIfAlreadyParsed);
            var tokensNeverRepeat = converterNeverRepeat.Convert(collection);
            var neverRepeat = (new ConverterFromTokenToExpression()).Convert(tokensNeverRepeat, delimiterCollection);

            Assert.IsTrue(test.OutputNormal == normal, "Test expression: compare output normal - " + test.Description);
            this.ValidateDebug(test, tokensNormal, normal);
            this.ValidateTokens(test, tokensNormal, normal);

            Assert.IsTrue(test.OutputAwaysRepeatDefinedExpression == awaysRepeat, "Test expression: compare output aways repeat - " + test.Description);
            this.ValidateDebug(test, tokensAwaysRepeat, awaysRepeat);
            this.ValidateTokens(test, tokensAwaysRepeat, awaysRepeat);

            Assert.IsTrue(test.OutputNeverRepeatDefinedExpressionIfAlreadyParsed == neverRepeat, "Test expression: compare output never repeat - " + test.Description);
            this.ValidateDebug(test, tokensNeverRepeat, neverRepeat);
            this.ValidateTokens(test, tokensNeverRepeat, neverRepeat);
        }

        private void ValidateDebug(TestExpression test, Dictionary<EDObject, TokenResult> tokens, string output)
        {
            var convert = new ConverterFromTokenToDebug();
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
    }
}