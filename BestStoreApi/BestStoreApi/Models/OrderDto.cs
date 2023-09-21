using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Models;

public class OrderDto
{
    [Required]
    public string ProductIdentifiers { get; set; } = "";
    [Required, MaxLength(100), MinLength(10)]
    public string DeliveryAddress { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
}
