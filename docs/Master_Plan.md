# Master Plan: Quản Lý Khám Bệnh Mini (WinForms + SQL Server)

Nguồn tổng hợp:

- Spec Word: `Quy_dinh_du_an_Quan_Ly_Kham_Benh_Mini_Nhom_Truong_v1_3_Cai_thien.docx`
- Technical Feedback: A–F (đã cung cấp)

Mục tiêu: hợp nhất nội dung spec + vá các điểm dễ vỡ demo (transaction/trạng thái/triển khai DB) thành 1 chuẩn triển khai.

## 0) Mục Lục Nhanh

1. Mục tiêu và phạm vi dự án
2. Quyền hạn và trách nhiệm nhóm trưởng
3. Luồng vận hành bắt buộc
4. Kiến trúc solution và quy tắc 3 lớp
5. CSDL chính thức và các ràng buộc
6. Stored procedure bắt buộc
7. Chuẩn DataProvider, DAL, BLL, DTO
8. Hợp đồng hàm cho từng thành viên
9. Phân công module và quyền sở hữu form
10. Quy tắc WinForms, Git/Merge, review code
11. Ưu tiên tính năng, timeline, checklist
12. Kịch bản demo và điều kiện hoàn thành
13. Bổ sung kỹ thuật (đã vá theo feedback)
14. Cam kết thực hiện

## 1) Mục Tiêu Và Phạm Vi

Mục tiêu bắt buộc:

- Demo mượt theo luồng: Login → Tiếp nhận → Tạo lượt khám → Khám bệnh → In phiếu → Tra lịch sử → Dashboard.
- Thể hiện đúng kiến trúc 3 lớp.
- Có phân quyền 2 vai trò: `TiepNhan`, `BacSi`.
- Có số thứ tự theo ngày, sinh an toàn bằng stored procedure.
- Có dữ liệu demo để tránh màn hình trống.

Phạm vi không ưu tiên:

- Không làm export Excel giai đoạn đầu.
- Không làm dashboard nâng cao nếu luồng chính chưa xong.
- Không đổi bảng/cột/tên project/namespace/DTO/form/trạng thái/sp sau khi chốt.

## 2) Vai Trò Nhóm Trưởng (Châu)

- Chốt kiến trúc, database, tên trường, DTO, DataProvider.
- Duyệt/khước từ yêu cầu thay đổi.
- Tích hợp code cuối, test luồng demo, chuẩn bị script + backup DB.

## 3) Luồng Vận Hành Bắt Buộc

- Tiếp nhận: tìm BN → thêm/sửa nếu cần → đăng ký khám (tạo lượt) → nhận số thứ tự.
- Bác sĩ: xem hàng đợi `DangCho` → bắt đầu khám (đổi `DangKham`) → nhập chẩn đoán → lưu + in phiếu → `DaKham`.
- Sau khám: tra lịch sử + dashboard 7 ngày.

## 4) Kiến Trúc Solution Và Quy Tắc 3 Lớp

### 4.1) 3‑Layer Rule (Non‑Negotiable)

- `GUI → BLL → DAL → DataProvider`.
- GUI không viết SQL string.

### 4.2) Connection String

- Connection string đặt tại `ClinicApp.GUI/App.config`.
- DAL không hardcode connection string.

## 5) CSDL Chính Thức Và Ràng Buộc

### 5.1) Bảng

- `BenhNhan`
- `NhanVien`
- `LuotKham`
- `ChiTietKham`
- `BoDemSoThuTu`

### 5.2) Schema Rules (đã vá)

- CCCD: `VARCHAR(12) NULL`.
  - Nếu có nhập: BLL validate đúng 12 chữ số.
  - Không UNIQUE ở DB.

**Fix từ feedback**:

- `LuotKham` có `NgayKham (DATETIME)` và `NgayKhamDate (DATE)`.
- Hai cột này **phải được set trong stored procedure từ cùng 1 `GETDATE()`**, GUI/DAL không được truyền vào để tránh lệch ngày.

### 5.3) Giá Trị Cố Định (mapping hiển thị)

- Vai trò lưu trong DB/code: `TiepNhan`, `BacSi`.
- Hiển thị GUI:
  - `TiepNhan` → `Nhân viên tiếp nhận`
  - `BacSi` → `Bác sĩ`

- Trạng thái lưu trong DB/code: `DangCho`, `DangKham`, `DaKham`, `DaHuy`.
- Hiển thị GUI:
  - `DangCho` → `Đang chờ`
  - `DangKham` → `Đang khám`
  - `DaKham` → `Đã khám`
  - `DaHuy` → `Đã hủy`

### 5.4) Constraint Bắt Buộc

- `UNIQUE(NhanVien.Username)`
- `UNIQUE(ChiTietKham.MaLK)`
- `UNIQUE(LuotKham.NgayKhamDate, LuotKham.SoThuTu)`
- `CHECK(NhanVien.VaiTro IN ('TiepNhan', 'BacSi'))`
- `CHECK(LuotKham.TrangThai IN ('DangCho', 'DangKham', 'DaKham', 'DaHuy'))`

## 6) Stored Procedure Bắt Buộc (đặc tả đã vá)

Nhóm chỉ cần 2 SP chính.

### 6.1) `sp_TaoLuotKham` (transaction + lock + output)

Yêu cầu bắt buộc (tích hợp feedback B.1, B.4, F.1):

- Tự lấy thời gian server: `@Now = GETDATE()`; `@Ngay = CAST(@Now AS DATE)`.
- Transaction bắt buộc.
- Khi sinh số thứ tự: khóa dòng `BoDemSoThuTu` theo `@Ngay` bằng cơ chế khóa phù hợp (ví dụ `UPDLOCK`, `HOLDLOCK`).
- Insert `LuotKham` với `NgayKham=@Now`, `NgayKhamDate=@Ngay`, `TrangThai='DangCho'`.
- OUTPUT: `@MaLK`, `@SoThuTu`.
- Nếu lỗi: rollback; BLL trả `null`; GUI hiển thị: `Không thể tạo lượt khám. Vui lòng thử lại.`

### 6.2) `sp_HoanTatKham` (đúng trạng thái + chống double‑save)

Yêu cầu bắt buộc (tích hợp feedback B.3, F.3):

- Chỉ cho phép chạy khi `LuotKham.TrangThai = 'DangKham'`.
- Insert `ChiTietKham` + update `LuotKham` sang `DaKham` trong 1 transaction.
- Nếu `MaLK` không tồn tại, không ở `DangKham`, hoặc đã có `ChiTietKham`: rollback và trả thất bại.

## 7) Chuẩn DataProvider, DAL, BLL, DTO

### 7.1) DataProvider

- Mở `SqlConnection` phải đặt trong `using`.
- Bắt exception tập trung + ghi log.
- Không hiển thị lỗi SQL thô ra GUI.
- Phải hỗ trợ gọi stored procedure có OUTPUT parameter (đặc biệt `sp_TaoLuotKham` gọi bằng `ExecuteNonQuery`).

Logging (tích hợp feedback C.9):

- Ưu tiên log ở `%LocalAppData%\ClinicApp\error.log` để tránh lỗi quyền ghi.

### 7.2) DTO

- DTO hậu tố `DTO`.

### 7.3) Naming

- Hàm tiếng Việt không dấu PascalCase: `DangNhap`, `TimBenhNhan`, `TaoLuotKham`, `HoanTatKham`.

### 7.4) Password (đã chốt)

**Fix từ feedback B.5/F.4**: SHA256 là **bắt buộc**.

- GUI nhận mật khẩu gốc.
- BLL hash SHA256 trước khi gọi DAL.
- DB lưu hash SHA256, không lưu plain text.
- Demo login vẫn nhập: `tiepnhan/123`, `bacsi/123`.

## 8) Hợp Đồng Hàm (Contract)

Quy ước return (tích hợp feedback C.1):

- `DataTable`: trả bảng rỗng nếu không có dữ liệu.
- `DTO`: trả `null` nếu không tìm thấy/thất bại.
- `bool`: trả `false` nếu validation/database failure.

Quy ước lỗi user-facing:

- GUI chỉ show thông báo thân thiện.
- Không show SQL error thô.

### 8.1) API Contract v0 (BLL/DAL)
Quy ước chung:

- BLL/DAL **không throw** ra GUI. Exception nội bộ phải được catch và trả `null/false`.
- `DataTable`: **không bao giờ** trả `null` (trả bảng rỗng).
- `DTO`: trả `null` khi không có dữ liệu hoặc fail.
- `bool`: trả `true` khi thao tác thành công và đúng 1 lượt được cập nhật; ngược lại `false`.
- Các hàm đổi trạng thái phải đảm bảo atomic bằng `WHERE TrangThai = <expected>` và check `RowsAffected == 1`.

DTO tối thiểu (đặt ở `ClinicApp.DTO`):

- `NhanVienDTO`: `MaNV`, `Username`, `Role` (`TiepNhan` | `BacSi`), `HoTen`.
- `BenhNhanDTO`: `MaBN`, `HoTen`, `NgaySinh?`, `GioiTinh`, `SDT`, `CCCD?`, `DiaChi?`.
- `LuotKhamDTO`: `MaLK`, `MaBN`, `SoThuTu`, `NgayKham`, `TrangThai`, `MaBacSi?`.
- `ChiTietKhamDTO`: `MaLK`, `TrieuChung`, `ChanDoan`, `ToaThuoc`, `LoiDan`.

#### 8.1.1) Auth

BLL:

```csharp
// passwordPlain là mật khẩu user nhập; BLL hash SHA256 rồi so với DB.
NhanVienDTO? DangNhap(string username, string passwordPlain);
```

DAL:

```csharp
NhanVienDTO? DangNhap(string username, string passwordSha256);
```

#### 8.1.2) Reception (BenhNhan, TaoLuotKham, Huy)

BLL:

```csharp
DataTable TimBenhNhan(string? keyword); // keyword: tên/SDT/CCCD; trả bảng rỗng nếu không có.
BenhNhanDTO? LayBenhNhanTheoMa(int maBN);

// Them/Sua: trả false nếu validate fail hoặc DB fail.
bool ThemBenhNhan(BenhNhanDTO bn);
bool CapNhatBenhNhan(BenhNhanDTO bn);

// Tạo lượt khám: trả null nếu sp fail/rollback.
LuotKhamDTO? TaoLuotKham(int maBN, int? maBacSi, string? ghiChu);

// Hủy lượt khám: chỉ DangCho -> DaHuy.
bool HuyLuotKham(int maLK);
```

DAL:

```csharp
DataTable TimBenhNhan(string? keyword);
BenhNhanDTO? LayBenhNhanTheoMa(int maBN);
bool ThemBenhNhan(BenhNhanDTO bn);
bool CapNhatBenhNhan(BenhNhanDTO bn);

// Gọi sp_TaoLuotKham với OUTPUT @MaLK, @SoThuTu (NgayKham lấy từ GETDATE trong SQL).
LuotKhamDTO? TaoLuotKham(int maBN, int? maBacSi, string? ghiChu);

bool HuyLuotKham(int maLK);
```

#### 8.1.3) Doctor (Queue, Start, Save+Complete, Back)

BLL:

```csharp
DataTable LayHangDoiDangCho();

// Atomic: DangCho -> DangKham.
bool ChuyenSangDangKham(int maLK, int maBacSi);

// Hoàn tất khám (atomic trong SP): chỉ chạy khi DangKham; insert ChiTietKham + update DaKham.
bool HoanTatKham(ChiTietKhamDTO ct);

// (Nếu làm) Atomic: DangKham -> DangCho.
bool ChuyenVeDangCho(int maLK);

// Dữ liệu để in/preview (GUI render theo template).
DataTable LayDuLieuInPhieu(int maLK);
```

DAL:

```csharp
DataTable LayHangDoiDangCho();
bool ChuyenSangDangKham(int maLK, int maBacSi);
bool HoanTatKham(ChiTietKhamDTO ct);
bool ChuyenVeDangCho(int maLK);
DataTable LayDuLieuInPhieu(int maLK);
```

#### 8.1.4) History / Dashboard

BLL:

```csharp
DataTable LayLichSuKham(DateTime fromDate, DateTime toDate, string? keyword);
DataTable LayThongKe7Ngay();
```

DAL:

```csharp
DataTable LayLichSuKham(DateTime fromDate, DateTime toDate, string? keyword);
DataTable LayThongKe7Ngay();
```

### 8.2) Atomic + SP Transaction/Locking (chốt)

Chỉ các thao tác dưới đây bắt buộc atomic/transaction theo đúng rules:

- `sp_TaoLuotKham`:
  - `@Now = GETDATE()`; `@Ngay = CAST(@Now AS DATE)`.
  - Transaction.
  - Khóa dòng `BoDemSoThuTu` theo ngày với `UPDLOCK, HOLDLOCK` khi tăng bộ đếm.
  - Insert `LuotKham` với `TrangThai='DangCho'`.
  - OUTPUT: `@MaLK`, `@SoThuTu`.
- `ChuyenSangDangKham` (DAL/BLL): `UPDATE ... WHERE MaLK=@MaLK AND TrangThai='DangCho'`.
- `HuyLuotKham` (DAL/BLL): `UPDATE ... WHERE MaLK=@MaLK AND TrangThai='DangCho'`.
- `ChuyenVeDangCho` (DAL/BLL): `UPDATE ... WHERE MaLK=@MaLK AND TrangThai='DangKham'`.
- `sp_HoanTatKham`:
  - Transaction.
  - Guard trạng thái: chỉ chạy khi `LuotKham.TrangThai='DangKham'`.
  - Insert `ChiTietKham` + update `LuotKham.TrangThai='DaKham'` trong 1 transaction.
  - Double-save phải fail (rollback) và trả `false` cho BLL/DAL.

## 9) Phân Công Module

Nguyên tắc tổ chức (cập nhật): **Châu làm toàn bộ UI/UX WinForms (GUI) trước**, 2 bạn còn lại tập trung làm tầng dưới theo đúng contract.

- Châu (Lead):
  - Làm toàn bộ WinForms GUI theo mockup trong `design/`.
  - Chốt contract BLL/DAL/DTO (tên hàm, input/output, message user-facing).
  - DB schema + Stored Procedure + DataProvider + tích hợp end-to-end.
- Dư (Reception):
  - Implement `BLL/DAL` cho module Tiếp nhận (bệnh nhân, tạo lượt, hủy lượt).
  - Không chỉnh WinForms UI (trừ khi Châu yêu cầu).
- Hùng (Doctor):
  - Implement `BLL/DAL` cho module Bác sĩ (hàng đợi, chuyển trạng thái, lưu khám, dữ liệu in phiếu).
  - Không chỉnh WinForms UI (trừ khi Châu yêu cầu).

Lợi ích: Châu làm GUI trước để khoá layout/label/luồng demo; 2 bạn còn lại làm logic song song dựa trên contract.

## 10) Quy Tắc WinForms, Git/Merge

WinForms:

- Không sửa `.Designer.cs` bằng tay.
- Không kéo thả control vào form của người khác.
- Nếu Châu đang làm toàn bộ GUI: các bạn **không** chỉnh project `ClinicApp.GUI` để tránh conflict.
- DataGridView refresh cần giữ focus theo `MaLK`.

Git/Merge:

- Bắt buộc có `.gitignore` (không commit `bin/`, `obj/`, `.vs/`).

Quy tắc thay đổi:

- Ai muốn đổi bảng/cột/hàm/nghiệp vụ phải báo nhóm trưởng.
- Châu cập nhật script SQL + DTO + contract + tài liệu trước, rồi các bạn mới sửa code.

## 11) Ưu Tiên, Timeline, Checklist

Ưu tiên:

- Luồng demo đúng và ổn định là số 1.
- Auto‑refresh chỉ làm khi core flow ổn.
- Dashboard giữ “simple dashboard”.

## 12) Demo Và Definition of Done

### 12.1) Kịch bản demo

1. Login tiếp nhận.
2. Tìm BN theo CCCD/SDT.
3. Thêm BN nếu chưa có.
4. Đăng ký khám, hiển thị STT.
5. Login bác sĩ.
6. Bác sĩ mở hàng đợi `DangCho`.
7. Chọn bệnh nhân, chuyển `DangKham`.
8. Nhập khám.
9. Lưu & In.
10. Trạng thái sang `DaKham`.
11. Lịch sử có dữ liệu.
12. Dashboard 7 ngày có dữ liệu.

### 12.2) Dữ liệu demo bắt buộc (đã vá)

- 10–15 bệnh nhân.
- 2 tài khoản: `tiepnhan/123`, `bacsi/123`.
- 5–6 lượt khám cũ để lịch sử/dashboard không trống.

**Fix từ feedback B.6/F.5**: chốt rõ triển khai DB:

- Có `Setup.sql` chạy được trên SQL Server sạch.
- Có `.bak` dự phòng.
- Chốt tên database + connection string template.
- Demo trên máy khác chỉ sửa `App.config`.

### 12.3) Definition of Done (đã vá)

- `sp_TaoLuotKham` transaction + lock, không trùng STT khi concurrent.
- `ChuyenSangDangKham`, `HuyLuotKham`, `ChuyenVeDangCho` là atomic (RowsAffected == 1).
- `sp_HoanTatKham` chỉ chạy khi `DangKham`, double‑save không làm crash.
- Print có fallback (preview/view), không phụ thuộc máy in.

## 13) Bổ Sung Kỹ Thuật (tích hợp Suggested Document Patch)

### 13.1) Giữ focus hàng đợi theo `MaLK`

- Khi refresh: lưu `MaLK` đang chọn, reload data, tìm lại dòng theo `MaLK`, set `CurrentCell`.

### 13.2) OUTPUT parameter khi gọi SP

- `sp_TaoLuotKham` trả OUTPUT (`@MaLK`, `@SoThuTu`) nên DAL gọi bằng `ExecuteNonQuery` + `SqlParameter.Direction=Output`.

### 13.3) Atomic status transition (đã vá)

- `ChuyenSangDangKham`: chỉ `DangCho → DangKham`.
- `HuyLuotKham`: chỉ `DangCho → DaHuy`.
- `ChuyenVeDangCho`: chỉ `DangKham → DangCho`.

Nếu update 0 dòng: GUI reload + báo lượt đã bị thay đổi.

### 13.4) In phiếu (đã vá)

- Demo không phụ thuộc máy in thật.
- Ưu tiên Print Preview hoặc hiển thị nội dung phiếu trên form.

## 14) Cam Kết

- Tài liệu này là chuẩn chính thức.
- Mọi thay đổi phải được nhóm trưởng duyệt trước khi code.
