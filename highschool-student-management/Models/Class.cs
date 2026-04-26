using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("classes")]
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Name { get; set; } = string.Empty;
        // VD: 10A1, 11B2

        public int? GradeLevel { get; set; }
        // Khối: 10, 11, 12

        [ForeignKey("SchoolYear")]
        public int SchoolYearId { get; set; }

        [ForeignKey("HomeroomTeacher")]
        public int? HomeroomTeacherId { get; set; }
        // Giáo viên chủ nhiệm

        public int? MaxStudents { get; set; }

        [MaxLength(20)]
        public string? Room { get; set; }

        // Navigation
        public SchoolYear SchoolYear { get; set; } = null!;
        public Teacher? HomeroomTeacher { get; set; }
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public ICollection<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}