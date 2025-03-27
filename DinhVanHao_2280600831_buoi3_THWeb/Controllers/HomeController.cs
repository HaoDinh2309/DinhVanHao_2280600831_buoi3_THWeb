using System.Diagnostics;
using DinhVanHao_2280600831_buoi3_THWeb.Models;
using DinhVanHao_2280600831_buoi3_THWeb.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DinhVanHao_2280600831_buoi3_THWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Hi?n th? danh sách s?n ph?m
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
       

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
