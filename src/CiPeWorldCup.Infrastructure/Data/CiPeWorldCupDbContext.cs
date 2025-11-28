using CiPeWorldCup.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CiPeWorldCup.Infrastructure.Data;

public class CiPeWorldCupDbContext : DbContext
{
    public CiPeWorldCupDbContext(DbContextOptions<CiPeWorldCupDbContext> options) : base(options)
    {
    }
    
    public DbSet<Participant> Participants { get; set; }
    public DbSet<MatchesToDB> Matches { get; set; }
    public DbSet<TournamentToDB> Tournaments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<TournamentToDB>()
            .HasMany(t => t.Matches)
            .WithOne()
            .HasForeignKey(m => m.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Participant>().HasData(
            new Participant("Alice", 1),
            new Participant("Bob", 2),
            new Participant("Charlie", 3),
            new Participant("Diana", 4),
            new Participant("Ethan", 5),
            new Participant("Fiona", 6),
            new Participant("George", 7),
            new Participant("Hannah", 8),
            new Participant("Isaac", 9),
            new Participant("Julia", 10),
            new Participant("Kevin", 11),
            new Participant("Laura", 12),
            new Participant("Michael", 13),
            new Participant("Nina", 14),
            new Participant("Oscar", 15),
            new Participant("Paula", 16),
            new Participant("Quentin", 17),
            new Participant("Rachel", 18),
            new Participant("Samuel", 19),
            new Participant("Tina", 20)
        );
    }
}
