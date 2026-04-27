using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;
using highschool_student_management.Services;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinary;

        public TeacherController(AppDbContext context, ICloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // GET: /Teacher
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(t =>
                    t.FullName.ToLower().Contains(search) ||
                    (t.Email != null && t.Email.ToLower().Contains(search)) ||
                    (t.Phone != null && t.Phone.Contains(search)));
            }

            // Lay user cung luc de xem trang thai
            var teachers = await query
                .OrderBy(t => t.FullName)
                .Select(t => new TeacherIndexViewModel
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    Avatar = t.Avatar,
                    Gender = t.Gender,
                    DateOfBirth = t.DateOfBirth,
                    Phone = t.Phone,
                    Email = t.Email,
                    Specialization = t.Specialization,
                    JoinDate = t.JoinDate,
                    Status = t.Status,
                    UserId = _context.Users.Where(u => u.TeacherId == t.Id).Select(u => u.Id).FirstOrDefault(),
                    IsUserActive = _context.Users.Where(u => u.TeacherId == t.Id).Select(u => u.IsActive).FirstOrDefault()
                })
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Title"] = "Giao vien";
            return View(teachers);
        }

        // GET: /Teacher/_CreateOrEdit?id=0 hoac id=X
        [HttpGet]
        public async Task<IActionResult> _CreateOrEdit(int id)
        {
            var model = new TeacherFormViewModel();

            if (id > 0)
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher == null)
                    return NotFound("Giao vien khong ton tai.");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.TeacherId == id);

                model = new TeacherFormViewModel
                {
                    Id = teacher.Id,
                    UserId = user?.Id,
                    FullName = teacher.FullName,
                    Gender = teacher.Gender,
                    DateOfBirth = teacher.DateOfBirth,
                    Phone = teacher.Phone,
                    Email = teacher.Email,
                    Address = teacher.Address,
                    Specialization = teacher.Specialization,
                    JoinDate = teacher.JoinDate,
                    Status = teacher.Status,
                    Avatar = teacher.Avatar,
                    UserEmail = user?.Email
                };
            }

            return PartialView(model);
        }

        // POST: /Teacher/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm] TeacherFormViewModel model)
        {
            var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Teacher");
            if (teacherRole == null)
                return Json(new { success = false, message = "Khong tim thay vai tro Giao vien trong he thong." });

            // Upload avatar len Cloudinary neu co
            string? avatarUrl = null;
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                avatarUrl = await _cloudinary.UploadImageAsync(model.AvatarFile, "teacher");
                if (avatarUrl == null)
                    return Json(new { success = false, message = "Upload anh that bai. Vui long thu lai." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model.Id == 0)
                {
                    // === Tao tai khoan User ===
                    string generatedUsername = !string.IsNullOrEmpty(model.Phone)
                        ? $"gv_{model.Phone}"
                        : $"gv_{Guid.NewGuid().ToString("N")[..8]}";

                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == generatedUsername);
                    if (existingUser != null)
                        generatedUsername = $"gv_{Guid.NewGuid().ToString("N")[..8]}";

                    var newUser = new QuanLyHocSinh.Models.User
                    {
                        Username = generatedUsername,
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Email = model.UserEmail ?? model.Email,
                        RoleId = teacherRole.Id,
                        IsActive = 1,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    // === Tao ho so Giao Vien ===
                    var newTeacher = new QuanLyHocSinh.Models.Teacher
                    {
                        FullName = model.FullName,
                        Gender = model.Gender,
                        DateOfBirth = model.DateOfBirth,
                        Phone = model.Phone,
                        Email = model.Email,
                        Address = model.Address,
                        Specialization = model.Specialization,
                        JoinDate = model.JoinDate ?? DateOnly.FromDateTime(DateTime.Now),
                        Status = model.Status,
                        Avatar = avatarUrl
                    };
                    _context.Teachers.Add(newTeacher);
                    await _context.SaveChangesAsync();

                    // Cap nhat User voi TeacherId
                    newUser.TeacherId = newTeacher.Id;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Them giao vien thanh cong. Tai khoan: " + generatedUsername + " (mat khau mac dinh: 123456)" });
                }
                else
                {
                    var teacher = await _context.Teachers.FindAsync(model.Id);
                    if (teacher == null)
                        return Json(new { success = false, message = "Giao vien khong ton tai." });

                    teacher.FullName = model.FullName;
                    teacher.Gender = model.Gender;
                    teacher.DateOfBirth = model.DateOfBirth;
                    teacher.Phone = model.Phone;
                    teacher.Email = model.Email;
                    teacher.Address = model.Address;
                    teacher.Specialization = model.Specialization;
                    teacher.JoinDate = model.JoinDate;
                    teacher.Status = model.Status;

                    if (avatarUrl != null)
                        teacher.Avatar = avatarUrl;

                    await _context.SaveChangesAsync();

                    // Cap nhat email tren User neu co
                    if (model.UserId > 0)
                    {
                        var user = await _context.Users.FindAsync(model.UserId);
                        if (user != null && !string.IsNullOrEmpty(model.Email))
                        {
                            user.Email = model.Email;
                            user.UpdatedAt = DateTime.Now;
                            await _context.SaveChangesAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Cap nhat giao vien thanh cong." });
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /Teacher/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher == null)
                    return Json(new { success = false, message = "Giao vien khong ton tai." });

                // Tim va xoa User qua TeacherId
                var user = await _context.Users.FirstOrDefaultAsync(u => u.TeacherId == id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }

                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Xoa giao vien thanh cong." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
