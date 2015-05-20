using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using EDO.Converter;

namespace EDO.Converter
{
    public class TokenToHierarchy : TokenToString
    {
        public TokenToHierarchy(bool ignoreSubTokensOfMainTokens = true, string delimiterMainTokens = null, string delimiterSubTokensOfMainTokens = null)
            : base(ignoreSubTokensOfMainTokens,
                   string.IsNullOrWhiteSpace(delimiterMainTokens) ? "\r\n-----\r\n" : delimiterMainTokens,
                   string.IsNullOrWhiteSpace(delimiterSubTokensOfMainTokens) ? "\r\n\r\n" : delimiterSubTokensOfMainTokens
                  )
        {
        }

        public override string Convert(List<Token> tokens)
        {
            StringBuilder strBuilder = new StringBuilder();
            var repeat = 3;
            var charRepeat = ".";
            var initLevel = tokens.FirstOrDefault().Level;

            foreach (var token in tokens)
            {
                if (token is TokenRecursive)
                {
                    strBuilder.Append(((HierarchicalEntity)token.TokenValue.Value).Identity);
                }
                else
                {
                    if (token.TokenValue is TokenValuePlus)
                    {
                        var spacing = String.Concat(Enumerable.Repeat(charRepeat, (token.Level - initLevel) * repeat));
                        strBuilder.AppendLine();
                        strBuilder.Append(spacing);
                    }
                    else if (token.TokenValue.Value is HierarchicalEntity)
                    { 
                        strBuilder.Append(((HierarchicalEntity)token.TokenValue.Value).Identity);
                    }
                }
            }

            return strBuilder.ToString();
        }
    }
}
