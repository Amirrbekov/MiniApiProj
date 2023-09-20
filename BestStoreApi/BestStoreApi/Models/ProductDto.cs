using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Models;

public class ProductDto
{
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    [MaxLength(1000)]
    public string? Description { get; set; }
    [MaxLength(50)]
    public string? Brand { get; set; }
    [Required]
    public decimal Price { get; set; }
    public IFormFile? ImageUrl { get; set; }
    [MaxLength(100)]
    public string Category { get; set; } = null!;
}
