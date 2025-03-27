using DinhVanHao_2280600831_buoi3_THWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Order> GetAllOrders()
    {
        return _context.Orders.ToList(); // Trả về danh sách đơn hàng từ DB
    }

    public Order GetOrderById(int id)
    {
        return _context.Orders.FirstOrDefault(o => o.Id == id);
    }
}

