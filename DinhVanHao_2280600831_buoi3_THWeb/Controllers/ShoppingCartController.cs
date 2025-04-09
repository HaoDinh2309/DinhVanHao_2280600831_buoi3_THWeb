using DinhVanHao_2280600831_buoi3_THWeb.Extensions;
using DinhVanHao_2280600831_buoi3_THWeb.Models;
using DinhVanHao_2280600831_buoi3_THWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DinhVanHao_2280600831_buoi3_THWeb.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string cartSessionKey = $"Cart_{user.Id}";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartSessionKey);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string cartSessionKey = $"Cart_{user.Id}";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartSessionKey);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(cartSessionKey);

            return View("OrderCompleted", order.Id);
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { count = 0 });
            }

            string cartSessionKey = $"Cart_{user.Id}";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartSessionKey) ?? new ShoppingCart();

            return Json(new { count = cart.Items.Sum(i => i.Quantity) });
        }

        [Authorize]
public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "You need to log in first!" });
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found!" });
            }

            // Lấy ảnh của sản phẩm từ bảng ProductImages
            var imageUrl = product.ImageUrl;  // Lấy URL ảnh từ thuộc tính ImageUrl

            string cartSessionKey = $"Cart_{user.Id}";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartSessionKey) ?? new ShoppingCart();

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;  // Cộng dồn số lượng
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = imageUrl  // Lưu URL ảnh vào giỏ hàng
                });
            }

            HttpContext.Session.SetObjectAsJson(cartSessionKey, cart);

            int totalQuantity = cart.Items.Sum(i => i.Quantity);
            return Json(new { success = true, message = $"Added {quantity} {product.Name} to cart!", totalQuantity = totalQuantity });
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string cartSessionKey = $"Cart_{user.Id}";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartSessionKey) ?? new ShoppingCart();

            // Gỡ lỗi: in ra đường dẫn ảnh của các sản phẩm trong giỏ hàng
            foreach (var item in cart.Items)
            {
                System.Diagnostics.Debug.WriteLine($"Product: {item.Name}, Image URL: {item.ImageUrl}");
            }

            return View(cart);
        }

        private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }

        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string cartSessionKey = $"Cart_{user.Id}";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartSessionKey);

            if (cart != null)
            {
                cart.RemoveItem(productId);
                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson(cartSessionKey, cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "You need to log in first!" });
            }

            string cartSessionKey = $"Cart_{user.Id}";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartSessionKey);

            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    // Kiểm tra số lượng hợp lệ (lớn hơn hoặc bằng 1)
                    if (quantity > 0)
                    {
                        item.Quantity = quantity;  // Cập nhật số lượng của sản phẩm trong giỏ hàng
                    }
                    else
                    {
                        // Nếu số lượng <= 0, xóa sản phẩm khỏi giỏ hàng
                        cart.Items.Remove(item);
                    }
                }

                // Lưu lại giỏ hàng vào session
                HttpContext.Session.SetObjectAsJson(cartSessionKey, cart);
            }

            // Tính tổng số lượng trong giỏ hàng
            int totalQuantity = cart?.Items.Sum(i => i.Quantity) ?? 0;

            return Json(new { success = true, totalQuantity = totalQuantity });
        }


    }
}
