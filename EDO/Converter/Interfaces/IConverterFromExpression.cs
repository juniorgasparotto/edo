using EDO.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Converter
{
    public interface IConverterFromExpression
    {
        HorizontalCollection Convert(string[] expressions);
        void Convert(HorizontalCollection collectionToAppy, string[] expressions);
    }
}
