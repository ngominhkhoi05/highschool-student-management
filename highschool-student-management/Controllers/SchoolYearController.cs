using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using QuanLyHocSinh.Models;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SchoolYearController : Controller
    {
        private readonly AppDbContext _context;

        public SchoolYearController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /SchoolYear
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var schoolYears = await _context.SchoolYears
                .Include(sy => sy.Semesters.OrderBy(s => s.SemesterNumber))
                .OrderByDescending(sy => sy.StartDate)
                .ToListAsync();

            ViewData["Title"] = "Quan ly nam hoc";
            return View(schoolYears);
        }

        // GET: /SchoolYear/_CreateOrEdit?id=0 (them moi) hoac id>0 (sua)
        [HttpGet]
        public IActionResult _CreateOrEdit(int id = 0)
        {
            var model = new SchoolYear();
            if (id > 0)
            {
                model = _context.SchoolYears.Find(id)!;
                if (model == null)
                {
                    return NotFound();
                }
            }
            return PartialView(model);
        }

        // POST: /SchoolYear/SaveSchoolYear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSchoolYear(SchoolYear model, bool autoCreateSemesters = false)
        {
            try
            {
                ModelState.Remove("SchoolYear");
                ModelState.Remove("TeacherClasses");
                ModelState.Remove("Scores");
                ModelState.Remove("SemesterResults");
                ModelState.Remove("ConductScores");
                ModelState.Remove("Violations");
                ModelState.Remove("Commendations");
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Du lieu khong hop le. Vui long kiem tra lai." });
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Ten nam hoc khong duoc de trong." });
                }

                if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate)
                {
                    return Json(new { success = false, message = "Ngay bat dau phai nho hon ngay ket thuc." });
                }

                // Kiem tra nam hoc khong bi trung voi nam hoc khac
                var allSchoolYears = _context.SchoolYears
                    .Where(sy => sy.Id != model.Id)
                    .ToList();

                foreach (var sy in allSchoolYears)
                {
                    if (!sy.StartDate.HasValue || !sy.EndDate.HasValue || !model.StartDate.HasValue || !model.EndDate.HasValue)
                        continue;

                    bool overlaps = model.StartDate.Value < sy.EndDate.Value && model.EndDate.Value > sy.StartDate.Value;
                    if (overlaps)
                    {
                        return Json(new { success = false, message = $"Nam hoc nay bi trung voi nam hoc '{sy.Name}' ({sy.StartDate:dd/MM/yyyy} - {sy.EndDate:dd/MM/yyyy}). Cac nam hoc khong duoc nam chong len nhau." });
                    }
                }

                if (model.Id == 0)
                {
                    // Tao moi
                    model.IsCurrent = 0;
                    _context.SchoolYears.Add(model);
                    _context.SaveChanges(); // Luu truoc de co Id thuc

                    if (autoCreateSemesters)
                    {
                        _context.Semesters.AddRange(
                            new Semester
                            {
                                SchoolYearId = model.Id,
                                Name = "Hoc ky 1",
                                SemesterNumber = 1,
                                StartDate = model.StartDate,
                                EndDate = model.StartDate.HasValue
                                    ? DateOnly.FromDateTime(model.StartDate.Value.ToDateTime(TimeOnly.MinValue).AddMonths(4))
                                    : null
                            },
                            new Semester
                            {
                                SchoolYearId = model.Id,
                                Name = "Hoc ky 2",
                                SemesterNumber = 2,
                                StartDate = model.StartDate.HasValue
                                    ? DateOnly.FromDateTime(model.StartDate.Value.ToDateTime(TimeOnly.MinValue).AddMonths(5))
                                    : null,
                                EndDate = model.EndDate
                            }
                        );
                        _context.SaveChanges();
                    }
                }
                else
                {
                    // Cap nhat
                    var existing = _context.SchoolYears.Find(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Nam hoc khong ton tai." });
                    }
                    existing.Name = model.Name;
                    existing.StartDate = model.StartDate;
                    existing.EndDate = model.EndDate;
                }

                _context.SaveChanges();
                return Json(new { success = true, message = model.Id == 0 ? "Tao nam hoc thanh cong." : "Cap nhat nam hoc thanh cong.", reload = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /SchoolYear/DeleteSchoolYear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSchoolYear(int id)
        {
            try
            {
                var schoolYear = _context.SchoolYears
                    .Include(sy => sy.Classes)
                    .Include(sy => sy.Semesters)
                    .FirstOrDefault(sy => sy.Id == id);

                if (schoolYear == null)
                {
                    return Json(new { success = false, message = "Nam hoc khong ton tai." });
                }

                // Kiem tra rang buoc: neu co lop hoc thi khong cho xoa
                if (schoolYear.Classes.Count > 0)
                {
                    return Json(new { success = false, message = $"Khong the xoa vi nam hoc nay co {schoolYear.Classes.Count} lop hoc dang su dung. Vui long xoa lop hoc truoc." });
                }

                // Kiem tra rang buoc: neu co hoc ky thi khong cho xoa
                if (schoolYear.Semesters.Count > 0)
                {
                    return Json(new { success = false, message = $"Khong the xoa vi nam hoc nay co {schoolYear.Semesters.Count} hoc ky. Vui long xoa hoc ky truoc." });
                }

                _context.SchoolYears.Remove(schoolYear);
                _context.SaveChanges();
                return Json(new { success = true, message = "Xoa nam hoc thanh cong.", reload = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi khi xoa: " + ex.Message });
            }
        }

        // POST: /SchoolYear/SetCurrentSchoolYear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetCurrentSchoolYear(int id)
        {
            try
            {
                var schoolYear = _context.SchoolYears.Find(id);
                if (schoolYear == null)
                {
                    return Json(new { success = false, message = "Nam hoc khong ton tai." });
                }

                var allSchoolYears = _context.SchoolYears.ToList();
                foreach (var sy in allSchoolYears)
                {
                    sy.IsCurrent = sy.Id == id ? 1 : 0;
                }
                _context.SaveChanges();

                return Json(new { success = true, message = "Da dat nam hoc hien tai thanh cong.", reload = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // GET: /SchoolYear/_CreateOrEditSemester?schoolYearId=1&id=0 (them moi) hoac id>0 (sua)
        [HttpGet]
        public IActionResult _CreateOrEditSemester(int schoolYearId, int id = 0)
        {
            var schoolYear = _context.SchoolYears.Find(schoolYearId);
            if (schoolYear == null)
            {
                return NotFound("Nam hoc khong ton tai.");
            }

            var model = new Semester { SchoolYearId = schoolYearId };
            if (id > 0)
            {
                model = _context.Semesters.Find(id)!;
                if (model == null || model.SchoolYearId != schoolYearId)
                {
                    return NotFound("Hoc ky khong ton tai.");
                }
            }
            ViewBag.SchoolYearName = schoolYear.Name;
            ViewBag.SchoolYearStart = schoolYear.StartDate?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.SchoolYearEnd = schoolYear.EndDate?.ToString("yyyy-MM-dd") ?? "";
            return PartialView(model);
        }

        // POST: /SchoolYear/SaveSemester
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSemester(Semester model)
        {
            try
            {
                ModelState.Remove("SchoolYear");
                ModelState.Remove("TeacherClasses");
                ModelState.Remove("Scores");
                ModelState.Remove("SemesterResults");
                ModelState.Remove("ConductScores");
                ModelState.Remove("Violations");
                ModelState.Remove("Commendations");
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Du lieu khong hop le. Vui long kiem tra lai." });
                }

                var schoolYear = _context.SchoolYears.Find(model.SchoolYearId);
                if (schoolYear == null)
                {
                    return Json(new { success = false, message = "Nam hoc khong ton tai." });
                }

                if (model.SemesterNumber != 1 && model.SemesterNumber != 2)
                {
                    return Json(new { success = false, message = "So thu tu hoc ky chi duoc phep la 1 hoac 2." });
                }

                if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate)
                {
                    return Json(new { success = false, message = "Ngay bat dau phai nho hon hoac bang ngay ket thuc." });
                }

                // Lay tat ca hoc ky cua nam hoc (tru chinh no)
                var existingSemesters = _context.Semesters
                    .Where(s => s.SchoolYearId == model.SchoolYearId && s.Id != model.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToList();

                // Kiem tra so hoc ky trung
                if (existingSemesters.Any(s => s.SemesterNumber == model.SemesterNumber))
                {
                    return Json(new { success = false, message = $"Da ton tai hoc ky {model.SemesterNumber} trong nam hoc nay. Vui long chon so khac." });
                }

                // Kiem tra hoc ky nam trong khoang nam hoc
                if (model.StartDate.HasValue)
                {
                    if (schoolYear.StartDate.HasValue && model.StartDate < schoolYear.StartDate)
                    {
                        return Json(new { success = false, message = $"Ngay bat dau hoc ky phai tu ngay {schoolYear.StartDate:dd/MM/yyyy} (ngay bat dau nam hoc)." });
                    }
                    if (schoolYear.EndDate.HasValue && model.StartDate > schoolYear.EndDate)
                    {
                        return Json(new { success = false, message = $"Ngay bat dau hoc ky phai truoc hoac bang ngay {schoolYear.EndDate:dd/MM/yyyy} (ngay ket thuc nam hoc)." });
                    }
                }
                if (model.EndDate.HasValue)
                {
                    if (schoolYear.EndDate.HasValue && model.EndDate > schoolYear.EndDate)
                    {
                        return Json(new { success = false, message = $"Ngay ket thuc hoc ky phai truoc hoac bang ngay {schoolYear.EndDate:dd/MM/yyyy} (ngay ket thuc nam hoc)." });
                    }
                    if (schoolYear.StartDate.HasValue && model.EndDate < schoolYear.StartDate)
                    {
                        return Json(new { success = false, message = $"Ngay ket thuc hoc ky phai tu ngay {schoolYear.StartDate:dd/MM/yyyy} (ngay bat dau nam hoc)." });
                    }
                }

                // Kiem tra khong bi de len nhau
                foreach (var existing in existingSemesters)
                {
                    if (existing.StartDate.HasValue && existing.EndDate.HasValue)
                    {
                        var eStart = existing.StartDate.Value;
                        var eEnd = existing.EndDate.Value;
                        var mStart = model.StartDate;
                        var mEnd = model.EndDate;

                        // De len nhau: (model.Start < existing.End) && (model.End > existing.Start)
                        bool overlaps = false;
                        if (mStart.HasValue && mEnd.HasValue)
                        {
                            overlaps = mStart.Value < eEnd && mEnd.Value > eStart;
                        }
                        else if (mStart.HasValue)
                        {
                            overlaps = mStart.Value <= eEnd;
                        }
                        else if (mEnd.HasValue)
                        {
                            overlaps = mEnd.Value >= eStart;
                        }

                        if (overlaps)
                        {
                            return Json(new { success = false, message = $"Hoc ky {model.SemesterNumber} bi trung voi hoc ky {existing.SemesterNumber} ({existing.Name}). Cac hoc ky khong duoc nam chong len nhau." });
                        }
                    }
                }

                // Kiem tra thu tu: HK1 phai ket thuc truoc khi HK2 bat dau
                if (model.SemesterNumber == 2)
                {
                    var hk1 = existingSemesters.FirstOrDefault(s => s.SemesterNumber == 1);
                    if (hk1 != null)
                    {
                        if (hk1.EndDate.HasValue && model.StartDate.HasValue && hk1.EndDate.Value >= model.StartDate.Value)
                        {
                            return Json(new { success = false, message = $"Hoc ky 1 phai ket thuc truoc khi hoc ky 2 bat dau. HK1 ket thuc: {hk1.EndDate:dd/MM/yyyy}." });
                        }
                    }
                }
                if (model.SemesterNumber == 1)
                {
                    var hk2 = existingSemesters.FirstOrDefault(s => s.SemesterNumber == 2);
                    if (hk2 != null)
                    {
                        if (model.EndDate.HasValue && hk2.StartDate.HasValue && model.EndDate.Value >= hk2.StartDate.Value)
                        {
                            return Json(new { success = false, message = $"Hoc ky 1 phai ket thuc truoc khi hoc ky 2 bat dau. HK2 bat dau: {hk2.StartDate:dd/MM/yyyy}." });
                        }
                    }
                }

                if (model.Id == 0)
                {
                    _context.Semesters.Add(model);
                }
                else
                {
                    var existing = _context.Semesters.Find(model.Id);
                    if (existing == null || existing.SchoolYearId != model.SchoolYearId)
                    {
                        return Json(new { success = false, message = "Hoc ky khong ton tai." });
                    }
                    existing.Name = model.Name;
                    existing.SemesterNumber = model.SemesterNumber;
                    existing.StartDate = model.StartDate;
                    existing.EndDate = model.EndDate;
                }

                _context.SaveChanges();
                return Json(new { success = true, message = model.Id == 0 ? "Tao hoc ky thanh cong." : "Cap nhat hoc ky thanh cong.", reload = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /SchoolYear/DeleteSemester
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSemester(int id)
        {
            try
            {
                var semester = _context.Semesters
                    .Include(s => s.Scores)
                    .Include(s => s.SemesterResults)
                    .Include(s => s.ConductScores)
                    .Include(s => s.Violations)
                    .Include(s => s.Commendations)
                    .Include(s => s.TeacherClasses)
                    .FirstOrDefault(s => s.Id == id);

                if (semester == null)
                {
                    return Json(new { success = false, message = "Hoc ky khong ton tai." });
                }

                // Kiem tra rang buoc du lieu
                if (semester.Scores.Count > 0)
                {
                    return Json(new { success = false, message = $"Khong the xoa vi hoc ky co {semester.Scores.Count} diem so. Vui long xoa diem truoc." });
                }
                if (semester.SemesterResults.Count > 0)
                {
                    return Json(new { success = false, message = $"Khong the xoa vi hoc ky co {semester.SemesterResults.Count} ket qua. Vui long xoa truoc." });
                }
                if (semester.ConductScores.Count > 0)
                {
                    return Json(new { success = false, message = $"Khong the xoa vi hoc ky co {semester.ConductScores.Count} diem hanh kiem. Vui long xoa truoc." });
                }
                if (semester.Violations.Count > 0)
                {
                    return Json(new { success = false, message = $"Khong the xoa vi hoc ky co {semester.Violations.Count} vi pham. Vui long xoa truoc." });
                }
                if (semester.Commendations.Count > 0)
                {
                    return Json(new { success = false, message = $"Khong the xoa vi hoc ky co {semester.Commendations.Count} khen thuong. Vui long xoa truoc." });
                }
                if (semester.TeacherClasses.Count > 0)
                {
                    return Json(new { success = false, message = $"Khong the xoa vi hoc ky co {semester.TeacherClasses.Count} phan cong. Vui long xoa truoc." });
                }

                _context.Semesters.Remove(semester);
                _context.SaveChanges();
                return Json(new { success = true, message = "Xoa hoc ky thanh cong.", reload = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi khi xoa: " + ex.Message });
            }
        }
    }
}
