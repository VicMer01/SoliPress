using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DocumentApprovalSystem.Models;

namespace DocumentApprovalSystem.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DocumentRequest> DocumentRequests { get; set; }
    public DbSet<DocumentHistory> DocumentHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ------------------------------------------
        // Identity key lengths
        // ------------------------------------------
        builder.Entity<User>(entity => entity.Property(m => m.Id).HasMaxLength(128));
        builder.Entity<IdentityRole>(entity => entity.Property(m => m.Id).HasMaxLength(128));
        builder.Entity<IdentityUserClaim<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(128));
        builder.Entity<IdentityRoleClaim<string>>(entity => entity.Property(m => m.RoleId).HasMaxLength(128));
        builder.Entity<IdentityUserLogin<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(128));
        builder.Entity<IdentityUserRole<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(128));
        builder.Entity<IdentityUserRole<string>>(entity => entity.Property(m => m.RoleId).HasMaxLength(128));
        builder.Entity<IdentityUserToken<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(128));

        // ------------------------------------------
        // Relationships
        // Avoid multiple cascade paths
        // ------------------------------------------

      builder.Entity<DocumentHistory>()
    .HasOne(h => h.DocumentRequest)
    .WithMany(r => r.Histories)
    .HasForeignKey(h => h.DocumentRequestId)
    .OnDelete(DeleteBehavior.NoAction); // más seguro que Restrict para SQL Server

builder.Entity<DocumentHistory>()
    .HasOne(h => h.User)
    .WithMany(u => u.DocumentHistories)
    .HasForeignKey(h => h.UserId)
    .OnDelete(DeleteBehavior.NoAction);

builder.Entity<DocumentRequest>()
    .HasOne(r => r.RequestedByUser)
    .WithMany(u => u.RequestedDocumentRequests)
    .HasForeignKey(r => r.RequestedByUserId)
    .OnDelete(DeleteBehavior.NoAction);

builder.Entity<DocumentRequest>()
    .HasOne(r => r.ApprovedByUser)
    .WithMany(u => u.ApprovedDocumentRequests)
    .HasForeignKey(r => r.ApprovedByUserId)
    .OnDelete(DeleteBehavior.NoAction);

    }
}
