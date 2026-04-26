using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace highschool_student_management.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Quản trị viên hệ thống", "Admin" },
                    { 2, "Giáo viên", "Teacher" },
                    { 3, "Học sinh", "Student" },
                    { 4, "Phụ huynh", "Parent" }
                });

            migrationBuilder.InsertData(
                table: "score_types",
                columns: new[] { "Id", "MinCount", "Name", "Weight" },
                values: new object[,]
                {
                    { 1, 5, "Điểm miệng", 1.0m },
                    { 2, 3, "Điểm 15 phút", 1.0m },
                    { 3, 2, "Điểm 1 tiết", 2.0m },
                    { 4, 1, "Điểm giữa kỳ", 2.0m },
                    { 5, 1, "Điểm cuối kỳ", 3.0m }
                });

            migrationBuilder.InsertData(
                table: "subjects",
                columns: new[] { "Id", "Code", "GradeLevel", "IsActive", "Name", "PeriodsPerWeek" },
                values: new object[,]
                {
                    { 1, "MATH", null, 1, "Toán học", 5 },
                    { 2, "LIT", null, 1, "Ngữ văn", 5 },
                    { 3, "ENG", null, 1, "Tiếng Anh", 4 },
                    { 4, "PHY", null, 1, "Vật lý", 3 },
                    { 5, "CHEM", null, 1, "Hóa học", 3 },
                    { 6, "BIO", null, 1, "Sinh học", 2 },
                    { 7, "HIS", null, 1, "Lịch sử", 2 },
                    { 8, "GEO", null, 1, "Địa lý", 2 },
                    { 9, "CIV", null, 1, "Giáo dục công dân", 1 },
                    { 10, "TECH", null, 1, "Công nghệ", 2 },
                    { 11, "ART", null, 1, "Nghệ thuật", 1 },
                    { 12, "PE", null, 1, "Thể dục", 2 }
                });

            migrationBuilder.InsertData(
                table: "violation_types",
                columns: new[] { "Id", "DeductPoints", "Description", "Name", "Severity" },
                values: new object[,]
                {
                    { 1, 1, "Không có mặt đúng giờ khi tiết học bắt đầu", "Đi học muộn", 1 },
                    { 2, 1, "Không hoàn thành bài tập hoặc không chuẩn bị bài ở nhà", "Không thuộc bài", 1 },
                    { 3, 2, "Gây ồn ào, ảnh hưởng đến việc học của các bạn khác", "Nói chuyện khi đang giảng bài", 2 },
                    { 4, 5, "Nghỉ học mà không có lý do chính đáng và không xin phép", "Vắng học không phép", 3 },
                    { 5, 5, "Mang vũ khí, chất kích thích hoặc các vật dụng nguy hiểm", "Mang đồ cấm vào trường", 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "score_types",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "score_types",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "score_types",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "score_types",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "score_types",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "violation_types",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "violation_types",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "violation_types",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "violation_types",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "violation_types",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
