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

        public static HierarchicalEntity ToHierarchicalEntity(params string[] expressions)
        {
            var converter = new ExpressionToHierarchicalEntity();
            return converter.Convert(expressions);
        }

        public static HierarchicalEntity ToHierarchicalEntityAndGetFirst(params string[] expressions)
        {
            var converter = new ExpressionToHierarchicalEntity();
            var root = converter.Convert(expressions);
            return root.Descendants().FirstOrDefault();
        }

        public static HierarchicalEntity ToHierarchicalEntity(string name, string[] expressions)
        {
            var converter = new ExpressionToHierarchicalEntity();
            var root = converter.Convert(expressions);
            return root.Descendants().FirstOrDefault(f=>f.Identity == name);
        }

        public static void ApplyExpression(HierarchicalEntity rootToApply, params string[] expressions)
        {
            var converter = new ExpressionToHierarchicalEntity();
            converter.Convert(rootToApply, expressions);
        }

        #endregion

        #region Readers (ignore sub tokens)

        public static string ToExpression(HierarchicalEntity root, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToString = new TokenToExpression(true);

            var tokenGroupCollection = converterToToken.Convert(root);
            var output = converterToString.Convert(tokenGroupCollection, root);

            return output;
        }

        public static string ToExpression(List<HierarchicalEntity> list, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToString = new TokenToExpression(true);

            var tokenGroupCollection = converterToToken.Convert(list);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToHierarchy(HierarchicalEntity root, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToString = new TokenToHierarchy(true);

            var tokenGroupCollection = converterToToken.Convert(root);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToHierarchy(List<HierarchicalEntity> list, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToString = new TokenToHierarchy(true);

            var tokenGroupCollection = converterToToken.Convert(list);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToHierarchyWithRoot(List<HierarchicalEntity> list, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToString = new TokenToHierarchy(true);

            var tokenGroupCollection = converterToToken.Convert(list);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToHierarchyInverse(HierarchicalEntity root, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToExp = new TokenToHierarchyInverse();

            var tokenGroupCollection = converterToToken.Convert(root);
            var output = converterToExp.Convert(tokenGroupCollection, root);
            return output;
        }

        public static string ToHierarchyInverse(List<HierarchicalEntity> list, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToExp = new TokenToHierarchyInverse();

            var tokenGroupCollection = converterToToken.Convert(list);
            var output = converterToExp.Convert(tokenGroupCollection);
            return output;
        }

        public static string ToDebug(HierarchicalEntity root, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToString = new TokenToDebug(true);

            var tokenGroupCollection = converterToToken.Convert(root);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        public static string ToDebug(List<HierarchicalEntity> list, TokenizeType tokenizeType = TokenizeType.Normal)
        {
            var converterToToken = new HierarchicalEntityToToken(tokenizeType);
            var converterToString = new TokenToDebug(true);

            var tokenGroupCollection = converterToToken.Convert(list);
            var output = converterToString.Convert(tokenGroupCollection);

            return output;
        }

        #endregion
    }
}
