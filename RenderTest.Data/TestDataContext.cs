using RenderTest.Data.Models;
using System.Data.Entity;
using Pikachu.Data.Framework.Repository;

namespace RenderTest.Data
{
    public class TestDataContext : DbContextBase
    {
        public TestDataContext()
            : base("name=TestDataContext")
        {
            Database.SetInitializer<TestDataContext>(null);
            Configuration.ProxyCreationEnabled = false;
        }

        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        public virtual DbSet<TestTable> TestTables { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestTable>()
                .Property(e => e.MyStringField)
                .IsUnicode(false);

            modelBuilder.Entity<TestTable>()
                .Property(e => e.MyMoneyField)
                .HasPrecision(19, 4);
        }
    }
}
