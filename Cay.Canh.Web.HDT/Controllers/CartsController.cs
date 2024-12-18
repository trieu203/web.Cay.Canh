using Cay.Canh.Web.HDT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cay.Canh.Web.HDT.Controllers
{
    public class CartsController : Controller
    {
        private readonly WebCayCanhContext _context;

        public CartsController(WebCayCanhContext context)
        {
            _context = context;
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Dangnhap", "Users");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId) && c.IsActive);

            if (cart == null || !cart.CartItems.Any())
            {
                ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                return View(new List<CartItem>());
            }

            var cartItems = cart.CartItems.Select(ci => new
            {
                ci.ProductId,
                ci.ProductName,
                ci.Quantity,
                ci.PriceAtTime,
                ci.ImageUrl,
                ci.Size,
                TotalPrice = ci.Quantity * ci.PriceAtTime
            }).ToList();

            return View(cart.CartItems);
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            // 1. Kiểm tra nếu người dùng chưa đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Dangnhap", "Users");
            }

            // 2. Lấy thông tin sản phẩm từ database
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Products");
            }

            // 3. Kiểm tra số lượng trong kho
            if (quantity <= 0 || quantity > product.Quantity)
            {
                TempData["ErrorMessage"] = "Số lượng sản phẩm không hợp lệ hoặc vượt quá số lượng trong kho.";
                return RedirectToAction("Details", "Products", new { id });
            }

            // 4. Lấy thông tin giỏ hàng
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await _context.Carts
                .Include(c => c.CartItems) // Include để tránh truy vấn thừa
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // 5. Thêm hoặc cập nhật sản phẩm trong giỏ hàng
            var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == id);

            if (cartItem != null)
            {
                if (cartItem.Quantity + quantity > product.Quantity)
                {
                    TempData["ErrorMessage"] = "Số lượng sản phẩm trong giỏ vượt quá số lượng trong kho.";
                    return RedirectToAction("Details", "Products", new { id });
                }
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = id,
                    Quantity = quantity,
                    Size = product.Size,
                    ProductName = product.ProductName,
                    PriceAtTime = product.Price,
                    ImageUrl = product.ImageUrl
                };
                _context.CartItems.Add(cartItem);
            }

            // 6. Cập nhật lại số lượng trong kho
            product.Quantity -= quantity;

            // 7. Lưu thay đổi vào database
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào giỏ hàng.";
            return RedirectToAction("Index", "Carts");
        }



        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            // Lấy ID người dùng từ Claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Dangnhap", "Users");
            }

            // Tìm CartItem trong cơ sở dữ liệu dựa trên `id` và `userId`
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == id && ci.Cart.UserId == int.Parse(userId) && ci.Cart.IsActive);

            if (cartItem != null)
            {
                // Xóa CartItem khỏi cơ sở dữ liệu
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                TempData["SuccessMessage"] = "Sản phẩm đã được xóa khỏi giỏ hàng.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng.";
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> UpdateQuantity(int cartItemId, int change)
        {
            // Tìm sản phẩm trong giỏ hàng
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            // Kiểm tra số lượng tồn kho trong cơ sở dữ liệu
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            // Tính toán số lượng mới
            int newQuantity = cartItem.Quantity + change;

            if (newQuantity <= 0)
            {
                // Không cho phép số lượng nhỏ hơn 1
                cartItem.Quantity = 1;
            }
            else if (newQuantity > product.Quantity)
            {
                // Không cho phép vượt quá số lượng tồn kho
                cartItem.Quantity = product.Quantity;
            }
            else
            {
                // Cập nhật số lượng mới hợp lệ
                cartItem.Quantity = newQuantity;
            }

            // Lưu thay đổi
            _context.Update(cartItem);
            await _context.SaveChangesAsync();

            // Trả về PartialView để cập nhật dữ liệu
            return Json(new { success = true, newQuantity = cartItem.Quantity, total = cartItem.Quantity * cartItem.PriceAtTime });
        }


        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest("Số lượng phải lớn hơn 0.");
            }

            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // GET: Carts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CartId,UserId,CreatedDate,IsActive")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", cart.UserId);
            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", cart.UserId);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartId,UserId,CreatedDate,IsActive")] Cart cart)
        {
            if (id != cart.CartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.CartId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", cart.UserId);
            return View(cart);
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CartId == id);
        }
    }
}
