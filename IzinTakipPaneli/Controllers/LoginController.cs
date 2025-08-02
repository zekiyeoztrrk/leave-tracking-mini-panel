using Microsoft.AspNetCore.Mvc;
using IzinTakipPaneli.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace IzinTakipPaneli.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        /* DbContext bağımlılığı */
        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        /* Boş login formunu göster */
        [HttpGet, AllowAnonymous]
        public IActionResult Index() => View();

        /* Kimlik doğrulama */
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Index(string username, string password)
        {
            // Boş alan kontrolü
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Kullanıcı adı ve şifre zorunludur.";
                return View();
            }

            // Aktif kullanıcıyı getir
            var user = await _context.Employees
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            // Kullanıcı yoksa veya şifre tutmuyorsa
            if (user is null || user.Password != password)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
                return View();
            }

            // Rol bilgisini hazırla (varsayılan user)
            var role = (user.UserRole ?? "user").ToLowerInvariant();

            /* Cookie Authentication */
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Oturumu başlat
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Küçük session saklaması 
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", role);

            // Rolüne göre yönlendir
            return role == "admin"
                   ? RedirectToAction("Index", "Admins")
                   : RedirectToAction("Index", "Users");
        }

        /* Çıkış yap ve login sayfasına dön */
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
