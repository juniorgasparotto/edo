using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace EDO.Converter
{
    /// <summary>
    /// Expression of dependence of objects (EDO) - Tokenize
    /// </summary>
    public class ConverterFromEDObjectToToken : IConverterFromEDObjectToToken
    {
        public TokenizeType Type { get; private set; }

        public ConverterFromEDObjectToToken(TokenizeType typeReading = TokenizeType.Normal)
        {
            this.Type = typeReading;
        }

        #region Parse tokens

        public Dictionary<EDObject, TokenResult> Convert(EDObjectCollection collection)
        {
            var res = new Dictionary<EDObject, TokenResult>();
            foreach (var obj in collection)
                res.Add(obj, this.Convert(obj));

            return res;
        }

        public TokenResult Convert(EDObject edoObj)
        {
            Dictionary<EDObject, List<Token>> tokenParsedBag = new Dictionary<EDObject, List<Token>>();
            this.ParseToken(edoObj, null, tokenParsedBag);
            tokenParsedBag = Helper.InvertDictionary(tokenParsedBag);
            return new TokenResult(edoObj, tokenParsedBag);
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

                    if (Type == TokenizeType.NeverRepeatDefinedExpressionIfAlreadyParsed)
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
                        if (exists != null && Type != TokenizeType.AwaysRepeatDefinedExpression)
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
    }
}
