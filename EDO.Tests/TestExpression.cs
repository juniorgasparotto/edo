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

        public string OutputAndNormal { get; set; }

        public string OutputAndAwaysRepeatDefinedExpression { get; set; }

        public string OutputIncludeRefAndAwaysRepeatDefinedExpression { get; set; }

        public string OutputIncludeRefAndNormal { get; set; }

        public string OutputIncludeRefAndNeverRepeatDefinedExpression { get; set; }

        public string ObjectMain { get; set; }

        public string Description { get; set; }
    }
}
