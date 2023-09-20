using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BestStoreApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    public AccountController(IConfiguration configuration, ApplicationDbContext db, IEmailService emailService)
    {
        _configuration = configuration;
        _db = db;
        _emailService = emailService;

    }

    //[HttpGet]
    //public IActionResult TestToken()
    //{
    //    User user = new() { Id = 2, Role = "Admin" };
    //    string jwt = CreateJWToken(user);
    //    var response = new { JWToken = jwt };
    //    return Ok(response);
    //}

    //[HttpPost("testEmail")]
    //public async Task<IActionResult> TestEmail(string token, string password)
    //{
    //    if (!ModelState.IsValid)
    //    {

    //    }
    //    await _emailService.SendEmail(token, password, mesage);
    //    return Ok();
    //} issue

    [HttpPost("register")]
    public IActionResult Register(UserDto userDto)
    {
        // check if the email address is alredy have or not
        var email = _db.Users.Count(u => u.Email == userDto.Email);
        if (email > 0)
        {
            ModelState.AddModelError("Email", "This Email address is alredy used");
            return BadRequest(ModelState);
        }

        // encrypt the password
        var passwordHasher = new PasswordHasher<User>();
        var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

        // create a new account
        User user = new()
        {
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Email = userDto.Email,
            Password = encryptedPassword,
            Phone = userDto.Phone ?? "",
            Address = userDto.Address,
            Role = "client",
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        var jwt = CreateJWToken(user);

        UserProfileDto userProfileDto = new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = DateTime.Now
        };

        var response = new
        {
            Token = jwt,
            User = userProfileDto
        };

        return Ok(response);
    }

    [HttpPost("login")]
    public IActionResult Login(string email, string password)
    {
        var user = _db.Users.FirstOrDefault(x => x.Email == email);
        if (user == null)
        {
            ModelState.AddModelError("Error", "Email or Password not valid");
            return BadRequest(ModelState);
        }

        // verify the password
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(new Models.User(), user.Password, password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("Password", "Wrong Password");
            return BadRequest(ModelState);
        }

        var jwt = CreateJWToken(user);

        UserProfileDto userProfileDto = new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = DateTime.Now
        };

        var response = new
        {
            Token = jwt,
            User = userProfileDto
        };

        return Ok(response);
    }


    private string CreateJWToken(User user)
    {
        List<Claim> claims = new()
        {
            new Claim("id", "" + user.Id),
            new Claim("role", user.Role)
        };

        string secretKey = _configuration["JwtSettings:Key"]!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"], audience: _configuration["JwtSettings:Audience"], claims: claims,
            expires: DateTime.Now.AddDays(1), signingCredentials: creds
            );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    [HttpPost("ForgotPassword")]
    public IActionResult ForgotPassword(string email)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            return NotFound();
        }

        //delete any old password reset request
        var oldPwdReset = _db.PasswordResets.FirstOrDefault(r => r.Email == email);
        if (oldPwdReset == null)
        {
            //delete old password reset request
            _db.Remove(oldPwdReset);
        }

        // create Password Reset Token
        string token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();

        var pwdReset = new PasswordReset()
        {
            Email = email,
            Token = token,
            CreatedAt = DateTime.Now,
        };

        _db.PasswordResets.Add(pwdReset);
        _db.SaveChanges();

        //send the password Reset Token bu email to the user
        string emailSubject = "Password Reset";
        string username = user.FirstName + " " + user.LastName;
        string emailMessage = "Dear" + username + "\n" +
            "We recieved your password reset request.\n" +
            "Please copy the following token and posts it in the Passsword Reset Form:\n" +
            token + "\n\n" +
            "Best Regars\n";

        _emailService.SendEmail(emailSubject, email, username, emailMessage).Wait();

        return Ok();
    }

    [HttpPost("ResetPassword")]
    public IActionResult ResetPassword(string token, string password)
    {
        var pwdReset = _db.PasswordResets.FirstOrDefault(r => r.Token == token);

        if (pwdReset == null)
        {
            ModelState.AddModelError("Token", "Wrong or Expired Token");
            return BadRequest(ModelState);
        }

        var user = _db.Users.FirstOrDefault(u => u.Email == pwdReset.Email);
        if (user == null)
        {
            ModelState.AddModelError("Token", "Wrong or Expired Token");
            return BadRequest(ModelState);
        }

        // encrypt password
        var passwordHasher = new PasswordHasher<User>();
        string encryptedPassword = passwordHasher.HashPassword(new User(), password);

        // save the new encrypted password
        user.Password = encryptedPassword;

        //delete the token
        _db.PasswordResets.Remove(pwdReset);

        _db.SaveChanges();

        return Ok();
    }

    [Authorize]
    [HttpGet("Profile")]
    public IActionResult GetProfile()
    {
        int id = JwtReader.GetUserId(User);

        var user = _db.Users.Find(id);
        if (user == null)
        {
            return Unauthorized();
        }

        UserProfileDto profile = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
        };

        return Ok(profile);
    }

    [Authorize]
    [HttpPut("updateProfile")]
    public IActionResult UpdateProfile(UserProfileUpdateDto userProfileUpdateDto)
    {
        int id = JwtReader.GetUserId(User);

        var user = _db.Users.Find(id);
        if (user == null)
        {
            return Unauthorized();
        }

        // update the user profile
        user.FirstName = userProfileUpdateDto.FirstName;
        user.LastName = userProfileUpdateDto.LastName;
        user.Email = userProfileUpdateDto.Email;
        user.Phone = userProfileUpdateDto.Phone ?? "";
        user.Address = userProfileUpdateDto.Address;

        _db.SaveChanges();

        UserProfileDto profile = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
        };

        return Ok(profile);
    }
    [Authorize]
    [HttpPut("updatePassword")]
    public IActionResult UpdatePassword([Required, MinLength(8)] string password)
    {
        int id = JwtReader.GetUserId(User);

        var user = _db.Users.Find(id);
        if (user == null)
        {
            return Unauthorized();
        }

        // encrypt password
        var passwordHasher = new PasswordHasher<User>();
        string encryptedPassword = passwordHasher.HashPassword(new Models.User(), password);

        //update the user password
        user.Password = encryptedPassword;

        _db.SaveChanges();

        return Ok();
    }

    //[Authorize]
    //[HttpGet("GetTokenClaims")]
    //public IActionResult GetTokenClaims()
    //{
    //    var identity = User.Identity as ClaimsIdentity; /*(ClaimsIdentity)User.Identity;*/
    //    if (identity != null)
    //    {
    //        Dictionary<string, string> claims = new();
    //        foreach (Claim claim in identity.Claims)
    //        {
    //            claims.Add(claim.Type, claim.Value);
    //        }

    //        return Ok(claims);
    //    }

    //    return Ok();
    //}

    //[Authorize]
    //[HttpGet("AuthorizeAithenticatedUsers")]
    //public IActionResult AuthorizeAithenticatedUsers()
    //{
    //    return Ok("You are Authorized");
    //}

    //[Authorize(Roles = "admin")]
    //[HttpGet("AuthorizeAdmin")]
    //public IActionResult AuthorizeAdmin()
    //{
    //    return Ok("You are Authorized");
    //}

    //[Authorize(Roles = "admin, seller")]
    //[HttpGet("AuthorizeAdminAndSeller")]
    //public IActionResult AuthorizeAdminAndSeller()
    //{
    //    return Ok("You are Authorized");
    //}
}