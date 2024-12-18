using AutoMapper;
using Cay.Canh.Web.HDT.Data;
using Cay.Canh.Web.HDT.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel.DataAnnotations;

namespace Cay.Canh.Web.HDT.Controllers
{
    public class UsersController : Controller
    {
        private readonly WebCayCanhContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(WebCayCanhContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách tất cả người dùng
            var users = await _context.Users.ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> DangKy()
        {
            return View();
        }

        public async Task<IActionResult> Dangnhap()
        {
            return View();
        }

        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Dangnhap", "Users");
        }

        //Đăng ký

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Trả về view nếu dữ liệu không hợp lệ
            }

            // Kiểm tra tuổi người dùng (nếu có ngày sinh)
            if (model.NgaySinh.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var birthDate = model.NgaySinh.Value;

                var age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;

                if (age < 20) // Thay đổi điều kiện tuổi
                {
                    ModelState.AddModelError(string.Empty, "Bạn phải ít nhất 20 tuổi để đăng ký.");
                    return View(model);
                }
            }

            // Kiểm tra xem người dùng đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == model.UserName || u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Tên người dùng hoặc email đã tồn tại.");
                return View(model);
            }

            // Xử lý lưu file ảnh đại diện (nếu có)
            string imageFileName = null;
            if (model.Image != null)
            {
                var fileExtension = Path.GetExtension(model.Image.FileName);
                imageFileName = Guid.NewGuid().ToString() + fileExtension;

                // Sử dụng WebRootPath thay vì Directory.GetCurrentDirectory()
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "users");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var filePath = Path.Combine(uploadFolder, imageFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }
            }

            // Tạo người dùng mới
            var user = new User
            {
                UserName = model.UserName,
                Password = model.Password, 
                Email = model.Email,
                FullName = model.FullName,
                Sdt = model.SDT,
                Address = model.Address,
                Role = "User",
                NgaySinh = model.NgaySinh?.ToDateTime(TimeOnly.MinValue),
                CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                ImageUrl = imageFileName // Lưu tên file ảnh
            };

            // Thêm người dùng vào cơ sở dữ liệu
            _context.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return RedirectToAction("Dangnhap");
        }

        // Đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dangnhap(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<User>(model);

                var dbUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == user.UserName && u.Password == user.Password);

                if (dbUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, dbUser.UserId.ToString()),
                    new Claim(ClaimTypes.Name, dbUser.UserName),
                    new Claim(ClaimTypes.Role, dbUser.Role),
                    new Claim("ImageUrl", dbUser.ImageUrl ?? "user_boy.jpg")
                };

                Console.WriteLine($"ImageUrl: {dbUser.ImageUrl}");

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                if (dbUser.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details()
        {
            // Lấy UserName từ User.Identity.Name
            string userName = User.Identity.Name;

            // Kiểm tra xem có tên người dùng trong User.Identity không
            if (string.IsNullOrEmpty(userName))
            {
                // Nếu không tìm thấy tên người dùng, trả về lỗi 404 hoặc chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }

            // Truy vấn User từ cơ sở dữ liệu dựa trên UserName
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == userName);  // Giả sử UserName là tên người dùng

            // Kiểm tra nếu không tìm thấy người dùng
            if (user == null)
            {
                // Nếu không tìm thấy người dùng, trả về lỗi 404
                return NotFound();
            }

            // Tạo ViewModel để hiển thị thông tin người dùng
            var userViewModel = new UserViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                ImageUrl = user.ImageUrl ?? "default-avatar.jpg", // Nếu không có ảnh, sử dụng ảnh mặc định
                SDT = user.Sdt,
                Address = user.Address,
                NgaySinh = user.NgaySinh,
                Role = user.Role
            };

            // Trả về View với dữ liệu của người dùng
            return View(userViewModel);
        }


        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserName,Password,Email,FullName,Role,CreatedDate")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserName,Password,Email,FullName,Role,Sdt,Address,NgaySinh,ImageUrl")] User user, IFormFile? uploadedImage)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra điều kiện tuổi > 20
                    if (user.NgaySinh.HasValue && user.NgaySinh.Value.AddYears(20) > DateTime.Now)
                    {
                        ModelState.AddModelError("NgaySinh", "Ngày sinh phải đủ 20 tuổi trở lên.");
                    }


                    // Kiểm tra định dạng email bằng EmailAddressAttribute
                    if (!string.IsNullOrEmpty(user.Email) && !new EmailAddressAttribute().IsValid(user.Email))
                    {
                        ModelState.AddModelError("Email", "Địa chỉ email không hợp lệ.");
                    }

                    // Nếu có lỗi thì trả về lại view mà không lưu dữ liệu
                    if (!ModelState.IsValid)
                    {
                        return View(user);
                    }

                    // Nếu có file hình ảnh được tải lên
                    if (uploadedImage != null)
                    {
                        // Xử lý việc lưu hình ảnh vào thư mục "wwwroot/img/users"
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "users", uploadedImage.FileName);

                        // Lưu hình ảnh vào thư mục
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedImage.CopyToAsync(stream);
                        }

                        // Cập nhật trường ImageUrl với đường dẫn đến hình ảnh mới
                        user.ImageUrl = Path.Combine("img", "users", uploadedImage.FileName);
                    }
                    else
                    {
                        // Nếu không có hình ảnh mới, giữ nguyên giá trị cũ
                        user.ImageUrl = existingUser.ImageUrl;
                    }

                    // Cập nhật các trường khác
                    existingUser.UserName = user.UserName ?? existingUser.UserName;
                    existingUser.Password = string.IsNullOrEmpty(user.Password) ? existingUser.Password : user.Password; 
                    existingUser.Email = user.Email ?? existingUser.Email;
                    existingUser.FullName = user.FullName ?? existingUser.FullName;
                    existingUser.Role = user.Role ?? existingUser.Role;
                    existingUser.Sdt = user.Sdt ?? existingUser.Sdt;
                    existingUser.Address = user.Address ?? existingUser.Address;
                    existingUser.NgaySinh = user.NgaySinh ?? existingUser.NgaySinh;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details));  // Redirect đến trang Details sau khi cập nhật thành công
            }

            // Trả về lại View nếu model không hợp lệ
            return View(user);
        }


        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
