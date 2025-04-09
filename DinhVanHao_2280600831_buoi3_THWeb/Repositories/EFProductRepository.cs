using DinhVanHao_2280600831_buoi3_THWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinhVanHao_2280600831_buoi3_THWeb.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)  // Include thông tin về category 
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        // Thêm phương thức tìm kiếm
        public async Task<IEnumerable<Product>> SearchAsync(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
                return await GetAllAsync();

            searchQuery = searchQuery.Trim(); // Loại bỏ khoảng trắng thừa

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => EF.Functions.Like(p.Name, $"%{searchQuery}%"))  // Sử dụng EF.Functions.Like để tìm kiếm không phân biệt chữ hoa/thường
                .ToListAsync();
        }
    }
}
