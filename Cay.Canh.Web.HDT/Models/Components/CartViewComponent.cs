using Cay.Canh.Web.HDT.Helper;
using Cay.Canh.Web.HDT.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace Cay.Canh.Web.HDT.Models.Components
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItemVM>>(Constand.Cart_Key) ?? new List<CartItemVM>();

            // Tính tổng số lượng và tổng giá trị giỏ hàng
            var quantity = cart.Sum(p => p.Quantity);
            var total = cart.Sum(p => p.PriceAtTime);

            // Trả về model mới chứa số lượng và tổng tiền
            return View("CartPanel", new Cartmodel
            {
                Quantity = quantity,
                Total = total
            });
        }

    }
}
