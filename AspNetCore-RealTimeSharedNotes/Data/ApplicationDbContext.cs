using AspNetCore_RealTimeSharedNotes.Models;
using AspNetCore_RealTimeSharedNotes.Models.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore_RealTimeSharedNotes.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string, IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Note> Notes { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //wire ApplicationUserRole so ef can join users with roles automatically
        builder.Entity<ApplicationUserRole>(b =>
        {
            b.HasOne(ur => ur.Role)
             .WithMany()
             .HasForeignKey(ur => ur.RoleId)
             .IsRequired();
        });

        //suppress unused Identity columns from AspNetUsers
        builder.Entity<ApplicationUser>(b =>
        {
            b.Ignore(u => u.TwoFactorEnabled);
            b.Ignore(u => u.PhoneNumber);
            b.Ignore(u => u.PhoneNumberConfirmed);
            b.Ignore(u => u.EmailConfirmed);

            //wire ApplicationUser with UserRoles so ef can join users with roles automatically
            b.HasMany(u => u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
        });

        builder.Entity<Note>(e =>
        {
            e.HasKey(n => new { n.NoteId, n.UserId });
            e.Property(n => n.NoteId).ValueGeneratedOnAdd();
            e.Property(n => n.Content).IsRequired().HasMaxLength(ModelConstants.NoteContentMaxLength);
            e.HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApiKey>(e =>
        {
            e.HasKey(a => new { a.UserId, a.ClientId });
            e.Property(a => a.ClientId).IsRequired().HasMaxLength(ModelConstants.ApiKeyClientIdMaxLength);
            e.Property(a => a.EncryptedClientSecret).IsRequired();
            e.HasOne(a => a.User)
                .WithOne(u => u.ApiKey)
                .HasForeignKey<ApiKey>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => a.ClientId).IsUnique();
        });
    }
}

