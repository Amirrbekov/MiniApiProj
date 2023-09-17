using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EFCodeFirst.Models;

public class Product
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(100)]
    public string? Description { get; set; }
    [MaxLength(100)]
    public string? Brand { get; set; }
    [Precision(16, 2)]
    public decimal Price { get; set; }
    [MaxLength(100)]
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
