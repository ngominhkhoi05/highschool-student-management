using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(search) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)));
            }

            var users = await query
                .OrderBy(u => u.Role.Name)
                .ThenBy(u => u.Username)
                .Select(u => new UserAccountViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    RoleName = u.Role.Name,
                    IsActive = u.IsActive,
                    LastLogin = u.LastLogin,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Title"] = "Quan ly tai khoan";
            return View(users);
        }

        // POST: /Account/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            try
            {
                var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(currentUserIdStr) && int.TryParse(currentUserIdStr, out var currentUserId))
                {
                    if (id == currentUserId)
                    {
                        return Json(new { success = false, message = "Ban khong the khoa/mo khoa tai khoan cua chinh minh." });
                    }
                }

                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Tai khoan khong ton tai." });
                }

                user.IsActive = user.IsActive == 1 ? 0 : 1;
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                var newStatus = user.IsActive == 1 ? "mo khoa" : "khoa";
                return Json(new { success = true, message = $"Tai khoan da duoc {newStatus} thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // GET: /Account/_ResetPassword?id=X
        [HttpGet]
        public IActionResult _ResetPassword(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("Tai khoan khong ton tai.");
            }

            var model = new ResetPasswordViewModel
            {
                UserId = user.Id,
                Username = user.Username
            };
            return PartialView(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    return Json(new { success = false, message = "Mat khau xac nhan khong khop." });
                }

                var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(currentUserIdStr) && int.TryParse(currentUserIdStr, out var currentUserId))
                {
                    if (model.UserId == currentUserId)
                    {
                        return Json(new { success = false, message = "Ban khong the dat lai mat khau cua chinh minh tai day. Vui long su dung chuc nang doi mat khau." });
                    }
                }

                var user = _context.Users.Find(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Tai khoan khong ton tai." });
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Json(new { success = true, message = "Dat lai mat khau thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
