using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections;

namespace EDO
{
    public class TokenGroup : IEnumerable<List<Token>>
    {
        private readonly Dictionary<HierarchicalEntity, List<Token>> tokenGroup;

        public HierarchicalEntity MainEdoObject
        {
            get; 
            private set; 
        }

        public List<Token> MainTokens
        {
            get
            {
                return this[MainEdoObject].ToList();
            }
        }

        public List<Token> this[HierarchicalEntity edo]
        {
            get
            {
                return tokenGroup[edo].ToList();
            }
        }

        public TokenGroup(HierarchicalEntity edoObject)
        {
            this.MainEdoObject = edoObject;
            this.tokenGroup = new Dictionary<HierarchicalEntity, List<Token>>();
        }

        public void Add(HierarchicalEntity addIn, List<Token> listToAdd)
        {
            this.GetListOrNew(addIn).AddRange(listToAdd);
        }

        public void Add(HierarchicalEntity addIn, Token add)
        {
            this.GetListOrNew(addIn).Add(add);
        }

        public bool ExistsGroup(HierarchicalEntity edoVerify)
        {
            return this.tokenGroup.ContainsKey(edoVerify);
        }
        
        private List<Token> GetListOrNew(HierarchicalEntity edoToAdd)
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

        public IEnumerator<List<Token>> GetEnumerator()
        {
            return this.tokenGroup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.tokenGroup.Values.GetEnumerator();
        }
    }
}
