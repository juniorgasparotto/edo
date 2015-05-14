using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO
{
    public class EntityAlreadyExistsException : Exception
    {
        public EntityAlreadyExistsException(string message)
        :    base(message)
        {

        }
    }
}
