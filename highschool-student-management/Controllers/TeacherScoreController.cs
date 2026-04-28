using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;
using System.Security.Claims;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherScoreController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherScoreController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentTeacherId()
        {
            // Uu tien doc tu Claim duoc set luc dang nhap
            var teacherIdClaim = User.FindFirstValue("TeacherId");
            if (!string.IsNullOrEmpty(teacherIdClaim) && int.TryParse(teacherIdClaim, out int teacherId) && teacherId > 0)
                return teacherId;

            // Fallback: doc NameIdentifier (User.Id) roi tim TeacherId trong DB
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return 0;

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user?.TeacherId ?? 0;
        }

        // GET: /TeacherScore
        public async Task<IActionResult> Index()
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Forbid();

            ViewData["Title"] = "So diem";
            return View();
        }

        // GET: /TeacherScore/GetFilterData
        public async Task<IActionResult> GetFilterData()
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien (TeacherId = 0). Vui long lien he quan tri.", teacherId = 0 });

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            if (currentYear == null)
                return Json(new { success = false, message = "Khong co nam hoc hien tai." });

            var assignments = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId && tc.Semester.SchoolYearId == currentYear.Id)
                .Include(tc => tc.Class)
                .Include(tc => tc.Semester)
                .Include(tc => tc.Subject)
                .ToListAsync();

            var classes = assignments
                .GroupBy(tc => new { tc.ClassId, tc.SemesterId })
                .Select(g => new ClassAssignmentDropdown
                {
                    ClassId = g.Key.ClassId,
                    SemesterId = g.Key.SemesterId,
                    ClassName = g.First().Class.Name,
                    SemesterName = g.First().Semester.Name
                })
                .ToList();

            var subjects = assignments
                .Select(tc => new SubjectDropdown { Id = tc.SubjectId, Name = tc.Subject.Name, Code = tc.Subject.Code })
                .DistinctBy(s => s.Id)
                .ToList();

            var semesters = await _context.Semesters
                .Where(s => s.SchoolYearId == currentYear.Id)
                .OrderBy(s => s.SemesterNumber)
                .Select(s => new SemesterDropdown { Id = s.Id, Name = s.Name, SchoolYearId = s.SchoolYearId })
                .ToListAsync();

            return Json(new
            {
                success = true,
                classes = classes,
                subjects = subjects,
                semesters = semesters,
                schoolYearId = currentYear.Id,
                schoolYearName = currentYear.Name
            });
        }

        // GET: /TeacherScore/GetSubjectsByClass?classId=X&semesterId=Y
        public async Task<IActionResult> GetSubjectsByClass(int classId, int semesterId)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            var subjects = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId
                    && tc.ClassId == classId
                    && tc.SemesterId == semesterId)
                .Include(tc => tc.Subject)
                .Select(tc => new SubjectByClassItem
                {
                    Id = tc.SubjectId,
                    Name = tc.Subject.Name,
                    Code = tc.Subject.Code,
                    SemesterId = tc.SemesterId,
                    SemesterName = tc.Semester.Name
                })
                .ToListAsync();

            return Json(new { success = true, subjects = subjects });
        }

        // GET: /TeacherScore/GetScoreGrid?classId=X&subjectId=Y&semesterId=Z
        public async Task<IActionResult> GetScoreGrid(int classId, int subjectId, int semesterId)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            try
            {
                // Kiem tra giao vien co phan cong day mon nay cho lop nay
                var assignment = await _context.TeacherClasses
                    .AnyAsync(tc => tc.TeacherId == teacherId
                        && tc.ClassId == classId
                        && tc.SubjectId == subjectId
                        && tc.SemesterId == semesterId);

                if (!assignment)
                    return Json(new { success = false, message = "Ban khong duoc phan cong day mon nay cho lop nay." });

                var cls = await _context.Classes.FindAsync(classId);
                var subject = await _context.Subjects.FindAsync(subjectId);
                var semester = await _context.Semesters.FindAsync(semesterId);

                if (cls == null || subject == null || semester == null)
                    return Json(new { success = false, message = "Du lieu khong hop le." });

                // Lay cac loai diem
                var scoreTypes = await _context.ScoreTypes
                    .OrderBy(st => st.Weight)
                    .ToListAsync();

                // Lay hoc sinh trong lop
                var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
                var studentClassRecords = await _context.StudentClasses
                    .Where(sc => sc.ClassId == classId && sc.SchoolYearId == currentYear!.Id)
                    .Include(sc => sc.Student)
                    .OrderBy(sc => sc.Student.FullName)
                    .ToListAsync();

                var studentIds = studentClassRecords.Select(sc => sc.StudentId).ToList();

                // Lay diem da co
                var existingScores = await _context.Scores
                    .Where(s => studentIds.Contains(s.StudentId)
                        && s.SubjectId == subjectId
                        && s.SemesterId == semesterId)
                    .ToListAsync();

                var scoreGrid = new Dictionary<(int, int), QuanLyHocSinh.Models.Score>();
                foreach (var s in existingScores)
                {
                    scoreGrid[(s.StudentId, s.ScoreTypeId)] = s;
                }

                var studentRows = studentClassRecords
                    .Select(sc => new StudentScoreRow
                    {
                        StudentId = sc.StudentId,
                        StudentCode = sc.Student.StudentCode,
                        FullName = sc.Student.FullName,
                        Scores = scoreTypes.Select(st =>
                        {
                            var key = (sc.StudentId, st.Id);
                            if (scoreGrid.TryGetValue(key, out var existingScore))
                            {
                                return new ScoreCell
                                {
                                    StudentId = sc.StudentId,
                                    ScoreTypeId = st.Id,
                                    ScoreId = existingScore.Id,
                                    Value = existingScore.ScoreValue,
                                    ExamDate = existingScore.ExamDate
                                };
                            }
                            return new ScoreCell
                            {
                                StudentId = sc.StudentId,
                                ScoreTypeId = st.Id,
                                ScoreId = 0,
                                Value = null,
                                ExamDate = null
                            };
                        }).ToList()
                    })
                    .ToList();

                var model = new ScoreGridViewModel
                {
                    ClassId = classId,
                    ClassName = cls.Name,
                    SubjectId = subjectId,
                    SubjectName = subject.Name,
                    SemesterId = semesterId,
                    SemesterName = semester.Name,
                    ScoreTypes = scoreTypes.Select(st => new ScoreTypeHeader
                    {
                        Id = st.Id,
                        Name = st.Name,
                        Weight = st.Weight
                    }).ToList(),
                    StudentRows = studentRows
                };

                return PartialView("_ScoreGrid", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /TeacherScore/SaveBulkScores
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBulkScores([FromBody] SaveBulkScoresRequest request)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            try
            {
                // Kiem tra phan cong
                var assignment = await _context.TeacherClasses
                    .AnyAsync(tc => tc.TeacherId == teacherId
                        && tc.ClassId == request.ClassId
                        && tc.SubjectId == request.SubjectId
                        && tc.SemesterId == request.SemesterId);

                if (!assignment)
                    return Json(new { success = false, message = "Ban khong duoc phan cong day mon nay cho lop nay." });

                if (request.Scores == null || !request.Scores.Any())
                    return Json(new { success = false, message = "Khong co du lieu de luu." });

                var now = DateTime.Now;
                var savedCount = 0;

                foreach (var item in request.Scores)
                {
                    if (!item.ScoreValue.HasValue)
                        continue;

                    // Kiem tra gia tri hop le (thang 10)
                    if (item.ScoreValue < 0 || item.ScoreValue > 10)
                        continue;

                    if (item.ScoreId > 0)
                    {
                        // Cap nhat diem da co
                        var existing = await _context.Scores.FindAsync(item.ScoreId);
                        if (existing != null)
                        {
                            existing.ScoreValue = item.ScoreValue;
                            existing.ExamDate = item.ExamDate ?? DateOnly.FromDateTime(now);
                            existing.Note = item.Note;
                            savedCount++;
                        }
                    }
                    else
                    {
                        // Tao moi diem
                        _context.Scores.Add(new QuanLyHocSinh.Models.Score
                        {
                            StudentId = item.StudentId,
                            SubjectId = request.SubjectId,
                            SemesterId = request.SemesterId,
                            ScoreTypeId = item.ScoreTypeId,
                            ScoreValue = item.ScoreValue,
                            ExamDate = item.ExamDate ?? DateOnly.FromDateTime(now),
                            Note = item.Note,
                            TeacherId = teacherId,
                            CreatedAt = now
                        });
                        savedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Luu {savedCount} diem thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
