using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Models;

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
        }
    }
}