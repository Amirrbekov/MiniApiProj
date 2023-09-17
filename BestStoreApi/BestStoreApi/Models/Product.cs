using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Models;

public class Product
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    [MaxLength(50)]
    public string? Brand { get; set; }
    [Precision(16,2)]
    public decimal Price { get; set; }
    [MaxLength(100)]
    public string ImageUrl { get; set; } = null!;
    [MaxLength(100)]
    public string Category { get; set; } = null!;
    public DateTime CreatedAt { get;set;} = DateTime.Now;
}
