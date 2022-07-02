using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GraduationPhotosManagement.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace GraduationPhotosManagement.Pages.Upload
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly PhotoDbContext _context;

        public EditModel(PhotoDbContext context)
        {
            _context = context;
        }

        public UploadedPhoto UploadedPhoto { get; set; } = default!;

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public int? Id { get; set; }
            [Required]
            [StringLength(200)]
            [Display(Name = "描述")]
            public string Description { get; set; }

        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var uploadedphoto =  await _context.UploadedPhotos.FirstOrDefaultAsync(m => m.Id == id);
            if (uploadedphoto == null)
            {
                return NotFound();
            }
            UploadedPhoto = uploadedphoto;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }
            var uploadedphoto = await _context.UploadedPhotos.FirstOrDefaultAsync(m => m.Id == Input.Id);
            if (uploadedphoto == null)
            {
                return NotFound();
            }
            UploadedPhoto = uploadedphoto;

            var guidString = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Guid").Value;
            var id = Guid.Parse(guidString);
            if (id == Guid.Empty)
            {
                throw new ApplicationException("Bad user claims");
            }

            if (UploadedPhoto.StudentId != id)
            {
                return Forbid();
            }

            UploadedPhoto.Description = Input.Description;
            _context.Attach(UploadedPhoto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UploadedPhotoExists(UploadedPhoto.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToPage("./Index");
        }

        private bool UploadedPhotoExists(int id)
        {
          return _context.UploadedPhotos.Any(e => e.Id == id);
        }
    }
}
