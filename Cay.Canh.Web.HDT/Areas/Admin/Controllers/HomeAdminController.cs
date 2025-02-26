using Cay.Canh.Web.HDT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cay.Canh.Web.HDT.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/home")]
    public class HomeAdminController : Controller
    {
        private readonly WebCayCanhContext _context;
        private readonly ILogger<HomeAdminController> _logger;

        public HomeAdminController(WebCayCanhContext context, ILogger<HomeAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Home/Index
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            _logger.LogInformation("Truy cập trang chính Admin.");
            return View();
        }

        // GET: Admin/Home/Category
        [Route("Category")]
        public async Task<IActionResult> Category()
        {
            _logger.LogInformation("Truy cập danh mục sản phẩm.");
            try
            {
                var categories = await _context.Categories.ToListAsync();
                _logger.LogInformation("Lấy danh sách danh mục thành công. Số lượng: {Count}", categories.Count);
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục.");
                return StatusCode(500, "Đã xảy ra lỗi, vui lòng thử lại sau.");
            }

        }

        //Create Category
        [Route("CreateCategory")]
        [HttpGet]
        public async Task<IActionResult> CreateCategory()
        {
            return View();
        }

        [Route("CreateCategory")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            // Kiểm tra xem các trường bắt buộc đã được nhập hay chưa
            if (string.IsNullOrEmpty(category.CategoryName) || string.IsNullOrEmpty(category.Description))
            {
                ModelState.AddModelError("", "Tên danh mục và Mô tả không được để trống.");
            }

            // Kiểm tra ModelState
            if (ModelState.IsValid)
            {
                // Thêm CreatedDate mặc định là ngày hiện tại
                category.CreatedDate = DateOnly.FromDateTime(DateTime.Now);

                // Lưu vào cơ sở dữ liệu
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction("Category");
            }

            return View(category);
        }

        //Detail Categry
        [Route("DetailCategory")]
        [HttpGet]
        public async Task<IActionResult> DetailCategory(int id)
        {
            // Tìm danh mục theo ID
            var category = await _context.Categories
                                         .Include(c => c.Products) // Bao gồm sản phẩm liên quan
                                         .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Message"] = "Danh mục không tồn tại.";
                return RedirectToAction("Category");
            }

            return View(category);
        }

        //Edit Category
        [Route("EditCategory")]
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        [Route("EditCategory")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category updatedCategory)
        {
            var existingCategory = await _context.Categories.FindAsync(id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            // Kiểm tra giá trị nhập vào, nếu rỗng thì giữ lại giá trị cũ
            existingCategory.CategoryName = string.IsNullOrEmpty(updatedCategory.CategoryName)
                ? existingCategory.CategoryName
                : updatedCategory.CategoryName;

            existingCategory.Description = string.IsNullOrEmpty(updatedCategory.Description)
                ? existingCategory.Description
                : updatedCategory.Description;

            // Gán ngày cập nhật hiện tại
            existingCategory.UpdatedDate = DateOnly.FromDateTime(DateTime.Now);

            // Lưu cập nhật vào CSDL
            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return RedirectToAction("Category");
        }

        //Delete Category
        [Route("DeleteCategory")]
        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Lấy danh mục từ CSDL
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                TempData["Message"] = "Danh mục không tồn tại.";
                return RedirectToAction("Category");
            }

            return View(category);
        }

        [Route("DeleteCategory")]
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Truy vấn danh mục từ CSDL
                var category = await _context.Categories
                                             .Include(c => c.Products) // Load các sản phẩm liên quan
                                             .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                {
                    TempData["Message"] = "Danh mục không tồn tại.";
                    return RedirectToAction("Category");
                }

                // Kiểm tra nếu danh mục có sản phẩm con
                if (category.Products != null && category.Products.Any())
                {
                    TempData["Message"] = "Không thể xóa danh mục vì vẫn còn sản phẩm liên quan.";
                    return RedirectToAction("Category");
                }

                // Xóa danh mục khỏi CSDL
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Danh mục đã được xóa thành công.";
                return RedirectToAction("Category");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(ex, "Lỗi khi xóa danh mục");
                TempData["Message"] = "Có lỗi xảy ra khi xóa danh mục.";
                return RedirectToAction("Category");
            }
        }


        // GET: Admin/Home/Product
        [Route("product")]
        public IActionResult Product(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var lstProduct = _context.Products
                                     .Include(p => p.Category)
                                     .OrderBy(p => p.ProductName)
                                     .ToPagedList(pageNumber, pageSize);

            return View(lstProduct);
        }

        //Detail Product
        [Route("DetailProduct")]
        [HttpGet]
        public async Task<IActionResult> DetailProduct(int id)
        {
            // Tìm sản phẩm theo ID
            var product = await _context.Products
                                        .Include(p => p.Category) // Bao gồm danh mục liên quan
                                        .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Product");
            }

            return View(product);
        }

        //Create Product
        [Route("CreateProduct")]
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.CategoryName = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [Route("CreateProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile? ImageFile)
        {
            // Kiểm tra xem các trường bắt buộc đã được nhập hay chưa
            if (string.IsNullOrEmpty(product.ProductName) || string.IsNullOrEmpty(product.Description))
            {
                ModelState.AddModelError("", "Tên danh mục và Mô tả không được để trống.");
            }

            // Kiểm tra nếu giá giảm vượt quá 100
            if (product.Discount > 100)
            {
                ModelState.AddModelError("Discount", "Giá giảm không được vượt quá 100%.");
            }

            // Kiểm tra ModelState
            if (ModelState.IsValid)
            {
                // Xử lý upload hình ảnh
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileExtension = Path.GetExtension(ImageFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";

                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "Products");
                    var filePath = Path.Combine(uploadPath, fileName);

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = fileName; // Lưu tên file vào CSDL
                }
                else
                {
                    // Nếu không chọn hình, gán hình ảnh mặc định
                    product.ImageUrl = "cay_1.jpg";
                }

                // Thêm CreatedDate mặc định là ngày hiện tại
                product.CreatedDate = DateOnly.FromDateTime(DateTime.Now);

                // Lưu vào cơ sở dữ liệu
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction("Product");
            }

            // Cung cấp lại danh sách danh mục để hiển thị trong View
            ViewBag.CategoryName = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "CategoryName");
            return View(product);
        }

        //Edit Product
        [Route("EditProduct")]
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            // Truy vấn sản phẩm từ CSDL
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }
            ViewBag.CategoryName = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View(product);
        }

        [Route("EditProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product updatedProduct, IFormFile? ImageFile)
        {
            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            // Kiểm tra giá trị Discount
            if (updatedProduct.Discount > 100 || updatedProduct.Discount < 0)
            {
                ModelState.AddModelError("Discount", "Giá giảm phải nằm trong khoảng từ 0 đến 100.");
            }

            // Kiểm tra ModelState trước khi tiếp tục
            if (!ModelState.IsValid)
            {
                // Trả lại giá trị cũ trong view
                updatedProduct.ProductName = existingProduct.ProductName;
                updatedProduct.Description = existingProduct.Description;
                updatedProduct.Price = existingProduct.Price;
                updatedProduct.Quantity = existingProduct.Quantity;
                updatedProduct.State = existingProduct.State;
                updatedProduct.ImageUrl = existingProduct.ImageUrl;
                updatedProduct.Discount = existingProduct.Discount;
                updatedProduct.CategoryId = existingProduct.CategoryId;

                ViewBag.CategoryName = new SelectList(_context.Categories, "CategoryId", "CategoryName", updatedProduct.CategoryId);
                return View(updatedProduct);
            }

            // Cập nhật thông tin sản phẩm
            existingProduct.ProductName = string.IsNullOrEmpty(updatedProduct.ProductName)
                ? existingProduct.ProductName
                : updatedProduct.ProductName;

            existingProduct.Description = string.IsNullOrEmpty(updatedProduct.Description)
                ? existingProduct.Description
                : updatedProduct.Description;

            existingProduct.Price = updatedProduct.Price > 0
                ? updatedProduct.Price
                : existingProduct.Price;

            existingProduct.Quantity = updatedProduct.Quantity >= 0
                ? updatedProduct.Quantity
                : existingProduct.Quantity;

            existingProduct.State = string.IsNullOrEmpty(updatedProduct.State)
                ? existingProduct.State
                : updatedProduct.State;

            // Xử lý hình ảnh
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileExtension = Path.GetExtension(ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "Products");
                var filePath = Path.Combine(uploadPath, fileName);

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldFilePath = Path.Combine(uploadPath, existingProduct.ImageUrl);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                existingProduct.ImageUrl = fileName;
            }

            // Cập nhật danh mục
            if (updatedProduct.CategoryId.HasValue && updatedProduct.CategoryId != existingProduct.CategoryId)
            {
                var newCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == updatedProduct.CategoryId.Value);
                if (newCategory != null)
                {
                    existingProduct.CategoryId = newCategory.CategoryId;
                    existingProduct.Category = newCategory;
                }
            }

            existingProduct.Discount = updatedProduct.Discount;
            existingProduct.UpdatedDate = DateOnly.FromDateTime(DateTime.Now);

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return RedirectToAction("Product");
        }


        // Delete Product
        [Route("DeleteProduct")]
        [HttpGet]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Tìm sản phẩm theo ID và bao gồm thông tin danh mục
            var product = await _context.Products
                                         .Include(p => p.Category) // Bao gồm thông tin danh mục
                                         .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Product");
            }

            return View(product);
        }


        [Route("DeleteProduct")]
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            try
            {
                // Truy vấn sản phẩm từ CSDL
                var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    TempData["Message"] = "Sản phẩm không tồn tại.";
                    return RedirectToAction("Product");
                }

                // Kiểm tra trạng thái sản phẩm
                if (product.State == "InStock")
                {
                    TempData["Message"] = "Không thể xóa sản phẩm đang ở trạng thái 'Còn hàng'.";
                    return RedirectToAction("Product");
                }

                // Xóa hình ảnh nếu tồn tại
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "Products", product.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Lấy danh mục liên quan (nếu có)
                var category = product.Category;

                // Xóa sản phẩm khỏi CSDL
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                // Kiểm tra và xóa danh mục nếu không còn sản phẩm liên kết
                if (category != null)
                {
                    bool hasOtherProducts = await _context.Products.AnyAsync(p => p.CategoryId == category.CategoryId);

                    if (!hasOtherProducts)
                    {
                        _context.Categories.Remove(category);
                        await _context.SaveChangesAsync();
                        TempData["Message"] += $" Danh mục '{category.CategoryName}' đã được xóa vì không còn sản phẩm liên kết.";
                    }
                }

                TempData["Message"] += " Sản phẩm đã được xóa thành công.";
                return RedirectToAction("Product");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm.");
                TempData["Message"] = "Có lỗi xảy ra khi xóa sản phẩm.";
                return RedirectToAction("Product");
            }
        }

        //User
        [Route("user")]
        public async Task<IActionResult> User(int? page)
        {
            int pageSize = 10; // Số lượng bản ghi trên mỗi trang
            int pageNumber = page ?? 1; // Nếu `page` là null thì mặc định là trang 1

            try
            {
                // Lấy danh sách người dùng theo phân trang
                var users = await _context.Users
                    .OrderBy(u => u.UserName) // Sắp xếp theo tên người dùng
                    .Skip((pageNumber - 1) * pageSize) // Bỏ qua các bản ghi của trang trước
                    .Take(pageSize) // Lấy số lượng bản ghi theo kích thước trang
                    .ToListAsync();

                // Tính tổng số người dùng
                var totalUsers = await _context.Users.CountAsync();

                // Gán giá trị cho ViewBag để sử dụng trong View
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize); // Tổng số trang
                ViewBag.CurrentPage = pageNumber; // Trang hiện tại

                _logger.LogInformation("Lấy danh sách người dùng thành công. Số lượng: {Count}", users.Count);
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng.");
                return StatusCode(500, "Đã xảy ra lỗi, vui lòng thử lại sau.");
            }
        }


        // Detail User
        [Route("DetailUser")]
        [HttpGet]
        public async Task<IActionResult> DetailUser(int id)
        {
            // Tìm danh mục theo ID
            var user = await _context.Users
                                         .Include(u => u.Carts)
                                         .Include(u => u.Orders)
                                         .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                TempData["Message"] = "Tài khoản không tồn tại.";
                return RedirectToAction("User");
            }

            return View(user);
        }


        //Delete User
        [HttpGet]
        [Route("deleteUser")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Tìm người dùng theo ID
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["Message"] = "Người dùng không tồn tại.";
                return RedirectToAction("Index"); // Chuyển hướng về danh sách người dùng
            }

            return View(user); // Trả về view với thông tin người dùng
        }


        [HttpPost]
        [Route("deleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            try
            {
                // Tìm người dùng trong cơ sở dữ liệu
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    TempData["Message"] = "Người dùng không tồn tại.";
                    return RedirectToAction("Index");
                }

                // Xóa người dùng
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Người dùng đã được xóa thành công.";
                return RedirectToAction("User"); // Chuyển hướng về danh sách người dùng
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi xóa người dùng với ID: {Id}", id);
                TempData["Message"] = "Đã xảy ra lỗi khi xóa người dùng.";
                return RedirectToAction("Index");
            }
        }

        // Index Order
        [Route("order")]
        public async Task<IActionResult> Order(int? page)
        {
            int pageSize = 10; // Số lượng bản ghi trên mỗi trang
            int pageNumber = page ?? 1; // Nếu `page` là null thì mặc định là trang 1

            try
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalOrders = await _context.Orders.CountAsync();
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
                ViewBag.CurrentPage = pageNumber;

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng.");
                return StatusCode(500, "Đã xảy ra lỗi, vui lòng thử lại sau.");
            }
        }

        // Detail Order
        [Route("orderdetail")]
        public async Task<IActionResult> DetailOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    TempData["Message"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("Order");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xem chi tiết đơn hàng với ID: {Id}", id);
                return StatusCode(500, "Đã xảy ra lỗi, vui lòng thử lại sau.");
            }
        }

        // Edit Order
        [Route("orderedit")]
        [HttpGet]
        public async Task<IActionResult> EditOrder(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);

                if (order == null)
                {
                    TempData["Message"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("Order");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy cập chỉnh sửa đơn hàng với ID: {Id}", id);
                return StatusCode(500, "Đã xảy ra lỗi, vui lòng thử lại sau.");
            }
        }

        [Route("orderedit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, Order updatedOrder)
        {
            try
            {
                var existingOrder = await _context.Orders.FindAsync(id);

                if (existingOrder == null)
                {
                    TempData["Message"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("Order");
                }

                // Chỉ cập nhật giá trị nếu có thay đổi
                existingOrder.OrderStatus = string.IsNullOrEmpty(updatedOrder.OrderStatus) ? existingOrder.OrderStatus : updatedOrder.OrderStatus;

                await _context.SaveChangesAsync();

                TempData["Message"] = "Cập nhật trạng thái đơn hàng thành công.";
                return RedirectToAction("Order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chỉnh sửa đơn hàng với ID: {Id}", id);
                return StatusCode(500, "Đã xảy ra lỗi, vui lòng thử lại sau.");
            }
        }


        //Delete Order
        // Controller Action for DeleteOrder
        [Route("deleteorder")]
        [HttpGet]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                // Truy vấn đơn hàng từ CSDL
                var order = await _context.Orders
                    .Include(o => o.OrderItems) // Bao gồm các sản phẩm trong đơn hàng
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    TempData["Message"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("Order");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn đơn hàng với ID: {Id}", id);
                return StatusCode(500, "Đã xảy ra lỗi, vui lòng thử lại sau.");
            }
        }

        [Route("deleteorder")]
        [HttpPost, ActionName("DeleteOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrderConfirmed(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy đơn hàng với ID: {Id}", id);
                    TempData["Message"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("Order");
                }

                // Kiểm tra trạng thái đơn hàng
                if (order.OrderStatus != "Completed" && order.OrderStatus != "Cancelled")
                {
                    _logger.LogWarning("Không thể xóa đơn hàng ID: {Id} vì chưa hoàn thành hoặc bị hủy.", id);
                    TempData["Message"] = "Chỉ có thể xóa đơn hàng đã Hoàn thành hoặc Bị hủy.";
                    return RedirectToAction("Order");
                }

                _logger.LogInformation("Tìm thấy đơn hàng với ID: {Id}. Số lượng sản phẩm: {Count}", id, order.OrderItems.Count);

                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    _context.OrderItems.RemoveRange(order.OrderItems);
                    _logger.LogInformation("Đã xóa tất cả sản phẩm liên quan đến đơn hàng {Id}.", id);
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã xóa đơn hàng thành công với ID: {Id}", id);

                TempData["Message"] = "Đơn hàng đã được xóa thành công.";
                return RedirectToAction("Order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa đơn hàng với ID: {Id}", id);
                TempData["Message"] = "Đã xảy ra lỗi khi xóa đơn hàng.";
                return RedirectToAction("Order");
            }
        }


    }
}
