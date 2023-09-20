using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreApi.Controllers;

[Authorize(Roles = "admin")]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public UsersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetUsers(int? page)
    {
        if (page == null)
        {
            page = 1;
        }

        int pageSize = 5;
        int totalPage = 0;

        decimal count = _db.Users.Count();
        totalPage = (int)Math.Ceiling(count / pageSize);

        var users = _db.Users.OrderByDescending(u => u.Id).Skip((int)(page-1) * pageSize).Take(pageSize).ToList();

        List<UserProfileDto> userProfiles = new();
        foreach (var user in users)
        {
            var userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            userProfiles.Add(userProfileDto);
        }

        var response = new
        {
            Users = userProfiles,
            TotalPages = totalPage,
            PageSize = pageSize,
            Page = page
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }
        var userProfileDto = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(userProfileDto);
    }
}
