using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCayCanh.HDT.Data;

namespace WebCayCanh.HDT.Controllers
{
    public class ThamchieuxController : Controller
    {
        private readonly WebHdtContext _context;

        public ThamchieuxController(WebHdtContext context)
        {
            _context = context;
        }

        // GET: Thamchieux
        public async Task<IActionResult> Index()
        {
            return View(await _context.Thamchieus.ToListAsync());
        }

        // GET: Thamchieux/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thamchieu = await _context.Thamchieus
                .FirstOrDefaultAsync(m => m.MaDh == id);
            if (thamchieu == null)
            {
                return NotFound();
            }

            return View(thamchieu);
        }

        // GET: Thamchieux/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Thamchieux/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDh,MaSp")] Thamchieu thamchieu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(thamchieu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(thamchieu);
        }

        // GET: Thamchieux/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thamchieu = await _context.Thamchieus.FindAsync(id);
            if (thamchieu == null)
            {
                return NotFound();
            }
            return View(thamchieu);
        }

        // POST: Thamchieux/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDh,MaSp")] Thamchieu thamchieu)
        {
            if (id != thamchieu.MaDh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thamchieu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThamchieuExists(thamchieu.MaDh))
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
            return View(thamchieu);
        }

        // GET: Thamchieux/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thamchieu = await _context.Thamchieus
                .FirstOrDefaultAsync(m => m.MaDh == id);
            if (thamchieu == null)
            {
                return NotFound();
            }

            return View(thamchieu);
        }

        // POST: Thamchieux/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thamchieu = await _context.Thamchieus.FindAsync(id);
            if (thamchieu != null)
            {
                _context.Thamchieus.Remove(thamchieu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ThamchieuExists(int id)
        {
            return _context.Thamchieus.Any(e => e.MaDh == id);
        }
    }
}
