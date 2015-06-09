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
        Func<HierarchicalEntity, string> viewFunc = f => f.Identity.ToString();

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
                var result = ExecuteExpression(test.Input).DescendantsOfAll();

                var convertToToken = new HierarchicalEntityToToken(viewFunc, TokenizeType.Normal);
                //var test2 = new TokenOrganizer().Organize(convertToToken.Convert(root));

                test.OutputNormal = (new TokenToExpression(viewFunc, test.IgnoreSubTokens(), delimiterMainTokens)).Convert(convertToToken.Convert(result));

                var convertToToken2 = new HierarchicalEntityToToken(viewFunc, TokenizeType.AwaysRepeatDefinedToken);
                test.OutputAwaysRepeatDefinedToken = (new TokenToExpression(viewFunc, test.IgnoreSubTokens(), delimiterMainTokens)).Convert(convertToToken2.Convert(result));

                var convertToToken3 = new HierarchicalEntityToToken(viewFunc, TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed);
                test.OutputNeverRepeatDefinedTokenIfAlreadyParsed = (new TokenToExpression(viewFunc, test.IgnoreSubTokens(), delimiterMainTokens)).Convert(convertToToken3.Convert(result));

                test.OutputHierarchyNormal = new TokenToHierarchy(viewFunc, test.IgnoreSubTokens()).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.Normal).Convert(result));
                test.OutputHierarchyAwaysRepeatDefinedToken = new TokenToHierarchy(viewFunc, test.IgnoreSubTokens()).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.AwaysRepeatDefinedToken).Convert(result));
                test.OutputHierarchyNeverRepeatDefinedTokenIfAlreadyParsed = (new TokenToHierarchy(viewFunc, test.IgnoreSubTokens())).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(result));

                test.OutputHierarchyInverseNormal = new TokenToHierarchyInverse(viewFunc).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.Normal).Convert(result));
                test.OutputHierarchyInverseAwaysRepeatDefinedToken = new TokenToHierarchyInverse(viewFunc).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.AwaysRepeatDefinedToken).Convert(result));
                test.OutputHierarchyInverseNeverRepeatDefinedTokenIfAlreadyParsed = new TokenToHierarchyInverse(viewFunc).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(result));

                test.OutputDebugNormal = new TokenToDebug(viewFunc, test.IgnoreSubTokens()).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.Normal).Convert(result));
                test.OutputDebugAwaysRepeatDefinedToken = new TokenToDebug(viewFunc, test.IgnoreSubTokens()).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.AwaysRepeatDefinedToken).Convert(result));
                test.OutputDebugNeverRepeatDefinedTokenIfAlreadyParsed = new TokenToDebug(viewFunc, test.IgnoreSubTokens()).Convert(new HierarchicalEntityToToken(viewFunc, TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed).Convert(result));

                test.HasUpdatedByCode = 1;
            }

            database.SaveChanges();
        }

        [TestMethod]
        public void TestMultiplesUsingDataBaseWithSpliting()
        {
            var init = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
            foreach (var test in testsExpressions)
            {
                var result = ExecuteExpressionWithSpliting(test.OutputNormal).DescendantsOfAll();
                this.ValidateCenaries(test, result);
                result = ExecuteExpressionWithSpliting(test.OutputAwaysRepeatDefinedToken).DescendantsOfAll();
                this.ValidateCenaries(test, result);
                result = ExecuteExpressionWithSpliting(test.OutputNeverRepeatDefinedTokenIfAlreadyParsed).DescendantsOfAll();
                this.ValidateCenaries(test, result);
            }

            var end = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
        }

        [TestMethod]
        public void TestMultiplesUsingDataBase()
        {
            var init = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
            foreach (var test in testsExpressions)
            {
                var result = ExecuteExpression(test.Input).DescendantsOfAll();
                this.ValidateCenaries(test, result);
                this.ValidateExpressionArray(test, TokenizeType.Normal, test.OutputNormal);
                this.ValidateExpressionArray(test, TokenizeType.AwaysRepeatDefinedToken, test.OutputAwaysRepeatDefinedToken);
                this.ValidateExpressionArray(test, TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed, test.OutputNeverRepeatDefinedTokenIfAlreadyParsed);
            }

            var end = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
        }

        #region Helpers

        public ListOfHierarchicalEntity ExecuteExpression(string exp)
        {
            var writer = new ExpressionToHierarchicalEntity();
            return writer.Convert(exp);
        }

        public ListOfHierarchicalEntity ExecuteExpressionWithSpliting(string exp)
        {
            var converter = new ExpressionToHierarchicalEntity();
            exp = exp.Replace(delimiterMainTokens, delimiterSubTokensOfMainTokens);
            var split = exp.Split(new string[] { delimiterSubTokensOfMainTokens }, StringSplitOptions.None);
            return converter.Convert(split);
        }

        public void ValidateExpressionArray(TestExpression test, TokenizeType type, string strInputTest)
        {
            var converterToken = new HierarchicalEntityToToken(viewFunc, type);
            var converterExpression = new TokenToExpression(viewFunc, test.IgnoreSubTokens());

            var result = ExecuteExpression(test.Input).DescendantsOfAll();
            var tokensCollection = converterToken.Convert(result);
            var edoMain = result.GetByIdentity(test.ObjectMain);
            var expressionOutput = converterExpression.Convert(tokensCollection[edoMain][edoMain]);

            var result2 = ExecuteExpressionWithSpliting(strInputTest).DescendantsOfAll();
            var edoMain2 = result2.GetByIdentity(test.ObjectMain);
            var tokensCollection2 = converterToken.Convert(result2);
            var expressionOutput2 = converterExpression.Convert(tokensCollection2[edoMain2].MainTokens);
            Assert.IsTrue(expressionOutput == expressionOutput2, "Testing creation by array of string :" + test.Description);
        }

        public void ValidateCenaries(TestExpression test, ListOfHierarchicalEntity result)
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

            var converterNormal = new HierarchicalEntityToToken(viewFunc, TokenizeType.Normal);
            var tokensNormal = converterNormal.Convert(result);
            var normal = (new TokenToExpression(viewFunc, test.IgnoreSubTokens(), delimiterMainTokens)).Convert(tokensNormal);

            var converterAwaysRepeat = new HierarchicalEntityToToken(viewFunc, TokenizeType.AwaysRepeatDefinedToken);
            var tokensAwaysRepeat = converterAwaysRepeat.Convert(result);
            var awaysRepeat = (new TokenToExpression(viewFunc, test.IgnoreSubTokens(), delimiterMainTokens)).Convert(tokensAwaysRepeat);

            var converterNeverRepeat = new HierarchicalEntityToToken(viewFunc, TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed);
            var tokensNeverRepeat = converterNeverRepeat.Convert(result);
            var neverRepeat = (new TokenToExpression(viewFunc, test.IgnoreSubTokens(), delimiterMainTokens)).Convert(tokensNeverRepeat);

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
            var converter = new TokenToHierarchy(viewFunc, test.IgnoreSubTokens());
            var output2 = converter.Convert(collection);
            Assert.IsTrue(output == output2, "Test hierarchy: compare output ´- " + test.Description);
        }

        private void ValidateHierarchyInverse(TestExpression test, TokenGroupCollection collection, string output)
        {
            var converter = new TokenToHierarchyInverse(viewFunc, delimiterMainTokens);
            var output2 = converter.Convert(collection);
            Assert.IsTrue(output == output2, "Test hierarchy inverse: compare output ´- " + test.Description);
        }

        private void ValidateDebug(TestExpression test, TokenGroupCollection collection, string output)
        {
            var delimiterDebugRef = "*******";
            var convert = new TokenToDebug(viewFunc, test.IgnoreSubTokens(), delimiterMainTokens, delimiterDebugRef);
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