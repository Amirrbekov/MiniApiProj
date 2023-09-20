using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Models;

public class UserProfileUpdateDto
{
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    [MaxLength(50)]
    public string LastName { get; set; } = null!;
    [MaxLength(50), EmailAddress]
    public string Email { get; set; } = null!; // unique in the database
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(100)]
    public string Address { get; set; } = null!;
}
