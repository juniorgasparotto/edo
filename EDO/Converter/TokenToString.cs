using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Converter
{
    public abstract class TokenToString
    {
        private bool ignoreSubTokensOfMainTokens;
        private string delimiterMainTokens;
        private string delimiterSubTokensOfMainTokens;

        public TokenToString(bool ignoreSubTokensOfMainTokens = true, string delimiterMainTokens = null, string delimiterSubTokensOfMainTokens = null)
        {
            delimiterSubTokensOfMainTokens = string.IsNullOrEmpty(delimiterSubTokensOfMainTokens) ? "\r\n" : delimiterSubTokensOfMainTokens;
            delimiterMainTokens = string.IsNullOrEmpty(delimiterMainTokens) ? "\r\n" : delimiterMainTokens;

            this.ignoreSubTokensOfMainTokens = ignoreSubTokensOfMainTokens;
            this.delimiterMainTokens = delimiterMainTokens;
            this.delimiterSubTokensOfMainTokens = delimiterSubTokensOfMainTokens;
        }

        public abstract string Convert(List<Token> tokens);

        public virtual string Convert(TokenGroupCollection collection, EDObject edoObject = null)
        {
            var strBuilder = new StringBuilder();
            var last = collection.LastOrDefault();
            List<TokenGroup> list;

            if (edoObject != null)
                list = collection.Where(f => f.MainEdoObject == edoObject).ToList();
            else
                list = collection.ToList();

            foreach (var keyPair in collection)
            {
                strBuilder.Append(this.Convert(keyPair));

                if (keyPair.GetHashCode() != last.GetHashCode())
                    strBuilder.Append(delimiterMainTokens);
            }

            return Helper.TrimAll(strBuilder.ToString());
        }

        protected virtual string Convert(TokenGroup tokenGroup)
        {
            var strBuilder = new StringBuilder();
            var last = tokenGroup.LastOrDefault();

            List<List<Token>> list;
            if (ignoreSubTokensOfMainTokens)
            {
                list = new List<List<Token>>();
                list.Add(tokenGroup.MainTokens);
            }
            else
            { 
                list = tokenGroup.ToList();
            }

            foreach (var parsedToken in list)
            {
                strBuilder.Append(this.Convert(parsedToken));
                if (parsedToken.GetHashCode() != last.GetHashCode())
                    strBuilder.Append(this.delimiterSubTokensOfMainTokens);
            }

            return Helper.TrimAll(strBuilder.ToString());
        }
    }
}
