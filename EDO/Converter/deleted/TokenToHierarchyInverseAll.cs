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
    public class TokenToHierarchyInverseAll : IConverterFromTokenToString
    {
        public string Convert(List<Token> tokens)
        {
            StringBuilder strBuilder = new StringBuilder();
            Dictionary<HierarchicalEntity, List<HierarchicalEntity>> organizedChilds = new Dictionary<HierarchicalEntity, List<HierarchicalEntity>>();
            strBuilder.AppendLine("Main: " + ((HierarchicalEntity)tokens.FirstOrDefault().TokenValue.Value).Name);
            var onlyValues = tokens.Where(f => f.TokenValue.Value is HierarchicalEntity).ToList();
            foreach (var token in onlyValues)
            {
                var value = (HierarchicalEntity)token.TokenValue.Value;
                if (!organizedChilds.ContainsKey(value))
                    organizedChilds.Add(value, new List<HierarchicalEntity>());

                if (token.Parent != null)
                    organizedChilds[value].Add((HierarchicalEntity)token.Parent.TokenValue.Value);
            }


            foreach (var child in organizedChilds)
            {
                if (child.Value.Count == 0)
                    strBuilder.AppendLine("[any references for this]");
                else
                    foreach (var parent in child.Value)
                        strBuilder.AppendLine(parent.Name);

                strBuilder.Append("..." + child.Key.Name);
                strBuilder.AppendLine();
            }

            return Helper.TrimAll(strBuilder.ToString());
        }

        public string Convert(TokenResult tokenResult, string delimiterReferences = null)
        {
            delimiterReferences = string.IsNullOrEmpty(delimiterReferences) ? "\r\n" : delimiterReferences;
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

        public string Convert(Dictionary<HierarchicalEntity, TokenResult> collection, string delimiterCollection = null, string delimiterReferences = null)
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
    }
}
