using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using VeniceDemo.App.Models;

#nullable disable

namespace VeniceDemo.App.Data
{
	/// <summary>
	/// Контекст базы данных SQLite
	/// </summary>
	public partial class VeniceDBContext : DbContext
	{
		public IConfiguration Configuration { get; }

		public VeniceDBContext(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public VeniceDBContext(DbContextOptions<VeniceDBContext> options, IConfiguration configuration)
			: base(options)
		{
			Configuration = configuration;
		}

		public virtual DbSet<Customer> Customers { get; set; }
		public virtual DbSet<Order> Orders { get; set; }
		public virtual DbSet<OrderPizza> OrderPizzas { get; set; }
		public virtual DbSet<Payment> Payments { get; set; }
		public virtual DbSet<Pizza> Pizzas { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlite(Configuration.GetConnectionString("VeniceDbConnection"));
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Customer>(entity =>
			{
				entity.HasIndex(e => e.Id, "IX_Customers_Id")
					.IsUnique();

				entity.HasIndex(e => e.Login, "IX_Customers_Login")
					.IsUnique();

				entity.Property(e => e.Login).IsRequired();
			});

			modelBuilder.Entity<Order>(entity =>
			{
				entity.HasIndex(e => e.Id, "IX_Orders_Id")
					.IsUnique();

				entity.Property(e => e.DateCreated).IsRequired();

				entity.HasOne(d => d.Customer)
					.WithMany(p => p.Orders)
					.HasForeignKey(d => d.CustomerId)
					.OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<OrderPizza>(entity =>
			{
				entity.HasIndex(e => e.Id, "IX_OrderPizzas_Id")
					.IsUnique();

				entity.HasOne(d => d.Order)
					.WithMany(p => p.OrderPizzas)
					.HasForeignKey(d => d.OrderId)
					.OnDelete(DeleteBehavior.ClientSetNull);

				entity.HasOne(d => d.Pizza)
					.WithMany(p => p.OrderPizzas)
					.HasForeignKey(d => d.PizzaId)
					.OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<Payment>(entity =>
			{
				entity.HasIndex(e => e.Id, "IX_Payments_Id")
					.IsUnique();

				entity.Property(e => e.DateCreated).IsRequired();

				entity.HasOne(d => d.Customer)
					.WithMany(p => p.Payments)
					.HasForeignKey(d => d.CustomerId)
					.OnDelete(DeleteBehavior.ClientSetNull);
			});

			modelBuilder.Entity<Pizza>(entity =>
			{
				entity.HasIndex(e => e.Id, "IX_Pizzas_Id")
					.IsUnique();

				entity.Property(e => e.Name).IsRequired();
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
