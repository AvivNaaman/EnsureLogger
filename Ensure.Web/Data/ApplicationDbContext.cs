using Ensure.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ensure.Web.Data
{
	public class ApplicationDbContext : IdentityDbContext<AppUser>
	{
		public DbSet<EnsureLog> Logs { get; set; }

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
				e.HasMany(u => u.Logs).WithOne().HasForeignKey(l => l.UserId).IsRequired(false);
				e.Property(u => u.DailyTarget).HasDefaultValue(2);
				e.Property(u => u.Joined).HasDefaultValueSql("GETUTCDATE()");
			});
		}
	}
}
