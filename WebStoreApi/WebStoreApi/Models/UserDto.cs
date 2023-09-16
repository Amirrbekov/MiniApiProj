using System.ComponentModel.DataAnnotations;

namespace WebStoreApi.Models;

public class UserDto
{
    [Required]
    public string FirstName { get; set; } = "";
    [Required(ErrorMessage = "Please provide your Last Name")]
    public string LastName { get; set; } = "";
    [Required, EmailAddress]
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    [Required, MinLength(5, ErrorMessage = "The Address should be at least 5 characters"), MaxLength(100, ErrorMessage = "The Address should be less than 100 chatacters")]
    public string Address { get; set; } = "";
}
