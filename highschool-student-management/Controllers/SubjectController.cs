using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using QuanLyHocSinh.Models;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubjectController : Controller
    {
        private readonly AppDbContext _context;

        public SubjectController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Subject
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var subjects = await _context.Subjects
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewData["Title"] = "Quan ly mon hoc";
            return View(subjects);
        }

        // GET: /Subject/_CreateOrEdit?id=0 (them moi) hoac id>0 (sua)
        [HttpGet]
        public IActionResult _CreateOrEdit(int id = 0)
        {
            var model = new Subject();
            if (id > 0)
            {
                model = _context.Subjects.Find(id)!;
                if (model == null)
                {
                    return NotFound("Mon hoc khong ton tai.");
                }
            }
            return PartialView(model);
        }

        // POST: /Subject/SaveSubject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSubject(Subject model)
        {
            try
            {
                // Loai bo navigation properties khoi model binding
                ModelState.Remove("TeacherClasses");
                ModelState.Remove("Scores");
                ModelState.Remove("SemesterResults");
                ModelState.Remove("Attendances");

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Du lieu khong hop le. Vui long kiem tra lai." });
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Ten mon hoc khong duoc de trong." });
                }

                if (string.IsNullOrWhiteSpace(model.Code))
                {
                    return Json(new { success = false, message = "Ma mon hoc khong duoc de trong." });
                }

                // Kiem tra trung lap Code
                var existing = _context.Subjects
                    .Where(s => s.Code == model.Code && s.Id != model.Id)
                    .FirstOrDefault();
                if (existing != null)
                {
                    return Json(new { success = false, message = $"Ma mon hoc '{model.Code}' da ton tai. Vui long chon ma khac." });
                }

                if (model.Id == 0)
                {
                    _context.Subjects.Add(model);
                }
                else
                {
                    var existingSubject = _context.Subjects.Find(model.Id);
                    if (existingSubject == null)
                    {
                        return Json(new { success = false, message = "Mon hoc khong ton tai." });
                    }
                    existingSubject.Name = model.Name;
                    existingSubject.Code = model.Code;
                    existingSubject.GradeLevel = model.GradeLevel;
                    existingSubject.PeriodsPerWeek = model.PeriodsPerWeek;
                    existingSubject.IsActive = model.IsActive;
                }

                _context.SaveChanges();
                return Json(new { success = true, message = model.Id == 0 ? "Them mon hoc thanh cong." : "Cap nhat mon hoc thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /Subject/DeleteSubject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSubject(int id)
        {
            try
            {
                var subject = _context.Subjects.Find(id);
                if (subject == null)
                {
                    return Json(new { success = false, message = "Mon hoc khong ton tai." });
                }

                _context.Subjects.Remove(subject);
                _context.SaveChanges();
                return Json(new { success = true, message = "Xoa mon hoc thanh cong." });
            }
            catch (DbUpdateException)
            {
                return Json(new { success = false, message = "Khong the xoa vi mon hoc nay dang duoc su dung (co diem, lop hoc, hoac ket qua). Vui long xoa du lieu lien quan truoc." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
