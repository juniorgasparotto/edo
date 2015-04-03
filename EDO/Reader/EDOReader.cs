using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using EDO.Dispatcher;

namespace EDO.Reader
{
    public enum TypeReader
    {
        Normal,
        AwaysRepeatDefinedExpression,
        NeverRepeatDefinedExpressionIfAlreadyParsed
    }

    /// <summary>
    /// Expression of dependence of objects (EDO) - Reader
    /// </summary>
    public class EDOReader
    {
        private EDObjectCollection objectCollection;
        private EDObject obj;
        public TypeReader Type { get; set; }

        public EDOReader(EDObjectCollection collection, TypeReader typeReading = TypeReader.Normal)
        {
            this.objectCollection = collection;
            this.Type = typeReading;
        }

        public EDOReader(EDObject obj, TypeReader typeReading = TypeReader.Normal)
        {
            if (obj == null)
                throw new Exception("Object 'object' can't be null'");

            this.obj = obj;
            this.Type = typeReading;
        }

        /// <summary>
        /// Return a expression of a object
        /// </summary>
        /// <returns></returns>
        public string ToExpression(bool includeReferences = false)
        {
            return ToExpression(this.obj, includeReferences);
        }

        public string ToExpression(EDObject edoObj, bool includeReferences = false)
        {
            StringBuilder strBuilder = new StringBuilder();
            var tokenParsedBag = this.GetTokens(edoObj, includeReferences);
            tokenParsedBag = this.InvertDictionary(tokenParsedBag);

            foreach (var parsedToken in tokenParsedBag)
                strBuilder.AppendLine(this.ToExpression(parsedToken.Value));

            return TrimAll(strBuilder.ToString());
        }

        public string ToExpression(List<Token> tokens)
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

        public string Debug(bool includeReferences = false)
        {
            return Debug(this.obj, includeReferences);
        }

        public string Debug(EDObject edoObj, bool includeReferences = false)
        {
            StringBuilder strBuilder = new StringBuilder();
            var tokenParsedBag = this.GetTokens(edoObj, includeReferences);
            tokenParsedBag = this.InvertDictionary(tokenParsedBag);

            foreach (var parsedToken in tokenParsedBag)
            {
                strBuilder.Append(this.Debug(parsedToken.Value));
                strBuilder.AppendLine();
            }

            return TrimAll(strBuilder.ToString());
        }

        public string Debug(List<Token> tokens)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (var token in tokens)
            {
                var resParent = "";

                if (token.Parent != null)
                    resParent = string.Format(" parent: (hashcode: {0}; value: {1})", token.Parent.GetHashCode(), token.Parent.TokenValue.ToString());

                strBuilder.Append(token.TokenValue.ToString().Trim());
                strBuilder.Append(string.Format(" hashcode: {0}", token.GetHashCode()));
                strBuilder.Append(resParent);
                strBuilder.Append(" level: " + token.Level);
                strBuilder.Append(" hashcodeValue: " + token.TokenValue.GetHashCode());
                strBuilder.AppendLine();
            }

            return strBuilder.ToString();
        }
        
        #region Parse tokens

        /// <summary>
        /// Get all tokens that represent a expression from the object
        /// </summary>
        /// <param name="object"></param>
        /// <returns>The list of the Token</returns>
        public Dictionary<EDObject, List<Token>> GetTokens(bool includeReferences = false)
        {
            return this.GetTokens(this.obj, includeReferences);
        }

        public Dictionary<EDObject, List<Token>> GetTokens(EDObject edoObj, bool includeReferences = false)
        {
            // this validate occurs because don't make sense show a expression of a edoObj without your definitions
            // the result will be for nothing
            if (!includeReferences && Type == TypeReader.NeverRepeatDefinedExpressionIfAlreadyParsed)
                throw new Exception("The parameter 'includeReference' can't be used with 'NeverRepeatDefinedExpressionIfAlreadyParsed'");

            Dictionary<EDObject, List<Token>> tokenParsedBag = new Dictionary<EDObject, List<Token>>();
            this.ParseToken(edoObj, null, tokenParsedBag);

            if (!includeReferences)
                tokenParsedBag = tokenParsedBag.Where(k => k.Key == edoObj).ToDictionary(k=>k.Key, v=>v.Value);

            return tokenParsedBag;
        }

        /// <summary>
        /// Parse a Token of the object.
        /// </summary>
        /// <param name="object">The object to be converted in a Token</param>
        /// <param name="tokenParent">This object is used exclusive in recursive action</param>
        /// <param name="tokenBag">This object is used exclusive in recursive action. This is fill in recursive process</param>
        /// <param name="level">The object is used exclusive in recursive process</param>
        /// <returns>Return a Token instance that represent a Object instance</returns>
        private Token ParseToken(EDObject edoObj, Token tokenParent = null, Dictionary<EDObject, List<Token>> tokenParsedBag = null, int level = 1)
        {
            if (tokenParsedBag == null)
                tokenParsedBag = new Dictionary<EDObject, List<Token>>();

            List<Token> tokenBag;
            if (!tokenParsedBag.ContainsKey(edoObj))
            {
                tokenBag = new List<Token>();
                tokenParsedBag[edoObj] = tokenBag;
            }
            else
            {
                tokenBag = tokenParsedBag[edoObj];
            }

            Token newTokenCurrent = GetOrCreateTokenObject(edoObj, tokenParent, tokenParsedBag, level);
            tokenBag.Add(newTokenCurrent);

            if (edoObj.References.Count > 0)
            {
                level++;

                foreach (var next in edoObj.References)
                {
                    // Verify if tokens already exists with the 'next' value
                    var exists = tokenParsedBag.ContainsKey(next) ? tokenParsedBag[next].FirstOrDefault(f => f.TokenValue.Value == next) : null;

                    if (Type == TypeReader.NeverRepeatDefinedExpressionIfAlreadyParsed)
                    {
                        if (exists != null)
                        {
                            tokenBag.Add(CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                            tokenBag.Add(new Token(exists.TokenValue, tokenParent, level));
                        }
                        else
                        {
                            tokenBag.Add(CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                            tokenBag.Add(this.ParseToken(next, newTokenCurrent, tokenParsedBag, level));
                        }
                    }
                    else
                    {
                        if (exists != null && Type != TypeReader.AwaysRepeatDefinedExpression)
                        {
                            tokenBag.Add(CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                            tokenBag.Add(new Token(exists.TokenValue, newTokenCurrent, level));
                        }
                        else
                        {
                            // No make sense in practice (circular reference), 
                            // but is fixes for prevent a infinite call
                            if (exists != null && next.HasDirectOrIndirectReference(next))
                            {
                                tokenBag.Add(CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                                tokenBag.Add(new TokenRecursive(exists.TokenValue, newTokenCurrent, level));
                            }
                            else
                            {
                                if (next.References.Count > 0)
                                {
                                    tokenBag.Add(CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                                    tokenBag.Add(CreateTokenOperand<TokenValueOpenParenthesis>(newTokenCurrent, level));
                                    
                                    if (!tokenParsedBag.ContainsKey(next))
                                        this.ParseToken(next, newTokenCurrent, tokenParsedBag, level);

                                    tokenBag.AddRange(tokenParsedBag[next]);
                                    tokenBag.Add(CreateTokenOperand<TokenValueCloseParenthesis>(newTokenCurrent, level));
                                }
                                else
                                {
                                    tokenBag.Add(CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                                    
                                    if (!tokenParsedBag.ContainsKey(next))
                                        this.ParseToken(next, newTokenCurrent, tokenParsedBag, level);

                                    tokenBag.AddRange(tokenParsedBag[next]);
                                }
                            }
                        }
                    }
                }
            }

            return newTokenCurrent;
        }

        /// <summary>
        /// Create a new Token of types "+", (", ")"
        /// </summary>
        /// <typeparam name="T">The sub type of Token to be a create</typeparam>
        /// <param name="tokenParent">The token parent</param>
        /// <param name="level">The level in expression</param>
        /// <returns>Return a new Token of type T</returns>
        private Token CreateTokenOperand<T>(Token tokenParent, int level) where T : TokenValue
        {
            TokenValue tokenValue = null;
            if (typeof(T) == typeof(TokenValuePlus))
                tokenValue = TokenValuePlus.Instance;
            else if (typeof(T) == typeof(TokenValueOpenParenthesis))
                tokenValue = TokenValueOpenParenthesis.Instance;
            else if (typeof(T) == typeof(TokenValueCloseParenthesis))
                tokenValue = TokenValueCloseParenthesis.Instance;
            else
                throw new Exception(string.Format("Sub type '{0}' of Token is not supported", typeof(T).Name));

            return new Token(tokenValue, tokenParent, level);
        }

        /// <summary>
        /// Create a new Token Object
        /// </summary>
        /// <typeparam name="T">The sub type of Token to be a create</typeparam>
        /// <param name="object">The object to be converted a Token</param>
        /// <param name="objectParent">The token parent</param>
        /// <param name="tokenBag">The token list to help a verify if object already exists and to find a parent token</param>
        /// <param name="level">The level in expression</param>
        /// <returns>Return a new Token of type T</returns>
        private Token GetOrCreateTokenObject(EDObject obj, Token tokenParent, Dictionary<EDObject, List<Token>> tokenBag, int level)
        {
            Token exists = tokenBag.ContainsKey(obj) ? tokenBag[obj].FirstOrDefault(f => f.TokenValue.Value == obj) : null;
            if (exists != null)
                return new Token(exists.TokenValue, tokenParent, level);

            return new Token(new TokenValue(obj), tokenParent, level); ;
        }

        #endregion

        #region Helpers

        private string TrimAll(string str)
        {
            return str.Trim().TrimStart('\r', '\n').TrimEnd('\r', '\n');
        }
        private Dictionary<TKey, TValue> InvertDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            var reverse = new Dictionary<TKey, TValue>();

            for (var i = dictionary.Count - 1; i >= 0; i--)
                reverse.Add(dictionary.Keys.ElementAt(i), dictionary.Values.ElementAt(i));

            return reverse;
        }

        #endregion
    }
}
