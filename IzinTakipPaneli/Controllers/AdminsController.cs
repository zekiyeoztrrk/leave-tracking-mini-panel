using IzinTakipPaneli.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace IzinTakipPaneli.Controllers
{
   
    [Authorize(Roles = "admin")]
    public class AdminsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AdminsController(ApplicationDbContext db) => _db = db;

        /* Aktif departman listesini ViewBag’e ekler */
        public async Task<IActionResult> Index()
        {
            ViewBag.Departments = await _db.Departments
                                           .Where(d => d.IsActive)
                                           .OrderBy(d => d.DepartmentName)
                                           .ToListAsync();
            return View();
        }

        /* ÇALIŞAN LİSTESİ */
        public async Task<IActionResult> Employees(int? deptId)
        {
            // Dropdown için aktif departmanlar
            ViewBag.Departments = await _db.Departments
                                           .Where(d => d.IsActive)
                                           .OrderBy(d => d.DepartmentName)
                                           .ToListAsync();

            // Temel sorgu
            var q = _db.Employees.Include(e => e.Department)
                                  .Where(e => e.IsActive);

            if (deptId.HasValue)
                q = q.Where(e => e.DepartmentID == deptId);

            // Maksimum yıllık izin (ayar yoksa 20)
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

            // Projection + kalan yıllık gün
            var list = await q.Select(e => new
            {
                e.EmployeeID,
                e.FullName,
                Dept = e.Department.DepartmentName,
                e.Username,
                UsedDays = _db.Leaves.Where(l => l.EmployeeID == e.EmployeeID && l.IsActive)
                                      .Sum(l => (int?)l.TotalDays) ?? 0,
                UsedAnnual = _db.Leaves.Where(l => l.EmployeeID == e.EmployeeID &&
                                                   l.IsActive &&
                                                   annualTypeIds.Contains(l.LeaveTypeID))
                                       .Sum(l => (int?)l.TotalDays) ?? 0
            }).AsNoTracking().ToListAsync();

            var vm = list.Select(r => new
            {
                r.EmployeeID,
                r.FullName,
                r.Dept,
                r.Username,
                r.UsedDays,
                Remaining = Math.Max(0, maxAnnual - r.UsedAnnual)
            });

            return View(vm);
        }

        /* Kullanıcı adı benzersiz mi */
        [HttpGet]
        public async Task<IActionResult> CheckUsername(string username) =>
            Json(new { exists = await _db.Employees.AnyAsync(e => e.Username == username && e.IsActive) });

        /* Çalışan ekle */
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(string fullName, int departmentId,
                                                    string username, string password, string userRole)
        {
            if (await _db.Employees.AnyAsync(e => e.Username == username))
                return Json(new { ok = false, message = "Bu kullanıcı adı zaten kullanılıyor!" });

            _db.Employees.Add(new Employee
            {
                FullName = fullName,
                DepartmentID = departmentId,
                Username = username,
                Password = password,
                UserRole = userRole,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        /* Çalışan pasifleştir */
        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var emp = await _db.Employees.FindAsync(id);
            if (emp is not null) { emp.IsActive = false; await _db.SaveChangesAsync(); }
            return RedirectToAction("Employees");
        }

        /* DEPARTMANLAR */
        public async Task<IActionResult> Departments()
        {
            var list = await _db.Departments
                                .Where(d => d.IsActive)
                                .OrderBy(d => d.DepartmentName)
                                .ToListAsync();
            return View(list);
        }

        /* Departman ekle */
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDept(string name)
        {
            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());

            bool exists = await _db.Departments.AnyAsync(d => d.DepartmentName == name && d.IsActive);
            if (exists) return Json(new { ok = false, message = "Bu departman zaten var!" });

            _db.Departments.Add(new Department
            {
                DepartmentName = name,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        /* Departman pasifleştir */
        [HttpPost]
        public async Task<IActionResult> DeleteDept(int id)
        {
            var d = await _db.Departments.FindAsync(id);
            if (d is not null) { d.IsActive = false; await _db.SaveChangesAsync(); }
            return RedirectToAction("Departments");
        }

        /* İZİNLER SAYFASI */
        public async Task<IActionResult> Leaves()
        {
            ViewBag.Departments = await _db.Departments.Where(d => d.IsActive).ToListAsync();
            return View();   // DataTable veriyi AJAX’la çekecek
        }

        /* LeavesData */
        [HttpGet]
        public async Task<IActionResult> LeavesData(string? name, int? dept,
                                                    string? dateFrom, string? dateTo)
        {
            var q = _db.Leaves
                       .Include(l => l.Employee).ThenInclude(e => e.Department)
                       .Include(l => l.LeaveType)
                       .Where(l => l.IsActive);

            if (!string.IsNullOrWhiteSpace(name))
                q = q.Where(l => l.Employee.FullName.Contains(name));

            if (dept.HasValue)
                q = q.Where(l => l.Employee.DepartmentID == dept);

            /* Tarih aralığı çakışma filtresi */
            DateTime? fromDt = null, toDt = null;
            if (DateOnly.TryParseExact(dateFrom ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var df))
                fromDt = df.ToDateTime(TimeOnly.MinValue);

            if (DateOnly.TryParseExact(dateTo ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var dt))
                toDt = dt.ToDateTime(TimeOnly.MaxValue);

            if (fromDt.HasValue && !toDt.HasValue) toDt = fromDt;
            if (fromDt.HasValue && toDt.HasValue)
                q = q.Where(l => l.StartDate <= toDt && l.EndDate >= fromDt);

            var data = await q.OrderByDescending(l => l.StartDate)
                              .Select(l => new
                              {
                                  employee = l.Employee.FullName,
                                  dept = l.Employee.Department.DepartmentName,
                                  type = l.LeaveType.LeaveTypeName,
                                  start = l.StartDate.ToString("dd.MM.yyyy"),
                                  end = l.EndDate.ToString("dd.MM.yyyy"),
                                  days = l.TotalDays
                              }).ToListAsync();

            return Json(new { data });
        }

        /* Yıllık izin limitini güncelle */
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAnnualLimit(int days)
        {
            var s = await _db.Settings.FirstOrDefaultAsync(s => s.IsActive);
            if (s is null)
            {
                _db.Settings.Add(new Setting { MaxAnnualLeaveDays = days, IsActive = true });
            }
            else
            {
                s.MaxAnnualLeaveDays = days;
                s.UpdatedAt = DateTime.Now;
            }
            await _db.SaveChangesAsync();
            return Ok();   // 200
        }

        /* İzin türü ekle */
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLeaveType(string name)
        {
            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.Trim().ToLower());

            bool exists = await _db.LeaveTypes.AnyAsync(t => t.LeaveTypeName == name && t.IsActive);
            if (exists) return Json(new { ok = false, message = "Bu izin türü zaten var!" });

            _db.LeaveTypes.Add(new LeaveType
            {
                LeaveTypeName = name,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }

        /* DASHBOARD APILERİ */

        /* Bugün izinli olan çalışanlar + kalan yıllık gün */
        [HttpGet]
        public async Task<IActionResult> TodayLeavesData()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Maksimum yıllık 
            int maxAnnual = await _db.Settings.Where(s => s.IsActive)
                                              .OrderByDescending(s => s.SettingID)
                                              .Select(s => s.MaxAnnualLeaveDays)
                                              .FirstOrDefaultAsync();
            if (maxAnnual <= 0) maxAnnual = 20;

            // Yıllık türü ID’leri
            var annualTypeIds = await _db.LeaveTypes
                                         .Where(t => t.IsActive && t.LeaveTypeName.ToLower() == "yıllık")
                                         .Select(t => t.LeaveTypeID)
                                         .ToListAsync();

            var list = await _db.Leaves
                .Include(l => l.Employee).ThenInclude(e => e.Department)
                .Include(l => l.LeaveType)
                .Where(l => l.IsActive &&
                            l.StartDate <= today.ToDateTime(TimeOnly.MaxValue) &&
                            l.EndDate >= today.ToDateTime(TimeOnly.MinValue))
                .Select(l => new
                {
                    employee = l.Employee.FullName,
                    dept = l.Employee.Department.DepartmentName,
                    type = l.LeaveType.LeaveTypeName,
                    returnDate = l.EndDate.AddDays(1).ToString("dd.MM.yyyy"),

                    // Kalan yıllık = maxAnnual – (kullanılan yıllık)
                    remaining = Math.Max(0,
                                   maxAnnual -
                                   (_db.Leaves.Where(a => a.EmployeeID == l.EmployeeID &&
                                                          a.IsActive &&
                                                          annualTypeIds.Contains(a.LeaveTypeID))
                                              .Sum(a => (int?)a.TotalDays) ?? 0))
                })
                .OrderBy(l => l.employee)
                .ToListAsync();

            return Json(list);
        }

        /* Bu yıl en çok izin kullanan 5 kişi */
        [HttpGet]
        public async Task<IActionResult> TopLeaveStats()
        {
            var yearStart = new DateTime(DateTime.Today.Year, 1, 1);

            var data = await _db.Leaves
                .Include(l => l.Employee)
                .Where(l => l.IsActive && l.StartDate >= yearStart)
                .GroupBy(l => l.Employee.FullName)
                .Select(g => new { Name = g.Key, Days = g.Sum(x => x.TotalDays) })
                .OrderByDescending(x => x.Days)
                .Take(5)
                .ToListAsync();

            return Json(new
            {
                labels = data.Select(d => d.Name),
                days = data.Select(d => d.Days)
            });
        }
    }
}
