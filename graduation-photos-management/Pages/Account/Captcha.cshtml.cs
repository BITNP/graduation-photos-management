using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GraduationPhotosManagement.Captcha;

namespace GraduationPhotosManagement.Pages.Account
{
    public class CaptchaModel : PageModel
    {
        private readonly SecurityCodeHelper _helper;
        public CaptchaModel(SecurityCodeHelper helper)
        {
            _helper = helper;
        }

        public IActionResult OnGet()
        {
            var code = _helper.GetRandomEnDigitalText(4);
            HttpContext.Session.SetString("CAPTCHA", code);
            var imgbyte = _helper.GetEnDigitalCodeByte(code);

            return File(imgbyte, "image/jpeg");
        }
    }
}
