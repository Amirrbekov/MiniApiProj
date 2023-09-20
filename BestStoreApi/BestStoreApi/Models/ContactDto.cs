using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Models;

public class ContactDto
{
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    [MaxLength(50)]
    public string LastName { get; set; } = null!;
    [MaxLength(100)]
    public string Email { get; set; } = null!;
    [MaxLength(15)]
    public string? Phone { get; set; }
    public int SubjectId { get; set; }
    [MinLength(20), MaxLength(1000)]
    public string? Message { get; set; } = null!;
}
