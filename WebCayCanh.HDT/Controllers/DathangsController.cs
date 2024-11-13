using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCayCanh.HDT.Data;

namespace WebCayCanh.HDT.Controllers
{
    public class DathangsController : Controller
    {
        private readonly WebHdtContext _context;

        public DathangsController(WebHdtContext context)
        {
            _context = context;
        }

        // GET: Dathangs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Dathangs.ToListAsync());
        }

        // GET: Dathangs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dathang = await _context.Dathangs
                .FirstOrDefaultAsync(m => m.MaKh == id);
            if (dathang == null)
            {
                return NotFound();
            }

            return View(dathang);
        }

        // GET: Dathangs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dathangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKh,MaGh")] Dathang dathang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dathang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dathang);
        }

        // GET: Dathangs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dathang = await _context.Dathangs.FindAsync(id);
            if (dathang == null)
            {
                return NotFound();
            }
            return View(dathang);
        }

        // POST: Dathangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKh,MaGh")] Dathang dathang)
        {
            if (id != dathang.MaKh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dathang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DathangExists(dathang.MaKh))
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
            return View(dathang);
        }

        // GET: Dathangs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dathang = await _context.Dathangs
                .FirstOrDefaultAsync(m => m.MaKh == id);
            if (dathang == null)
            {
                return NotFound();
            }

            return View(dathang);
        }

        // POST: Dathangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dathang = await _context.Dathangs.FindAsync(id);
            if (dathang != null)
            {
                _context.Dathangs.Remove(dathang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DathangExists(int id)
        {
            return _context.Dathangs.Any(e => e.MaKh == id);
        }
    }
}
