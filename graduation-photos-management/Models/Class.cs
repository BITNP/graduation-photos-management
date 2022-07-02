namespace GraduationPhotosManagement.Models
{
    public enum ClassType
    {
        Undergraduate,
        Postgraduate,
        PhD
    }
    public class Class
    {
        public int Id { get; set; }
        public string ClassName { get; set; }
        public ClassType Type { get; set; }
        public ICollection<Student> Students { get; set; }
        public ICollection<Photo> Photos { get; set; }
        public List<StudentClass> StudentClasses { get; set; }
    }
}
