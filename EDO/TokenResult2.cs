using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections;

namespace EDO
{
    public class TokenGroupCollection : IEnumerable
    {
        private List<TokenGroup> results = new List<TokenGroup>();

        public TokenGroup this[EDObject edo]
        {
            get
            {
                return results.FirstOrDefault(f => f.MainEdoObject == edo);
            }
        }

        public void Add(TokenGroup item)
        {
            this.results.Add(item);
        }

        public IEnumerator GetEnumerator()
        {
            return results.GetEnumerator();
        }
    }

    public class TokenGroup
    {
        private Dictionary<EDObject, List<Token>> tokenGroup;

        public EDObject MainEdoObject
        {
            get; 
            private set; 
        }

        public IEnumerable<Token> MainTokens
        {
            get
            {
                return this[MainEdoObject];
            }
        }

        public IEnumerable<Token> this[EDObject edo]
        {
            get
            {
                return tokenGroup[edo].AsEnumerable();
            }
        }

        public TokenGroup(EDObject edoObject)
        {
            this.MainEdoObject = edoObject;
        }

        public void Add(EDObject addIn, List<Token> listToAdd)
        {
            this.GetListOrNew(addIn).AddRange(listToAdd);
        }

        public void Add(EDObject addIn, Token add)
        {
            this.GetListOrNew(addIn).Add(add);
        }

        public bool ExistsGroup(EDObject edoVerify)
        {
            return this.tokenGroup.ContainsKey(edoVerify);
        }

        private List<Token> GetListOrNew(EDObject edoToAdd)
        {
            List<Token> exists;
            if (!this.tokenGroup.ContainsKey(edoToAdd))
            {
                exists = new List<Token>();
                this.tokenGroup.Add(edoToAdd, exists);
            }
            else
            {
                exists = this.tokenGroup[edoToAdd];
            }

            return exists;
        }
    }
}
