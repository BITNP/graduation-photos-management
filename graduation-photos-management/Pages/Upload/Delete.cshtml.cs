using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GraduationPhotosManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace GraduationPhotosManagement.Pages.Upload
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly PhotoDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _imagePath;

        public DeleteModel(PhotoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _imagePath = _configuration["uploadpath"];
        }

        // [BindProperty]
        public UploadedPhoto UploadedPhoto { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.UploadedPhotos == null)
            {
                return NotFound();
            }

            var uploadedphoto = await _context.UploadedPhotos.FirstOrDefaultAsync(m => m.Id == id);

            if (uploadedphoto == null)
            {
                return NotFound();
            }
            else 
            {
                UploadedPhoto = uploadedphoto;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.UploadedPhotos == null)
            {
                return NotFound();
            }
            var uploadedphoto = await _context.UploadedPhotos.FindAsync(id);

            if (uploadedphoto != null)
            {
                var guidString = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Guid").Value;
                var studentid = Guid.Parse(guidString);
                if (studentid == Guid.Empty)
                {
                    throw new ApplicationException("Bad user claims");
                }
                if (uploadedphoto.StudentId != studentid)
                {
                    return Forbid();
                }
                UploadedPhoto = uploadedphoto;
                var saveToPath = Path.Combine(_imagePath, UploadedPhoto.StorageName);
                System.IO.File.Delete(saveToPath);
                _context.UploadedPhotos.Remove(UploadedPhoto);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
