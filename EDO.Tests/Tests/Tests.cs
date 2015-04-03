using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using EDO.Dispatcher;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EDO.Writer;
using EDO.Reader;
using EDO.Tests;

namespace EDO.Unit
{
    [TestClass]
    public class Tests
    {
        #region Helpers
        public List<TestExpression> TestsExpressions;

        [TestInitialize]
        public void Setup()
        {
            var database = new DatabaseTests();
            TestsExpressions = database.TestExpressions.ToList();
        }

        [TestMethod]
        public EDObjectCollection CreateObjectCollection(string exp)
        {
            var collection = new EDObjectCollection();
            var writer = new EDOWriter(collection);
            writer.Writer(exp);
            return collection;
        }

        #endregion

        [TestMethod]
        public void TestMultiplesExpressions()
        {
            var init = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
            foreach (var test in TestsExpressions)
            {
                var collection = CreateObjectCollection(test.Input);
                var expReader = new EDOReader(collection.GetObjectByName(test.ObjectMain));
                this.TestCommomCenaries(test, expReader);

                expReader = new EDOReader(collection.GetObjectByName(test.ObjectMain), TypeReader.AwaysRepeatDefinedExpression);
                this.TestCommomCenaries(test, expReader);
            }
            var end = DateTime.Now.ToString("{0:MM/dd/yyy hh:mm:ss.fff}");
        }

        [TestMethod]
        public void TestParseTokens()
        {
            var expressionInput = "A + (B + (C + D)) + C";
            var collection = CreateObjectCollection(expressionInput);
            var tokens = new EDOReader(collection.GetObjectByName("A")).GetTokens().FirstOrDefault().Value;

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
        public void TestCollection()
        {
            var expressionInput = "A + C + D+ (B + (C + D)) + C";
            var collection = CreateObjectCollection(expressionInput);
            var reader = new EDOReader(collection.GetObjectByName("A"));
            var res = reader.ToExpression(true);

            var reader2 = new EDOReader(collection.GetObjectByName("A"), TypeReader.AwaysRepeatDefinedExpression);
            var res2 = reader2.ToExpression(true);

            var reader3 = new EDOReader(collection.GetObjectByName("A"), TypeReader.NeverRepeatDefinedExpressionIfAlreadyParsed);
            var res3 = reader3.ToExpression(true);

            //var reader4 = new EDOReader(collection.GetObjectByName("A"), TypeReader.NeverRepeatDefinedExpressionIfAlreadyParsed);
            //var res4 = reader4.ToExpression();
            //var tokens = new EDOReader(collection.GetObjectByName("A")).GetTokens();
        }


        /// <summary>
        /// Test if method ToExpression is returned a expected result
        /// Test if method Debug is returned a expected result
        /// Test if method GetTokens return a correct sequence and the specials tokens [+, (, )] are of the unique instance
        /// </summary>
        /// <param name="outputExpected"></param>
        /// <param name="expReader"></param>
        public void TestCommomCenaries(TestExpression test, EDOReader expReader)
        {
            var output = test.OutputAndNormal;
            if (string.IsNullOrWhiteSpace(output))
                output = test.Input;

            if (expReader.Type == TypeReader.AwaysRepeatDefinedExpression && !string.IsNullOrWhiteSpace(test.OutputAndAwaysRepeatDefinedExpression))
                output = test.OutputAndAwaysRepeatDefinedExpression;

            if (string.IsNullOrWhiteSpace(output))
                throw new Exception("Output is null");

            output = output.Replace(" ", "");
            output = output.Replace("+", " + ");
            output = output.Replace("-", " - ");

            Assert.IsTrue(output == expReader.ToExpression(), "Test expression: compare output - " + test.Description);

            var debug = expReader.Debug();
            var debugSplit = debug.Split(new char[] { '\r', '\n' });
            debugSplit = debugSplit.Where(f => f != "").ToArray();

            var regEx = @"[a-zA-Z\._]+|\(|\)|\+|\-";

            // Testing debug
            string[] outputExpectedSplit = Regex.Matches(output, regEx).Cast<Match>().Select(m => m.Value).ToArray();
            for (var i = 0; i < outputExpectedSplit.Length; i++)
            {
                string[] debugSplit2 = Regex.Matches(debugSplit[i], regEx).Cast<Match>().Select(m => m.Value).ToArray();
                Assert.IsTrue(outputExpectedSplit[i] == debugSplit2[0], "Test debug: compare output ´- " + test.Description);
            }

            var tokenParsedBag = expReader.GetTokens();
            var tokens = tokenParsedBag.FirstOrDefault().Value;

            for (var i = 0; i < outputExpectedSplit.Length; i++)
            {
                if (tokens[i].TokenValue is TokenValuePlus)
                    Assert.IsTrue(tokens[i].TokenValue == TokenValuePlus.Instance, "Test get token: plus instance - " + test.Description);

                if (tokens[i].TokenValue is TokenValueOpenParenthesis)
                    Assert.IsTrue(tokens[i].TokenValue == TokenValueOpenParenthesis.Instance, "Test get token: open parenthesis instance - " + test.Description);

                if (tokens[i].TokenValue is TokenValueCloseParenthesis)
                    Assert.IsTrue(tokens[i].TokenValue == TokenValueCloseParenthesis.Instance, "Test get token: close parenthesis instance - " + test.Description);

                // Trim() because the token plus coming with space " + "
                Assert.IsTrue(outputExpectedSplit[i] == tokens[i].TokenValue.ToString().Trim(), "Test get token: compare ouput - " + test.Description);
            }
        }
    }
}