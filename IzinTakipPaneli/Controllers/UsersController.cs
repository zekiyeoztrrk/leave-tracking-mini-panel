using IzinTakipPaneli.Models;
using IzinTakipPaneli.Models.ViewModels;
using IzinTakipPaneli.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IzinTakipPaneli.Controllers
{
    [Authorize(Roles = "user")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWorkingDayCalculator _workdays;

        public UsersController(ApplicationDbContext db, IWorkingDayCalculator workdays)
        {
            _db = db;
            _workdays = workdays;
        }

        /* Kullanıcı dashboard */
        public async Task<IActionResult> Index()
        {
            // Oturum açan kullanıcının adı
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Login");

            // Çalışanı departmanıyla birlikte çek
            var emp = await _db.Employees
                               .Include(e => e.Department)
                               .FirstOrDefaultAsync(e => e.Username == username && e.IsActive);
            if (emp is null)
                return RedirectToAction("Index", "Login");

            int currentYear = DateTime.Today.Year;

            // Yıllık izin üst sınırı (ayar yoksa 20)
            int maxAnnual = await _db.Settings.Where(s => s.IsActive)
                                              .OrderByDescending(s => s.SettingID)
                                              .Select(s => s.MaxAnnualLeaveDays)
                                              .FirstOrDefaultAsync();
            if (maxAnnual <= 0) maxAnnual = 20;

            // “Yıllık” izin türü ID’leri
            var annualTypeIds = await _db.LeaveTypes
                                         .Where(t => t.IsActive &&
                                                     t.LeaveTypeName.ToLower() == "yıllık")
                                         .Select(t => t.LeaveTypeID)
                                         .ToListAsync();

            // Bu yıl kullanılan toplam yıllık izin
            int usedAnnual = await _db.Leaves
                                      .Where(l => l.EmployeeID == emp.EmployeeID &&
                                                  l.IsActive &&
                                                  annualTypeIds.Contains(l.LeaveTypeID) &&
                                                  l.StartDate.Year == currentYear &&
                                                  l.EndDate.Year == currentYear)
                                      .SumAsync(l => (int?)l.TotalDays) ?? 0;

            // Kullanıcıya ait izin listesi
            var list = await _db.Leaves
                                .Include(l => l.LeaveType)
                                .Where(l => l.EmployeeID == emp.EmployeeID && l.IsActive)
                                .OrderByDescending(l => l.CreatedAt)
                                .Select(l => new LeaveRowVm
                                {
                                    LeaveType = l.LeaveType.LeaveTypeName,
                                    StartDate = l.StartDate,
                                    EndDate = l.EndDate,
                                    TotalDays = l.TotalDays,
                                    CreatedAt = l.CreatedAt
                                })
                                .ToListAsync();

            // İzin türü dropdown’u
            var leaveTypes = await _db.LeaveTypes
                                      .Where(t => t.IsActive)
                                      .OrderBy(t => t.LeaveTypeName)
                                      .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                                      {
                                          Value = t.LeaveTypeID.ToString(),
                                          Text = t.LeaveTypeName
                                      })
                                      .ToListAsync();

            // ViewModel 
            var vm = new UserDashboardVm
            {
                Username = emp.Username,
                FullName = emp.FullName,
                DepartmentName = emp.Department?.DepartmentName ?? "-",
                MaxAnnualDays = maxAnnual,
                UsedAnnualDays = usedAnnual,
                Leaves = list,
                LeaveTypes = leaveTypes
            };

            return View(vm);
        }

        /* İzin aralığını sunucu tarafında doğrula (AJAX) */
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateLeaveRange(DateTime startDate,
                                                            DateTime endDate,
                                                            int leaveTypeId)
        {
            // Bitiş tarihi başlangıçtan önce olamaz
            if (endDate < startDate)
                return Json(new { ok = false, message = "Bitiş tarihi başlangıçtan önce olamaz." });

            // Sadece içinde bulunduğumuz yıl için izin alınabilir
            int year = DateTime.Today.Year;
            if (startDate.Year != year || endDate.Year != year)
                return Json(new { ok = false, message = $"İzin yalnızca {year} yılı içinde seçilmelidir." });

            // Geçmiş tarih engeli
            var today = DateTime.Today;
            if (startDate.Date < today || endDate.Date < today)
                return Json(new { ok = false, message = "Bugünden önceki tarihler için izin alamazsınız." });

            // Kullanıcıyı bul
            var username = User.Identity?.Name;
            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Username == username && e.IsActive);
            if (emp is null)
                return Json(new { ok = false, message = "Kullanıcı bulunamadı." });

            // Çakışan başka izin var mı?
            bool hasOverlap = await _db.Leaves.AnyAsync(l =>
                l.EmployeeID == emp.EmployeeID &&
                l.IsActive &&
                !(l.EndDate < startDate || l.StartDate > endDate)  // tamamen dışında değilse çakışır
            );
            if (hasOverlap)
                return Json(new { ok = false, message = "Seçilen tarih aralığı mevcut bir izinle çakışıyor." });

            // İş günü hesabı
            int businessDays = _workdays.CountBusinessDays(startDate, endDate);
            if (businessDays <= 0)
                return Json(new { ok = false, message = "Seçilen aralıkta iş günü yok." });

            // İzin türü geçerli mi
            var type = await _db.LeaveTypes.FindAsync(leaveTypeId);
            if (type is null || !type.IsActive)
                return Json(new { ok = false, message = "Geçersiz izin türü." });

            // Yıllık izin ise yıllık limit kontrolü
            if (type.LeaveTypeName.Equals("Yıllık", StringComparison.OrdinalIgnoreCase))
            {
                int maxAnnual = await _db.Settings.Where(s => s.IsActive)
                                                  .OrderByDescending(s => s.SettingID)
                                                  .Select(s => s.MaxAnnualLeaveDays)
                                                  .FirstOrDefaultAsync();
                if (maxAnnual <= 0) maxAnnual = 20;

                int usedAnnual = await _db.Leaves
                                          .Include(l => l.LeaveType)
                                          .Where(l => l.EmployeeID == emp.EmployeeID &&
                                                      l.IsActive &&
                                                      l.StartDate.Year == year &&
                                                      l.EndDate.Year == year &&
                                                      l.LeaveType.LeaveTypeName.ToLower() == "yıllık")
                                          .SumAsync(l => (int?)l.TotalDays) ?? 0;

                if (usedAnnual + businessDays > maxAnnual)
                {
                    var kalan = Math.Max(0, maxAnnual - usedAnnual);
                    return Json(new
                    {
                        ok = false,
                        message = $"Yıllık izin limitini aşıyorsunuz. Kalan: {kalan} gün."
                    });
                }
            }

            return Json(new
            {
                ok = true,
                message = $"Toplam {businessDays} iş günü uygundur.",
                totalDays = businessDays
            });
        }

        /* İzin talebini kaydet */
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLeave(DateTime startDate,
                                                      DateTime endDate,
                                                      int leaveTypeId)
        {
            // Sunucu tarafı tekrar doğrulama
            var validation = await ValidateLeaveRange(startDate, endDate, leaveTypeId) as JsonResult;
            dynamic val = validation!.Value!;
            if (val.ok != true)
            {
                TempData["Error"] = (string)val.message;
                return RedirectToAction("Index");
            }

            int totalDays = (int)val.totalDays;

            var username = User.Identity!.Name!;
            var emp = await _db.Employees.FirstAsync(e => e.Username == username && e.IsActive);

            // Yeni izin kaydı oluştur
            var leave = new Leave
            {
                EmployeeID = emp.EmployeeID,
                LeaveTypeID = leaveTypeId,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                TotalDays = totalDays,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Leaves.Add(leave);
            await _db.SaveChangesAsync();

            TempData["Success"] = "İzin talebiniz kaydedildi.";
            return RedirectToAction("Index");
        }
    }
}
