using GraduationPhotosManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;

namespace GraduationPhotosManagement.Controllers
{
    /// <summary>
    /// controller for upload large file
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly ILogger<FileUploadController> _logger;
        private readonly PhotoDbContext _context;
        private readonly static string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly IConfiguration _configuration;
        private readonly string _imagePath;
        public FileUploadController(ILogger<FileUploadController> logger, PhotoDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _imagePath = _configuration["uploadpath"];
        }

        /// <summary>
        /// Action for upload large file
        /// </summary>
        /// <remarks>
        /// Request to this action will not trigger any model binding or model validation,
        /// because this is a no-argument action
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(UploadLargeFile))]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadLargeFile()
        {
            var guidString = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Guid").Value;
            var id = Guid.Parse(guidString);
            if (id == Guid.Empty)
            {
                throw new ApplicationException("Bad user claims");
            }
            var photos = 
                await _context.UploadedPhotos.Where(p => p.StudentId == id).CountAsync();
            if (photos >= 2)
            {
                return BadRequest("Too many photos uploaded.");
            }
            var request = HttpContext.Request;
            // validation of Content-Type
            // 1. first, it must be a form-data request
            // 2. a boundary should be found in the Content-Type
            if (!request.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
            var section = await reader.ReadNextSectionAsync();

            // This sample try to get the first file from request and save it
            // Make changes according to your needs in actual use
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var contentDisposition);

                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    string untrustedFileName = Path.GetFileName(contentDisposition.FileName.Value);
                    var ext = Path.GetExtension(untrustedFileName).ToLowerInvariant();
                    if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                    {
                        return BadRequest("Bad file extension.");
                    }
                    var noextname = Path.ChangeExtension(untrustedFileName, null);
                    var addextname = Path.ChangeExtension($"{noextname}-{Guid.NewGuid()}", ext);
                    var saveToPath = Path.Combine(_imagePath, addextname);
                    
                    // Console.WriteLine($"file write to {saveToPath}");
                    using var targetStream = System.IO.File.Create(saveToPath);
                    await section.Body.CopyToAsync(targetStream);
                    var record = new UploadedPhoto
                    {
                        Description = "未填写",
                        UserFileName = untrustedFileName,
                        StorageName = addextname,
                        FileSize = targetStream.Length,
                        UploadTime = DateTime.UtcNow,
                        StudentId = id,
                    };
                    await _context.UploadedPhotos.AddAsync(record);
                    await _context.SaveChangesAsync();
                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            // If the code runs to this location, it means that no files have been saved
            return BadRequest("No files data in the request.");
        }
    }
}