namespace QuanLyHocSinh.Models.Enums
{
    // Giới tính
    public enum Gender { Nam = 1, Nu = 2 }

    // Trạng thái tài khoản
    public enum UserStatus { HoatDong = 1, BiKhoa = 0 }

    // Loại người dùng liên kết
    public enum RelatedType { Admin, Teacher, Student, Parent }

    // Trạng thái giáo viên
    public enum TeacherStatus { DangDay = 1, NghiPhep = 2, NghiViec = 3 }

    // Trạng thái học sinh
    public enum StudentStatus { DangHoc = 1, NghiHoc = 2, TotNghiep = 3, ChuyenTruong = 4 }

    // Quan hệ phụ huynh
    public enum ParentRelationship { Cha = 1, Me = 2, NguoiGiamHo = 3 }

    // Loại thông báo
    public enum NotificationType { ThongBaoChung = 1, HocVu = 2, SuKien = 3, Khan = 4 }

    // Đối tượng nhận thông báo
    public enum NotificationTargetType { ToanTruong = 1, TheoKhoi = 2, TheoLop = 3, CaNhan = 4 }

    // Xếp loại học lực
    public enum AcademicRank { Gioi = 1, Kha = 2, TrungBinh = 3, Yeu = 4, Kem = 5 }

    // Xếp loại hạnh kiểm
    public enum ConductRank { Tot = 1, Kha = 2, TrungBinh = 3, Yeu = 4 }

    // Trạng thái điểm danh
    public enum AttendanceStatus { CoMat = 1, VangCoPhep = 2, VangKhongPhep = 3, DiTre = 4 }

    // Mức độ vi phạm
    public enum ViolationSeverity { Nhe = 1, TrungBinh = 2, Nang = 3 }

    // Hình thức xử lý vi phạm
    public enum ActionTaken { NhacNho = 1, PhatLaoDong = 2, MoiPhuHuynh = 3, KyLuat = 4 }

    // Loại khen thưởng
    public enum CommendationType { HocSinhGioi = 1, HocSinhTienTien = 2, ThiHSG = 3, TheThao = 4, Khac = 5 }

    // Cấp khen thưởng
    public enum CommendationLevel { CapLop = 1, CapTruong = 2, CapHuyen = 3, CapTinh = 4, QuocGia = 5 }
}