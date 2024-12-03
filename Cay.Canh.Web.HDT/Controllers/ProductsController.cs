using Cay.Canh.Web.HDT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cay.Canh.Web.HDT.ViewModel;

namespace Cay.Canh.Web.HDT.Controllers
{
    public class ProductsController : Controller
    {
        private readonly WebCayCanhContext _context;

        public ProductsController(WebCayCanhContext context)
        {
            _context = context;
        }

        // GET: Products
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
                    Discount = p.Discount,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            // Tạo đối tượng PaginatedList với các tham số: danh sách sản phẩm, tổng số sản phẩm, trang hiện tại, số sản phẩm trên mỗi trang
            var paginatedResult = new PaginatedList<ProductVM>(products, totalItems, page, pageSize);

            // Trả về đối tượng PaginatedList cho view
            return View(paginatedResult);
        }


        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var p = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.CartItems)
                .SingleOrDefaultAsync(m => m.ProductId == id);

            if (p == null)
            {
                return NotFound();
            }

            var result = new ProductVMDT
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                Height = p.Height,
                Quantity = p.Quantity,
                Discount = p.Discount,
                ImageUrl = p.ImageUrl,
                State = p.State,
                Size = p.Size,

                CartId = p.CartItems.FirstOrDefault()?.CartId ?? 0,
            };

            return View(result);
        }

        public async Task<IActionResult> Search(string? query, int page = 1, int pageSize = 9)
        {
            // Khởi tạo truy vấn cơ bản
            var productQuery = _context.Products.AsQueryable();

            // Nếu có từ khóa tìm kiếm, áp dụng lọc theo tên sản phẩm
            if (!string.IsNullOrWhiteSpace(query))
            {
                productQuery = productQuery.Where(p => p.ProductName.Contains(query));
            }

            // Tính toán tổng số sản phẩm và phân trang
            int totalItems = await productQuery.CountAsync();

            var products = await productQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductVM
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Discount = p.Discount,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            // Tạo đối tượng PaginatedList chứa sản phẩm đã phân trang
            var paginatedResult = new PaginatedList<ProductVM>(products, totalItems, page, pageSize);

            // Truyền đối tượng PaginatedList vào View
            return View(paginatedResult);
        }


        // GET: Products/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,Price,ImageUrl,Description,Quantity,Size,Height,State,Discount,CategoryId")] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống.");
            }
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", "Giá sản phẩm phải lớn hơn 0.");
            }
            if (product.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục.");
            }

            if (ModelState.IsValid)
            {
                product.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
                product.UpdatedDate = DateOnly.FromDateTime(DateTime.Now);

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryName"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Price,ImageUrl,Description,Quantity,Size,Height,State,Discount,CategoryId")] Product updatedProduct)
        {
            if (id != updatedProduct.ProductId)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Giữ nguyên các giá trị cũ nếu không nhập hoặc bỏ trống
                product.ProductName = !string.IsNullOrWhiteSpace(updatedProduct.ProductName) ? updatedProduct.ProductName : product.ProductName;
                product.Price = updatedProduct.Price > 0 ? updatedProduct.Price : product.Price;
                product.ImageUrl = !string.IsNullOrWhiteSpace(updatedProduct.ImageUrl) ? updatedProduct.ImageUrl : product.ImageUrl;
                product.Description = !string.IsNullOrWhiteSpace(updatedProduct.Description) ? updatedProduct.Description : product.Description;
                product.Quantity = updatedProduct.Quantity != 0 ? updatedProduct.Quantity : product.Quantity;
                product.Size = !string.IsNullOrWhiteSpace(updatedProduct.Size) ? updatedProduct.Size : product.Size;
                product.Height = updatedProduct.Height != 0 ? updatedProduct.Height : product.Height;
                product.State = !string.IsNullOrWhiteSpace(updatedProduct.State) ? updatedProduct.State : product.State;
                product.Discount = updatedProduct.Discount != 0 ? updatedProduct.Discount : product.Discount;
                product.CategoryId = updatedProduct.CategoryId != 0 ? updatedProduct.CategoryId : product.CategoryId;

                // Tự động cập nhật ngày chỉnh sửa
                product.UpdatedDate = DateOnly.FromDateTime(DateTime.Now);

                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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

            ViewData["CategoryName"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", updatedProduct.CategoryId);
            return View(updatedProduct);
        }


        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
