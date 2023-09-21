using System.ComponentModel.DataAnnotations;

namespace WebApiWithDapper.Models;

public class ProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Brand { get; set; } = "";
    [Required]
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    [Required]
    public decimal Price { get; set; } = 0;
}
