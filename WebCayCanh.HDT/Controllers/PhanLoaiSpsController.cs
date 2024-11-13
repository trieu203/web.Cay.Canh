using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCayCanh.HDT.Data;

namespace WebCayCanh.HDT.Controllers
{
    public class PhanLoaiSpsController : Controller
    {
        private readonly WebHdtContext _context;

        public PhanLoaiSpsController(WebHdtContext context)
        {
            _context = context;
        }

        // GET: PhanLoaiSps
        public async Task<IActionResult> Index()
        {
            return View(await _context.PhanLoaiSps.ToListAsync());
        }

        // GET: PhanLoaiSps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phanLoaiSp = await _context.PhanLoaiSps
                .FirstOrDefaultAsync(m => m.MaPhanLoai == id);
            if (phanLoaiSp == null)
            {
                return NotFound();
            }

            return View(phanLoaiSp);
        }

        // GET: PhanLoaiSps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PhanLoaiSps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhanLoai,TenPhanLoai,MoTaPl")] PhanLoaiSp phanLoaiSp)
        {
            // Kiểm tra nếu `TenPhanLoai` hoặc `MoTaPl` bị bỏ trống
            if (string.IsNullOrWhiteSpace(phanLoaiSp.TenPhanLoai))
            {
                ModelState.AddModelError("TenPhanLoai", "Tên phân loại không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(phanLoaiSp.MoTaPl))
            {
                ModelState.AddModelError("MoTaPl", "Mô tả không được để trống.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(phanLoaiSp);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phanLoaiSp);
        }


        // GET: PhanLoaiSps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phanLoaiSp = await _context.PhanLoaiSps.FindAsync(id);
            if (phanLoaiSp == null)
            {
                return NotFound();
            }
            return View(phanLoaiSp);
        }

        // POST: PhanLoaiSps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhanLoai,TenPhanLoai,MoTaPl")] PhanLoaiSp phanLoaiSp)
        {
            if (id != phanLoaiSp.MaPhanLoai)
            {
                return NotFound();
            }

            var existingPhanLoaiSp = await _context.PhanLoaiSps.FindAsync(id);
            if (existingPhanLoaiSp == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu thuộc tính bị bỏ trống, thì giữ giá trị cũ
            if (string.IsNullOrWhiteSpace(phanLoaiSp.TenPhanLoai))
            {
                phanLoaiSp.TenPhanLoai = existingPhanLoaiSp.TenPhanLoai;
            }

            if (string.IsNullOrWhiteSpace(phanLoaiSp.MoTaPl))
            {
                phanLoaiSp.MoTaPl = existingPhanLoaiSp.MoTaPl;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(existingPhanLoaiSp).CurrentValues.SetValues(phanLoaiSp);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhanLoaiSpExists(phanLoaiSp.MaPhanLoai))
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
            return View(phanLoaiSp);
        }

        // GET: PhanLoaiSps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phanLoaiSp = await _context.PhanLoaiSps
                .FirstOrDefaultAsync(m => m.MaPhanLoai == id);
            if (phanLoaiSp == null)
            {
                return NotFound();
            }

            return View(phanLoaiSp);
        }

        // POST: PhanLoaiSps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phanLoaiSp = await _context.PhanLoaiSps.FindAsync(id);
            if (phanLoaiSp != null)
            {
                _context.PhanLoaiSps.Remove(phanLoaiSp);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhanLoaiSpExists(int id)
        {
            return _context.PhanLoaiSps.Any(e => e.MaPhanLoai == id);
        }
    }
}
