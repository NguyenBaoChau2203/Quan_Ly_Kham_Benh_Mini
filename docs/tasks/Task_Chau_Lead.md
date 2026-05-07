# Task: Châu (Lead)

Chi tiết kế hoạch theo phase và cách giao việc cho AI agent: `docs/tasks/Task_Chau_Agent_Runbook.md`.

## Phạm Vi

Bạn chịu trách nhiệm các phần “core”, **toàn bộ WinForms GUI**, và chốt chuẩn để 2 bạn còn lại implement đúng BLL/DAL:

- Database schema + constraint + dữ liệu demo.
- `DataProvider` + chuẩn DAL.
- Stored procedures: `sp_TaoLuotKham`, `sp_HoanTatKham`.
- SHA256 password hashing rule (bắt buộc).
- `AuthDAL.DangNhap`: implement truy vấn đăng nhập (Châu đã implement `AuthBLL` nên sở hữu luôn DAL tương ứng).
- Module History/Dashboard: implement `ThongKeBLL` + `ThongKeDAL` (ưu tiên simple dashboard).
- Chuẩn bị deliverables DB: `Setup.sql` + `.bak` + hướng dẫn setup.

## Non‑Negotiable Rules

- 3‑Layer: `GUI → BLL → DAL → DataProvider`.
- GUI không viết SQL.
- Atomic status updates: `RowsAffected == 1`.
- `sp_TaoLuotKham` dùng transaction + row lock (`UPDLOCK/HOLDLOCK`) khi tăng bộ đếm.
- `NgayKham/NgayKhamDate` tự sinh trong SQL Server từ `GETDATE()`.

## Đầu Ra (Deliverables)

- `sql/Setup.sql` (1 script chạy trên SQL Server sạch).
- `sql/backup/*.bak`.
- Chốt **tên database** + **connection string template** (đưa vào Master_Plan).
- Document hoá contract BLL/DAL cho các hàm:
  - `DangNhap`
  - `TimBenhNhan`
  - `TaoLuotKham` (trả `null` khi fail)
  - `LayHangDoiDangCho`
  - `ChuyenSangDangKham` (atomic)
  - `HoanTatKham`
  - `HuyLuotKham` (atomic)
  - `ChuyenVeDangCho` (atomic)

## Lưu Ý Tổ Chức (GUI-first)

- Châu làm GUI trước để khoá layout/label/luồng demo theo `design/`.
- Trong thời gian này, Dư/Hùng làm BLL/DAL theo contract; không chỉnh project GUI để tránh conflict.
- Khi BLL/DAL xong, Châu wire-up vào GUI và chạy demo end-to-end.

## Checklist Kỹ Thuật (tích hợp từ feedback F)

1. `sp_TaoLuotKham`

- Tự lấy `@Now = GETDATE()`; `@Ngay = CAST(@Now AS DATE)`.
- Khóa dòng `BoDemSoThuTu` theo `@Ngay` trong transaction khi tăng `SoCuoi`.
- Insert `LuotKham` với `TrangThai='DangCho'`.
- OUTPUT: `@MaLK`, `@SoThuTu`.
- Lỗi → rollback.

2. Atomic transitions

- `ChuyenSangDangKham`: chỉ `DangCho → DangKham`.
- `HuyLuotKham`: chỉ `DangCho → DaHuy`.
- `ChuyenVeDangCho`: chỉ `DangKham → DangCho`.

3. `sp_HoanTatKham`

- Chỉ chạy khi trạng thái hiện tại là `DangKham`.
- Insert `ChiTietKham` + update `LuotKham` trong 1 transaction.
- Double‑save → phải fail “êm” (rollback + trả thất bại rõ ràng để GUI xử lý).

4. Password SHA256 (bắt buộc)

- DB lưu hash.
- Demo user vẫn gõ `123`.

5. Database deployment

- Script SQL chạy được trên môi trường sạch.
- `.bak` dự phòng.
- Chốt DB name + hướng dẫn setup.

## Definition of Done

- Setup DB trên máy khác thành công bằng `Setup.sql`.
- 2 stored procedures chạy đúng theo rules (transaction/lock/status).
- Login dùng SHA256 thống nhất với dữ liệu demo (`AuthDAL` + `AuthBLL` chạy end-to-end).
- `ThongKeDAL`/`ThongKeBLL` trả đúng `DataTable` cho lịch sử và dashboard.
- Các hàm đổi trạng thái trả về đúng theo `RowsAffected == 1`.
