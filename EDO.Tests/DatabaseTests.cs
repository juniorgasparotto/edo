namespace EDO.Tests
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DatabaseTests : DbContext
    {
        public DatabaseTests()
            : base("name=DatabaseTests")
        {
        }

        public virtual DbSet<TestExpression> TestExpressions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
