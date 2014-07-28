using System.Data.Entity;

namespace Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(string connectionString) : 
            base(connectionString)
        { }

        public DbSet<PowerNet> PowerNets { get; set; }
    }
}
