using BestStoreApi.Models;
using BestStoreApi.Services;
using EllipticCurve.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public OrdersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetOrders(int? page)
    {
        int userId = JwtReader.GetUserId(User);
        string role = _db.Users.Find(userId)?.Role ?? ""; /*JwtReader.GetUserRole(User);*/

        IQueryable<Order> query = _db.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.Product);

        if (role != "admin")
        {
            query = query.Where(o => o.UserId == userId);
        }

        query = query.OrderByDescending(o => o.Id);

        if (page == null || page < 1)
        {
            page = 1;
        }

        int pageSize = 5;
        int totalPages = 0;

        decimal count = query.Count();
        totalPages = (int)Math.Ceiling(count / pageSize);

        query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);

        var orders = query.ToList();

        foreach (var order in orders)
        {
            foreach (var item in order.OrderItems)
            {
                item.Order = null;
            }

            order.User.Password = "";
        }

        var response = new
        {
            Orders = orders,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetOrder(int id)
    {
        int userId = JwtReader.GetUserId(User);
        string role = _db.Users.Find(userId)?.Role ?? "";

        Order? order = null;

        if (role == "admin")
        {
            order = _db.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefault(o => o.Id == id);
        }
        else
        {
            order = _db.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefault(o => o.Id == id && o.UserId == userId);
        }

        if (order == null)
        {
            return NotFound();
        }

        // get rid of the object cycle
        foreach (var item in order.OrderItems)
        {
            item.Order = null;
        }

        order.User.Password = "";

        return Ok(order);   
    }


    [Authorize]
    [HttpPost]
    public IActionResult CreateOrder(OrderDto orderDto)
    {
        if (!OrderHelper.PaymentMethods.ContainsKey(orderDto.PaymentMethod))
        {
            ModelState.AddModelError("Payment Method", "Please select a valid payment method");
            return BadRequest(ModelState);
        }

        int userId = JwtReader.GetUserId(User);
        var user = _db.Users.FirstOrDefault(x => x.Id == userId);
        if (user == null)
        {
            ModelState.AddModelError("Order", "Unable to create the order");
            return BadRequest(ModelState);
        }

        var productDictionary = OrderHelper.GetProductDictionary(orderDto.ProductIdentifiers);

        Order order = new Order();

        order.UserId = userId;
        order.CreatedAt = DateTime.Now;
        order.ShippingFee = OrderHelper.ShippingFee;
        order.DeliveryAddress = orderDto.DeliveryAddress;
        order.PaymentMethod = orderDto.PaymentMethod;
        order.PaymentStatus = OrderHelper.PaymentStatuses[0]; // Pending
        order.OrderStatus = OrderHelper.OrderStatues[0]; // Created

        foreach (var pair in productDictionary)
        {
            int productId = pair.Key;
            var product = _db.Products.Find(productId);
            if (product == null)
            {
                ModelState.AddModelError("Product", "Product with id " + productId + " is not available");
                return BadRequest(ModelState);
            }

            var orderItem = new OrderItem();
            orderItem.ProductId = productId;
            orderItem.Quantity = pair.Value;
            orderItem.UnitPrice = product.Price;


            order.OrderItems.Add(orderItem);
        }

        if (order.OrderItems.Count < 1)
        {
            ModelState.AddModelError("Order", "Unable to create the order");
            return BadRequest(ModelState);
        }

        _db.Orders.Add(order);
        _db.SaveChanges();

        foreach (var item in order.OrderItems)
        {
            item.Order = null;
        }

        order.User.Password = "";

        return Ok(order);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateOrder(int id, string? paymentStatus, string? orderStatus)
    {
        if (orderStatus == null && paymentStatus == null)
        {
            ModelState.AddModelError("Update Order", "There is nothing to update");
            return BadRequest(ModelState);
        }
        if (paymentStatus != null && !OrderHelper.PaymentStatuses.Contains(paymentStatus))
        {
            ModelState.AddModelError("Payment Status", "The Payment Status is not valid");
            return BadRequest(ModelState);
        }
        if (orderStatus != null && !OrderHelper.OrderStatues.Contains(orderStatus))
        {
            ModelState.AddModelError("Order Status", "The Order Status is not valid");
            return BadRequest(ModelState);
        }

        var order = _db.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        if (paymentStatus != null)
        {
            order.PaymentMethod = paymentStatus;
        }
        if (orderStatus != null)
        {
            order.OrderStatus = orderStatus;
        }

        _db.SaveChanges();

        return Ok(order);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteOrder(int id)
    {
        var order = _db.Orders.FirstOrDefault(p => p.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        _db.Orders.Remove(order);
        _db.SaveChanges();

        return Ok(order);
    }
}
