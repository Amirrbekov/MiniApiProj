﻿using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebApiWithDapper.Models;

namespace WebApiWithDapper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly string connectionString;
    public ProductsController(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpPost]
    public IActionResult Create(ProductDto productDto)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO products " +
                    "(Name, Brand, Category, Price, Description, CreatedAt) " +
                    "OUTPUT INSERTED.*" +
                    "VALUES (@Name, @Brand, @Category, @Price, @Description, @CreatedAt)";

                var product = new Product()
                {
                    Name = productDto.Name,
                    Brand = productDto.Brand,
                    Category = productDto.Category,
                    Price = productDto.Price,
                    Description = productDto.Description,
                    CreatedAt = DateTime.Now
                };

                var newproduct = connection.QuerySingleOrDefault<Product>(sql, product);
                if (newproduct != null)
                {
                    return Ok(newproduct);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("We have an exception: \n" + ex.Message);
        }

        return BadRequest();
    }

    [HttpGet]
    public IActionResult GetProducts()
    {
        List<Product> products = new List<Product>();

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT * FROM products";
                var data = connection.Query<Product>(sql);

                products = data.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("We have an exception: \n" + ex.Message);
        }

        return Ok(products);
    }

    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT * FROM products WHERE id=@id";

                var product = connection.QuerySingleOrDefault<Product>(sql, new { Id = id });

                if (product != null)
                {
                    return Ok(product);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("We have an exception: \n" + ex.Message);
            return BadRequest();
        }

        return NotFound();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, ProductDto productDto)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "UPDATE products SET Name=@Name, Brand=@Brand, Category = @Category, " +
                    "Price=@Price, Description=@Description WHERE Id=@Id";

                var product = new Product()
                {
                    Id = id,
                    Name = productDto.Name,
                    Brand = productDto.Brand,
                    Category = productDto.Category,
                    Price = productDto.Price,
                    Description = productDto.Description,
                    CreatedAt = DateTime.Now
                };

                int count = connection.Execute(sql, product);
                if (count > 0)
                {
                    return NotFound();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("We have an exception: \n" + ex.Message);
            return BadRequest();
        }
        return GetProduct(id);
    }

    [HttpDelete]
    public IActionResult DeleteProduct(int id)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM products WHERE Id=@Id";

                int count = connection.Execute(sql, new { Id = id });
                if (count < 1)
                {
                    return NotFound();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("We have an exception: \n" + ex.Message);
            return BadRequest();
        }

        return Ok();
    }
}
