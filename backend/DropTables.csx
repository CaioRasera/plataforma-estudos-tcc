using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudyPlatform.Infrastructure.Persistence;
using StudyPlatform.Domain.Entities;

var builder = new ConfigurationBuilder().AddJsonFile("StudyPlatform.API/appsettings.Development.json");
var config = builder.Build();

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

using (var context = new AppDbContext(optionsBuilder.Options))
{
    var sql = File.ReadAllText("drop.sql");
    context.Database.ExecuteSqlRaw(sql);
}
