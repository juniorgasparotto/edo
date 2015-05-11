﻿using System;
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
    public class TestAuto2
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
        
        [TestMethod]
        public void TestMultiplesUsingTexts()
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

                    // Create output if not exists
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);

                        System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
                        file.WriteLine(strBuilder.ToString());
                        file.Close();
                    }
                    // Case output exists (already homologated in the past) else
                    // compare with the output generated in runtime (above). Remove DEBUG section,
                    // in compare, because the DEBUG converter use a hashcode number.
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

        #region Helpers

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