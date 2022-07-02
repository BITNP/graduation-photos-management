using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraduationPhotosManagement.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        public LogoutModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }
        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User {Name} logged out at {Time}.",
                    User.Identity.Name, DateTime.UtcNow);
            }
            return RedirectToPage("/Index");
        }
    }
}
