using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCayCanh.HDT.Data;

namespace WebCayCanh.HDT.Controllers
{
    public class DanhGiaDhsController : Controller
    {
        private readonly WebHdtContext _context;

        public DanhGiaDhsController(WebHdtContext context)
        {
            _context = context;
        }

        // GET: DanhGiaDhs
        public async Task<IActionResult> Index()
        {
            return View(await _context.DanhGiaDhs.ToListAsync());
        }

        // GET: DanhGiaDhs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGiaDh = await _context.DanhGiaDhs
                .FirstOrDefaultAsync(m => m.MaDg == id);
            if (danhGiaDh == null)
            {
                return NotFound();
            }

            return View(danhGiaDh);
        }

        // GET: DanhGiaDhs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DanhGiaDhs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDg,MaDh,NoiDungDg,ThangDiem,NgayDg")] DanhGiaDh danhGiaDh)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhGiaDh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(danhGiaDh);
        }

        // GET: DanhGiaDhs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGiaDh = await _context.DanhGiaDhs.FindAsync(id);
            if (danhGiaDh == null)
            {
                return NotFound();
            }
            return View(danhGiaDh);
        }

        // POST: DanhGiaDhs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDg,MaDh,NoiDungDg,ThangDiem,NgayDg")] DanhGiaDh danhGiaDh)
        {
            if (id != danhGiaDh.MaDg)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhGiaDh);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhGiaDhExists(danhGiaDh.MaDg))
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
            return View(danhGiaDh);
        }

        // GET: DanhGiaDhs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhGiaDh = await _context.DanhGiaDhs
                .FirstOrDefaultAsync(m => m.MaDg == id);
            if (danhGiaDh == null)
            {
                return NotFound();
            }

            return View(danhGiaDh);
        }

        // POST: DanhGiaDhs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhGiaDh = await _context.DanhGiaDhs.FindAsync(id);
            if (danhGiaDh != null)
            {
                _context.DanhGiaDhs.Remove(danhGiaDh);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DanhGiaDhExists(int id)
        {
            return _context.DanhGiaDhs.Any(e => e.MaDg == id);
        }
    }
}
