using GraduationPhotosManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace GraduationPhotosManagement.Pages.Upload
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public IList<UploadedPhoto> UploadedPhoto { get; set; }
        private readonly PhotoDbContext _context;
        //private readonly IConfiguration _configuration;
        //private readonly string _imagePath;

        public IndexModel(PhotoDbContext context)
        {
            _context = context;
            //_configuration = configuration;
            //_imagePath = _configuration["uploadpath"];
        }
        public async Task OnGetAsync()
        {
            var guidString = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Guid").Value;
            var id = Guid.Parse(guidString);
            if (id == Guid.Empty)
            {
                throw new ApplicationException("Bad user claims");
            }
            UploadedPhoto = await _context.UploadedPhotos
                .Where(u => u.StudentId == id)
                .ToListAsync();
        }
    }
}
