using Microsoft.EntityFrameworkCore;
using SkladUchet1.Models;

namespace SkladUchet1.Data;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Batch> Batches { get; set; }
    public DbSet<WriteOff> WriteOffs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=sklad.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Администратор", AccessLevel = 2 },
            new Role { Id = 2, Name = "Кладовщик", AccessLevel = 1 }
        );

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, FullName = "Тестовый Пользователь", Login = "admin", PasswordHash = "123", RoleId = 1 }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Продукты питания" },
            new Category { Id = 2, Name = "Лекарства" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Молоко", CategoryId = 1, Unit = "л", Price = 80, MinShelfLifeDays = 5 },
            new Product { Id = 2, Name = "Парацетамол", CategoryId = 2, Unit = "уп", Price = 150, MinShelfLifeDays = 30 }
        );
    }
}