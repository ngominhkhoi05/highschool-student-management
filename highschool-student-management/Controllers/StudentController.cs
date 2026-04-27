using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;
using highschool_student_management.Services;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinary;

        public StudentController(AppDbContext context, ICloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // GET: /Student
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s =>
                    s.FullName.ToLower().Contains(search) ||
                    s.StudentCode.ToLower().Contains(search) ||
                    (s.Phone != null && s.Phone.Contains(search)));
            }

            var students = await query
                .OrderBy(s => s.FullName)
                .Select(s => new StudentIndexViewModel
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName,
                    Avatar = s.Avatar,
                    Gender = s.Gender,
                    DateOfBirth = s.DateOfBirth,
                    Phone = s.Phone,
                    Status = s.Status,
                    EnrollmentDate = s.EnrollmentDate,
                    UserId = _context.Users.Where(u => u.StudentId == s.Id).Select(u => u.Id).FirstOrDefault(),
                    IsUserActive = _context.Users.Where(u => u.StudentId == s.Id).Select(u => u.IsActive).FirstOrDefault()
                })
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Title"] = "Hoc sinh";
            return View(students);
        }

        // GET: /Student/_CreateOrEdit?id=0 hoac id=X
        [HttpGet]
        public async Task<IActionResult> _CreateOrEdit(int id)
        {
            var model = new StudentFormViewModel();

            if (id > 0)
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                    return NotFound("Hoc sinh khong ton tai.");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.StudentId == id);

                model = new StudentFormViewModel
                {
                    Id = student.Id,
                    UserId = user?.Id,
                    StudentCode = student.StudentCode,
                    FullName = student.FullName,
                    Gender = student.Gender,
                    DateOfBirth = student.DateOfBirth,
                    PlaceOfBirth = student.PlaceOfBirth,
                    Address = student.Address,
                    Phone = student.Phone,
                    Ethnicity = student.Ethnicity,
                    Religion = student.Religion,
                    EnrollmentDate = student.EnrollmentDate,
                    Status = student.Status,
                    Avatar = student.Avatar,
                    UserEmail = user?.Email
                };
            }

            return PartialView(model);
        }

        // POST: /Student/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm] StudentFormViewModel model)
        {
            if (model.Id == 0)
            {
                var existingCode = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == model.StudentCode);
                if (existingCode != null)
                    return Json(new { success = false, message = "Ma hoc sinh da ton tai. Vui long su dung ma khac." });
            }

            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
            if (studentRole == null)
                return Json(new { success = false, message = "Khong tim thay vai tro Hoc sinh trong he thong." });

            // Upload avatar len Cloudinary neu co
            string? avatarUrl = null;
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                avatarUrl = await _cloudinary.UploadImageAsync(model.AvatarFile, "student");
                if (avatarUrl == null)
                    return Json(new { success = false, message = "Upload anh that bai. Vui long thu lai." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model.Id == 0)
                {
                    string generatedUsername = $"hs_{model.StudentCode}";
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == generatedUsername);
                    if (existingUser != null)
                        generatedUsername = $"hs_{model.StudentCode}_{Guid.NewGuid().ToString("N")[..4]}";

                    var newUser = new QuanLyHocSinh.Models.User
                    {
                        Username = generatedUsername,
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Email = model.UserEmail,
                        RoleId = studentRole.Id,
                        IsActive = 1,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    var newStudent = new QuanLyHocSinh.Models.Student
                    {
                        StudentCode = model.StudentCode,
                        FullName = model.FullName,
                        Gender = model.Gender,
                        DateOfBirth = model.DateOfBirth,
                        PlaceOfBirth = model.PlaceOfBirth,
                        Address = model.Address,
                        Phone = model.Phone,
                        Ethnicity = model.Ethnicity,
                        Religion = model.Religion,
                        EnrollmentDate = model.EnrollmentDate ?? DateOnly.FromDateTime(DateTime.Now),
                        Status = model.Status,
                        Avatar = avatarUrl
                    };
                    _context.Students.Add(newStudent);
                    await _context.SaveChangesAsync();

                    newUser.StudentId = newStudent.Id;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Them hoc sinh thanh cong. Tai khoan: " + generatedUsername + " (mat khau mac dinh: 123456)" });
                }
                else
                {
                    var student = await _context.Students.FindAsync(model.Id);
                    if (student == null)
                        return Json(new { success = false, message = "Hoc sinh khong ton tai." });

                    if (student.StudentCode != model.StudentCode)
                    {
                        var duplicate = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == model.StudentCode && s.Id != model.Id);
                        if (duplicate != null)
                        {
                            await transaction.RollbackAsync();
                            return Json(new { success = false, message = "Ma hoc sinh da ton tai." });
                        }
                    }

                    student.StudentCode = model.StudentCode;
                    student.FullName = model.FullName;
                    student.Gender = model.Gender;
                    student.DateOfBirth = model.DateOfBirth;
                    student.PlaceOfBirth = model.PlaceOfBirth;
                    student.Address = model.Address;
                    student.Phone = model.Phone;
                    student.Ethnicity = model.Ethnicity;
                    student.Religion = model.Religion;
                    student.EnrollmentDate = model.EnrollmentDate;
                    student.Status = model.Status;

                    if (avatarUrl != null)
                        student.Avatar = avatarUrl;

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Cap nhat hoc sinh thanh cong." });
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /Student/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                    return Json(new { success = false, message = "Hoc sinh khong ton tai." });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.StudentId == id);
                if (user != null)
                    _context.Users.Remove(user);

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Xoa hoc sinh thanh cong." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
