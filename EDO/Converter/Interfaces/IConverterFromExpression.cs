using EDO.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Converter
{
    public interface IConverterFromExpression
    {
        void Convert(string expression, EDObjectCollection collectionToAppy);
        void Convert(string expression, EDObject edoObjectToApply);
    }
}
