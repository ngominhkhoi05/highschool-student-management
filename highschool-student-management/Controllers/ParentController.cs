using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;
using highschool_student_management.Services;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ParentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinary;

        public ParentController(AppDbContext context, ICloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // GET: /Parent
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Parents
                .Include(p => p.ParentStudents)
                    .ThenInclude(ps => ps.Student)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.FullName.ToLower().Contains(search) ||
                    (p.Phone != null && p.Phone.Contains(search)));
            }

            var parents = await query
                .OrderBy(p => p.FullName)
                .Select(p => new ParentIndexViewModel
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    Gender = p.Gender,
                    DateOfBirth = p.DateOfBirth,
                    Phone = p.Phone,
                    Email = p.Email,
                    Occupation = p.Occupation,
                    Relationship = p.Relationship,
                    Address = p.Address,
                    UserId = _context.Users.Where(u => u.ParentId == p.Id).Select(u => u.Id).FirstOrDefault(),
                    IsUserActive = _context.Users.Where(u => u.ParentId == p.Id).Select(u => u.IsActive).FirstOrDefault(),
                    AssignedStudentNames = p.ParentStudents.Where(ps => ps.Student != null).Select(ps => ps.Student!.FullName).ToList()
                })
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Title"] = "Phu huynh";
            return View(parents);
        }

        // GET: /Parent/_CreateOrEdit?id=0 hoac id=X
        [HttpGet]
        public async Task<IActionResult> _CreateOrEdit(int id)
        {
            var model = new ParentFormViewModel();

            if (id > 0)
            {
                var parent = await _context.Parents.FindAsync(id);
                if (parent == null)
                    return NotFound("Phu huynh khong ton tai.");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.ParentId == id);

                model = new ParentFormViewModel
                {
                    Id = parent.Id,
                    UserId = user?.Id,
                    FullName = parent.FullName,
                    Gender = parent.Gender,
                    DateOfBirth = parent.DateOfBirth,
                    Phone = parent.Phone,
                    Email = parent.Email,
                    Occupation = parent.Occupation,
                    Address = parent.Address,
                    Relationship = parent.Relationship,
                    UserEmail = user?.Email
                };
            }

            return PartialView(model);
        }

        // POST: /Parent/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm] ParentFormViewModel model)
        {
            var parentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Parent");
            if (parentRole == null)
                return Json(new { success = false, message = "Khong tim thay vai tro Phu huynh trong he thong." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model.Id == 0)
                {
                    // Username = so dien thoai
                    string generatedUsername = $"ph_{model.Phone}";
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == generatedUsername);
                    if (existingUser != null)
                        generatedUsername = $"ph_{model.Phone}_{Guid.NewGuid().ToString("N")[..4]}";

                    var newUser = new QuanLyHocSinh.Models.User
                    {
                        Username = generatedUsername,
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Email = model.UserEmail ?? model.Email,
                        RoleId = parentRole.Id,
                        IsActive = 1,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    var newParent = new QuanLyHocSinh.Models.Parent
                    {
                        FullName = model.FullName,
                        Gender = model.Gender,
                        DateOfBirth = model.DateOfBirth,
                        Phone = model.Phone,
                        Email = model.Email,
                        Occupation = model.Occupation,
                        Address = model.Address,
                        Relationship = model.Relationship
                    };
                    _context.Parents.Add(newParent);
                    await _context.SaveChangesAsync();

                    newUser.ParentId = newParent.Id;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Them phu huynh thanh cong. Tai khoan: " + generatedUsername + " (mat khau mac dinh: 123456)" });
                }
                else
                {
                    var parent = await _context.Parents.FindAsync(model.Id);
                    if (parent == null)
                        return Json(new { success = false, message = "Phu huynh khong ton tai." });

                    parent.FullName = model.FullName;
                    parent.Gender = model.Gender;
                    parent.DateOfBirth = model.DateOfBirth;
                    parent.Phone = model.Phone;
                    parent.Email = model.Email;
                    parent.Occupation = model.Occupation;
                    parent.Address = model.Address;
                    parent.Relationship = model.Relationship;

                    await _context.SaveChangesAsync();

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
                    return Json(new { success = true, message = "Cap nhat phu huynh thanh cong." });
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /Parent/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var parent = await _context.Parents
                    .Include(p => p.ParentStudents)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (parent == null)
                    return Json(new { success = false, message = "Phu huynh khong ton tai." });

                // Xoa cac lien ket hoc sinh
                if (parent.ParentStudents.Any())
                    _context.ParentStudents.RemoveRange(parent.ParentStudents);

                // Xoa User qua ParentId
                var user = await _context.Users.FirstOrDefaultAsync(u => u.ParentId == id);
                if (user != null)
                    _context.Users.Remove(user);

                _context.Parents.Remove(parent);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Xoa phu huynh thanh cong." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // GET: /Parent/_AssignStudents?parentId=X
        [HttpGet]
        public async Task<IActionResult> _AssignStudents(int parentId)
        {
            var parent = await _context.Parents.FindAsync(parentId);
            if (parent == null)
                return NotFound("Phu huynh khong ton tai.");

            var allStudents = await _context.Students
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var assignedIds = await _context.ParentStudents
                .Where(ps => ps.ParentId == parentId)
                .Select(ps => ps.StudentId)
                .ToListAsync();

            var model = new AssignStudentsViewModel
            {
                ParentId = parentId,
                ParentName = parent.FullName,
                AllStudents = allStudents.Select(s => new StudentOption
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName,
                    IsSelected = assignedIds.Contains(s.Id)
                }).ToList(),
                SelectedStudentIds = assignedIds
            };

            return PartialView(model);
        }

        // POST: /Parent/SaveAssignedStudents
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAssignedStudents([FromForm] int parentId, [FromForm] List<int> studentIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var parentExists = await _context.Parents.FindAsync(parentId);
                if (parentExists == null)
                    return Json(new { success = false, message = "Phu huynh khong ton tai." });

                // Xoa lien ket cu
                var oldLinks = await _context.ParentStudents
                    .Where(ps => ps.ParentId == parentId)
                    .ToListAsync();
                _context.ParentStudents.RemoveRange(oldLinks);

                // Them lien ket moi
                if (studentIds != null && studentIds.Any())
                {
                    foreach (var studentId in studentIds.Where(id => id > 0))
                    {
                        _context.ParentStudents.Add(new QuanLyHocSinh.Models.ParentStudent
                        {
                            ParentId = parentId,
                            StudentId = studentId,
                            IsPrimary = studentId == studentIds.First() ? 1 : 0
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Gan hoc sinh cho phu huynh thanh cong." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
