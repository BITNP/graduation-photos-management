namespace GraduationPhotosManagement.Models
{
    public class Photo
    {
        public int Id { get; set; }
        // public string Name { get; set; }
        public string Description { get; set; }
        public string StoragePath { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; }
    }
}
