using Base.DomainClasses;
using Microsoft.EntityFrameworkCore;

namespace Base.DataLayer.Context;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<User> Users { set; get; } = default!;
    public virtual DbSet<Role> Roles { set; get; } = default!;
    public virtual DbSet<UserRole> UserRoles { get; set; } = default!;
    public virtual DbSet<UserLocation> UserLocations { get; set; } = default!;
    public virtual DbSet<UserToken> UserTokens { get; set; } = default!;

    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<Base.DomainClasses.File> Files { get; set; }
    public virtual DbSet<Folder> Folders { get; set; }
    public virtual DbSet<FilePattern> FilePatterns { get; set; }
    public virtual DbSet<PatternField> PatternFields { get; set; }
    public virtual DbSet<United> Uniteds { get; set; }
    public virtual DbSet<City> Cities { get; set; }
    public virtual DbSet<Setting> Settings { get; set; }
    public virtual DbSet<SupportRequest> SupportRequests { get; set; }
    public virtual DbSet<SupportResponse> SupportResponses { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<MessageSeen> MessageSeens { get; set; }
    public virtual DbSet<Report> Reports { get; set; }
    public virtual DbSet<UserClaim> UserClaims { set; get; } = default!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // it should be placed here, otherwise it will rewrite the following settings!
        base.OnModelCreating(modelBuilder);

        // Custom application mappings
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Username).HasMaxLength(maxLength: 450).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.SerialNumber).HasMaxLength(maxLength: 450);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(maxLength: 450).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new
            {
                e.UserId,
                e.RoleId
            });

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RoleId);
            entity.Property(e => e.UserId);
            entity.Property(e => e.RoleId);
            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles).HasForeignKey(d => d.RoleId);
            entity.HasOne(d => d.User).WithMany(p => p.UserRoles).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<UserLocation>(entity =>
        {
            entity.HasKey(e => new
            {
                e.UserId,
                e.CityId
            });

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CityId);
            entity.Property(e => e.UserId);
            entity.Property(e => e.CityId);
            entity.HasOne(d => d.City).WithMany(p => p.UserLocations).HasForeignKey(d => d.CityId);
            entity.HasOne(d => d.User).WithMany(p => p.UserLocations).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasOne(ut => ut.User).WithMany(u => u.UserTokens).HasForeignKey(ut => ut.UserId);

            entity.Property(ut => ut.RefreshTokenIdHash).HasMaxLength(maxLength: 450).IsRequired();
            entity.Property(ut => ut.RefreshTokenIdHashSource).HasMaxLength(maxLength: 450);
        });

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ParentFolderId);
            entity.HasOne(f => f.ParentFolder)
                .WithMany(p => p.SubFolders)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FilePattern>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Name).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Pattern).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<PatternField>(entity =>
        {
            entity.HasIndex(e => e.FilePatternId);
            entity.Property(e => e.FieldName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Placeholder).HasMaxLength(200).IsRequired();
            entity.HasOne(pf => pf.FilePattern)
                .WithMany(fp => fp.Fields)
                .HasForeignKey(pf => pf.FilePatternId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Base.DomainClasses.File>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.FolderId);
            entity.HasIndex(e => e.FilePatternId);
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(500);
            entity.Property(e => e.Path).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(200).IsRequired();
            entity.HasOne(f => f.Folder)
                .WithMany(folder => folder.Files)
                .HasForeignKey(f => f.FolderId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(f => f.FilePattern)
                .WithMany(fp => fp.Files)
                .HasForeignKey(f => f.FilePatternId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}