using System.Diagnostics;
using Cay.Canh.Web.HDT.Data;
using Cay.Canh.Web.HDT.Models;
using Cay.Canh.Web.HDT.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cay.Canh.Web.HDT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WebCayCanhContext _context;

        public HomeController(WebCayCanhContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? Category, int page = 1, int pageSize = 9)
        {
            var productQuery = _context.Products.AsQueryable();

            // Nếu có Category, lọc theo CategoryId
            if (Category.HasValue)
            {
                productQuery = productQuery.Where(p => p.CategoryId == Category.Value);
            }

            // Lấy tổng số sản phẩm
            int totalItems = await productQuery.CountAsync();

            // Lấy danh sách sản phẩm với phân trang
            var products = await productQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductVM
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    State = p.State,
                    Discount = p.Discount,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            // Tạo đối tượng PaginatedList với các tham số: danh sách sản phẩm, tổng số sản phẩm, trang hiện tại, số sản phẩm trên mỗi trang
            var paginatedResult = new PaginatedList<ProductVM>(products, totalItems, page, pageSize);

            // Trả về đối tượng PaginatedList cho view
            return View(paginatedResult);
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
