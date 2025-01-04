using Cay.Canh.Web.HDT.Data;
using Cay.Canh.Web.HDT.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace Cay.Canh.Web.HDT.Models.Components
{
    public class CartViewComponent : ViewComponent
    {
        private readonly WebCayCanhContext _context;

        public CartViewComponent(WebCayCanhContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            // Lấy User ID từ Claims
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                // Nếu người dùng chưa đăng nhập, trả về giỏ hàng trống
                return View("CartPanel", new Cartmodel
                {
                    Quantity = 0
                });
            }

            // Lấy giỏ hàng từ cơ sở dữ liệu
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product) // Bao gồm thông tin sản phẩm
                .FirstOrDefault(c => c.UserId == int.Parse(userId) && c.IsActive);

            if (cart == null || !cart.CartItems.Any())
            {
                // Nếu không có giỏ hàng hoặc giỏ hàng trống
                return View("CartPanel", new Cartmodel
                {
                    Quantity = 0
                });
            }

            // Tổng hợp thông tin giỏ hàng
            var cartModel = new Cartmodel
            {
                Quantity = cart.CartItems.Sum(ci => ci.Quantity)
            };

            return View("CartPanel", cartModel);
        }
    }
}
