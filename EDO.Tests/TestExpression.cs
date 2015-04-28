namespace EDO.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TestExpression")]
    public partial class TestExpression
    {
        public int Id { get; set; }
        public string Input { get; set; }
        public string ObjectMain { get; set; }
        public string Description { get; set; }
        public string OutputNormal { get; set; }
        public string OutputAwaysRepeatDefinedToken { get; set; }
        public string OutputNeverRepeatDefinedTokenIfAlreadyParsed { get; set; }

        public string OutputHierarchyNormal { get; set; }
        public string OutputHierarchyAwaysRepeatDefinedToken { get; set; }
        public string OutputHierarchyNeverRepeatDefinedTokenIfAlreadyParsed { get; set; }
        
        public string OutputHierarchyInverseNormal { get; set; }
        public string OutputHierarchyInverseAwaysRepeatDefinedToken { get; set; }
        public string OutputHierarchyInverseNeverRepeatDefinedTokenIfAlreadyParsed { get; set; }

        public string OutputDebugNormal { get; set; }
        public string OutputDebugAwaysRepeatDefinedToken { get; set; }
        public string OutputDebugNeverRepeatDefinedTokenIfAlreadyParsed { get; set; }

        public int? HasUpdatedByCode { get; set; }
        public int? OnlyMainTokens { get; set; }
    }
}
