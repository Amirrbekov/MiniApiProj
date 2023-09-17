using EFCodeFirst.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCodeFirst.Services;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}
