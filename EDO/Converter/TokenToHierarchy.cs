using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using EDO.Converter;

namespace EDO.Converter
{
    public class TokenToHierarchy : IConverterFromTokenToString
    {
        public string Convert(List<Token> tokens)
        {
            StringBuilder strBuilder = new StringBuilder();
            var repeat = 3;
            var charRepeat = ".";
            var initLevel = tokens.FirstOrDefault().Level;

            foreach (var token in tokens)
            {
                if (token is TokenRecursive)
                {
                    strBuilder.Append(((EDObject)token.TokenValue.Value).Name);
                }
                else
                {
                    if (token.TokenValue is TokenValuePlus)
                    {
                        var spacing = String.Concat(Enumerable.Repeat(charRepeat, (token.Level - initLevel) * repeat));
                        strBuilder.AppendLine();
                        strBuilder.Append(spacing);
                    }
                    else if (token.TokenValue.Value is EDObject)
                    { 
                        strBuilder.Append(((EDObject)token.TokenValue.Value).Name);
                    }
                }
            }

            return strBuilder.ToString();
        }

        public string Convert(TokenResult tokenResult, string delimiterReferences = null)
        {
            delimiterReferences = string.IsNullOrEmpty(delimiterReferences) ? "\r\n\r\n" : delimiterReferences;
            var strBuilder = new StringBuilder();
            var last = tokenResult.Tokens.LastOrDefault();
            foreach (var parsedToken in tokenResult.Tokens) 
            { 
                strBuilder.Append(this.Convert(parsedToken.Value));
                if (parsedToken.GetHashCode() != last.GetHashCode())
                    strBuilder.Append(delimiterReferences);
            }

            return Helper.TrimAll(strBuilder.ToString());
        }

        public string Convert(Dictionary<EDObject, TokenResult> collection, string delimiterCollection = null, string delimiterReferences = null)
        {
            delimiterCollection = string.IsNullOrEmpty(delimiterCollection) ? "\r\n" : delimiterCollection;
            var strBuilder = new StringBuilder();
            var last = collection.LastOrDefault();

            foreach (var keyPair in collection)
            {
                strBuilder.Append(this.Convert(keyPair.Value, delimiterReferences));

                if (keyPair.GetHashCode() != last.GetHashCode())
                    strBuilder.Append(delimiterCollection);
            }

            return Helper.TrimAll(strBuilder.ToString());
        }
        /*

        public string ToStringHierarchy(bool showFullTree = false)
        {
            var strBuilder = new StringBuilder();
            var loadeds = new List<string>();
            ToStringHierarchy(this, strBuilder, showFullTree, loadeds);
            strBuilder.AppendLine();

            //strBuilder.AppendLine("--------");
            //strBuilder.AppendLine("Total projects: " + loadeds.Count);
            //strBuilder.AppendLine("Total projects 2: " + this.ProjectCollection.Projects.Count());

            //strBuilder.AppendLine("DEBUG");
            //foreach (var i in loadeds)
            //    strBuilder.AppendLine(i);

            //strBuilder.AppendLine("bag:DEBUG");
            //foreach (var i in this.ProjectCollection.Projects)
            //    strBuilder.AppendLine(i.Name);

            return strBuilder.ToString();
        }

        private void ToStringHierarchy(Project project, StringBuilder strBuider, bool showFullTree, List<string> loadeds, int level = 0)
        {
            level++;
            var resume = string.Format(" (count refs: {0}, count inverse refs: {1})", project.ProjectsReferences.Count, project.ProjectsParents.Count);
            strBuider.Append(project.Name + resume);

            if (project.ProjectsReferences.Count > 0)
            {
                foreach (var pRef in project.ProjectsReferences)
                {
                    strBuider.AppendLine();
                    strBuider.Append(new String('.', level * 3));
                    if (!showFullTree && loadeds.Contains(pRef.Name))
                        strBuider.Append(pRef.Name + "*");
                    else
                    {
                        ToStringHierarchy(pRef, strBuider, showFullTree, loadeds, level);
                        if (!showFullTree && !loadeds.Contains(pRef.Name))
                            loadeds.Add(pRef.Name);
                    }
                }
            }

            if (!loadeds.Contains(project.Name))
                loadeds.Add(project.Name);
        }
         * */
    }
}
