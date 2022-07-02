using System;
using System.Linq;
using System.Text;
using GraduationPhotosManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
namespace GraduationPhotosManagement.Data
{
    public class DbInitializer
    {
        private static string Byte2Hex(byte[] bytes)
        {
            var s = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                s.Append((bytes[i] & 0xFF).ToString("x2"));
            }
            return s.ToString();
        }
        public static void Initialize(PhotoDbContext context, string salt)
        {
            return;
            //context.Database.EnsureCreated();

            // Look for any students.
            if (context.Students.Any())
            {
                return;   // DB has been seeded
            }

            var classes = new Class[]
            {
                new Class { ClassName="30411801", Type = ClassType.Undergraduate }
            };
            foreach (var c in classes)
            {
                context.Classes.Add(c);
            }
            context.SaveChanges();
            var hashsalted = SHA256.HashData(Encoding.Default.GetBytes($"123456{salt}"));
            var students = new Student[]
            {
                new Student { Name = "张正",   StudentNumber = "1120181393",
                SaltedPasswordHash=Byte2Hex(hashsalted)},
            };
            foreach (var s in students)
            {
                context.Students.Add(s);
            }
            context.SaveChanges();

            var sc = new StudentClass[]
            {
                new StudentClass { StudentId = students[0].Id, ClassId = classes[0].Id },
            };
            foreach (var s in sc)
            {
                context.StudentClasses.Add(s);
            }
            context.SaveChanges();

            var photos = new Photo[]
            {
                new Photo {Description="1班毕业照（学校上传）", StoragePath="/photos/test.jpg", ClassId = classes[0].Id},
            };
            foreach (var s in photos)
            {
                context.GraduationPhotos.Add(s);
            }
            context.SaveChanges();
        }
    }
}
