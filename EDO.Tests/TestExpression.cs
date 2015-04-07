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
        public string OutputAwaysRepeatDefinedExpression { get; set; }
        public string OutputNeverRepeatDefinedExpressionIfAlreadyParsed { get; set; }
        public int HasUpdatedByCode { get; set; }
    }
}
