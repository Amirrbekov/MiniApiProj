using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Models;

[Index("Email", IsUnique = true)]
public class User
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Email { get; set; } = ""; // unique in the database
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Address { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Password { get; set; } = "";
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
