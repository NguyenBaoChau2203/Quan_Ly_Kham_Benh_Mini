# Quan_Ly_Kham_Benh_Mini

Mini Clinic Management System (WinForms + SQL Server) cho bài môn học.

## Quick Start (Cho Team)

1. Đọc quy định/đặc tả dự án:
   - `Quy_dinh_du_an_Quan_Ly_Kham_Benh_Mini_Nhom_Truong_v1_3_Cai_thien.docx`
2. Đọc chuẩn triển khai đã chốt (bắt buộc theo đúng kiến trúc/luồng/trạng thái):
   - `docs/Master_Plan.md`
3. Mỗi bạn đọc task của mình và làm đúng “hợp đồng hàm”:
   - Châu (Lead): `docs/tasks/Task_Chau_Lead.md`
   - Dự (Reception): `docs/tasks/Task_Du_Reception.md`
   - Hùng (Doctor): `docs/tasks/Task_Hung_Doctor.md`
4. Xem UI mẫu (ảnh + HTML tham khảo từ Google Stitch):
   - `design/stitch_mini_clinic_management_system/`
   - Mỗi màn hình có `screen.png` (ảnh) và `code.html` (tham khảo layout/style).

## Repo Structure

- `docs/`: tài liệu chuẩn triển khai và phân công
- `design/`: UI mockups (Stitch export)
- `sql/`: script DB (Setup/backup placeholders)
- `src/`: Visual Studio solution (sẽ đặt 4 projects: DTO/DAL/BLL/GUI)

## Non-Negotiables (Tóm tắt)

- Kiến trúc: `GUI → BLL → DAL → DataProvider` (GUI không viết SQL string)
- Update trạng thái lượt khám phải atomic: kiểm tra `RowsAffected == 1`
- Stored procedure có transaction + locking (`UPDLOCK`, `HOLDLOCK`) theo `docs/Master_Plan.md`
- Trạng thái chuẩn dùng thống nhất: `DangCho`, `DangKham`, `DaKham`, `DaHuy`
