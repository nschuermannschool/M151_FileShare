using FileShareDataAccessLayer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FileShareDataAccessLayer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Models.File> Files { get; set; }
        public DbSet<ApplicationUserFile> ApplicationUserFile { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.File>()
                .HasMany(x => x.Users)
                .WithMany(x => x.Files)
                .UsingEntity<ApplicationUserFile>(x => x.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId), x=> x.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId));

        }
    }
}