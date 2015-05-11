using EDO.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDO
{
    public class EdoUtils
    {
        #region Writers

        public static EDObjectCollection ToEdoObjectCollection(params string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            return converter.Convert(expressions);
        }
        
        public static EDObject ToEdoObject(params string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            var collection = converter.Convert(expressions);
            return collection.FirstOrDefault();
        }

        public static EDObject ToEdoObject(string name, string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            var collection = converter.Convert(expressions);
            return collection.Contains(name);
        }

        public static void ApplyExpression(EDObjectCollection collectionToApply, params string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            converter.Convert(collectionToApply, expressions);
        }

        //public static void ApplyExpression(EDObject edoToApply, params string[] expressions)
        //{
        //    var converter = new ExpressionToEdoObject();
        //    converter.Convert(edoToApply, expressions);
        //}
        
        #endregion

        #region Readers (ignore sub tokens)

        public static string ToExpression(EDObjectCollection collection, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToExpression(true);

            var tokenGroupCollection = converterToToken.Convert(collection);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToExpression(EDObject edo, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToExpression(true);

            var tokenGroupCollection = converterToToken.Convert(edo);
            var output = converterToString.Convert(tokenGroupCollection, edo);

            return output;
        }

        public static string ToHierarchy(EDObjectCollection collection, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToHierarchy(true);

            var tokenGroupCollection = converterToToken.Convert(collection);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToHierarchy(EDObject edo, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToHierarchy(true);

            var tokenGroupCollection = converterToToken.Convert(edo);
            var output = converterToString.Convert(tokenGroupCollection, edo);

            return output;
        }

        public static string ToHierarchyInverse(EDObjectCollection collection, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToHierarchyInverse();

            var tokenGroupCollection = converterToToken.Convert(collection);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToHierarchyInverse(EDObject edo, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToExp = new TokenToHierarchyInverse();

            var tokenGroupCollection = converterToToken.Convert(edo);
            var output = converterToExp.Convert(tokenGroupCollection, edo);

            return output;
        }

        public static string ToDebug(EDObjectCollection collection, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToDebug(true);

            var tokenGroupCollection = converterToToken.Convert(collection);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToDebug(EDObject edo, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToDebug(true);

            var tokenGroupCollection = converterToToken.Convert(edo);
            var output = converterToString.Convert(tokenGroupCollection, edo);

            return output;
        }

        #endregion
    }
}
