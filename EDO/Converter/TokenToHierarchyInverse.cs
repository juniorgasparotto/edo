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
    public class TokenToHierarchyInverse
    {
        private string delimiterMainTokens;
        private const string noReferenceLabel = "[no reference]";

        public TokenToHierarchyInverse(string delimiterMainTokens = null)
        {
            delimiterMainTokens = string.IsNullOrEmpty(delimiterMainTokens) ? "\r\n-----\r\n" : delimiterMainTokens;
            this.delimiterMainTokens = delimiterMainTokens;
        }
        
        public string Convert(TokenGroupCollection collection, EDObject edoObject = null)
        {
            var organizedChilds = new Dictionary<EDObject, List<EDObject>>();
            foreach (var keyPair in collection)
            {
                foreach (var tokenResult in keyPair)
                    this.OrganizeChildrensAndParents(tokenResult, organizedChilds, edoObject);
            }

            return this.GetString(organizedChilds);
        }

        private void OrganizeChildrensAndParents(List<Token> tokens, Dictionary<EDObject, List<EDObject>> organizedChilds, EDObject onlyThis)
        {
            var onlyValues = tokens.Where(f => f.TokenValue.Value is EDObject).ToList();
            foreach (var token in onlyValues)
            {
                var value = (EDObject)token.TokenValue.Value;

                if (onlyThis != null && onlyThis != value)
                    continue;

                if (!organizedChilds.ContainsKey(value))
                    organizedChilds.Add(value, new List<EDObject>());

                var parent = token.Parent != null ? (EDObject)token.Parent.TokenValue.Value : null;
                if (parent != null && !organizedChilds[value].Contains(parent))
                    organizedChilds[value].Add(parent);
            }
        }

        private string GetString(Dictionary<EDObject, List<EDObject>> organizedChilds)
        {
            StringBuilder strBuilder = new StringBuilder();
            var last = organizedChilds.LastOrDefault();
            foreach (var child in organizedChilds)
            {
                if (child.Value.Count == 0)
                    strBuilder.AppendLine(noReferenceLabel);
                else
                    foreach (var parent in child.Value)
                        strBuilder.AppendLine(parent.Name);

                strBuilder.Append("..." + child.Key.Name);
                if (child.GetHashCode() != last.GetHashCode())
                    strBuilder.Append(delimiterMainTokens);
            }

            return Helper.TrimAll(strBuilder.ToString());
        }
    }
}
