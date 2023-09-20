using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHost;

    private readonly List<string> listCategory = new()
    {
        "Phones", "Computers", "Accessories", "Printers", "Cameras", "Other"
    };

    public ProductsController(ApplicationDbContext db, IWebHostEnvironment webHost)
    {
        _db = db;
        _webHost = webHost;
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        return Ok(listCategory);
    }

    [HttpGet]
    public IActionResult GetProducts(string? search, string? category, int? minPrice,
        int? maxPrice, string? sort, string? order, int? page)
    {
        IQueryable<Product> query = _db.Products;

        // search functionality
        if (search != null)
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        if (category != null)
        {
            query = query.Where(p => p.Category == category);
        }

        if (minPrice != null)
        {
            query = query.Where(p => p.Price >= minPrice);
        }

        if (maxPrice != null)
        {
            query = query.Where(p => p.Price <= maxPrice);
        }

        // sort functionality
        if (sort == null) sort = "id";
        if (order == null || order != "asc") order = "desc";

        if (sort.ToLower() == "name")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Name);
            }
            else
            {
                query = query.OrderByDescending(p => p.Name);
            }
        }
        else if (sort.ToLower() == "brand")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Brand);
            }
            else
            {
                query = query.OrderByDescending(p => p.Brand);
            }
        }
        else if (sort.ToLower() == "category")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Category);
            }
            else
            {
                query = query.OrderByDescending(p => p.Category);
            }
        }
        else if (sort.ToLower() == "price")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else
            {
                query = query.OrderByDescending(p => p.Price);
            }
        }
        else if (sort.ToLower() == "date")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.CreatedAt);
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
        }
        else
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Id);
            }
            else
            {
                query = query.OrderByDescending(p => p.Id);
            }
        }

        // pagination functionality
        if (page == null || page < 1) page = 1;

        int pageSize = 5;
        int totalPages = 0;

        decimal count = query.Count();
        totalPages = (int)Math.Ceiling(count / pageSize);

        query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);

        var products = query.ToList();

        var response = new
        {
            Products = products,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page,
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        var product = _db.Products.FirstOrDefault(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult CreateProduct([FromForm] ProductDto productDto)
    {
        if (!listCategory.Contains(productDto.Category))
        {
            ModelState.AddModelError("Category", "Please select a valid category");
            return BadRequest(ModelState);
        }

        if (productDto.ImageUrl == null)
        {
            ModelState.AddModelError("ImageFile", "The Image File is required");
            return BadRequest(ModelState);
        }
        // save the image on the server

        string imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        imageFileName += Path.GetExtension(productDto.ImageUrl.FileName);

        string imageFolder = _webHost.WebRootPath + "/images/products/";

        using (var stream = System.IO.File.Create(imageFolder + imageFileName))
        {
            productDto.ImageUrl.CopyTo(stream);
        }

        // save product in the database

        Product product = new()
        {
            Name = productDto.Name,
            Brand = productDto.Brand,
            Category = productDto.Category,
            Price = productDto.Price,
            Description = productDto.Description ?? "",
            ImageUrl = imageFileName,
            CreatedAt = DateTime.Now,
        };

        _db.Products.Add(product);
        _db.SaveChanges();

        return Ok(product);
    }
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, [FromForm] ProductDto productDto)
    {
        if (!listCategory.Contains(productDto.Category))
        {
            ModelState.AddModelError("Category", "Please select a valid category");
            return BadRequest(ModelState);
        }

        var product = _db.Products.FirstOrDefault(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        string imageFileName = product.ImageUrl;
        if (productDto.ImageUrl != null)
        {
            // save the image on the server

            imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            imageFileName += Path.GetExtension(productDto.ImageUrl.FileName);

            string imagesFolder = _webHost.WebRootPath + "/images/products/";

            using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
            {
                productDto.ImageUrl.CopyTo(stream);
            }

            //delete the old image
            System.IO.File.Delete(imagesFolder + product.ImageUrl);
        }

        product.Name = productDto.Name;
        product.Brand = productDto.Brand;
        product.Price = productDto.Price;
        product.Category = productDto.Category;
        product.Description = productDto.Description ?? "";
        product.ImageUrl = imageFileName;

        _db.SaveChanges();

        return Ok(product);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = _db.Products.FirstOrDefault(_ => _.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        // delete the image on the server
        string imageFolder = _webHost.WebRootPath + "/images/products/";
        System.IO.File.Delete(imageFolder + product.ImageUrl);

        //delete the product from the database
        _db.Products.Remove(product);
        _db.SaveChanges();

        return Ok();
    }

}
