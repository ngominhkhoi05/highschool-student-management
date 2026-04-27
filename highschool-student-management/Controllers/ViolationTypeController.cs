using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using QuanLyHocSinh.Models;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ViolationTypeController : Controller
    {
        private readonly AppDbContext _context;

        public ViolationTypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /ViolationType
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var violationTypes = await _context.ViolationTypes
                .OrderBy(vt => vt.Name)
                .ToListAsync();

            ViewData["Title"] = "Quan ly loai vi pham";
            return View(violationTypes);
        }

        // GET: /ViolationType/_CreateOrEdit?id=0 (them moi) hoac id>0 (sua)
        [HttpGet]
        public IActionResult _CreateOrEdit(int id = 0)
        {
            var model = new ViolationType();
            if (id > 0)
            {
                model = _context.ViolationTypes.Find(id)!;
                if (model == null)
                {
                    return NotFound("Loai vi pham khong ton tai.");
                }
            }
            return PartialView(model);
        }

        // POST: /ViolationType/SaveViolationType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveViolationType(ViolationType model)
        {
            try
            {
                ModelState.Remove("Violations");

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Du lieu khong hop le. Vui long kiem tra lai." });
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Ten loai vi pham khong duoc de trong." });
                }

                if (model.Severity.HasValue && (model.Severity < 1 || model.Severity > 3))
                {
                    return Json(new { success = false, message = "Muc do vi pham chi duoc phep la 1 (Nhe), 2 (Trung binh), hoac 3 (Nang)." });
                }

                if (model.Id == 0)
                {
                    _context.ViolationTypes.Add(model);
                }
                else
                {
                    var existing = _context.ViolationTypes.Find(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Loai vi pham khong ton tai." });
                    }
                    existing.Name = model.Name;
                    existing.Severity = model.Severity;
                    existing.DeductPoints = model.DeductPoints;
                    existing.Description = model.Description;
                }

                _context.SaveChanges();
                return Json(new { success = true, message = model.Id == 0 ? "Them loai vi pham thanh cong." : "Cap nhat loai vi pham thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /ViolationType/DeleteViolationType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteViolationType(int id)
        {
            try
            {
                var violationType = _context.ViolationTypes.Find(id);
                if (violationType == null)
                {
                    return Json(new { success = false, message = "Loai vi pham khong ton tai." });
                }

                _context.ViolationTypes.Remove(violationType);
                _context.SaveChanges();
                return Json(new { success = true, message = "Xoa loai vi pham thanh cong." });
            }
            catch (DbUpdateException)
            {
                return Json(new { success = false, message = "Khong the xoa vi loai vi pham nay dang duoc su dung (co vi pham hoc sinh). Vui long xoa du lieu lien quan truoc." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
