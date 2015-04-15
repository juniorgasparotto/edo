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
        public string Convert(TokenResult tokenResult, EDObject onlyThis)
        {
            if (onlyThis == null) throw new NullReferenceException("Parameter 'onlyThis' can't be null");
            return this.Convert(tokenResult, null, onlyThis);
        }

        public string Convert(Dictionary<EDObject, TokenResult> collection, EDObject onlyThis)
        {
            if (onlyThis == null) throw new NullReferenceException("Parameter 'onlyThis' can't be null");
            return this.Convert(collection, null, onlyThis);
        }

        public string Convert(TokenResult tokenResult, string delimiter)
        {
            return this.Convert(tokenResult, delimiter, null);
        }

        public string Convert(Dictionary<EDObject, TokenResult> collection, string delimiter)
        {
            return this.Convert(collection, delimiter, null);
        }

        private string Convert(TokenResult tokenResult, string delimiter, EDObject onlyThis)
        {
            var organizedChilds = new Dictionary<EDObject, List<EDObject>>();
            foreach (var tokens in tokenResult.Tokens)
                this.OrganizeChildrensAndParents(tokens.Value, organizedChilds, onlyThis);

            return this.GetString(organizedChilds, delimiter);
        }

        private string Convert(Dictionary<EDObject, TokenResult> collection, string delimiter, EDObject onlyThis)
        {
            var organizedChilds = new Dictionary<EDObject, List<EDObject>>();
            foreach (var keyPair in collection)
            {
                foreach (var tokenResult in keyPair.Value.Tokens)
                    this.OrganizeChildrensAndParents(tokenResult.Value, organizedChilds, onlyThis);
            }

            return this.GetString(organizedChilds, delimiter);
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

            //if (onlyThis != null)
            //    organizedChilds = organizedChilds.Where(f => f.Key == onlyThis).ToDictionary(k => k.Key, v => v.Value);
        }

        private string GetString(Dictionary<EDObject, List<EDObject>> organizedChilds, string delimiter)
        {
            delimiter = string.IsNullOrEmpty(delimiter) ? "\r\n" : delimiter;
            
            StringBuilder strBuilder = new StringBuilder();
            var last = organizedChilds.LastOrDefault();
            foreach (var child in organizedChilds)
            {
                if (child.Value.Count == 0)
                    strBuilder.AppendLine("[any references for this]");
                else
                    foreach (var parent in child.Value)
                        strBuilder.AppendLine(parent.Name);

                strBuilder.Append("..." + child.Key.Name);
                if (child.GetHashCode() != last.GetHashCode())
                    strBuilder.Append(delimiter);
            }

            return Helper.TrimAll(strBuilder.ToString());
        }
    }
}
