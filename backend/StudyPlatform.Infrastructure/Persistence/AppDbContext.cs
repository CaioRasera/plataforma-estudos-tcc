using Microsoft.EntityFrameworkCore;
using StudyPlatform.Domain.Entities;

namespace StudyPlatform.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Plan> Plans { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Chunk> Chunks { get; set; } = null!;
    public DbSet<AssessmentItem> AssessmentItems { get; set; } = null!;
    public DbSet<UserItemProgress> UserItemProgresses { get; set; } = null!;
    public DbSet<TokenTransaction> TokenTransactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Chunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Embedding).HasColumnType("vector(768)");
            entity.HasIndex(e => e.Embedding).HasMethod("hnsw").HasOperators("vector_cosine_ops");
        });

        modelBuilder.Entity<UserItemProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ItemId }).IsUnique();
        });

                modelBuilder.Entity<AssessmentItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WrongAnswers).HasColumnName("WrongAnswers");
        });

        base.OnModelCreating(modelBuilder);
    }
}


