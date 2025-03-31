using DinhVanHao_2280600831_buoi3_THWeb.Extensions;
using DinhVanHao_2280600831_buoi3_THWeb.Models;
using DinhVanHao_2280600831_buoi3_THWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
                // Xử lý giỏ hàng trống...
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
                // Xử lý giỏ hàng trống...
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

            // Xóa giỏ hàng sau khi đặt hàng thành công
            HttpContext.Session.Remove(cartSessionKey);

            return View("OrderCompleted", order.Id); // Trang xác nhận hoàn thành đơn hàng
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
                return RedirectToAction("Login", "Account");
            }

            var product = await GetProductFromDatabase(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Tạo key session theo User ID để mỗi tài khoản có giỏ hàng riêng
            string cartSessionKey = $"Cart_{user.Id}";

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartSessionKey) ?? new ShoppingCart();

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity; // Cộng dồn số lượng
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            // Lưu lại giỏ hàng vào Session với key riêng từng User
            HttpContext.Session.SetObjectAsJson(cartSessionKey, cart);

            // Có thể thêm TempData để hiển thị thông báo thành công
            TempData["SuccessMessage"] = $"Đã thêm {quantity} {product.Name} vào giỏ hàng!";

            return RedirectToAction("Index");
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

        public async Task<IActionResult> UpdateCartQuantity(int productId, int quantity)
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
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    item.Quantity = quantity; 
                }

                
                HttpContext.Session.SetObjectAsJson(cartSessionKey, cart);
            }

            return RedirectToAction("Index");
        }
    }
}