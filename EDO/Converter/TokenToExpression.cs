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
    public class TokenToExpression : IConverterFromTokenToString
    {
        public string Convert(List<Token> tokens)
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
    }
}
