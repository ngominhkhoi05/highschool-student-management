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
                .Include(u => u.Teacher)
                .Include(u => u.Student)
                .Include(u => u.Parent)
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
                TeacherId = user.TeacherId,
                StudentId = user.StudentId,
                ParentId = user.ParentId,
            };

            // Lay thong tin chi tiet tu bang lien quan dua tren Role
            switch (user.Role.Name)
            {
                case "Teacher":
                    if (user.Teacher != null)
                    {
                        profile.FullName = user.Teacher.FullName;
                        profile.Gender = user.Teacher.Gender;
                        profile.DateOfBirth = user.Teacher.DateOfBirth;
                        profile.Address = user.Teacher.Address;
                        profile.Phone = user.Teacher.Phone;
                        profile.Specialization = user.Teacher.Specialization;
                        profile.JoinDate = user.Teacher.JoinDate;
                        profile.TeacherStatus = user.Teacher.Status;
                    }
                    break;

                case "Student":
                    if (user.Student != null)
                    {
                        profile.FullName = user.Student.FullName;
                        profile.Gender = user.Student.Gender;
                        profile.DateOfBirth = user.Student.DateOfBirth;
                        profile.Address = user.Student.Address;
                        profile.Phone = user.Student.Phone;
                        profile.StudentCode = user.Student.StudentCode;
                        profile.PlaceOfBirth = user.Student.PlaceOfBirth;
                        profile.Ethnicity = user.Student.Ethnicity;
                        profile.Religion = user.Student.Religion;
                        profile.EnrollmentDate = user.Student.EnrollmentDate;
                        profile.StudentStatus = user.Student.Status;
                    }
                    break;

                case "Parent":
                    if (user.Parent != null)
                    {
                        profile.FullName = user.Parent.FullName;
                        profile.Gender = user.Parent.Gender;
                        profile.DateOfBirth = user.Parent.DateOfBirth;
                        profile.Address = user.Parent.Address;
                        profile.Phone = user.Parent.Phone;
                        profile.Occupation = user.Parent.Occupation;
                        profile.Relationship = user.Parent.Relationship;
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
                .Include(u => u.Teacher)
                .Include(u => u.Student)
                .Include(u => u.Parent)
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
                TeacherId = user.TeacherId,
                StudentId = user.StudentId,
                ParentId = user.ParentId,
            };

            switch (user.Role.Name)
            {
                case "Teacher":
                    if (user.Teacher != null)
                    {
                        profile.FullName = user.Teacher.FullName;
                        profile.Gender = user.Teacher.Gender;
                        profile.DateOfBirth = user.Teacher.DateOfBirth;
                        profile.Address = user.Teacher.Address;
                        profile.Phone = user.Teacher.Phone;
                        profile.Specialization = user.Teacher.Specialization;
                    }
                    break;

                case "Student":
                    if (user.Student != null)
                    {
                        profile.FullName = user.Student.FullName;
                        profile.Gender = user.Student.Gender;
                        profile.DateOfBirth = user.Student.DateOfBirth;
                        profile.Address = user.Student.Address;
                        profile.Phone = user.Student.Phone;
                    }
                    break;

                case "Parent":
                    if (user.Parent != null)
                    {
                        profile.FullName = user.Parent.FullName;
                        profile.Gender = user.Parent.Gender;
                        profile.DateOfBirth = user.Parent.DateOfBirth;
                        profile.Address = user.Parent.Address;
                        profile.Phone = user.Parent.Phone;
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
                .Include(u => u.Teacher)
                .Include(u => u.Student)
                .Include(u => u.Parent)
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
            switch (user.Role.Name)
            {
                case "Teacher":
                    if (user.Teacher != null)
                    {
                        if (!string.IsNullOrEmpty(model.FullName)) user.Teacher.FullName = model.FullName;
                        if (model.Gender.HasValue) user.Teacher.Gender = model.Gender;
                        if (model.DateOfBirth.HasValue) user.Teacher.DateOfBirth = model.DateOfBirth;
                        if (model.Address != null) user.Teacher.Address = model.Address;
                        if (model.Phone != null) user.Teacher.Phone = model.Phone;
                        if (model.Specialization != null) user.Teacher.Specialization = model.Specialization;
                    }
                    break;

                case "Student":
                    if (user.Student != null)
                    {
                        if (!string.IsNullOrEmpty(model.FullName)) user.Student.FullName = model.FullName;
                        if (model.Gender.HasValue) user.Student.Gender = model.Gender;
                        if (model.DateOfBirth.HasValue) user.Student.DateOfBirth = model.DateOfBirth;
                        if (model.Address != null) user.Student.Address = model.Address;
                        if (model.Phone != null) user.Student.Phone = model.Phone;
                    }
                    break;

                case "Parent":
                    if (user.Parent != null)
                    {
                        if (!string.IsNullOrEmpty(model.FullName)) user.Parent.FullName = model.FullName;
                        if (model.Gender.HasValue) user.Parent.Gender = model.Gender;
                        if (model.DateOfBirth.HasValue) user.Parent.DateOfBirth = model.DateOfBirth;
                        if (model.Address != null) user.Parent.Address = model.Address;
                        if (model.Phone != null) user.Parent.Phone = model.Phone;
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
