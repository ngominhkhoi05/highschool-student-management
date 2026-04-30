using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using QuanLyHocSinh.Models;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ScoreTypeController : Controller
    {
        private readonly AppDbContext _context;

        public ScoreTypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /ScoreType
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.ScoreTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(st => st.Name.ToLower().Contains(search));
            }

            var scoreTypes = await query
                .OrderBy(st => st.Name)
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Title"] = "Quan ly loai diem";
            return View(scoreTypes);
        }

        // GET: /ScoreType/_CreateOrEdit?id=0 (them moi) hoac id>0 (sua)
        [HttpGet]
        public IActionResult _CreateOrEdit(int id = 0)
        {
            var model = new ScoreType();
            if (id > 0)
            {
                model = _context.ScoreTypes.Find(id)!;
                if (model == null)
                {
                    return NotFound("Loai diem khong ton tai.");
                }
            }
            return PartialView(model);
        }

        // POST: /ScoreType/SaveScoreType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveScoreType(ScoreType model)
        {
            try
            {
                ModelState.Remove("Scores");

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Du lieu khong hop le. Vui long kiem tra lai." });
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Ten loai diem khong duoc de trong." });
                }

                if (model.Weight <= 0)
                {
                    return Json(new { success = false, message = "He so phai lon hon 0." });
                }

                if (model.MinCount < 0)
                {
                    return Json(new { success = false, message = "So lan kiem tra toi thieu khong duoc am." });
                }

                if (model.Id == 0)
                {
                    _context.ScoreTypes.Add(model);
                }
                else
                {
                    var existing = _context.ScoreTypes.Find(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Loai diem khong ton tai." });
                    }
                    existing.Name = model.Name;
                    existing.Weight = model.Weight;
                    existing.MinCount = model.MinCount;
                }

                _context.SaveChanges();
                return Json(new { success = true, message = model.Id == 0 ? "Them loai diem thanh cong." : "Cap nhat loai diem thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /ScoreType/DeleteScoreType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteScoreType(int id)
        {
            try
            {
                var scoreType = _context.ScoreTypes.Find(id);
                if (scoreType == null)
                {
                    return Json(new { success = false, message = "Loai diem khong ton tai." });
                }

                _context.ScoreTypes.Remove(scoreType);
                _context.SaveChanges();
                return Json(new { success = true, message = "Xoa loai diem thanh cong." });
            }
            catch (DbUpdateException)
            {
                return Json(new { success = false, message = "Khong the xoa vi loai diem nay dang duoc su dung (co diem hoc sinh). Vui long xoa du lieu lien quan truoc." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
