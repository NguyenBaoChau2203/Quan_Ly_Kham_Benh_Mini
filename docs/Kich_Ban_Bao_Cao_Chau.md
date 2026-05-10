# Kịch Bản Báo Cáo - Châu

Vai trò: nhóm trưởng, phụ trách core, database, GUI WinForms, tích hợp và demo.

## 1. Mở Đầu

Em chào thầy và các bạn. Em là Châu, nhóm trưởng của nhóm. Đề tài của nhóm em là **Quản lý khám bệnh mini**, được xây dựng bằng **WinForms C# và SQL Server**.

Mục tiêu của hệ thống là mô phỏng một quy trình khám bệnh cơ bản trong phòng khám nhỏ, gồm các bước: đăng nhập, tiếp nhận bệnh nhân, tạo lượt khám, bác sĩ nhận bệnh, khám bệnh, in phiếu khám, xem lịch sử và dashboard thống kê.

## 2. Giới Thiệu Tổng Quan Hệ Thống

Hệ thống tập trung vào luồng nghiệp vụ chính, không mở rộng quá nhiều module ngoài phạm vi để tránh bị loãng khi demo.

Luồng chính:

1. Nhân viên tiếp nhận đăng nhập.
2. Tìm hoặc thêm bệnh nhân.
3. Tạo lượt khám và nhận số thứ tự.
4. Bác sĩ đăng nhập.
5. Bác sĩ xem hàng đợi và nhận bệnh nhân vào khám.
6. Nhập triệu chứng, chẩn đoán, toa thuốc, lời dặn.
7. Hoàn tất khám và in phiếu.
8. Xem lại lịch sử khám và dashboard 7 ngày.

Hệ thống có 2 vai trò:

- `TiepNhan`: làm việc với bệnh nhân, tạo lượt khám, hủy lượt khi còn chờ.
- `BacSi`: xem hàng đợi, bắt đầu khám, hoàn tất khám, in phiếu.

## 3. Vai Trò Nhóm Trưởng Và Cách Tổ Chức

Với vai trò nhóm trưởng, phần việc của em không chỉ là code một màn hình riêng, mà là chốt kiến trúc chung, chia module, quy định cách đặt tên hàm, cách các tầng giao tiếp với nhau và tích hợp cuối cùng.

Em chia nhóm theo module và theo tầng:

- Châu: phụ trách core, database, GUI WinForms, DataProvider, stored procedure, tích hợp và test demo.
- Dũ/Dư: phụ trách logic tiếp nhận, gồm bệnh nhân, tìm kiếm, thêm/sửa, tạo lượt khám, hủy lượt khám.
- Hùng: phụ trách logic bác sĩ, gồm hàng đợi, bắt đầu khám, hoàn tất khám, dữ liệu in phiếu.

Cách tổ chức:

- Em chốt trước `Master_Plan.md` và các file task trong `docs/tasks`.
- Mỗi bạn làm theo contract BLL/DAL, không tự ý đổi tên bảng, cột, DTO, stored procedure.
- GUI do em giữ chính để tránh conflict.
- Các bạn code logic tầng dưới, em wire-up vào GUI và test end-to-end.

Khi nói với thầy:

> Em không để mỗi bạn tự đặt tên hàm riêng, vì như vậy khi ghép GUI sẽ rất dễ lệch. Em chốt contract trước, ví dụ hàm trả `DataTable`, `bool`, hoặc `DTO?`, và quy định BLL/DAL không throw lỗi SQL trực tiếp ra GUI.

## 4. Kiến Trúc 3 Lớp

Hệ thống dùng mô hình 3 lớp:

```text
GUI -> BLL -> DAL -> DataProvider -> SQL Server
```

Giải thích:

- `GUI`: hiển thị giao diện, nhận thao tác người dùng.
- `BLL`: xử lý nghiệp vụ và validate dữ liệu.
- `DAL`: truy vấn database hoặc gọi stored procedure.
- `DataProvider`: lớp dùng chung để mở kết nối SQL, chạy query, gọi stored procedure và log lỗi.

Quy tắc quan trọng:

- GUI không viết SQL trực tiếp.
- Connection string nằm trong `App.config`.
- DAL gọi database thông qua `DataProvider`.

Chỉ code khi báo cáo:

- `src/ClinicApp.GUI/Forms/FrmLogin.cs`: form đăng nhập gọi `AuthBLL.DangNhap`.
- `src/ClinicApp.BLL/AuthBLL.cs`: hash password SHA256.
- `src/ClinicApp.DAL/AuthDAL.cs`: query bảng `NhanVien`.
- `src/ClinicApp.DAL/DataProvider.cs`: mở connection, `ExecuteQuery`, `ExecuteNonQuerySP`, log lỗi.

## 5. Phần Châu Phụ Trách

Phần chính em phụ trách:

- Thiết kế và hoàn thiện toàn bộ giao diện WinForms.
- Làm shell chính: top bar, sidebar, điều hướng theo role.
- Làm các form: Login, Bệnh nhân, Tạo lượt khám, Hàng đợi, Khám bệnh, In phiếu, Lịch sử, Dashboard.
- Thiết kế database và script `Setup.sql`.
- Làm stored procedure quan trọng: `sp_TaoLuotKham`, `sp_HoanTatKham`.
- Chuẩn hóa `DataProvider`.
- Cấu hình connection string.
- Tạo dữ liệu demo.
- Tích hợp BLL/DAL của các bạn vào GUI.
- Test tay toàn bộ luồng demo và sửa các lỗi giao diện/logic.

## 6. Cơ Sở Dữ Liệu

Database tên là `ClinicAppDB`.

Các bảng chính:

- `BenhNhan`: lưu thông tin bệnh nhân.
- `NhanVien`: lưu tài khoản đăng nhập, password hash và vai trò.
- `LuotKham`: lưu mỗi lượt khám, số thứ tự, ngày khám, trạng thái.
- `ChiTietKham`: lưu nội dung khám như triệu chứng, chẩn đoán, toa thuốc, lời dặn.
- `BoDemSoThuTu`: dùng để sinh số thứ tự theo từng ngày.

Ràng buộc quan trọng:

- `NhanVien.Username` là duy nhất.
- `NhanVien.VaiTro` chỉ được là `TiepNhan` hoặc `BacSi`.
- `LuotKham.TrangThai` chỉ được là `DangCho`, `DangKham`, `DaKham`, `DaHuy`.
- `LuotKham.NgayKhamDate + SoThuTu` là duy nhất, để không trùng số thứ tự trong cùng ngày.
- `ChiTietKham.MaLK` là duy nhất, để một lượt khám chỉ có một chi tiết khám.
- `BenhNhan.SDT` là duy nhất.
- `BenhNhan.CCCD` là duy nhất nếu có nhập, nhưng vẫn cho phép null.

Chỉ code database:

- Mở `sql/Setup.sql`.
- Chỉ phần tạo database và bảng: từ đầu file đến hết bảng `BoDemSoThuTu`.
- Chỉ phần unique index:
  - `UX_BenhNhan_SDT`
  - `UX_BenhNhan_CCCD_NotNull`
- Chỉ phần constraint:
  - `CK_NhanVien_VaiTro`
  - `CK_LuotKham_TrangThai`
  - `UQ_LuotKham_NgaySTT`
  - `UQ_ChiTietKham_MaLK`

Nói với thầy:

> Ở đây em có ràng buộc dữ liệu ở cả tầng BLL lẫn database. BLL chặn sớm để thông báo thân thiện cho người dùng, còn database có unique index để bảo vệ dữ liệu nếu có thao tác chèn trực tiếp hoặc lỗi lập trình.

## 7. Stored Procedure Tạo Lượt Khám

Stored procedure: `sp_TaoLuotKham`.

Mục đích:

- Tạo một lượt khám mới.
- Tự sinh số thứ tự theo ngày.
- Lấy thời gian từ SQL Server bằng `GETDATE()`.
- Trả về `MaLK`, `SoThuTu`, `NgayKham`.

Tại sao cần transaction:

- Tạo lượt khám không chỉ insert vào `LuotKham`, mà còn tăng bộ đếm trong `BoDemSoThuTu`.
- Nếu tăng bộ đếm thành công nhưng insert lượt khám thất bại thì dữ liệu sai.
- Transaction giúp đảm bảo thành công hết hoặc rollback hết.

Tại sao cần lock:

- Nếu hai nhân viên tạo lượt cùng lúc, cả hai có thể lấy cùng một số thứ tự nếu không khóa.
- Bảng `BoDemSoThuTu` giữ số cuối của mỗi ngày.
- Khi tạo lượt, stored procedure khóa dòng ngày hiện tại, tăng `SoCuoi`, rồi mới insert lượt khám.

Chỉ code:

- `sql/Setup.sql`
- Tìm `CREATE PROCEDURE sp_TaoLuotKham`.
- Chỉ các dòng:
  - `SET @NgayKham = GETDATE()`
  - `DECLARE @Ngay DATE = CAST(@Now AS DATE)`
  - `BEGIN TRANSACTION`
  - `MERGE BoDemSoThuTu WITH (HOLDLOCK)`
  - `INSERT INTO LuotKham (...)`
  - `COMMIT TRANSACTION`
  - `ROLLBACK TRANSACTION`

Nói với thầy:

> Em không để GUI tự sinh số thứ tự. GUI chỉ gửi mã bệnh nhân và ghi chú xuống BLL/DAL. Số thứ tự được sinh ở SQL Server để đảm bảo đúng ngày server và tránh trùng khi có nhiều người tạo lượt cùng lúc.

## 8. Stored Procedure Hoàn Tất Khám

Stored procedure: `sp_HoanTatKham`.

Mục đích:

- Chỉ cho lưu khám khi lượt đang ở trạng thái `DangKham`.
- Insert vào `ChiTietKham`.
- Update `LuotKham` sang `DaKham`.
- Chống bấm lưu hai lần.

Tại sao cần guard trạng thái:

- Nếu lượt khám không ở `DangKham` thì không được hoàn tất.
- Nếu đã có `ChiTietKham` thì không được insert thêm lần nữa.

Chỉ code:

- `sql/Setup.sql`
- Tìm `CREATE PROCEDURE sp_HoanTatKham`.
- Chỉ các dòng:
  - `BEGIN TRANSACTION`
  - `WHERE MaLK = @MaLK AND TrangThai = 'DangKham'`
  - `IF EXISTS (SELECT 1 FROM ChiTietKham WHERE MaLK = @MaLK)`
  - `INSERT INTO ChiTietKham`
  - `UPDATE LuotKham SET TrangThai = 'DaKham'`
  - `SET @KetQua = 1`
  - `ROLLBACK TRANSACTION`

Nói với thầy:

> Phần này giúp đảm bảo việc hoàn tất khám là atomic. Nếu insert chi tiết khám thành công mà update trạng thái thất bại thì transaction rollback. Ngoài ra unique constraint trên `ChiTietKham.MaLK` và guard trong stored procedure giúp tránh double-save.

## 9. Luồng Trạng Thái

Trạng thái lượt khám:

```text
DangCho -> DangKham -> DaKham
DangCho -> DaHuy
DangKham -> DangCho (nếu cần quay lại hàng đợi)
```

Giải thích:

- `DangCho`: vừa tạo lượt, đang chờ bác sĩ.
- `DangKham`: bác sĩ đã nhận bệnh nhân.
- `DaKham`: đã hoàn tất và có thể in phiếu.
- `DaHuy`: lượt bị hủy khi còn đang chờ.

Chỉ code:

- `src/ClinicApp.DAL/KhamDAL.cs`
  - `ChuyenSangDangKham`
  - `HoanTatKham`
  - `ChuyenVeDangCho`
- `src/ClinicApp.DAL/LuotKhamDAL.cs`
  - `HuyLuotKham`
- `sql/Setup.sql`
  - `sp_ChuyenSangDangKham`
  - `sp_HoanTatKham`

Nói với thầy:

> Các thao tác chuyển trạng thái đều có điều kiện trạng thái hiện tại. Ví dụ chỉ `DangCho` mới được chuyển sang `DangKham`. Nếu hai bác sĩ cùng nhận một lượt, chỉ người đầu tiên update được, người sau sẽ nhận false.

## 10. Đăng Nhập Và Bảo Mật

Tài khoản demo:

- `tiepnhan / 123`
- `bacsi / 123`

Mật khẩu không lưu plain text.

Luồng đăng nhập:

1. GUI nhận username/password.
2. `AuthBLL` hash password bằng SHA256.
3. `AuthDAL` so sánh username và password hash với bảng `NhanVien`.
4. Nếu đúng, trả về `NhanVienDTO`.
5. `FrmMain` đọc role và hiện menu phù hợp.

Chỉ code:

- `src/ClinicApp.GUI/Forms/FrmLogin.cs`
- `src/ClinicApp.BLL/AuthBLL.cs`
- `src/ClinicApp.DAL/AuthDAL.cs`
- `sql/Setup.sql`, phần insert `NhanVien`.

Nói với thầy:

> Password người dùng nhập là `123`, nhưng database không lưu `123`, mà lưu chuỗi SHA256. Khi đăng nhập, BLL hash lại password người dùng nhập rồi DAL mới so sánh.

## 11. Luồng Demo Khi Thao Tác

Khi demo, nói theo các bước:

1. Đăng nhập tiếp nhận bằng `tiepnhan/123`.
2. Vào màn bệnh nhân, tìm bệnh nhân theo tên, SĐT hoặc CCCD.
3. Nếu chưa có, thêm bệnh nhân. Giải thích validate: họ tên không rỗng, SĐT đúng định dạng và không trùng, CCCD đúng 12 số và không trùng nếu có nhập.
4. Vào tạo lượt khám, chọn bệnh nhân, nhập lý do khám, bấm đăng ký.
5. Giải thích số thứ tự trả về từ `sp_TaoLuotKham`.
6. Đăng xuất, đăng nhập bác sĩ bằng `bacsi/123`.
7. Vào hàng đợi khám, chỉ thấy các lượt `DangCho`.
8. Bấm bắt đầu khám, trạng thái thành `DangKham`.
9. Nhập chẩn đoán/toa thuốc/lời dặn, bấm lưu và xem phiếu.
10. Giải thích `sp_HoanTatKham` insert chi tiết và update trạng thái `DaKham`.
11. Vào in phiếu, có thể tải phiếu theo mã lượt khám hoặc xem danh sách đã khám.
12. Vào lịch sử, lọc theo ngày, mã BN, họ tên, SĐT, CCCD.
13. Vào dashboard xem thống kê 7 ngày.

## 12. Các File Cần Mở Khi Báo Cáo

### Database

- `sql/Setup.sql`
  - Tạo bảng.
  - Constraint.
  - Unique index.
  - `sp_TaoLuotKham`.
  - `sp_HoanTatKham`.
  - Dữ liệu demo.

### DataProvider

- `src/ClinicApp.DAL/DataProvider.cs`
  - `ExecuteQuery`
  - `ExecuteNonQuery`
  - `ExecuteNonQuerySP`
  - `LogError`

### Đăng Nhập

- `src/ClinicApp.BLL/AuthBLL.cs`
  - `HashSha256`
- `src/ClinicApp.DAL/AuthDAL.cs`
  - Query bảng `NhanVien`

### Tiếp Nhận

- `src/ClinicApp.GUI/Forms/FrmBenhNhan.cs`
  - validate và lưu bệnh nhân
- `src/ClinicApp.BLL/BenhNhanBLL.cs`
  - chặn trùng SĐT/CCCD
- `src/ClinicApp.DAL/BenhNhanDAL.cs`
  - `TonTaiSDT`, `TonTaiCCCD`
- `src/ClinicApp.GUI/Forms/FrmTaoLuotKham.cs`
  - tạo lượt khám
- `src/ClinicApp.DAL/LuotKhamDAL.cs`
  - gọi `sp_TaoLuotKham`

### Bác Sĩ

- `src/ClinicApp.GUI/Forms/FrmHangDoiKham.cs`
  - bác sĩ nhận bệnh nhân
- `src/ClinicApp.GUI/Forms/FrmKhamBenh.cs`
  - nhập khám và lưu
- `src/ClinicApp.DAL/KhamDAL.cs`
  - hàng đợi, chuyển trạng thái, hoàn tất khám, lấy dữ liệu in phiếu
- `src/ClinicApp.GUI/Forms/FrmInPhieu.cs`
  - preview/in phiếu, danh sách đã khám

### Lịch Sử Và Dashboard

- `src/ClinicApp.GUI/Forms/FrmLichSu.cs`
- `src/ClinicApp.GUI/Forms/FrmDashboard.cs`
- `src/ClinicApp.DAL/ThongKeDAL.cs`

## 13. Câu Trả Lời Nhanh Khi Thầy Hỏi

### Vì sao dùng transaction?

Vì có thao tác ảnh hưởng nhiều bảng hoặc nhiều bước. Tạo lượt khám vừa tăng bộ đếm vừa insert lượt khám. Hoàn tất khám vừa insert chi tiết vừa update trạng thái. Transaction đảm bảo thành công hết hoặc rollback hết.

### Vì sao cần bảng `BoDemSoThuTu`?

Để sinh số thứ tự theo ngày an toàn. Nếu chỉ đếm số dòng trong `LuotKham`, khi nhiều người tạo cùng lúc có thể bị trùng. Bảng `BoDemSoThuTu` giữ số cuối mỗi ngày và được khóa khi tăng số.

### Vì sao không cho GUI viết SQL?

Để tách rõ trách nhiệm. GUI chỉ hiển thị và nhận input. BLL xử lý nghiệp vụ. DAL mới truy vấn DB. Cách này dễ bảo trì, dễ chia việc và tránh lỗi khi tích hợp.

### Nếu lưu khám 2 lần thì sao?

Lần đầu `sp_HoanTatKham` insert `ChiTietKham` và chuyển sang `DaKham`. Lần sau không còn trạng thái `DangKham` hoặc đã có `ChiTietKham`, nên stored procedure rollback và trả thất bại.

### Nếu hai bác sĩ cùng nhận một lượt thì sao?

Lệnh update có điều kiện `WHERE TrangThai = 'DangCho'`. Bác sĩ đầu tiên update thành công. Bác sĩ thứ hai update 0 dòng và GUI báo lượt khám không còn hợp lệ.

### Nếu trùng SĐT hoặc CCCD thì sao?

BLL kiểm tra trước bằng `TonTaiSDT` và `TonTaiCCCD`. Database cũng có unique index để bảo vệ dữ liệu.

## 14. Kết Luận

Qua bài này, nhóm em đã xây dựng được một hệ thống quản lý khám bệnh mini theo đúng luồng nghiệp vụ cơ bản. Với vai trò nhóm trưởng, em tập trung vào việc chốt kiến trúc, chia việc, thiết kế database, làm GUI, tích hợp và kiểm thử.

Hiện tại hệ thống đã chạy được luồng end-to-end:

```text
Login -> Tiếp nhận -> Tạo lượt -> Khám -> In phiếu -> Lịch sử -> Dashboard
```

Phần em muốn nhấn mạnh khi báo cáo là: nhóm không chỉ làm giao diện, mà có xử lý nghiệp vụ và database tương đối chặt, như phân quyền, hash mật khẩu, sinh số thứ tự an toàn, transaction, ràng buộc trạng thái và dữ liệu demo.
