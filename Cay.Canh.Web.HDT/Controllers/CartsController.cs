using Cay.Canh.Web.HDT.Data;
using Cay.Canh.Web.HDT.ViewModel;
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
            //product.Quantity -= quantity;

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


        //Checkout

        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Dangnhap", "Users");
            }

            // Lấy thông tin người dùng từ CSDL
            var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Carts");
            }

            // Lấy thông tin giỏ hàng
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == int.Parse(userId) && c.IsActive);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Carts");
            }

            // Điền thông tin người dùng vào ViewModel
            var model = new CheckoutViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.Sdt,
                ShippingAddress = user.Address,
                CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    ProductName = ci.Product?.ProductName ?? "Không có tên sản phẩm",
                    Quantity = ci.Quantity,
                    PriceAtTime = ci.PriceAtTime,
                    Discount = ci.Product?.Discount ?? 0
                }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để đặt hàng.";
                return RedirectToAction("Dangnhap", "Users");
            }

            // Lấy giỏ hàng
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId) && c.IsActive);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Carts");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin nhập không hợp lệ. Vui lòng kiểm tra lại.";
                // Giữ lại thông tin giỏ hàng để hiển thị trong View
                model.CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    ProductName = ci.Product?.ProductName ?? "Không có tên sản phẩm",
                    Quantity = ci.Quantity,
                    PriceAtTime = ci.PriceAtTime,
                    Discount = ci.Product?.Discount ?? 0
                }).ToList();
                return View(model);
            }

            // Kiểm tra nếu thông tin người dùng còn thiếu
            if (string.IsNullOrEmpty(model.FullName) ||
                string.IsNullOrEmpty(model.Email) ||
                string.IsNullOrEmpty(model.PhoneNumber) ||
                string.IsNullOrEmpty(model.ShippingAddress))
            {
                TempData["ErrorMessage"] = "Thông tin thanh toán không được để trống.";
                model.CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    ProductName = ci.Product?.ProductName ?? "Không có tên sản phẩm",
                    Quantity = ci.Quantity,
                    PriceAtTime = ci.PriceAtTime,
                    Discount = ci.Product?.Discount ?? 0
                }).ToList();
                return View(model);
            }

            // Tạo đơn hàng
            var order = new Order
            {
                UserId = int.Parse(userId),
                OrderDate = DateTime.Now,
                TotalAmount = cart.CartItems.Sum(ci => ci.PriceAtTime * ci.Quantity) -
                              cart.CartItems.Sum(ci => (ci.PriceAtTime * ci.Quantity * ci.Product?.Discount ?? 0) / 100) + 50000,
                OrderStatus = "Pending",
                ShippingAddress = model.ShippingAddress,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Thêm các sản phẩm vào OrderItems
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.PriceAtTime,
                    Size = cartItem.Size
                };
                _context.OrderItems.Add(orderItem);

                // Trừ số lượng sản phẩm trong kho
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Quantity -= cartItem.Quantity;
                    _context.Products.Update(product);
                }
            }

            // Xóa các mục trong giỏ hàng
            _context.CartItems.RemoveRange(cart.CartItems);

            // Cập nhật trạng thái giỏ hàng
            cart.IsActive = false;
            _context.Carts.Update(cart);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đơn hàng của bạn đã được tạo thành công.";
            return RedirectToAction("Index", "Orders");
        }


        //update quantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItemId == id && ci.Cart.UserId == int.Parse(userId) && ci.Cart.IsActive);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            if (quantity > cartItem.Product.Quantity)
            {
                return Json(new { success = false, message = "Số lượng vượt quá số lượng trong kho." });
            }

            // Cập nhật số lượng sản phẩm
            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            // Tính toán tổng tiền giỏ hàng
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId) && c.IsActive);

            var subtotal = cart.CartItems.Sum(ci => ci.Quantity * ci.PriceAtTime);
            var total = subtotal + 100000; // Thêm phí vận chuyển

            return Json(new { success = true, subtotal, total });
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
