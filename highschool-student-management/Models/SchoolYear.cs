using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("school_years")]
    public class SchoolYear
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Name { get; set; } = string.Empty;
        // VD: "2024-2025"

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int IsCurrent { get; set; } = 0;
        // 1: năm học hiện tại, 0: đã qua

        // Navigation
        public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    }
}