using Microsoft.EntityFrameworkCore;

namespace GraduationPhotosManagement.Models
{
    public class PhotoDbContext : DbContext
    {
        public PhotoDbContext(DbContextOptions<PhotoDbContext> options)
            : base(options)
        {
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Photo> GraduationPhotos { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<UploadedPhoto> UploadedPhotos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 学生和班级多对多
            modelBuilder.Entity<Student>()
                .HasMany(p => p.Classes)
                .WithMany(p => p.Students)
                .UsingEntity<StudentClass>(
                    j => j
                        .HasOne(pt => pt.Class)
                        .WithMany(t => t.StudentClasses)
                        .HasForeignKey(pt => pt.ClassId),
                    j => j
                        .HasOne(pt => pt.Student)
                        .WithMany(p => p.StudentClasses)
                        .HasForeignKey(pt => pt.StudentId),
                    j =>
                    {
                        j.HasKey(t => new { t.StudentId, t.ClassId });
                    });
        }
    }
}