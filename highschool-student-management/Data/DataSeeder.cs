using Bogus;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Models;

namespace QuanLyHocSinh.Data
{
    public static class DataSeeder
    {
        private const string DefaultPassword = "123456";

        public static async Task SeedAsync(AppDbContext context)
        {
            Console.WriteLine("[Seeder] Bat dau kiem tra du lieu...");

            if (context.Users.Any())
            {
                Console.WriteLine("[Seeder] Du lieu da ton tai, bo qua.");
                return;
            }

            Console.WriteLine("[Seeder] Bat dau seed du lieu...");

            try
            {
                var fakerVi = new Faker("vi");

                // =====================================================
                // 1. Năm học hiện tại
            // =====================================================
            var schoolYear = new SchoolYear
            {
                Name = "2025-2026",
                StartDate = new DateOnly(2025, 9, 1),
                EndDate = new DateOnly(2026, 5, 31),
                IsCurrent = 1
            };
            context.SchoolYears.Add(schoolYear);
            await context.SaveChangesAsync();

            // =====================================================
            // 2. Hai học kỳ
            // =====================================================
            var semester1 = new Semester
            {
                SchoolYearId = schoolYear.Id,
                Name = "Học kỳ 1",
                SemesterNumber = 1,
                StartDate = new DateOnly(2025, 9, 1),
                EndDate = new DateOnly(2026, 1, 15)
            };
            var semester2 = new Semester
            {
                SchoolYearId = schoolYear.Id,
                Name = "Học kỳ 2",
                SemesterNumber = 2,
                StartDate = new DateOnly(2026, 1, 16),
                EndDate = new DateOnly(2026, 5, 31)
            };
            context.Semesters.AddRange(semester1, semester2);
            await context.SaveChangesAsync();

            // =====================================================
            // 3. Lấy Role IDs
            // =====================================================
            int roleAdminId   = 1;
            int roleTeacherId = 2;
            int roleStudentId = 3;
            int roleParentId  = 4;

            // =====================================================
            // 4. Tài khoản Admin
            // =====================================================
            context.Users.Add(new User
            {
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
                Email = "admin@school.edu.vn",
                RoleId = roleAdminId,
                RelatedType = "admin",
                IsActive = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
            await context.SaveChangesAsync();

            // =====================================================
            // 5. Giáo viên (10) + User tương ứng
            // =====================================================
            var teachers = new List<Teacher>();
            var teacherUsers = new List<User>();

            string[] teacherSpecializations = {
                "Toán học", "Ngữ văn", "Tiếng Anh", "Vật lý", "Hóa học",
                "Sinh học", "Lịch sử", "Địa lý", "Giáo dục công dân", "Thể dục"
            };

            for (int i = 0; i < 10; i++)
            {
                int gender = fakerVi.Random.Bool() ? 1 : 2;
                var teacher = new Teacher
                {
                    FullName = fakerVi.Name.FullName(gender == 1 ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female),
                    Gender = gender,
                    DateOfBirth = fakerVi.Date.BetweenDateOnly(new DateOnly(1970, 1, 1), new DateOnly(1995, 12, 31)),
                    Phone = fakerVi.Phone.PhoneNumber("09########"),
                    Email = fakerVi.Internet.Email(),
                    Address = fakerVi.Address.FullAddress(),
                    Specialization = teacherSpecializations[i],
                    JoinDate = fakerVi.Date.BetweenDateOnly(new DateOnly(2010, 1, 1), new DateOnly(2022, 12, 31)),
                    Status = 1
                };
                teachers.Add(teacher);
            }
            context.Teachers.AddRange(teachers);
            await context.SaveChangesAsync();

            for (int i = 0; i < teachers.Count; i++)
            {
                teacherUsers.Add(new User
                {
                    Username = $"teacher{i + 1}",
                    Password = BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
                    Email = teachers[i].Email,
                    RoleId = roleTeacherId,
                    RelatedId = teachers[i].Id,
                    RelatedType = "teacher",
                    IsActive = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            context.Users.AddRange(teacherUsers);
            await context.SaveChangesAsync();

            // =====================================================
            // 6. Học sinh (50) + User tương ứng
            // =====================================================
            var students = new List<Student>();

            for (int i = 0; i < 50; i++)
            {
                int gender = fakerVi.Random.Bool() ? 1 : 2;
                students.Add(new Student
                {
                    StudentCode = $"HS2025{(i + 1):D3}",
                    FullName = fakerVi.Name.FullName(gender == 1 ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female),
                    Gender = gender,
                    DateOfBirth = fakerVi.Date.BetweenDateOnly(new DateOnly(2008, 1, 1), new DateOnly(2012, 12, 31)),
                    PlaceOfBirth = fakerVi.Address.City(),
                    Address = fakerVi.Address.FullAddress(),
                    Phone = fakerVi.Phone.PhoneNumber("09########"),
                    Ethnicity = "Kinh",
                    Religion = null,
                    EnrollmentDate = new DateOnly(2025, 9, 1),
                    Status = 1
                });
            }
            context.Students.AddRange(students);
            await context.SaveChangesAsync();

            var studentUsers = new List<User>();
            for (int i = 0; i < students.Count; i++)
            {
                studentUsers.Add(new User
                {
                    Username = students[i].StudentCode.ToLower(),
                    Password = BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
                    Email = fakerVi.Internet.Email(students[i].FullName.ToLower().Replace(" ", "")),
                    RoleId = roleStudentId,
                    RelatedId = students[i].Id,
                    RelatedType = "student",
                    IsActive = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            context.Users.AddRange(studentUsers);
            await context.SaveChangesAsync();

            // =====================================================
            // 7. Phụ huynh (30) + User tương ứng + ParentStudent
            // =====================================================
            var parents = new List<Parent>();
            var parentUsers = new List<User>();
            var parentStudents = new List<ParentStudent>();

            for (int i = 0; i < 30; i++)
            {
                int gender = fakerVi.Random.Bool() ? 1 : 2;
                int relationship = fakerVi.Random.Bool() ? 1 : 2;
                parents.Add(new Parent
                {
                    FullName = fakerVi.Name.FullName(gender == 1 ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female),
                    Gender = gender,
                    DateOfBirth = fakerVi.Date.BetweenDateOnly(new DateOnly(1970, 1, 1), new DateOnly(1985, 12, 31)),
                    Phone = fakerVi.Phone.PhoneNumber("09########"),
                    Email = fakerVi.Internet.Email(),
                    Occupation = fakerVi.Name.JobTitle(),
                    Address = fakerVi.Address.FullAddress(),
                    Relationship = relationship
                });
            }
            context.Parents.AddRange(parents);
            await context.SaveChangesAsync();

            for (int i = 0; i < parents.Count; i++)
            {
                parentUsers.Add(new User
                {
                    Username = $"parent{i + 1}",
                    Password = BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
                    Email = parents[i].Email,
                    RoleId = roleParentId,
                    RelatedId = parents[i].Id,
                    RelatedType = "parent",
                    IsActive = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            context.Users.AddRange(parentUsers);
            await context.SaveChangesAsync();

            // Mỗi phụ huynh được liên kết với 1-3 học sinh (ngẫu nhiên)
            var rng = new Random(42);
            foreach (var parent in parents)
            {
                int studentCount = rng.Next(1, 4);
                var linkedStudents = students.OrderBy(_ => rng.Next()).Take(studentCount);
                foreach (var student in linkedStudents)
                {
                    bool isPrimary = parentStudents.Count(ps => ps.ParentId == parent.Id) == 0;
                    parentStudents.Add(new ParentStudent
                    {
                        ParentId = parent.Id,
                        StudentId = student.Id,
                        IsPrimary = isPrimary ? 1 : 0
                    });
                }
            }
            context.ParentStudents.AddRange(parentStudents);
            await context.SaveChangesAsync();

            // =====================================================
            // 8. Lớp học (10) — gán ngẫu nhiên giáo viên chủ nhiệm
            // =====================================================
            var classes = new List<Class>();
            string[] gradeNames = { "10", "11", "12" };
            int[] gradeDistribution = { 4, 3, 3 }; // 4 lớp khối 10, 3 khối 11, 3 khối 12

            int classIndex = 0;
            for (int g = 0; g < gradeNames.Length; g++)
            {
                for (int n = 1; n <= gradeDistribution[g]; n++)
                {
                    var homeroom = teachers[rng.Next(teachers.Count)];
                    classes.Add(new Class
                    {
                        Name = $"{gradeNames[g]}A{n}",
                        GradeLevel = int.Parse(gradeNames[g]),
                        SchoolYearId = schoolYear.Id,
                        HomeroomTeacherId = homeroom.Id,
                        MaxStudents = 45,
                        Room = $"P{30 + classIndex}"
                    });
                    classIndex++;
                }
            }
            context.Classes.AddRange(classes);
            await context.SaveChangesAsync();

            // =====================================================
            // 9. Phân bổ học sinh vào lớp (StudentClass)
            // =====================================================
            var studentClasses = new List<StudentClass>();
            foreach (var student in students)
            {
                var targetClass = classes[rng.Next(classes.Count)];
                studentClasses.Add(new StudentClass
                {
                    StudentId = student.Id,
                    ClassId = targetClass.Id,
                    SchoolYearId = schoolYear.Id,
                    IsCurrent = 1,
                    JoinedAt = new DateOnly(2025, 9, 1)
                });
            }
            context.StudentClasses.AddRange(studentClasses);
            await context.SaveChangesAsync();

            // =====================================================
            // 10. Gán giáo viên dạy môn cho lớp (TeacherClass)
            // =====================================================
            var subjects = context.Subjects.ToList();
            var teacherClasses = new List<TeacherClass>();

            foreach (var cls in classes)
            {
                foreach (var subject in subjects)
                {
                    var teacherForSubject = teachers[rng.Next(teachers.Count)];
                    teacherClasses.Add(new TeacherClass
                    {
                        TeacherId = teacherForSubject.Id,
                        ClassId = cls.Id,
                        SubjectId = subject.Id,
                        SemesterId = semester1.Id,
                        PeriodsPerWeek = subject.PeriodsPerWeek
                    });
                    teacherClasses.Add(new TeacherClass
                    {
                        TeacherId = teacherForSubject.Id,
                        ClassId = cls.Id,
                        SubjectId = subject.Id,
                        SemesterId = semester2.Id,
                        PeriodsPerWeek = subject.PeriodsPerWeek
                    });
                }
            }
            context.TeacherClasses.AddRange(teacherClasses);
            await context.SaveChangesAsync();

            // =====================================================
            // 11. Điểm số mẫu (mỗi học sinh 1 số điểm cho mỗi môn/học kỳ)
            // =====================================================
            var scoreTypes = context.ScoreTypes.ToList();
            var scores = new List<Score>();

            foreach (var student in students)
            {
                foreach (var subject in subjects)
                {
                    foreach (var semester in new[] { semester1, semester2 })
                    {
                        foreach (var scoreType in scoreTypes)
                        {
                            decimal? value = null;
                            if (fakerVi.Random.Bool(0.85f))
                            {
                                value = Math.Round(fakerVi.Random.Decimal(3.0m, 10.0m), 2);
                            }
                            scores.Add(new Score
                            {
                                StudentId = student.Id,
                                SubjectId = subject.Id,
                                SemesterId = semester.Id,
                                ScoreTypeId = scoreType.Id,
                                ScoreValue = value,
                                ExamDate = fakerVi.Date.BetweenDateOnly(semester.StartDate!.Value, semester.EndDate!.Value),
                                Note = null,
                                TeacherId = teachers[rng.Next(teachers.Count)].Id,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                }
            }
            context.Scores.AddRange(scores);
            await context.SaveChangesAsync();

            // =====================================================
            // 12. Kết quả học kỳ (SemesterResult)
            // =====================================================
            var semesterResults = new List<SemesterResult>();

            foreach (var student in students)
            {
                foreach (var semester in new[] { semester1, semester2 })
                {
                    foreach (var subject in subjects)
                    {
                        var studentScores = scores
                            .Where(s => s.StudentId == student.Id
                                     && s.SemesterId == semester.Id
                                     && s.SubjectId == subject.Id
                                     && s.ScoreValue.HasValue)
                            .ToList();

                        decimal? avg = null;
                        if (studentScores.Any())
                        {
                            avg = Math.Round(studentScores.Average(s => s.ScoreValue!.Value), 2);
                        }

                        semesterResults.Add(new SemesterResult
                        {
                            StudentId = student.Id,
                            SemesterId = semester.Id,
                            SubjectId = subject.Id,
                            AverageScore = avg,
                            AcademicRank = null,
                            ConductRank = null,
                            IsPromoted = null,
                            Note = null
                        });
                    }

                    semesterResults.Add(new SemesterResult
                    {
                        StudentId = student.Id,
                        SemesterId = semester.Id,
                        SubjectId = null,
                        AverageScore = null,
                        AcademicRank = null,
                        ConductRank = 1,
                        IsPromoted = null,
                        Note = "Tổng kết học kỳ"
                    });
                }
            }
            context.SemesterResults.AddRange(semesterResults);
            await context.SaveChangesAsync();

            // =====================================================
            // 13. Điểm danh mẫu
            // =====================================================
            var attendances = new List<Attendance>();
            var attendanceStatuses = new[] { 1, 1, 1, 1, 2, 3, 4 };

            foreach (var student in students)
            {
                var scEntry = studentClasses.First(sc => sc.StudentId == student.Id);
                var cls = classes.First(c => c.Id == scEntry.ClassId);

                int totalDays = 50;
                for (int d = 0; d < totalDays; d++)
                {
                    var date = semester1.StartDate!.Value.AddDays(d * 7);
                    if (date > semester1.EndDate!.Value) break;

                    attendances.Add(new Attendance
                    {
                        StudentId = student.Id,
                        ClassId = cls.Id,
                        SubjectId = null,
                        Date = date,
                        Period = null,
                        Status = attendanceStatuses[rng.Next(attendanceStatuses.Length)],
                        Note = null,
                        RecordedBy = cls.HomeroomTeacherId,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            context.Attendances.AddRange(attendances);
            await context.SaveChangesAsync();

            // =====================================================
            // 14. Hạnh kiểm (ConductScore)
            // =====================================================
            var conductScores = new List<ConductScore>();

            foreach (var student in students)
            {
                foreach (var semester in new[] { semester1, semester2 })
                {
                    conductScores.Add(new ConductScore
                    {
                        StudentId = student.Id,
                        SemesterId = semester.Id,
                        ConductRank = 1,
                        Score = 100,
                        Note = null,
                        EvaluatedBy = teachers[rng.Next(teachers.Count)].Id,
                        EvaluatedAt = DateTime.Now
                    });
                }
            }
            context.ConductScores.AddRange(conductScores);
            await context.SaveChangesAsync();

            // =====================================================
            // 15. Vi phạm (Violation)
            // =====================================================
            var violationTypes = context.ViolationTypes.ToList();
            var violations = new List<Violation>();

            int violationCount = 0;
            foreach (var student in students)
            {
                int numViolations = rng.Next(0, 4);
                for (int v = 0; v < numViolations; v++)
                {
                    var vType = violationTypes[rng.Next(violationTypes.Count)];
                    var semester = rng.Next(2) == 0 ? semester1 : semester2;
                    violations.Add(new Violation
                    {
                        StudentId = student.Id,
                        ViolationTypeId = vType.Id,
                        SemesterId = semester.Id,
                        ViolationDate = fakerVi.Date.BetweenDateOnly(semester.StartDate!.Value, semester.EndDate!.Value),
                        Description = null,
                        ActionTaken = rng.Next(1, 5),
                        RecordedBy = teachers[rng.Next(teachers.Count)].Id,
                        CreatedAt = DateTime.Now
                    });
                    violationCount++;
                }
            }
            context.Violations.AddRange(violations);
            await context.SaveChangesAsync();

            // =====================================================
            // 16. Khen thưởng (Commendation)
            // =====================================================
            var commendations = new List<Commendation>();
            foreach (var student in students)
            {
                if (fakerVi.Random.Bool(0.3f))
                {
                    var semester = rng.Next(2) == 0 ? semester1 : semester2;
                    commendations.Add(new Commendation
                    {
                        StudentId = student.Id,
                        SemesterId = semester.Id,
                        Type = 1,
                        Title = "Học sinh Giỏi",
                        Level = rng.Next(1, 4),
                        AwardedDate = fakerVi.Date.BetweenDateOnly(semester.StartDate!.Value, semester.EndDate!.Value),
                        Description = "Đạt danh hiệu học sinh giỏi cấp trường",
                        AddPoints = 5
                    });
                }
            }
            context.Commendations.AddRange(commendations);
            await context.SaveChangesAsync();

            // =====================================================
            // 17. Cài đặt mặc định (Setting)
            // =====================================================
            context.Settings.AddRange(
                new Setting { Key = "school_name",    Value = "Trường THPT Demo",       Description = "Tên trường" },
                new Setting { Key = "school_address", Value = "123 Đường ABC, TP.HCM",   Description = "Địa chỉ trường" },
                new Setting { Key = "school_phone",   Value = "02812345678",             Description = "Số điện thoại trường" },
                new Setting { Key = "pass_score",    Value = "5.0",                    Description = "Điểm đạt (thang 10)" },
                new Setting { Key = "semester_rule", Value = "auto",                   Description = "Quy tắc tính điểm trung bình" }
            );
                await context.SaveChangesAsync();

                Console.WriteLine("[Seeder] Seed du lieu thanh cong!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seeder] LOI: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[Seeder] Stack: {ex.StackTrace}");
            }
        }
    }
}
