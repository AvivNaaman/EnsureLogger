using Ensure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ensure.Web.Data
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<EnsureLog> Logs { get; set; }

        public DbSet<AppUser> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<EnsureLog>(e =>
			{
				e.HasKey(l => l.Id);
				e.Property(l => l.Logged).IsRequired().HasPrecision(4);
			});

			builder.Entity<AppUser>(e =>
			{
				e.HasKey(u => u.Id);
				e.Property(u => u.Id).HasMaxLength(128)
					.HasColumnType("varchar");

				e.Property(u => u.UserName).IsRequired()
					.HasMaxLength(64).HasColumnType("varchar");

				e.Property(u => u.Email).IsRequired()
					.HasMaxLength(256).HasColumnType("varchar");

				e.Property(u => u.SecurityKey).HasMaxLength(128)
					.HasColumnType("varchar");

				e.Property(u => u.PasswordHash).HasMaxLength(128);

				e.HasMany(u => u.Logs).WithOne()
					.HasForeignKey(l => l.UserId).IsRequired(false);
				e.Property(u => u.DailyTarget).HasDefaultValue(2);
				e.Property(u => u.Joined).HasDefaultValueSql("GETUTCDATE()");
			});
		}
	}
}
