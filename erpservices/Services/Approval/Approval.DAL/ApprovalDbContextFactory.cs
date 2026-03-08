using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Approval.DAL
{
    internal class ApprovalDbContextFactory : IDesignTimeDbContextFactory<ApprovalDbContext>
    {
        public ApprovalDbContext CreateDbContext(string[] args)
        {
            // Build configuration - navigate from Approval.DAL to Approval.API (same level)
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Approval.API"));
            
            Console.WriteLine($"Base path: {basePath}");
            Console.WriteLine($"Path exists: {Directory.Exists(basePath)}");
            Console.WriteLine($"AppSettings exists: {File.Exists(Path.Combine(basePath, "appsettings.json"))}");
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApprovalDbContext>();
            var connectionString = configuration.GetConnectionString("Default");
            
            Console.WriteLine($"Connection string: {connectionString ?? "NULL"}");

            optionsBuilder.UseNpgsql(connectionString);

            return new ApprovalDbContext(optionsBuilder.Options);
        }
    }
}
