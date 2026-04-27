using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Models;
using QuanLyHocSinh.Models.Enums;

namespace QuanLyHocSinh.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ===== DbSets =====
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SchoolYear> SchoolYears { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ScoreType> ScoreTypes { get; set; }
        public DbSet<ViolationType> ViolationTypes { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ParentStudent> ParentStudents { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<TeacherClass> TeacherClasses { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<SemesterResult> SemesterResults { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<ConductScore> ConductScores { get; set; }
        public DbSet<Violation> Violations { get; set; }
        public DbSet<Commendation> Commendations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== UNIQUE CONSTRAINTS =====

            // Username phải duy nhất
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();

            // Mã học sinh duy nhất
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentCode).IsUnique();

            // Mã môn học duy nhất
            modelBuilder.Entity<Subject>()
                .HasIndex(s => s.Code).IsUnique();

            // Setting key duy nhất
            modelBuilder.Entity<Setting>()
                .HasIndex(s => s.Key).IsUnique();

            // Một học sinh chỉ vào 1 lớp 1 lần trong 1 năm học
            modelBuilder.Entity<StudentClass>()
                .HasIndex(sc => new { sc.StudentId, sc.ClassId, sc.SchoolYearId }).IsUnique();

            // Một học sinh chỉ có 1 hạnh kiểm/học kỳ
            modelBuilder.Entity<ConductScore>()
                .HasIndex(cs => new { cs.StudentId, cs.SemesterId }).IsUnique();

            // Kết quả học kỳ không bị trùng
            modelBuilder.Entity<SemesterResult>()
                .HasIndex(sr => new { sr.StudentId, sr.SemesterId, sr.SubjectId }).IsUnique();

            // ===== RELATIONSHIPS =====

            // Attendance - RecordedByTeacher
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.RecordedByTeacher)
                .WithMany(t => t.Attendances)
                .HasForeignKey(a => a.RecordedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Violation - RecordedByTeacher
            modelBuilder.Entity<Violation>()
                .HasOne(v => v.RecordedByTeacher)
                .WithMany(t => t.RecordedViolations)
                .HasForeignKey(v => v.RecordedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ConductScore - EvaluatedByTeacher
            modelBuilder.Entity<ConductScore>()
                .HasOne(cs => cs.EvaluatedByTeacher)
                .WithMany(t => t.ConductScores)
                .HasForeignKey(cs => cs.EvaluatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Score - Teacher
            modelBuilder.Entity<Score>()
                .HasOne(s => s.Teacher)
                .WithMany(t => t.Scores)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // Class - HomeroomTeacher (tránh cascade cycle)
            modelBuilder.Entity<Class>()
                .HasOne(c => c.HomeroomTeacher)
                .WithMany(t => t.HomeroomClasses)
                .HasForeignKey(c => c.HomeroomTeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // TeacherClass - các FK tránh cascade cycle trong SQL Server
            modelBuilder.Entity<TeacherClass>()
                .HasOne(tc => tc.Teacher)
                .WithMany(t => t.TeacherClasses)
                .HasForeignKey(tc => tc.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherClass>()
                .HasOne(tc => tc.Class)
                .WithMany(c => c.TeacherClasses)
                .HasForeignKey(tc => tc.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // Score - cascade restrict để tránh multiple cascade paths
            modelBuilder.Entity<Score>()
                .HasOne(s => s.Student)
                .WithMany(st => st.Scores)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Semester)
                .WithMany(sem => sem.Scores)
                .HasForeignKey(s => s.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // SemesterResult cascades
            modelBuilder.Entity<SemesterResult>()
                .HasOne(sr => sr.Student)
                .WithMany(s => s.SemesterResults)
                .HasForeignKey(sr => sr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SemesterResult>()
                .HasOne(sr => sr.Semester)
                .WithMany(s => s.SemesterResults)
                .HasForeignKey(sr => sr.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attendance cascades
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ConductScore cascades
            modelBuilder.Entity<ConductScore>()
                .HasOne(cs => cs.Student)
                .WithMany(s => s.ConductScores)
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConductScore>()
                .HasOne(cs => cs.Semester)
                .WithMany()
                .HasForeignKey(cs => cs.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Violation cascades
            modelBuilder.Entity<Violation>()
                .HasOne(v => v.Student)
                .WithMany(s => s.Violations)
                .HasForeignKey(v => v.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Violation>()
                .HasOne(v => v.Semester)
                .WithMany(s => s.Violations)
                .HasForeignKey(v => v.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Commendation cascades
            modelBuilder.Entity<Commendation>()
                .HasOne(c => c.Student)
                .WithMany(s => s.Commendations)
                .HasForeignKey(c => c.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Commendation>()
                .HasOne(c => c.Semester)
                .WithMany(s => s.Commendations)
                .HasForeignKey(c => c.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // NotificationRecipient - cascade delete khi xóa Notification
            modelBuilder.Entity<NotificationRecipient>()
                .HasOne(nr => nr.Notification)
                .WithMany(n => n.Recipients)
                .HasForeignKey(nr => nr.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotificationRecipient>()
                .HasOne(nr => nr.User)
                .WithMany(u => u.NotificationRecipients)
                .HasForeignKey(nr => nr.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            // Fix student_classes - 3 FK cascade gây conflict
            modelBuilder.Entity<StudentClass>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentClasses)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentClass>()
                .HasOne(sc => sc.Class)
                .WithMany(c => c.StudentClasses)
                .HasForeignKey(sc => sc.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentClass>()
                .HasOne(sc => sc.SchoolYear)
                .WithMany(sy => sy.StudentClasses)
                .HasForeignKey(sc => sc.SchoolYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Teacher/Student/Parent nullable FK
            modelBuilder.Entity<User>()
                .HasOne(u => u.Teacher)
                .WithOne()
                .HasForeignKey<User>(u => u.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne()
                .HasForeignKey<User>(u => u.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Parent)
                .WithOne()
                .HasForeignKey<User>(u => u.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== STATIC DATA (HasData) =====

            // Role — 4 vai trò hệ thống
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Quản trị viên hệ thống" },
                new Role { Id = 2, Name = "Teacher", Description = "Giáo viên" },
                new Role { Id = 3, Name = "Student", Description = "Học sinh" },
                new Role { Id = 4, Name = "Parent", Description = "Phụ huynh" }
            );

            // ScoreType — các loại điểm với hệ số tương ứng
            modelBuilder.Entity<ScoreType>().HasData(
                new ScoreType { Id = 1, Name = "Điểm miệng",     Weight = 1.0m, MinCount = 5 },
                new ScoreType { Id = 2, Name = "Điểm 15 phút",  Weight = 1.0m, MinCount = 3 },
                new ScoreType { Id = 3, Name = "Điểm 1 tiết",    Weight = 2.0m, MinCount = 2 },
                new ScoreType { Id = 4, Name = "Điểm giữa kỳ",  Weight = 2.0m, MinCount = 1 },
                new ScoreType { Id = 5, Name = "Điểm cuối kỳ",   Weight = 3.0m, MinCount = 1 }
            );

            // ViolationType — các lỗi vi phạm phổ biến, map ViolationSeverity enum
            modelBuilder.Entity<ViolationType>().HasData(
                new ViolationType
                {
                    Id = 1,
                    Name = "Đi học muộn",
                    Severity = (int)ViolationSeverity.Nhe,
                    DeductPoints = 1,
                    Description = "Không có mặt đúng giờ khi tiết học bắt đầu"
                },
                new ViolationType
                {
                    Id = 2,
                    Name = "Không thuộc bài",
                    Severity = (int)ViolationSeverity.Nhe,
                    DeductPoints = 1,
                    Description = "Không hoàn thành bài tập hoặc không chuẩn bị bài ở nhà"
                },
                new ViolationType
                {
                    Id = 3,
                    Name = "Nói chuyện khi đang giảng bài",
                    Severity = (int)ViolationSeverity.TrungBinh,
                    DeductPoints = 2,
                    Description = "Gây ồn ào, ảnh hưởng đến việc học của các bạn khác"
                },
                new ViolationType
                {
                    Id = 4,
                    Name = "Vắng học không phép",
                    Severity = (int)ViolationSeverity.Nang,
                    DeductPoints = 5,
                    Description = "Nghỉ học mà không có lý do chính đáng và không xin phép"
                },
                new ViolationType
                {
                    Id = 5,
                    Name = "Mang đồ cấm vào trường",
                    Severity = (int)ViolationSeverity.Nang,
                    DeductPoints = 5,
                    Description = "Mang vũ khí, chất kích thích hoặc các vật dụng nguy hiểm"
                }
            );

            // Subject — một số môn học mẫu
            modelBuilder.Entity<Subject>().HasData(
                new Subject { Id = 1,  Name = "Toán học",          Code = "MATH",  GradeLevel = null, PeriodsPerWeek = 5 },
                new Subject { Id = 2,  Name = "Ngữ văn",          Code = "LIT",   GradeLevel = null, PeriodsPerWeek = 5 },
                new Subject { Id = 3,  Name = "Tiếng Anh",         Code = "ENG",   GradeLevel = null, PeriodsPerWeek = 4 },
                new Subject { Id = 4,  Name = "Vật lý",            Code = "PHY",   GradeLevel = null, PeriodsPerWeek = 3 },
                new Subject { Id = 5,  Name = "Hóa học",          Code = "CHEM",  GradeLevel = null, PeriodsPerWeek = 3 },
                new Subject { Id = 6,  Name = "Sinh học",          Code = "BIO",   GradeLevel = null, PeriodsPerWeek = 2 },
                new Subject { Id = 7,  Name = "Lịch sử",           Code = "HIS",   GradeLevel = null, PeriodsPerWeek = 2 },
                new Subject { Id = 8,  Name = "Địa lý",            Code = "GEO",   GradeLevel = null, PeriodsPerWeek = 2 },
                new Subject { Id = 9,  Name = "Giáo dục công dân", Code = "CIV",   GradeLevel = null, PeriodsPerWeek = 1 },
                new Subject { Id = 10, Name = "Công nghệ",          Code = "TECH",  GradeLevel = null, PeriodsPerWeek = 2 },
                new Subject { Id = 11, Name = "Nghệ thuật",         Code = "ART",   GradeLevel = null, PeriodsPerWeek = 1 },
                new Subject { Id = 12, Name = "Thể dục",            Code = "PE",    GradeLevel = null, PeriodsPerWeek = 2 }
            );
        }
    }
}