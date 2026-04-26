# Models Documentation

Danh sach 23 entity model trong he thong quan ly hoc sinh.

---

## 1. Role

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Name | string(50) | Ten vai tro (Admin, Teacher, Student, Parent) |
| Description | string(255)? | Mo ta vai tro |

---

## 2. User

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Username | string(50) | Ten dang nhap (unique) |
| Password | string(255) | Mat khau da hash (BCrypt) |
| Email | string(100)? | Dia chi email |
| RoleId | int | FK -> Role |
| RelatedId | int? | ID tuong ung trong bang teachers/students/parents |
| RelatedType | string(20)? | "teacher", "student", "parent", "admin" |
| IsActive | int | 1: hoat dong, 0: bi khoa |
| LastLogin | DateTime? | Lan dang nhap cuoi |
| CreatedAt | DateTime | Ngay tao |
| UpdatedAt | DateTime | Ngay cap nhat |

**Quan he:** Role (1-N)

---

## 3. SchoolYear

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Name | string(20) | Ten nam hoc (VD: "2025-2026") |
| StartDate | DateOnly? | Ngay bat dau |
| EndDate | DateOnly? | Ngay ket thuc |
| IsCurrent | int | 1: nam hoc hien tai, 0: da qua |

**Quan he:** Semester (1-N), Class (1-N), StudentClass (1-N)

---

## 4. Semester

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| SchoolYearId | int | FK -> SchoolYear |
| Name | string(20)? | Ten hoc ky ("Hoc ky 1", "Hoc ky 2") |
| SemesterNumber | int? | 1: HK1, 2: HK2 |
| StartDate | DateOnly? | Ngay bat dau |
| EndDate | DateOnly? | Ngay ket thuc |

**Quan he:** SchoolYear (N-1), TeacherClass (1-N), Score (1-N), SemesterResult (1-N), ConductScore (1-N), Violation (1-N), Commendation (1-N)

---

## 5. Subject

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Name | string(100) | Ten mon hoc |
| Code | string(20) | Ma mon (unique, VD: "MATH", "LIT") |
| GradeLevel | int? | Khoi lop (10, 11, 12). NULL = tat ca khoi |
| PeriodsPerWeek | int? | So tiet/tuan |
| IsActive | int | 1: dang day, 0: ngung |

**Quan he:** TeacherClass (1-N), Score (1-N), SemesterResult (1-N), Attendance (1-N)

---

## 6. ScoreType

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Name | string(50) | Ten loai diem ("Diem mieng", "15 phut", ...) |
| Weight | decimal(3,1) | He so (1.0, 2.0, 3.0) |
| MinCount | int | So lan kiem tra toi thieu/hoc ky |

**Quan he:** Score (1-N)

---

## 7. ViolationType

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Name | string(100) | Ten loai vi pham |
| Severity | int? | 1: Nhe, 2: Trung binh, 3: Nang |
| DeductPoints | int | Diem tru hanh kiem |
| Description | string? | Mo ta chi tiet |

**Quan he:** Violation (1-N)

---

## 8. Teacher

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| FullName | string(100) | Ho va ten |
| Gender | int? | 1: Nam, 2: Nu |
| DateOfBirth | DateOnly? | Ngay sinh |
| Phone | string(15)? | So dien thoai |
| Email | string(100)? | Email |
| Address | string? | Dia chi |
| Specialization | string(100)? | Chuyen mon (Toan hoc, Ngu Van...) |
| JoinDate | DateOnly? | Ngay gia nhap |
| Status | int | 1: dang day, 2: nghi phep, 3: nghi viec |
| Avatar | string(255)? | Duong dan anh dai dien |

**Quan he:** HomeroomClasses (Class 1-N), TeacherClass (1-N), Scores (1-N), Attendances (1-N), ConductScores (1-N), RecordedViolations (1-N)

---

## 9. Student

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| StudentCode | string(20) | Ma hoc sinh (unique, VD: "HS2025001") |
| FullName | string(100) | Ho va ten |
| Gender | int? | 1: Nam, 2: Nu |
| DateOfBirth | DateOnly? | Ngay sinh |
| PlaceOfBirth | string(100)? | Noi sinh |
| Address | string? | Dia chi |
| Phone | string(15)? | So dien thoai |
| Ethnicity | string(50)? | Dan toc |
| Religion | string(50)? | Ton giao |
| EnrollmentDate | DateOnly? | Ngay nhap hoc |
| Status | int | 1: dang hoc, 2: nghi hoc, 3: tot nghiep, 4: chuyen truong |
| Avatar | string(255)? | Duong dan anh dai dien |

**Quan he:** StudentClasses (1-N), ParentStudents (1-N), Scores (1-N), SemesterResults (1-N), Attendances (1-N), ConductScores (1-N), Violations (1-N), Commendations (1-N)

---

## 10. Parent

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| FullName | string(100) | Ho va ten |
| Gender | int? | 1: Nam, 2: Nu |
| DateOfBirth | DateOnly? | Ngay sinh |
| Phone | string(15)? | So dien thoai |
| Email | string(100)? | Email |
| Occupation | string(100)? | Nghe nghiep |
| Address | string? | Dia chi |
| Relationship | int? | 1: Cha, 2: Me, 3: Nguoi giam ho |

**Quan he:** ParentStudents (1-N)

---

## 11. Class

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Name | string(20) | Ten lop (VD: "10A1", "11B2") |
| GradeLevel | int? | Khoi (10, 11, 12) |
| SchoolYearId | int | FK -> SchoolYear |
| HomeroomTeacherId | int? | FK -> Teacher (giao vien chu nhiem) |
| MaxStudents | int? | So hoc sinh toi da |
| Room | string(20)? | Phong hoc (VD: "P301") |

**Quan he:** SchoolYear (N-1), HomeroomTeacher (N-1 Teacher), StudentClasses (1-N), TeacherClasses (1-N), Attendances (1-N)

---

## 12. ParentStudent (Join Table)

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| ParentId | int | FK -> Parent |
| StudentId | int | FK -> Student |
| IsPrimary | int | 1: lien he chinh, 0: phu |

**Quan he:** Parent (N-1), Student (N-1)

---

## 13. StudentClass (Join Table)

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| StudentId | int | FK -> Student |
| ClassId | int | FK -> Class |
| SchoolYearId | int | FK -> SchoolYear |
| IsCurrent | int | 1: nam hoc hien tai |
| JoinedAt | DateOnly? | Ngay gia nhap lop |

**Constraint unique:** (StudentId, ClassId, SchoolYearId)

**Quan he:** Student (N-1), Class (N-1), SchoolYear (N-1)

---

## 14. TeacherClass (Join Table)

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| TeacherId | int | FK -> Teacher |
| ClassId | int | FK -> Class |
| SubjectId | int | FK -> Subject |
| SemesterId | int | FK -> Semester |
| PeriodsPerWeek | int? | So tiet/tuan |

**Quan he:** Teacher (N-1), Class (N-1), Subject (N-1), Semester (N-1)

---

## 15. Score

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| StudentId | int | FK -> Student |
| SubjectId | int | FK -> Subject |
| SemesterId | int | FK -> Semester |
| ScoreTypeId | int | FK -> ScoreType |
| ScoreValue | decimal(4,2)? | Diem so (thang 10) |
| ExamDate | DateOnly? | Ngay thi/khao sat |
| Note | string(255)? | Ghi chu |
| TeacherId | int? | FK -> Teacher (giao vien nhap diem) |
| CreatedAt | DateTime | Ngay tao |

**Quan he:** Student (N-1), Subject (N-1), Semester (N-1), ScoreType (N-1), Teacher (N-1?)

---

## 16. SemesterResult

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| StudentId | int | FK -> Student |
| SemesterId | int | FK -> Semester |
| SubjectId | int? | FK -> Subject. NULL = tong ket toan ky |
| AverageScore | decimal(4,2)? | Diem trung binh |
| AcademicRank | int? | 1: Gioi, 2: Kha, 3: TB, 4: Yeu, 5: Kem |
| ConductRank | int? | 1: Tot, 2: Kha, 3: TB, 4: Yeu (chi o ban ghi tong ket, SubjectId=NULL) |
| IsPromoted | int? | 1: len lop, 0: o lai (chi HK2) |
| Note | string? | Ghi chu |

**Constraint unique:** (StudentId, SemesterId, SubjectId)

**Quan he:** Student (N-1), Semester (N-1), Subject (N-1?)

---

## 17. Attendance

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| StudentId | int | FK -> Student |
| ClassId | int | FK -> Class |
| SubjectId | int? | FK -> Subject. NULL = diem danh ca ngay |
| Date | DateOnly | Ngay diem danh |
| Period | int? | Tiet thu may (1-10). NULL = ca ngay |
| Status | int? | 1: Co mat, 2: Vang co phep, 3: Vang khong phep, 4: Di tre |
| Note | string(255)? | Ghi chu |
| RecordedBy | int? | FK -> Teacher |
| CreatedAt | DateTime | Ngay tao |

**Quan he:** Student (N-1), Class (N-1), Subject (N-1?), Teacher (N-1?)

---

## 18. ConductScore

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| StudentId | int | FK -> Student |
| SemesterId | int | FK -> Semester |
| ConductRank | int? | 1: Tot, 2: Kha, 3: Trung binh, 4: Yeu |
| Score | int? | Diem hanh kiem tich luy |
| Note | string? | Ghi chu |
| EvaluatedBy | int? | FK -> Teacher |
| EvaluatedAt | DateTime | Ngay danh gia |

**Constraint unique:** (StudentId, SemesterId)

**Quan he:** Student (N-1), Semester (N-1?), Teacher (N-1?)

---

## 19. Violation

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| StudentId | int | FK -> Student |
| ViolationTypeId | int | FK -> ViolationType |
| SemesterId | int | FK -> Semester |
| ViolationDate | DateOnly | Ngay vi pham |
| Description | string? | Mo ta chi tiet |
| ActionTaken | int? | 1: Nhac nho, 2: Phat lao dong, 3: Moi phu huynh, 4: Ky luat |
| RecordedBy | int? | FK -> Teacher |
| CreatedAt | DateTime | Ngay tao |

**Quan he:** Student (N-1), ViolationType (N-1), Semester (N-1), Teacher (N-1?)

---

## 20. Commendation

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| StudentId | int | FK -> Student |
| SemesterId | int | FK -> Semester |
| Type | int? | 1: HS Gioi, 2: HS Tien Tien, 3: Thi HSG, 4: The thao, 5: Khac |
| Title | string(200)? | Tieu de khen thuong |
| Level | int? | 1: Cap lop, 2: Cap truong, 3: Cap huyen, 4: Cap tinh, 5: Quoc gia |
| AwardedDate | DateOnly? | Ngay duoc khen |
| Description | string? | Mo ta |
| AddPoints | int | Diem cong hanh kiem |

**Quan he:** Student (N-1), Semester (N-1)

---

## 21. Notification

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Title | string(200) | Tieu de thong bao |
| Content | string? | Noi dung |
| Type | int? | 1: Thong bao chung, 2: Hoc vu, 3: Su kien, 4: Khan |
| SenderId | int? | FK -> User (nguoi gui) |
| TargetType | int? | 1: Toan truong, 2: Theo khoi, 3: Theo lop, 4: Ca nhan |
| CreatedAt | DateTime | Ngay tao |
| PublishedAt | DateTime? | Ngay xuat ban |

**Quan he:** Sender (N-1 User?), Recipients (NotificationRecipient 1-N)

---

## 22. NotificationRecipient (Join Table)

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| NotificationId | int | FK -> Notification |
| UserId | int | FK -> User |
| IsRead | int | 1: da doc, 0: chua doc |
| ReadAt | DateTime? | Thoi diem doc |

**Quan he:** Notification (N-1, Cascade), User (N-1)

---

## 23. Setting

| Trường | Kieu | Mo ta |
|--------|------|-------|
| Id | int | Khoa chinh |
| Key | string(100) | Kho cai dat (unique, VD: "school_name") |
| Value | string? | Gia tri cai dat |
| Description | string(255)? | Mo ta |
| UpdatedAt | DateTime | Ngay cap nhat |
| UpdatedBy | int? | FK -> User |

**Quan he:** UpdatedByUser (N-1 User?)

---

## Tom tat quan he giua cac bang

```
Roles
 └── Users (1-N)

SchoolYear (1-N) Semester (1-N) TeacherClass
SchoolYear (1-N) Class (1-N) StudentClass (N-1) Student
SchoolYear (1-N) Class (1-N) TeacherClass (N-1) Teacher
SchoolYear (1-N) Class (1-N) TeacherClass (N-1) Subject

Student (1-N) StudentClass (N-1) Class
Student (1-N) ParentStudent (N-1) Parent
Student (1-N) Score (N-1) Subject, Semester, ScoreType, Teacher
Student (1-N) SemesterResult (N-1) Semester, Subject?
Student (1-N) Attendance (N-1) Class, Subject?, Teacher?
Student (1-N) ConductScore (N-1) Semester, Teacher?
Student (1-N) Violation (N-1) ViolationType, Semester, Teacher?
Student (1-N) Commendation (N-1) Semester

Notification (1-N) NotificationRecipient (N-1) User
Setting (N-1?) User
```
