using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("student_classes")]
    public class StudentClass
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [ForeignKey("SchoolYear")]
        public int SchoolYearId { get; set; }

        public int IsCurrent { get; set; } = 1;
        // 1: năm học hiện tại

        public DateOnly? JoinedAt { get; set; }

        // Navigation
        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public SchoolYear SchoolYear { get; set; } = null!;
    }
}