using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCayCanh.HDT.Data;

namespace WebCayCanh.HDT.Controllers
{
    public class SanPhamsController : Controller
    {
        private readonly WebHdtContext _context;

        public SanPhamsController(WebHdtContext context)
        {
            _context = context;
        }

        // GET: SanPhams
        public async Task<IActionResult> Index()
        {
            return View(await _context.SanPhams.ToListAsync());
        }

        // GET: SanPhams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .FirstOrDefaultAsync(m => m.MaSp == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // GET: SanPhams/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSp,MaPhanLoai,TenSp,GiaSp,SoLuongTonKho,MoTaSp")] SanPham sanPham)
        {
            // Kiểm tra nếu thuộc tính `TenSp` và `MoTaSp` bị bỏ trống
            if (string.IsNullOrWhiteSpace(sanPham.TenSp))
            {
                ModelState.AddModelError("TenSp", "Tên sản phẩm không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(sanPham.MoTaSp))
            {
                ModelState.AddModelError("MoTaSp", "Mô tả sản phẩm không được để trống.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sanPham);
        }


        // GET: SanPhams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }
            return View(sanPham);
        }

        // POST: SanPhams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSp,MaPhanLoai,TenSp,GiaSp,SoLuongTonKho,MoTaSp")] SanPham sanPham)
        {
            if (id != sanPham.MaSp)
            {
                return NotFound();
            }

            var existingSanPham = await _context.SanPhams.FindAsync(id);
            if (existingSanPham == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu thuộc tính bị bỏ trống thì giữ giá trị cũ
            if (string.IsNullOrWhiteSpace(sanPham.TenSp))
            {
                sanPham.TenSp = existingSanPham.TenSp;
            }

            if (string.IsNullOrWhiteSpace(sanPham.MoTaSp))
            {
                sanPham.MoTaSp = existingSanPham.MoTaSp;
            }

            // Giữ nguyên các giá trị cũ nếu không có thay đổi
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(existingSanPham).CurrentValues.SetValues(sanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSp))
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
            return View(sanPham);
        }


        // GET: SanPhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .FirstOrDefaultAsync(m => m.MaSp == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // POST: SanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null)
            {
                _context.SanPhams.Remove(sanPham);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SanPhamExists(int id)
        {
            return _context.SanPhams.Any(e => e.MaSp == id);
        }
    }
}
