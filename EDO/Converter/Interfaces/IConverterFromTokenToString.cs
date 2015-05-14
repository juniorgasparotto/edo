using EDO.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Converter
{
    public interface IConverterFromTokenToString
    {
        string Convert(List<Token> tokens);
        string Convert(TokenResult edoObject, string delimiterReferences = null);
        string Convert(Dictionary<HierarchicalEntity, TokenResult> collection, string delimiterCollection = null, string delimiterReferences = null);
    }
}
