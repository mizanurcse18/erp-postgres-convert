using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Mail.DAL
{
    internal class MailDbContextFactory : IDesignTimeDbContextFactory<MailDbContext>
    {
        public MailDbContext CreateDbContext(string[] args)
        {
            // Build configuration - navigate from Mail.DAL to Mail.API (same level)
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Mail.API"));
            
            Console.WriteLine($"Base path: {basePath}");
            Console.WriteLine($"Path exists: {Directory.Exists(basePath)}");
            Console.WriteLine($"AppSettings exists: {File.Exists(Path.Combine(basePath, "appsettings.json"))}");
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<MailDbContext>();
            var connectionString = configuration.GetConnectionString("Default");
            
            Console.WriteLine($"Connection string: {connectionString ?? "NULL"}");

            optionsBuilder.UseNpgsql(connectionString);

            return new MailDbContext(optionsBuilder.Options);
        }
    }
}
