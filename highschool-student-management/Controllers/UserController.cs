using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using QuanLyHocSinh.Models;
using highschool_student_management.ViewModels;

namespace highschool_student_management.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /User
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var profile = new ProfileViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role.Name,
                RelatedId = user.RelatedId,
                RelatedType = user.RelatedType,
            };

            // Lay thong tin chi tiet tu bang lien quan dua tren RelatedType
            switch (user.RelatedType?.ToLower())
            {
                case "teacher":
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == user.RelatedId);
                    if (teacher != null)
                    {
                        profile.FullName = teacher.FullName;
                        profile.Gender = teacher.Gender;
                        profile.DateOfBirth = teacher.DateOfBirth;
                        profile.Address = teacher.Address;
                        profile.Phone = teacher.Phone;
                        profile.Specialization = teacher.Specialization;
                        profile.JoinDate = teacher.JoinDate;
                        profile.TeacherStatus = teacher.Status;
                    }
                    break;

                case "student":
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == user.RelatedId);
                    if (student != null)
                    {
                        profile.FullName = student.FullName;
                        profile.Gender = student.Gender;
                        profile.DateOfBirth = student.DateOfBirth;
                        profile.Address = student.Address;
                        profile.Phone = student.Phone;
                        profile.StudentCode = student.StudentCode;
                        profile.PlaceOfBirth = student.PlaceOfBirth;
                        profile.Ethnicity = student.Ethnicity;
                        profile.Religion = student.Religion;
                        profile.EnrollmentDate = student.EnrollmentDate;
                        profile.StudentStatus = student.Status;
                    }
                    break;

                case "parent":
                    var parent = await _context.Parents.FirstOrDefaultAsync(p => p.Id == user.RelatedId);
                    if (parent != null)
                    {
                        profile.FullName = parent.FullName;
                        profile.Gender = parent.Gender;
                        profile.DateOfBirth = parent.DateOfBirth;
                        profile.Address = parent.Address;
                        profile.Phone = parent.Phone;
                        profile.Occupation = parent.Occupation;
                        profile.Relationship = parent.Relationship;
                    }
                    break;

                default:
                    // Admin: chi co thong tin tai khoan, khong co bang chi tiet
                    profile.FullName = user.Username;
                    break;
            }

            ViewData["Title"] = "Ho so ca nhan";
            return View(profile);
        }

        // GET: /User/EditProfile (Tra ve Partial View cho popup)
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var profile = new ProfileViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role.Name,
                RelatedId = user.RelatedId,
                RelatedType = user.RelatedType,
            };

            switch (user.RelatedType?.ToLower())
            {
                case "teacher":
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == user.RelatedId);
                    if (teacher != null)
                    {
                        profile.FullName = teacher.FullName;
                        profile.Gender = teacher.Gender;
                        profile.DateOfBirth = teacher.DateOfBirth;
                        profile.Address = teacher.Address;
                        profile.Phone = teacher.Phone;
                        profile.Specialization = teacher.Specialization;
                    }
                    break;

                case "student":
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == user.RelatedId);
                    if (student != null)
                    {
                        profile.FullName = student.FullName;
                        profile.Gender = student.Gender;
                        profile.DateOfBirth = student.DateOfBirth;
                        profile.Address = student.Address;
                        profile.Phone = student.Phone;
                    }
                    break;

                case "parent":
                    var parent = await _context.Parents.FirstOrDefaultAsync(p => p.Id == user.RelatedId);
                    if (parent != null)
                    {
                        profile.FullName = parent.FullName;
                        profile.Gender = parent.Gender;
                        profile.DateOfBirth = parent.DateOfBirth;
                        profile.Address = parent.Address;
                        profile.Phone = parent.Phone;
                    }
                    break;

                default:
                    profile.FullName = user.Username;
                    break;
            }

            return PartialView("_EditProfileModal", profile);
        }

        // POST: /User/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, message = "Khong xac dinh duoc nguoi dung." });
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Json(new { success = false, message = "Tai khoan khong ton tai." });
            }

            // Khong cho sua Username va Role
            if (!string.IsNullOrEmpty(model.Email))
            {
                user.Email = model.Email;
                user.UpdatedAt = DateTime.Now;
            }

            // Cap nhat bang chi tiet
            switch (user.RelatedType?.ToLower())
            {
                case "teacher":
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == user.RelatedId);
                    if (teacher != null)
                    {
                        if (!string.IsNullOrEmpty(model.FullName)) teacher.FullName = model.FullName;
                        if (model.Gender.HasValue) teacher.Gender = model.Gender;
                        if (model.DateOfBirth.HasValue) teacher.DateOfBirth = model.DateOfBirth;
                        if (model.Address != null) teacher.Address = model.Address;
                        if (model.Phone != null) teacher.Phone = model.Phone;
                        if (model.Specialization != null) teacher.Specialization = model.Specialization;
                    }
                    break;

                case "student":
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == user.RelatedId);
                    if (student != null)
                    {
                        if (!string.IsNullOrEmpty(model.FullName)) student.FullName = model.FullName;
                        if (model.Gender.HasValue) student.Gender = model.Gender;
                        if (model.DateOfBirth.HasValue) student.DateOfBirth = model.DateOfBirth;
                        if (model.Address != null) student.Address = model.Address;
                        if (model.Phone != null) student.Phone = model.Phone;
                    }
                    break;

                case "parent":
                    var parent = await _context.Parents.FirstOrDefaultAsync(p => p.Id == user.RelatedId);
                    if (parent != null)
                    {
                        if (!string.IsNullOrEmpty(model.FullName)) parent.FullName = model.FullName;
                        if (model.Gender.HasValue) parent.Gender = model.Gender;
                        if (model.DateOfBirth.HasValue) parent.DateOfBirth = model.DateOfBirth;
                        if (model.Address != null) parent.Address = model.Address;
                        if (model.Phone != null) parent.Phone = model.Phone;
                    }
                    break;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cap nhat ho so thanh cong." });
        }

        // GET: /User/ChangePassword (Tra ve Partial View cho popup)
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return PartialView("_ChangePasswordModal", new ChangePasswordViewModel());
        }

        // POST: /User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, message = "Khong xac dinh duoc nguoi dung." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Tai khoan khong ton tai." });
            }

            // Kiem tra mat khau cu
            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
            {
                return Json(new { success = false, message = "Mat khau cu khong chinh xac." });
            }

            // Hash va luu mat khau moi
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Doi mat khau thanh cong." });
        }
    }
}
