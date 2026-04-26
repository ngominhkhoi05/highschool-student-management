using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("parent_students")]
    public class ParentStudent
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Parent")]
        public int ParentId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public int IsPrimary { get; set; } = 0;
        // 1: liên hệ chính, 0: phụ

        // Navigation
        public Parent Parent { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}