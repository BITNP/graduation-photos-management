using GraduationPhotosManagement.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GraduationPhotosManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;


namespace GraduationPhotosManagement.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly PhotoDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _salt;
        public LoginModel(ILogger<LoginModel> logger, PhotoDbContext context, IMemoryCache cache, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
            _configuration = configuration;
            _salt = _configuration["salt"];
            DbInitializer.Initialize(_context, _salt);
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "姓名")]
            public string FullName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [StringLength(6)]
            [Display(Name = "身份证号后六位")]
            public string Password { get; set; }
            [Required]
            [StringLength(4)]
            [Display(Name = "验证码")]
            public string Captcha { get; set; }
        }

        public IActionResult OnGet()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                var captcha = HttpContext.Session.GetString("CAPTCHA");
                if (string.IsNullOrEmpty(captcha) || !captcha.Equals(Input.Captcha, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "验证码错误");
                    if (!string.IsNullOrEmpty(captcha))
                    {
                        HttpContext.Session.Remove("CAPTCHA");
                    }
                    return Page();
                }
                HttpContext.Session.Remove("CAPTCHA");
                var user = await AuthenticateUser(Input.FullName, Input.Password);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "身份验证失败");
                    return Page();
                }

                var claims = new List<Claim>
                {
                    new Claim("Guid", user.Guid.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User {FullName} logged in at {Time}.",
                    user.FullName, DateTime.UtcNow);

                return LocalRedirect(Url.GetLocalUrl(returnUrl));
            }

            // Something failed. Redisplay the form.
            return Page();
        }
        public class ApplicationUserCache
        {
            public string Name { get; set; }
            public string PasswordHash { get; set; }
            public Guid Id { get; set; }
        }
        private static string Byte2Hex(byte[] bytes)
        {
            var s = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                s.Append((bytes[i] & 0xFF).ToString("x2"));
            }
            return s.ToString();
        }
        private async Task<ApplicationUser> AuthenticateUser(string fullName, string password)
        {
            var salted = Encoding.Default.GetBytes($"{password.ToUpper()}{_salt}");
            var hash = SHA256.HashData(salted);
            var cacheKey = fullName;
            if (!_cache.TryGetValue(cacheKey, out List<ApplicationUserCache> cache))
            {
                var q =
                    from u in _context.Students
                    where u.Name == fullName
                    select new ApplicationUserCache()
                        { 
                            Name = u.Name, 
                            PasswordHash = u.SaltedPasswordHash,
                            Id = u.Id
                        };
                cache = await q.ToListAsync();
                _cache.Set(cacheKey, cache, TimeSpan.FromMinutes(20));
            }

            var id = (from c in cache
                      where c.PasswordHash == Byte2Hex(hash)
                      select c.Id).SingleOrDefault();
            if (id == Guid.Empty)
            {
                Console.WriteLine($"{fullName} 登录失败");
                return null;
            }
            else
            {
                return new ApplicationUser()
                {
                    FullName = fullName,
                    Guid = id
                };
            }
        }
    }
}
