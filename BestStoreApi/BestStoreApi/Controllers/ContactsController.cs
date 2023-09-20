using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BestStoreApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContactsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly EmailService _emailService;

    public ContactsController(ApplicationDbContext db, EmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    [HttpGet("subject")]
    public IActionResult GetSubjects()
    {
        var subjects = _db.Subjects.ToList();
        return Ok(subjects);
    }
    [Authorize(Roles = "admin")]
    [HttpGet]
    public IActionResult GetContacts(int? page)
    {
        if (page == null || page < 1)
        {
            page = 1;
        }

        int pageSize = 5;
        int totalPages = 0;

        decimal count = _db.Contacts.Count();
        totalPages = (int)Math.Ceiling(count / pageSize);

        var contacts = _db.Contacts
            .Include(c => c.Subject)
            .OrderByDescending(c => c.Id)
            .Skip((int)(page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = new
        {
            Contact = contacts,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page
        };

        return Ok(response);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("{id}")]
    public IActionResult GetContact(int id) 
    { 
        var contact = _db.Contacts.Include(c => c.Subject).FirstOrDefault(x => x.Id == id);

        if (contact == null)
        {
            return NotFound();
        }

        return Ok(contact);
    }

    [HttpPost]
    public IActionResult CreateContact(ContactDto contactDto)
    {
        var subject = _db.Subjects.FirstOrDefault(x => x.Id == contactDto.SubjectId);
        if (subject == null)
        {
            ModelState.AddModelError("Subject", "Please select a valid subject");
            return BadRequest(ModelState);
        }

        Contact contact = new()
        {
            FirstName = contactDto.FirstName,
            LastName = contactDto.LastName,
            Email = contactDto.Email,
            Phone = contactDto.Phone ?? "",
            Subject = subject,
            Message = contactDto.Message,
            CreatedAt = DateTime.Now,
        };

        _db.Contacts.Add(contact);

        _db.SaveChanges();

        // send confirmation email
        string emailSubject = "Contact Confirmation";
        string username = contactDto.FirstName + " " + contactDto.LastName;
        string emailMessage = "Dear" + username + "\n" +
            "We recieved your message. Thank you for contacting us.\n" +
            "Our team will contact you very soon:\n" +
            "Best Regars\n" +
            "Your Message:\n" + contactDto.Message;

        _emailService.SendEmail(emailSubject, contact.Email, username, emailMessage).Wait();

        return Ok(contact);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateContact(int id, ContactDto contactDto)
    {
        var subject = _db.Subjects.FirstOrDefault(x => x.Id == contactDto.SubjectId);

        if (subject == null)
        {
            ModelState.AddModelError("Subject", "Please select a valid subject");
            return BadRequest(ModelState);
        }

        var contact = _db.Contacts.FirstOrDefault(x => x.Id == id);

        if (contact == null)
        {
            return NotFound();
        }
        contact.FirstName = contactDto.FirstName;
        contact.LastName = contactDto.LastName;
        contact.Email = contactDto.Email;
        contact.Phone = contactDto.Phone ?? "";
        contact.Subject = subject;
        contact.Message = contactDto.Message;
        
        _db.SaveChanges();

        return Ok(contact);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteContact(int id)
    {
        // Method 1
        //var contact = _db.Contacts.FirstOrDefault(_ => _.Id == id);
        //if(contact == null)
        //{
        //    return NotFound();
        //}

        //_db.Contacts.Remove(contact);
        //_db.SaveChanges();

        //return Ok();

        // Method 2
        try
        {
            Contact contact = new() { Id = id, Subject = new() };
            _db.Contacts.Remove(contact);
            _db.SaveChanges();

        }
        catch (Exception)
        {
            return NotFound();
        }

        return Ok();
    }
}
