using DinhVanHao_2280600831_buoi3_THWeb.Models;
using DinhVanHao_2280600831_buoi3_THWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DinhVanHao_2280600831_buoi3_THWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra và lưu ảnh
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                // Kiểm tra danh mục có tồn tại không
                var categoryExists = await _categoryRepository.GetByIdAsync(product.CategoryId);
                if (categoryExists == null)
                {
                    ModelState.AddModelError("CategoryId", "Danh mục không tồn tại. Vui lòng chọn danh mục hợp lệ.");
                    var categories = await _categoryRepository.GetAllAsync();
                    ViewBag.Categories = new SelectList(categories, "Id", "Name");
                    return View(product);
                }

                // Lưu sản phẩm vào cơ sở dữ liệu
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, trả về View với thông báo lỗi
            var categoriesList = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categoriesList, "Id", "Name");



            return View(product);
        }

        public async Task<IActionResult> DSSP(string searchQuery)
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = await _productRepository.SearchAsync(searchQuery);
            }
            else
            {
                products = await _productRepository.GetAllAsync();
            }

            ViewBag.CurrentFilter = searchQuery;
            return View(products);
        }



        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile? imageUrl)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Nếu không có ảnh mới, giữ ảnh cũ
            if (imageUrl == null)
            {
                product.ImageUrl = existingProduct.ImageUrl;
            }
            else
            {
                product.ImageUrl = await SaveImage(imageUrl);
            }

            // Cập nhật thông tin sản phẩm
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.ImageUrl = product.ImageUrl;

            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View(product);
            }

            await _productRepository.UpdateAsync(existingProduct);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Display1(int id)
        {
            // Thay thế _context bằng _productRepository
            var product = await _productRepository.GetByIdAsync(id);

            // Kiểm tra nếu sản phẩm không tồn tại
            if (product == null)
            {
                return NotFound(); // Nếu sản phẩm không tồn tại, trả về lỗi 404
            }

            // Trả về view với thông tin sản phẩm
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
