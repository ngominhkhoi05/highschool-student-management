using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using QuanLyHocSinh.Models;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;

        public SettingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Setting
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Settings
                .Include(s => s.UpdatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s =>
                    s.Key.ToLower().Contains(search) ||
                    (s.Value != null && s.Value.ToLower().Contains(search)));
            }

            var settings = await query
                .OrderBy(s => s.Key)
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Title"] = "Cau hinh he thong";
            return View(settings);
        }

        // GET: /Setting/_CreateOrEdit?id=0 (them moi) hoac id>0 (sua)
        [HttpGet]
        public IActionResult _CreateOrEdit(int id = 0)
        {
            var model = new Setting();
            if (id > 0)
            {
                model = _context.Settings.Find(id)!;
                if (model == null)
                {
                    return NotFound("Cau hinh khong ton tai.");
                }
            }
            return PartialView(model);
        }

        // POST: /Setting/SaveSetting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSetting(Setting model)
        {
            try
            {
                ModelState.Remove("UpdatedByUser");

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Du lieu khong hop le. Vui long kiem tra lai." });
                }

                if (string.IsNullOrWhiteSpace(model.Key))
                {
                    return Json(new { success = false, message = "Khoa cai dat khong duoc de trong." });
                }

                // Kiem tra trung lap Key
                var existing = _context.Settings
                    .Where(s => s.Key == model.Key && s.Id != model.Id)
                    .FirstOrDefault();
                if (existing != null)
                {
                    return Json(new { success = false, message = $"Khoa cai dat '{model.Key}' da ton tai. Vui long chon khoa khac." });
                }

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = string.IsNullOrEmpty(userIdStr) ? (int?)null : int.Parse(userIdStr);

                if (model.Id == 0)
                {
                    model.UpdatedAt = DateTime.Now;
                    model.UpdatedBy = userId;
                    _context.Settings.Add(model);
                }
                else
                {
                    var existingSetting = _context.Settings.Find(model.Id);
                    if (existingSetting == null)
                    {
                        return Json(new { success = false, message = "Cau hinh khong ton tai." });
                    }
                    existingSetting.Key = model.Key;
                    existingSetting.Value = model.Value;
                    existingSetting.Description = model.Description;
                    existingSetting.UpdatedAt = DateTime.Now;
                    existingSetting.UpdatedBy = userId;
                }

                _context.SaveChanges();
                return Json(new { success = true, message = model.Id == 0 ? "Them cau hinh thanh cong." : "Cap nhat cau hinh thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /Setting/DeleteSetting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSetting(int id)
        {
            try
            {
                var setting = _context.Settings.Find(id);
                if (setting == null)
                {
                    return Json(new { success = false, message = "Cau hinh khong ton tai." });
                }

                _context.Settings.Remove(setting);
                _context.SaveChanges();
                return Json(new { success = true, message = "Xoa cau hinh thanh cong." });
            }
            catch (DbUpdateException)
            {
                return Json(new { success = false, message = "Khong the xoa vi cau hinh nay dang duoc su dung." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
