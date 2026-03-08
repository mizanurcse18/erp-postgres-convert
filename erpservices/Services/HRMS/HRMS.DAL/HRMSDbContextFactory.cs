using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace HRMS.DAL
{
    internal class HRMSDbContextFactory : IDesignTimeDbContextFactory<HRMSDbContext>
    {
        public HRMSDbContext CreateDbContext(string[] args)
        {
            // Build configuration - navigate from HRMS.DAL to HRMS.API (same level)
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "HRMS.API"));
            
            Console.WriteLine($"Base path: {basePath}");
            Console.WriteLine($"Path exists: {Directory.Exists(basePath)}");
            Console.WriteLine($"AppSettings exists: {File.Exists(Path.Combine(basePath, "appsettings.json"))}");
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<HRMSDbContext>();
            var connectionString = configuration.GetConnectionString("Default");
            
            Console.WriteLine($"Connection string: {connectionString ?? "NULL"}");

            optionsBuilder.UseNpgsql(connectionString);

            return new HRMSDbContext(optionsBuilder.Options);
        }
    }
}
