namespace GraduationPhotosManagement.Models
{
    public class Student
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string StudentNumber { get; set; }
        // sha256($"{idcardnumber}{salt}")
        public string SaltedPasswordHash { get; set; }
        public ICollection<UploadedPhoto> UploadedPhotos { get; set; }
        public ICollection<Class> Classes { get; set; } 
        public List<StudentClass> StudentClasses { get; set; }
    }



    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public List<PostTag> PostTags { get; set; }
    }

    public class Tag
    {
        public string TagId { get; set; }

        public ICollection<Post> Posts { get; set; }
        public List<PostTag> PostTags { get; set; }
    }

    public class PostTag
    {
        public DateTime PublicationDate { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public string TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
