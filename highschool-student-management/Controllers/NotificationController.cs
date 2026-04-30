using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;

namespace highschool_student_management.Controllers
{
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Notification
        public async Task<IActionResult> Index(string? search)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var query = _context.NotificationRecipients
                .Where(nr => nr.UserId == userId)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Sender)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(nr =>
                    nr.Notification.Title.ToLower().Contains(search) ||
                    (nr.Notification.Content != null && nr.Notification.Content.ToLower().Contains(search)));
            }

            var notifications = await query
                .OrderByDescending(nr => nr.Notification.CreatedAt)
                .Select(nr => new NotificationViewModel
                {
                    Id = nr.NotificationId,
                    Title = nr.Notification.Title,
                    Content = nr.Notification.Content,
                    Type = nr.Notification.Type ?? 1,
                    SenderName = nr.Notification.Sender != null ? nr.Notification.Sender.Username : "He thong",
                    IsRead = nr.IsRead,
                    ReadAt = nr.ReadAt,
                    CreatedAt = nr.Notification.CreatedAt,
                    PublishedAt = nr.Notification.PublishedAt
                })
                .ToListAsync();

            var unreadCount = notifications.Count(n => n.IsRead == 0);
            ViewData["UnreadCount"] = unreadCount;
            ViewData["Search"] = search;

            return View(notifications);
        }

        // GET: /Notification/_CreateNotification (Partial View - Chi Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> _CreateNotification()
        {
            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .Select(s => new SemesterDropdown { Id = s.Id, Name = s.Name, SchoolYearId = s.SchoolYearId })
                    .ToListAsync()
                : new List<SemesterDropdown>();

            var classes = currentYear != null
                ? await _context.Classes
                    .Where(c => c.SchoolYearId == currentYear.Id)
                    .OrderBy(c => c.GradeLevel)
                    .ThenBy(c => c.Name)
                    .Select(c => new ClassDropdown { Id = c.Id, Name = c.Name, GradeLevel = c.GradeLevel ?? 0 })
                    .ToListAsync()
                : new List<ClassDropdown>();

            var users = await _context.Users
                .Where(u => u.IsActive == 1)
                .OrderBy(u => u.RoleId)
                .ThenBy(u => u.Username)
                .Select(u => new UserDropdown
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleName = u.Role != null ? u.Role.Name : ""
                })
                .ToListAsync();

            var viewModel = new NotificationCreateViewModel
            {
                Semesters = semesters,
                Classes = classes,
                Users = users
            };

            return PartialView("_CreateNotification", viewModel);
        }

        // POST: /Notification/SendNotification (Chi Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(NotificationCreateViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (string.IsNullOrWhiteSpace(model.Title))
                return Json(new { success = false, message = "Tieu de khong duoc trong." });

            var notification = new QuanLyHocSinh.Models.Notification
            {
                Title = model.Title.Trim(),
                Content = model.Content?.Trim(),
                Type = model.Type,
                SenderId = userId,
                TargetType = model.TargetType,
                CreatedAt = DateTime.Now,
                PublishedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Lay danh sach UserId theo TargetType
            var userIds = new List<int>();

            switch (model.TargetType)
            {
                case 1: // Toan truong
                    userIds = await _context.Users
                        .Where(u => u.IsActive == 1)
                        .Select(u => u.Id)
                        .ToListAsync();
                    break;

                case 2: // Theo khoi (lay hoc sinh cua cac lop trong khoi)
                    var gradeLevels = model.GradeLevels ?? new List<int>();
                    if (gradeLevels.Any())
                    {
                        var classIds = await _context.Classes
                            .Where(c => gradeLevels.Contains(c.GradeLevel ?? 0))
                            .Select(c => c.Id)
                            .ToListAsync();

                        userIds = await _context.StudentClasses
                            .Where(sc => classIds.Contains(sc.ClassId))
                            .Join(_context.Students, sc => sc.StudentId, s => s.Id, (sc, s) => s.Id)
                            .Distinct()
                            .ToListAsync();

                        var parentUserIds = await _context.ParentStudents
                            .Where(ps => userIds.Contains(ps.StudentId))
                            .Join(_context.Users.Where(u => u.ParentId != null), ps => ps.ParentId, u => u.ParentId ?? 0, (ps, u) => u.Id)
                            .ToListAsync();

                        userIds.AddRange(parentUserIds);

                        var teacherUserIds = await _context.Classes
                            .Where(c => classIds.Contains(c.Id) && c.HomeroomTeacherId != null)
                            .Join(_context.Users.Where(u => u.TeacherId != null), c => c.HomeroomTeacherId, u => u.TeacherId ?? 0, (c, u) => u.Id)
                            .ToListAsync();
                        userIds.AddRange(teacherUserIds);
                    }
                    break;

                case 3: // Theo lop
                    var selectedClassIds = model.ClassIds ?? new List<int>();
                    if (selectedClassIds.Any())
                    {
                        userIds = await _context.StudentClasses
                            .Where(sc => selectedClassIds.Contains(sc.ClassId))
                            .Join(_context.Students, sc => sc.StudentId, s => s.Id, (sc, s) => s.Id)
                            .Distinct()
                            .ToListAsync();

                        var parentUserIds = await _context.ParentStudents
                            .Where(ps => userIds.Contains(ps.StudentId))
                            .Join(_context.Users.Where(u => u.ParentId != null), ps => ps.ParentId, u => u.ParentId ?? 0, (ps, u) => u.Id)
                            .ToListAsync();
                        userIds.AddRange(parentUserIds);

                        var teacherUserIds = await _context.Classes
                            .Where(c => selectedClassIds.Contains(c.Id) && c.HomeroomTeacherId != null)
                            .Join(_context.Users.Where(u => u.TeacherId != null), c => c.HomeroomTeacherId, u => u.TeacherId ?? 0, (c, u) => u.Id)
                            .ToListAsync();
                        userIds.AddRange(teacherUserIds);
                    }
                    break;

                case 4: // Ca nhan
                    userIds = model.UserIds ?? new List<int>();
                    break;
            }

            userIds = userIds.Distinct().ToList();

            if (userIds.Any())
            {
                var recipients = userIds.Select(uid => new QuanLyHocSinh.Models.NotificationRecipient
                {
                    NotificationId = notification.Id,
                    UserId = uid,
                    IsRead = 0
                }).ToList();

                _context.NotificationRecipients.AddRange(recipients);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine($"[Notification] Da gui '{notification.Title}' den {userIds.Count} nguoi.");

            return Json(new { success = true, message = $"Gui thong bao thanh cong den {userIds.Count} nguoi." });
        }

        // POST: /Notification/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(nr => nr.NotificationId == notificationId && nr.UserId == userId);

            if (recipient == null)
                return Json(new { success = false, message = "Khong tim thay thong bao." });

            if (recipient.IsRead == 0)
            {
                recipient.IsRead = 1;
                recipient.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
    }
}
