namespace GraduationPhotosManagement.Models
{
    public class StudentClass
    {
        public int ClassId { get; set; }
        public Class Class { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
    }
}
