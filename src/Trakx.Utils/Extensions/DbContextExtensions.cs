using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq;

namespace Trakx.Utils.Extensions;

public static class DbContextExtensions
{
    public static void LogMigrations(this DbContext context, ILogger logger)
    {
        var migrationList = context.Database.GetMigrations().ToList();
        logger.Information("Current Db State: {migrationList}", migrationList);
    }
}