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
    public class TestAuto1
    {
        private List<TestExpression> testsExpressions;
        private DatabaseTests database;
        private string delimiterMainTokens = "\r\n-----\r\n";
        private string delimiterSubTokensOfMainTokens = "\r\n";

        [TestInitialize]
        public void Setup()
        {
            DatabaseTests.SetDataDirectory();
            this.database = new DatabaseTests();
            this.UpdateDataBaseByCode();
            this.testsExpressions = database.TestExpressions.ToList();
        }

        public void UpdateDataBaseByCode()
        {
            // update only lines with dosen't was updated and homologated
            var tests = database.TestExpressions.Where(f => f.HasUpdatedByCode != 1).ToList();
            foreach (var test in tests)
            {
                var collection = CreateObjectCollection(test.Input);

                var convertToToken = new EdoObjectToToken(TokenizeType.Normal);
                //var test2 = new TokenOrganizer().Organize(convertToToken.Convert(collection));

                test.OutputNormal = (new TokenToExpression(test.IgnoreSubTokens(), delimiterMainTokens)).Convert(convertToToken.Convert(collection));

                var convertToToken2 = new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken);
                test.OutputAwaysRepeatDefinedToken = (new TokenToExpression(test.IgnoreSubTokens(), delimiterMainTokens)).Convert(convertToToken2.Convert(collection));

                var convertToToken3 = new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed);
                test.OutputNeverRepeatDefinedTokenIfAlreadyParsed = (new TokenToExpression(test.IgnoreSubTokens(), delimiterMainTokens)).Convert(convertToToken3.Convert(collection));

                test.OutputHierarchyNormal = new TokenToHierarchy(test.IgnoreSubTokens()).Convert(new EdoObjectToToken(TokenizeType.Normal).Convert(collection));
                test.OutputHierarchyAwaysRepeatDefinedToken = new TokenToHierarchy(test.IgnoreSubTokens()).Convert(new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken).Convert(collection));
                test.OutputHierarchyNeverRepeatDefinedTokenIfAlreadyParsed = (new TokenToHierarchy(test.IgnoreSubTokens())).Convert(new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(collection));

                test.OutputHierarchyInverseNormal = new TokenToHierarchyInverse().Convert(new EdoObjectToToken(TokenizeType.Normal).Convert(collection));
                test.OutputHierarchyInverseAwaysRepeatDefinedToken = new TokenToHierarchyInverse().Convert(new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken).Convert(collection));
                test.OutputHierarchyInverseNeverRepeatDefinedTokenIfAlreadyParsed = new TokenToHierarchyInverse().Convert(new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(collection));

                test.OutputDebugNormal = new TokenToDebug(test.IgnoreSubTokens()).Convert(new EdoObjectToToken(TokenizeType.Normal).Convert(collection));
                test.OutputDebugAwaysRepeatDefinedToken = new TokenToDebug(test.IgnoreSubTokens()).Convert(new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken).Convert(collection));
                test.OutputDebugNeverRepeatDefinedTokenIfAlreadyParsed = new TokenToDebug(test.IgnoreSubTokens()).Convert(new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(collection));

                test.HasUpdatedByCode = 1;
            }

            database.SaveChanges();
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
        public void TestMultiplesUsingDataBase()
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

        public HorizontalCollection CreateObjectCollection(string exp)
        {
            var collection = new HorizontalCollection();
            var writer = new ExpressionToEdoObject();
            writer.Convert(collection, exp);
            return collection;
        }

        public HorizontalCollection CreateObjectCollectionSpliting(string exp)
        {
            var converter = new ExpressionToEdoObject();
            exp = exp.Replace(delimiterMainTokens, delimiterSubTokensOfMainTokens);
            var split = exp.Split(new string[] { delimiterSubTokensOfMainTokens }, StringSplitOptions.None);
            return converter.Convert(split);
        }

        public void ValidateExpressionArray(TestExpression test, TokenizeType type, string strInputTest)
        {
            var converterToken = new EdoObjectToToken(type);
            var converterExpression = new TokenToExpression(test.IgnoreSubTokens());

            var collection = CreateObjectCollection(test.Input);
            var tokensCollection = converterToken.Convert(collection);
            var edoMain = collection.Contains(test.ObjectMain);
            var expressionOutput = converterExpression.Convert(tokensCollection[edoMain][edoMain]);

            var collection2 = CreateObjectCollectionSpliting(strInputTest);
            var edoMain2 = collection2.Contains(test.ObjectMain);
            var tokensCollection2 = converterToken.Convert(collection2);
            var expressionOutput2 = converterExpression.Convert(tokensCollection2[edoMain2].MainTokens);
            Assert.IsTrue(expressionOutput == expressionOutput2, "Testing creation by array of string :" + test.Description);
        }

        public void ValidateCenaries(TestExpression test, HorizontalCollection collection)
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
            var normal = (new TokenToExpression(test.IgnoreSubTokens(), delimiterMainTokens)).Convert(tokensNormal);
            
            var converterAwaysRepeat = new EdoObjectToToken(TokenizeType.AwaysRepeatDefinedToken);
            var tokensAwaysRepeat = converterAwaysRepeat.Convert(collection);
            var awaysRepeat = (new TokenToExpression(test.IgnoreSubTokens(), delimiterMainTokens)).Convert(tokensAwaysRepeat);

            var converterNeverRepeat = new EdoObjectToToken(TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed);
            var tokensNeverRepeat = converterNeverRepeat.Convert(collection);
            var neverRepeat = (new TokenToExpression(test.IgnoreSubTokens(), delimiterMainTokens)).Convert(tokensNeverRepeat);

            Assert.IsTrue(test.OutputNormal == normal, "Test expression: compare output normal - " + test.Description);
            this.ValidateDebug(test, tokensNormal, normal);
            this.ValidateTokens(test, tokensNormal, normal);

            Assert.IsTrue(test.OutputAwaysRepeatDefinedToken == awaysRepeat, "Test expression: compare output aways repeat - " + test.Description);
            this.ValidateDebug(test, tokensAwaysRepeat, awaysRepeat);
            this.ValidateTokens(test, tokensAwaysRepeat, awaysRepeat);
            
            Assert.IsTrue(test.OutputNeverRepeatDefinedTokenIfAlreadyParsed == neverRepeat, "Test expression: compare output never repeat - " + test.Description);
            this.ValidateDebug(test, tokensNeverRepeat, neverRepeat);
            this.ValidateTokens(test, tokensNeverRepeat, neverRepeat);

            // Test hierarchy 
            this.ValidateHierarchy(test, tokensNormal, test.OutputHierarchyNormal);
            this.ValidateHierarchy(test, tokensAwaysRepeat, test.OutputHierarchyAwaysRepeatDefinedToken);
            this.ValidateHierarchy(test, tokensNeverRepeat, test.OutputHierarchyNeverRepeatDefinedTokenIfAlreadyParsed);
            this.ValidateHierarchyInverse(test, tokensNormal, test.OutputHierarchyInverseNormal);
            this.ValidateHierarchyInverse(test, tokensAwaysRepeat, test.OutputHierarchyInverseAwaysRepeatDefinedToken);
            this.ValidateHierarchyInverse(test, tokensNeverRepeat, test.OutputHierarchyInverseNeverRepeatDefinedTokenIfAlreadyParsed);
        }

        private void ValidateHierarchy(TestExpression test, TokenGroupCollection collection, string output)
        {
            var converter = new TokenToHierarchy(test.IgnoreSubTokens());
            var output2 = converter.Convert(collection);
            Assert.IsTrue(output == output2, "Test hierarchy: compare output ´- " + test.Description);
        }

        private void ValidateHierarchyInverse(TestExpression test, TokenGroupCollection collection, string output)
        {
            var converter = new TokenToHierarchyInverse(delimiterMainTokens);
            var output2 = converter.Convert(collection);
            Assert.IsTrue(output == output2, "Test hierarchy inverse: compare output ´- " + test.Description);
        }

        private void ValidateDebug(TestExpression test, TokenGroupCollection collection, string output)
        {
            var delimiterDebugRef = "*******";
            var convert = new TokenToDebug(test.IgnoreSubTokens(), delimiterMainTokens, delimiterDebugRef);
            var debug = convert.Convert(collection);

            var outputSplitCollection = output.Split(new string[] { delimiterMainTokens }, StringSplitOptions.None);
            var debugSplitCollection = debug.Split(new string[] { delimiterMainTokens }, StringSplitOptions.None);
            for (var i = 0; i < outputSplitCollection.Length; i++)
            {
                var outputSplitRefs = outputSplitCollection[i].Split(new string[] { delimiterSubTokensOfMainTokens }, StringSplitOptions.None);
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

        private void ValidateTokens(TestExpression test, TokenGroupCollection collection, string output)
        {
            var outputSplitCollection = output.Split(new string[] { delimiterMainTokens }, StringSplitOptions.None);
            for (var i = 0; i < outputSplitCollection.Length; i++)
            {
                var outputSplitRefs = outputSplitCollection[i].Split(new string[] { delimiterSubTokensOfMainTokens }, StringSplitOptions.None);
                for (var iRef = 0; iRef < outputSplitRefs.Length; iRef++)
                {
                    var regEx = @"[a-zA-Z\._]+|\(|\)|\+|\-";
                    var tokens = collection.ElementAt(i).ElementAt(iRef);

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