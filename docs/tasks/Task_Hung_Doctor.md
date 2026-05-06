# Task: Hùng (Doctor)

## Phạm Vi

Bạn tập trung vào **logic tầng dưới** cho module bác sĩ (không làm WinForms UI).

- BLL/DAL: load hàng đợi (chỉ `TrangThai='DangCho'`).
- BLL/DAL: bắt đầu khám (atomic `DangCho → DangKham`).
- BLL/DAL: lưu khám (SP/transaction) và đổi `DaKham`.
- BLL/DAL: cung cấp dữ liệu cho màn in/print preview (GUI sẽ render).
- (Nếu làm) BLL/DAL: quay lại hàng đợi (atomic `DangKham → DangCho`).

WinForms GUI do Châu phụ trách.

## Non‑Negotiable Rules

- 3‑Layer: `GUI → BLL → DAL → DataProvider`.
- GUI không viết SQL.
- Atomic transitions: check `RowsAffected == 1`.
- DataGridView giữ focus theo `MaLK` khi refresh.

## Yêu Cầu Từ Feedback (quan trọng)

1. `ChuyenSangDangKham` phải atomic

- DAL/BLL phải cập nhật có điều kiện: chỉ `DangCho → DangKham`.
- Nếu trả `false`: GUI reload hàng đợi và báo: `Lượt khám đã được bác sĩ khác nhận hoặc không còn ở trạng thái chờ.`

2. `sp_HoanTatKham` chống double‑save

- Chỉ cho phép lưu khi lượt đang `DangKham`.
- GUI phải disable nút Lưu sau click đầu (hoặc xử lý click 2 lần bằng message thân thiện).

3. Print fallback

- Demo không phụ thuộc máy in thật.
- `FrmInPhieu` phải hiển thị nội dung phiếu hoặc Print Preview.

## Auto‑Refresh (khuyến nghị)

- Nếu chưa ổn định: để manual refresh.
- Nếu có auto‑refresh: giữ focus theo `MaLK` (không theo index, không theo MaBN).

## Definition of Done

- Hàng đợi chỉ hiển thị `DangCho`.
- Bắt đầu khám không bị “2 bác sĩ nhận 1 lượt”; trường hợp xung đột phải fail êm.
- Lưu khám 1 lần thành công; bấm Lưu 2 lần không crash.
- In phiếu xem được trên màn hình (preview/view).
