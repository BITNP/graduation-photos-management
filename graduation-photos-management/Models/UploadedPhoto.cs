namespace GraduationPhotosManagement.Models
{
    public class UploadedPhoto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string UserFileName { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadTime { get; set; }
        public string StorageName { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
    }
}
