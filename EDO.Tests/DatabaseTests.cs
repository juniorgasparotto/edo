namespace EDO.Tests
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.IO;

    public partial class DatabaseTests : DbContext
    {
        public DatabaseTests()
            : base("name=DatabaseTestsAttached")
        {
        }

        public virtual DbSet<TestExpression> TestExpressions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        public static void SetDataDirectory()
        {
            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = (System.IO.Path.GetDirectoryName(executable));
            path = Path.GetFullPath(Path.Combine(path, "../../"));
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
        }
    }
}
