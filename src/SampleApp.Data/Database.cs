using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Data
{
    public class Database : DbContext
    {
        public DbSet<Invoice> Invoices { get; set; }

        public Database()
        {

        }

        public Database(DbContextOptions<Database> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.Number);
        }
    }
}
