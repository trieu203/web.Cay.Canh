using Cay.Canh.Web.HDT.Data;
using Cay.Canh.Web.HDT.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cay.Canh.Web.HDT.Models.Components
{
    public class MenuCategory : ViewComponent
    {
        private readonly WebCayCanhContext db;
        public MenuCategory(WebCayCanhContext context) => db = context;

        public IViewComponentResult Invoke()
        {
            var data = db.Categories
                .Include(ca => ca.Products)
                .Select(ca => new MenuCategoryVM
                {
                    CategoryId = ca.CategoryId,
                    CategoryName = ca.CategoryName,
                    SoLuong = ca.Products.Sum(ps => ps.Quantity),
                    Tong = db.Products.Sum(ps => ps.Quantity)
                }).ToList();

            return View("_MenuCategory", data);
        }
    }
}
