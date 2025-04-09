using DinhVanHao_2280600831_buoi3_THWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DinhVanHao_2280600831_buoi3_THWeb.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);

        // Thêm phương thức tìm kiếm
        Task<IEnumerable<Product>> SearchAsync(string searchQuery);
    }
}
