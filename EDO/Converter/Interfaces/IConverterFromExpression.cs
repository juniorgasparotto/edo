using EDO.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Converter
{
    public interface IConverterFromExpression
    {
        EDObjectCollection Convert(string[] expressions);
        void Convert(EDObjectCollection collectionToAppy, string[] expressions);
    }
}
