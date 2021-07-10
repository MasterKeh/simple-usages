using Microsoft.EntityFrameworkCore;
using SUs.KeepLatest.Cli.DataAccess.Models;
using System;
using System.IO;

namespace SUs.KeepLatest.Cli.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<KeepLatestItem> Items { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(string.Concat("Data source=", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.db")));
        }
    }
}
