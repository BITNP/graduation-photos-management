using GraduationPhotosManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace GraduationPhotosManagement.Pages
{
    [Authorize]
    public class DownloadModel : PageModel
    {
        private readonly ILogger<DownloadModel> _logger;
        private readonly PhotoDbContext _context;
        private readonly IMemoryCache _cache;
        public DownloadPageData PageData;
        public class PhotoInfo
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string ClassName { get; set; }
            public string StoragePath { get; set; }
            public string ThumbnailPath { get; set; }

        }
        public class DownloadPageData
        {
            public Guid Id { get; set; }
            public string StudentName { get; set; }
            public List<PhotoInfo> Photos { get; set; } = new();
        }
        public DownloadModel(ILogger<DownloadModel> logger, PhotoDbContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var guidString = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Guid").Value;
            var id = Guid.Parse(guidString);
            if (id == Guid.Empty)
            {
                throw new ApplicationException("Bad user claims");
            }
            // 缓存命中，直接返回
            if (_cache.TryGetValue(id, out DownloadPageData data))
            {
                PageData = data;
                return Page();
            }
            // 没有缓存，需要查询数据
            Console.WriteLine(HttpContext.User.Identity.Name);
            var q =
                from c in _context.Students
                        .Include(a => a.Classes)
                            .ThenInclude(a => a.Photos)
                where c.Id == id
                select c;
            var s = await q.SingleAsync();
            // 页面数据
            PageData = new()
            {
                Id = id,
                StudentName = s.Name
            };
            foreach (var c in s.Classes)
            {
                foreach (var p in c.Photos)
                {
                    var photo = new PhotoInfo()
                    {
                        Id = p.Id,
                        Description = p.Description,
                        StoragePath = p.StoragePath,
                        ThumbnailPath = p.StoragePath + "-small.jpg",
                        ClassName = c.ClassName
                    };
                    PageData.Photos.Add(photo);
                }
            }

            // 缓存10分钟
            _cache.Set(id, PageData, TimeSpan.FromMinutes(10));
            return Page();

        }
    }
}
