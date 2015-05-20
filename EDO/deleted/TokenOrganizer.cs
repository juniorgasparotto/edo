//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using NCalc;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Linq;
//using System.Diagnostics;
//using EDO.Converter;

//namespace EDO.Converter
//{
//    public class TokenOrganizer
//    {
//        public Dictionary<HierarchicalEntity, List<Token>> Organize(Dictionary<HierarchicalEntity, TokenResult> collection)
//        {
//            var res = new Dictionary<HierarchicalEntity, List<Token>>();
//            var organizedChilds = new Dictionary<HierarchicalEntity, List<HierarchicalEntity>>();
//            foreach (var keyPair in collection)
//                res.Add(keyPair.Key, GetMain(keyPair.Value));

//            return res;
//        }

//        private List<Token> GetMain(TokenResult tokenResult)
//        {
//            return tokenResult.Tokens[tokenResult.EdoObject];
//        }
//    }
//}
