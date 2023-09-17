using EFCodeFirst.Models;
using EFCodeFirst.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer("Name=ConnectionStrings:Default");
});

var app = builder.Build();

app.MapGet("/", (ApplicationDbContext context) =>
{
    // create
    var product = new Product()
    {
        Name = "Iphone 15",
        Brand = "Apple",
        Category = "Phone",
        CreatedAt = DateTime.Now,
        Description = "Description",
        Price = 879
    };

    context.Products.Add(product);
    context.SaveChanges();

    //read products

    var products = context.Products.ToList();

    return products;
});

app.Run();
