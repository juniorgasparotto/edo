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
    public class EdoObjectToToken2
    {
        public TokenizeType Type { get; private set; }

        public EdoObjectToToken2(TokenizeType typeReading = TokenizeType.Normal)
        {
            this.Type = typeReading;
        }

        #region Parse tokens

        public TokenGroupCollection Convert(EDObjectCollection collection)
        {
            var res = new TokenGroupCollection();
            foreach (var obj in collection)
                res.Add(this.GetToGroup(obj));

            return res;
        }

        public TokenGroupCollection Convert(EDObject edoObj)
        {
            // Convert
            var list = new List<EDObject>();
            list.Add(edoObj);
            list.AddRange(edoObj.References.Traverse(f => f.References));

            var res = new TokenGroupCollection();
            foreach (var obj in list)
                res.Add(this.GetToGroup(obj));
            
            return res;
        }

        private TokenGroup GetToGroup(EDObject edoObj)
        {
            var tokenParsedBag = new TokenGroup(edoObj);
            this.ParseToken(edoObj, null, tokenParsedBag);
            //tokenParsedBag = Helper.InvertDictionary(tokenParsedBag);
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
        private Token ParseToken(EDObject edoObj, Token tokenParent = null, TokenGroup tokenParsedBag = null, int level = 1)
        {            
            Token newTokenCurrent = GetOrCreateTokenObject(edoObj, tokenParent, tokenParsedBag, level);
            tokenParsedBag.Add(edoObj, newTokenCurrent);

            if (edoObj.References.Count > 0)
            {
                level++;

                foreach (var next in edoObj.References)
                {
                    // Verify if tokens already exists with the 'next' value
                    var exists = tokenParsedBag.ExistsGroup(next) ? tokenParsedBag[next].FirstOrDefault(f => f.TokenValue.Value == next) : null;

                    if (Type == TokenizeType.NeverRepeatDefinedTokenIfAlreadyParsed)
                    {
                        if (exists != null)
                        {
                            tokenParsedBag.Add(edoObj, CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                            tokenParsedBag.Add(edoObj, new Token(exists.TokenValue, newTokenCurrent, level));
                        }
                        else
                        {
                            tokenParsedBag.Add(edoObj, CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                            var tokenNext = this.ParseToken(next, newTokenCurrent, tokenParsedBag, level);
                            tokenParsedBag.Add(edoObj, new Token(tokenNext.TokenValue, newTokenCurrent, level));
                        }
                    }
                    else
                    {
                        if (exists != null && Type == TokenizeType.Normal)
                        {
                            tokenParsedBag.Add(edoObj, CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                            tokenParsedBag.Add(edoObj, new Token(exists.TokenValue, newTokenCurrent, level));
                        }
                        else
                        {
                            // No make sense in practice (circular reference), 
                            // but is fixes for prevent a infinite call
                            if (exists != null && next.HasDirectOrIndirectReference(next))
                            {
                                tokenParsedBag.Add(edoObj, CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                                tokenParsedBag.Add(edoObj, new TokenRecursive(exists.TokenValue, newTokenCurrent, level));
                            }
                            else
                            {
                                if (next.References.Count > 0)
                                {
                                    tokenParsedBag.Add(edoObj, CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                                    tokenParsedBag.Add(edoObj, CreateTokenOperand<TokenValueOpenParenthesis>(newTokenCurrent, level));

                                    if (exists == null)
                                    { 
                                        this.ParseToken(next, newTokenCurrent, tokenParsedBag, level);
                                        tokenParsedBag.Add(edoObj, this.CopyParsedTokens(tokenParsedBag[next].ToList(), newTokenCurrent, level));
                                    }
                                    else
                                    {
                                        tokenParsedBag.Add(edoObj, this.CopyParsedTokens(tokenParsedBag[next].ToList(), newTokenCurrent, level));
                                    }

                                    tokenParsedBag.Add(edoObj, CreateTokenOperand<TokenValueCloseParenthesis>(newTokenCurrent, level));
                                }
                                else
                                {
                                    tokenParsedBag.Add(edoObj, CreateTokenOperand<TokenValuePlus>(newTokenCurrent, level));
                                    
                                    if (!tokenParsedBag.ExistsGroup(next))
                                    { 
                                        this.ParseToken(next, newTokenCurrent, tokenParsedBag, level);
                                        tokenParsedBag.Add(edoObj, this.CopyParsedTokens(tokenParsedBag[next].ToList(), newTokenCurrent, level));
                                    }
                                    else
                                    {
                                        tokenParsedBag.Add(edoObj, this.CopyParsedTokens(tokenParsedBag[next].ToList(), newTokenCurrent, level));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return newTokenCurrent;
        }

        /// <summary>
        /// Copy a list tokens has been parsed and changes yours values to respect a new token position.
        /// </summary>
        /// <param name="listToCopy"></param>
        /// <param name="parent"></param>
        /// <param name="levelStart"></param>
        /// <returns></returns>
        private List<Token> CopyParsedTokens(List<Token> listToCopy, Token parent, int levelStart)
        {
            var firstFrom = listToCopy.FirstOrDefault();
            var firstTo = new Token(firstFrom.TokenValue, parent, levelStart);
            var targetList = new List<Token>();
            targetList.Add(firstTo);

            foreach(var from in listToCopy.Skip(1))
            {
                // Set parent
                var indexParent = listToCopy.IndexOf(from.Parent);
                var parentToSet = targetList.ElementAt(indexParent);

                // Set level progressesly relative by level start
                var levelToSet = (from.Level - firstFrom.Level) + levelStart;

                // Add in list
                targetList.Add(new Token(from.TokenValue, parentToSet, levelToSet));
            }

            return targetList;
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
        private Token GetOrCreateTokenObject(EDObject obj, Token tokenParent, TokenGroup tokenBag, int level)
        {
            Token exists = tokenBag.ExistsGroup(obj) ? tokenBag[obj].FirstOrDefault(f => f.TokenValue.Value == obj) : null;
            if (exists != null)
                return new Token(exists.TokenValue, tokenParent, level);

            return new Token(new TokenValue(obj), tokenParent, level); ;
        }

        #endregion
    }
}
