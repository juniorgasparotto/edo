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

        public static HorizontalCollection ToEdoObjectCollection(params string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            return converter.Convert(expressions);
        }
        
        public static HierarchicalEntity ToEdoObject(params string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            var collection = converter.Convert(expressions);
            return collection.FirstOrDefault();
        }

        public static HierarchicalEntity ToEdoObject(string name, string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            var collection = converter.Convert(expressions);
            return collection.Contains(name);
        }

        public static void ApplyExpression(HorizontalCollection collectionToApply, params string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            converter.Convert(collectionToApply, expressions);
        }

        public static HorizontalCollection ApplyExpression(HierarchicalEntity edoToApply, params string[] expressions)
        {
            var converter = new ExpressionToEdoObject();
            return converter.Convert(edoToApply, expressions);
        }
        
        #endregion

        #region Readers (ignore sub tokens)

        public static string ToExpression(HorizontalCollection collection, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToExpression(true);

            var tokenGroupCollection = converterToToken.Convert(collection);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToExpression(HierarchicalEntity edo, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToExpression(true);

            var tokenGroupCollection = converterToToken.Convert(edo);
            var output = converterToString.Convert(tokenGroupCollection, edo);

            return output;
        }

        public static string ToHierarchy(HorizontalCollection collection, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToHierarchy(true);

            var tokenGroupCollection = converterToToken.Convert(collection);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToHierarchy(HierarchicalEntity edo, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToHierarchy(true);

            var tokenGroupCollection = converterToToken.Convert(edo);
            var output = converterToString.Convert(tokenGroupCollection, edo);

            return output;
        }

        public static string ToHierarchyInverse(HorizontalCollection collection, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToHierarchyInverse();

            var tokenGroupCollection = converterToToken.Convert(collection);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToHierarchyInverse(HierarchicalEntity edo, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToExp = new TokenToHierarchyInverse();

            var tokenGroupCollection = converterToToken.Convert(edo);
            var output = converterToExp.Convert(tokenGroupCollection, edo);

            return output;
        }

        public static string ToDebug(HorizontalCollection collection, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new EdoObjectToToken(tokenizeType);
            var converterToString = new TokenToDebug(true);

            var tokenGroupCollection = converterToToken.Convert(collection);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToDebug(HierarchicalEntity edo, TokenizeType tokenizeType = TokenizeType.Normal)
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
