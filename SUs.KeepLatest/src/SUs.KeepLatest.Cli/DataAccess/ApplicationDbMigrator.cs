using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SUs.KeepLatest.Cli.DataAccess
{
    public static class ApplicationDbMigrator
    {
        public static async Task MigrateAsync()
        {
            using var db = new ApplicationDbContext();
            await db.Database.MigrateAsync();
        }
    }
}
