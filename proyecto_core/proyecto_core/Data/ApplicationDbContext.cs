using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using proyecto_core.Models;
using proyecto_core.Models.ContentViewModels;

namespace proyecto_core.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            var au = builder.Entity<ApplicationUser>();
            //au.HasKey(u => u.Id);
            au.HasMany(u => u.ApplicationContentCollection)
                .WithOne(c => c.ApplicationUser);

            /*var ac = builder.Entity<ApplicationContent>();
            //ac.HasKey(c => c.Id);
            ac.HasOne(c => c.ApplicationUser);*/
        }

        public DbSet<ApplicationContent> Content { get; set; }
    }
}
