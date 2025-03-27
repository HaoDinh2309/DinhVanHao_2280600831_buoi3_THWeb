using DinhVanHao_2280600831_buoi3_THWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOrderRepository
{
    List<Order> GetAllOrders(); // Lấy danh sách đơn hàng
    Order GetOrderById(int id); // Lấy đơn hàng theo ID
}

