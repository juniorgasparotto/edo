using EDO.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Converter
{
    public interface IConverterFromEDObjectToToken
    {
        TokenResult Convert(EDObject obj);
        Dictionary<EDObject, TokenResult> Convert(EDObjectCollection collection);
    }
}
