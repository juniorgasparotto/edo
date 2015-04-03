using NCalc;
using System;
using System.Collections.Generic;

namespace EDO
{
    internal class ObjectParamExpression : IOperations
    {
        private EDObject Object;
        public ObjectParamExpression(EDObject Object)
        {
            this.Object = Object;
        }

        #region Implements IOperations

        object IOperations.Add(object b)
        {
            var p = (ObjectParamExpression)b;
            this.Object.AddReference(p.Object);
            return this;
        }

        object IOperations.Soustract(object b)
        {
            var p = (ObjectParamExpression)b;
            this.Object.RemoveReference(p.Object);
            return this;
        }

        object IOperations.Multiply(object b)
        {
            throw new NotImplementedException();
        }

        object IOperations.Divide(object b)
        {
            throw new NotImplementedException();
        }

        object IOperations.Modulo(object b)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
