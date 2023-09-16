using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreApi.Filters;
using WebStoreApi.Models;
using WebStoreApi.Services;

namespace WebStoreApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private static List<UserDto> ListUsers = new()
    { 
        new UserDto { FirstName = "Bill", LastName = "Gates", Email = "bill@microsoft.com", Phone = "50505050", Address = "New York, USA"},
        new UserDto { FirstName = "Valeh", LastName = "Amirbekov", Email = "valeh@gmail.com", Phone = "010101010", Address = "Baku, Azerbaijan"},
        new UserDto { FirstName = "Bob", LastName = "Smith", Email = "bob@gmail.com", Phone = "02020202", Address = "New York, USA"}
    };

    [HttpGet("info")]
    [DebugFilter]
    public IActionResult GetInfo(int? id, string? name, int? page,
        [FromServices] IConfiguration configuration,
        [FromServices] TimeServices timeServices)
    {
        if (id != null  || name is not null || page != null)
        {
            var response = new
            {
                Id = id,
                Name = name,
                Page = page,
                ErrorMessage = "The search functionality is not supported yet"
            };

            return Ok(response);
        }
        List<string> listInfo = new();
        listInfo.Add("AppName" + configuration["AppName"]);
        listInfo.Add("Language" + configuration["Language"]);
        listInfo.Add("Country" + configuration["Country"]);
        listInfo.Add("Logging" + configuration["Logging:LogLevel:Default"]);
        listInfo.Add("Date: " + timeServices.GetDate());
        listInfo.Add("Time: " + timeServices.GetTime());

        return Ok(listInfo);
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        if (ListUsers.Count > 0)
        {
            return Ok(ListUsers);
        }

        return NoContent();
    }

    [HttpGet("{id:int}")]
    public IActionResult GetUser(int id)
    {
        if (id >= 0 && id < ListUsers.Count)
        {
            return Ok(ListUsers[id]);
        }

        return NotFound();
    }

    [HttpGet("{name}")]
    public IActionResult GetUser(string name)
    {
        var user = ListUsers.FirstOrDefault(u => u.FirstName.Contains(name) || u.LastName == name);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public IActionResult AddUser(UserDto user)
    {
        // check that the email address is not authorized
        if (user.Email.Equals("user@example.com"))
        {
            ModelState.AddModelError("Email", "This Email Address is not authorized");
            return BadRequest();
        }

        ListUsers.Add(user);
        return Ok();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, UserDto user)
    {
        // check that the email address is not authorized
        if (user.Email.Equals("user@example.com"))
        {
            ModelState.AddModelError("Email", "This Email Address is not authorized");
            return BadRequest();
        }

        if (id >= 0 && id < ListUsers.Count)
        {
            ListUsers[id] = user;
        }
        return Ok();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        if (id >= 0 && id < ListUsers.Count)
        {
            ListUsers.RemoveAt(id);
        }
        return NoContent();
    }
}
