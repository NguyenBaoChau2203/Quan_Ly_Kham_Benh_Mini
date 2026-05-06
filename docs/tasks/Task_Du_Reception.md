# Task: Dư (Reception)

## Phạm Vi

Bạn tập trung vào **logic tầng dưới** cho module tiếp nhận (không làm WinForms UI).

- BLL/DAL: quản lý bệnh nhân (danh sách, thêm/sửa, tìm theo tên/số điện thoại/CCCD).
- BLL/DAL: tạo lượt khám (đăng ký khám) và trả về `SoThuTu`.
- (Nếu làm) BLL/DAL: hủy lượt khám: chỉ cho `DangCho`.

WinForms GUI do Châu phụ trách. Bạn chỉ cần đảm bảo BLL/DAL chạy đúng theo contract và demo-safe.

## Non‑Negotiable Rules

- 3‑Layer: `GUI → BLL → DAL → DataProvider`.
- GUI không viết SQL.
- Không tự sinh STT trên GUI.
- Atomic update: hàm hủy/chuyển trạng thái phải check `RowsAffected == 1`.

## Yêu Cầu Từ Feedback (quan trọng)

1. `sp_TaoLuotKham` có thể rollback/fail

- BLL sẽ trả `null`.
- GUI bắt buộc check `null` và hiển thị: `Không thể tạo lượt khám. Vui lòng thử lại.`

2. CCCD/Trùng dữ liệu

- CCCD được phép NULL.
- Nếu người dùng nhập CCCD: validate đúng 12 số.
- Nếu CCCD đã tồn tại: khuyến nghị chặn thêm mới và hướng người dùng sang “Sửa bệnh nhân”.
- Số điện thoại trùng: chỉ cảnh báo (không luôn chặn).

3. `HuyLuotKham` phải atomic

- Chỉ cho phép `DangCho → DaHuy`.
- Nếu trả `false`: reload danh sách và báo: `Lượt khám đã thay đổi trạng thái hoặc không còn hợp lệ.`

## Checklist UI/UX (demo‑safe)

Phần này Châu sẽ implement trong GUI, nhưng bạn cần đảm bảo BLL/DAL trả về đúng để GUI hiển thị ổn:

- Không throw exception ra GUI; lỗi nghiệp vụ trả `null/false` theo contract.
- Message hiển thị cho user do GUI quyết định, nhưng BLL phải phân biệt được các case fail quan trọng (vd: trùng CCCD, sp fail, trạng thái không hợp lệ).

## Definition of Done

- Tìm BN theo tên/SDT/CCCD hoạt động đúng cả khi CCCD NULL.
- Tạo lượt khám hiển thị đúng STT; fail thì hiện đúng message.
- (Nếu có) Hủy lượt: chỉ hủy được `DangCho`.
