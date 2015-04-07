using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace EDO
{
    public class Token
    {
        public TokenValue TokenValue { get; private set; }
        public Token Parent { get; private set; }
        public int Level { get; private set; }

        public Token(TokenValue token, Token tokenPositionParent, int level)
        {
            this.TokenValue = token;
            this.Parent = tokenPositionParent;
            this.Level = level;
        }

        public override string ToString()
        {
            return this.TokenValue.ToString();
        }
    }
    
    public class TokenRecursive : Token
    {
        public TokenRecursive(TokenValue token, Token tokenPositionParent, int level)
            : base(token, tokenPositionParent, level) 
        { }
    }

    public class TokenValue
    {
        public object Value { get; private set; }

        public TokenValue(object value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            var res = "";
            if (this is TokenValuePlus)
                res = " + ";
            else if (this is TokenValueOpenParenthesis)
                res = "(";
            else if (this is TokenValueCloseParenthesis)
                res = ")";
            else
                res = ((EDObject)this.Value).Name;

            return res;
        }
    }

    public class TokenValuePlus : TokenValue
    {
        public static TokenValuePlus Instance = new TokenValuePlus();
        private TokenValuePlus() : base(null) { }
    }

    public class TokenValueOpenParenthesis : TokenValue
    {
        public static TokenValueOpenParenthesis Instance = new TokenValueOpenParenthesis();
        private TokenValueOpenParenthesis() : base(null) { }
    }

    public class TokenValueCloseParenthesis : TokenValue
    {
        public static TokenValueCloseParenthesis Instance = new TokenValueCloseParenthesis();
        private TokenValueCloseParenthesis() : base(null) { }
    }
}
