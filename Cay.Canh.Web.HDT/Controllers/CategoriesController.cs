using Cay.Canh.Web.HDT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cay.Canh.Web.HDT.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly WebCayCanhContext _context;

        public CategoriesController(WebCayCanhContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,Description,CreatedDate,UpdatedDate")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(category.CategoryName))
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục không được để trống.");
                }
                if (string.IsNullOrEmpty(category.Description))
                {
                    ModelState.AddModelError("Description", "Mô tả không được để trống.");
                }

                if (ModelState.IsValid) // Nếu không có lỗi, tiến hành lưu
                {
                    category.CreatedDate = DateOnly.FromDateTime(DateTime.Now);  // Chuyển đổi từ DateTime sang DateOnly
                    category.UpdatedDate = DateOnly.FromDateTime(DateTime.Now);  // Chuyển đổi từ DateTime sang DateOnly

                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,Description,CreatedDate,UpdatedDate")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id);

                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    // Sao chép dữ liệu từ category vào existingCategory
                    existingCategory.CategoryName = string.IsNullOrEmpty(category.CategoryName) ? existingCategory.CategoryName : category.CategoryName;
                    existingCategory.Description = string.IsNullOrEmpty(category.Description) ? existingCategory.Description : category.Description;
                    existingCategory.UpdatedDate = DateOnly.FromDateTime(DateTime.Now);

                    // Đính kèm thực thể đã tồn tại để cập nhật
                    _context.Entry(existingCategory).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }


        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
