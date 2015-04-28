using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Converter
{
    public class TokenToExpression : TokenToString
    {
        public TokenToExpression(bool ignoreSubTokensOfMainTokens = true, string delimiterMainTokens = null, string delimiterSubTokensOfMainTokens = null)
            : base(ignoreSubTokensOfMainTokens, delimiterMainTokens, delimiterSubTokensOfMainTokens)
        {
        }

        public override string Convert(List<Token> tokens)
        {
            StringBuilder strBuilder = new StringBuilder();
            
            foreach (var token in tokens)
            {
                if (token is TokenRecursive)
                {
                    strBuilder.Append(((EDObject)token.TokenValue.Value).Name);
                }
                else
                {
                    if (token.TokenValue is TokenValuePlus)
                        strBuilder.Append(" + ");
                    else if (token.TokenValue is TokenValueOpenParenthesis)
                        strBuilder.Append("(");
                    else if (token.TokenValue is TokenValueCloseParenthesis)
                        strBuilder.Append(")");
                    else
                        strBuilder.Append(((EDObject)token.TokenValue.Value).Name);
                }
            }

            return strBuilder.ToString();
        }
    }
}
