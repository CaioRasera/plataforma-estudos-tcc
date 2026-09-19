using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudyPlatform.Infrastructure.Persistence;

class Program {
    static void Main() {
        var builder = new ConfigurationBuilder().AddJsonFile("C:/Projetos/tcc-plataforma-estudos/backend/StudyPlatform.API/appsettings.Development.json");
        var config = builder.Build();
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));
        
        using (var context = new AppDbContext(optionsBuilder.Options))
        {
            var sql = File.ReadAllText("C:/Projetos/tcc-plataforma-estudos/backend/drop.sql");
            context.Database.ExecuteSqlRaw(sql);
            Console.WriteLine("Tables dropped.");
        }
    }
}
