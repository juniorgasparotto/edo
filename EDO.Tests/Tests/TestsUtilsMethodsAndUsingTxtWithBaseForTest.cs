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
    public class TestsUtilsMethodsAndUsingTxtWithBaseForTest
    {
        private List<TestExpression> testsExpressions;
        private DatabaseTests database;

        [TestInitialize]
        public void Setup()
        {
            DatabaseTests.SetDataDirectory();
            this.database = new DatabaseTests();
            this.testsExpressions = database.TestExpressions.ToList();
        }
        
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
        public void TestParseTokens()
        {
            var expressionInput = "A + (B + (C + D)) + C";
            var collection = CreateObjectCollection(expressionInput);
            var converterToken = new EdoObjectToToken(TokenizeType.Normal);
            var edoTest = collection.GetObjectByName("A");
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

            var edoMain = collection.GetObjectByName("A");
            var expressionOutput = converterExpression.Convert(tokensCollection[edoMain].MainTokens);
            Assert.IsTrue(expressionInput == expressionOutput);

            // Other method override and the same instances
            tokensCollection = converterToken.Convert(edoMain);

            var expressionOutput2 = converterExpression.Convert(tokensCollection[edoMain].MainTokens);
            Assert.IsTrue(expressionInput == expressionOutput2);
        }

        
        [TestMethod]
        public void TestMultiplesExpressionsUsingTexts()
        {
            this.UpdateTextsByCode();
        }

        #region Helpers

        public void UpdateTextsByCode()
        {
            foreach (var test in this.testsExpressions)
            {
                var types = Enum.GetValues(typeof(TokenizeType));
                foreach (var typeObj in types)
                {
                    var type = (TokenizeType)typeObj;
                    var strBuilder = new StringBuilder();
                    var expressionInput = test.Input;
                    var main = test.ObjectMain;
                    var collection = EdoUtils.ToEdoObjectCollection(expressionInput);
                    var edo = EdoUtils.ToEdoObject(expressionInput);
                    var expressionOutputEdo = EdoUtils.ToExpression(edo, type);
                    var hierarchyOutputEdo = EdoUtils.ToHierarchy(edo, type);
                    var hierarchyInverseOutputEdo = EdoUtils.ToHierarchyInverse(edo, type);
                    var debugOutputEdo = EdoUtils.ToDebug(edo, type);
                    var expressionOutputCollection = EdoUtils.ToExpression(collection, type);
                    var hierarchyOutputCollection = EdoUtils.ToHierarchy(collection, type);
                    var hierarchyInverseOutputCollection = EdoUtils.ToHierarchyInverse(collection, type);
                    var debugOutputCollection = EdoUtils.ToDebug(collection, type);

                    strBuilder.AppendLine("-> Input");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(expressionInput, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("-> Description");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(test.Description, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("-> Print object '" + main + "'");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("Expression:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(expressionOutputEdo, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("Hierarchy:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(hierarchyOutputEdo, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("Hierarchy inverse:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(hierarchyInverseOutputEdo, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("Debug:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(debugOutputEdo, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("-> Print all objects:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("Expression:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(expressionOutputCollection, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("Hierarchy:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(hierarchyOutputCollection, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("Hierarchy Inverse:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(hierarchyInverseOutputCollection, 3));
                    strBuilder.AppendLine();
                    strBuilder.AppendLine("Debug:");
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(Indent(debugOutputCollection, 3));
                    strBuilder.AppendLine();

                    string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string path = (System.IO.Path.GetDirectoryName(executable));
                    path = Path.GetFullPath(Path.Combine(path, "../../"));

                    var nameFile = "normal";
                    if (type == TokenizeType.AwaysRepeatDefinedToken)
                        nameFile = "aways-repeat";
                    else if (type == TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed)
                        nameFile = "never-repeat";

                    var directory = path + string.Format("Tests/Output/{0}", test.Id.ToString());
                    var filename = directory + string.Format("/{0}.txt", nameFile);

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);

                        System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
                        file.WriteLine(strBuilder.ToString());
                        file.Close();
                    }
                    else
                    {
                        var contentExists = File.ReadAllText(filename);
                        contentExists = Regex.Replace(contentExists, @"(Debug\:.*?)(->)", "$2", RegexOptions.Singleline);
                        contentExists = Regex.Replace(contentExists, @"(Debug\:.*?)($)", "$2", RegexOptions.Singleline);

                        var compare = strBuilder.ToString();
                        compare = Regex.Replace(compare, @"(Debug\:.*?)(->)", "$2", RegexOptions.Singleline);
                        compare = Regex.Replace(compare, @"(Debug\:.*?)($)", "$2", RegexOptions.Singleline);
                        Assert.IsTrue(contentExists == compare, test.Description);
                    }
                }
            }
        }

        public string Indent(string str, int count)
        {
            var builder = new StringBuilder();
            var split = str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var i = 0;
            foreach (var s in split)
            {
                if (i++ == split.Length - 1)
                    builder.Append("".PadLeft(count) + s);
                else
                    builder.AppendLine("".PadLeft(count) + s);
            }

            return builder.ToString();
        }

        #endregion
    }
}