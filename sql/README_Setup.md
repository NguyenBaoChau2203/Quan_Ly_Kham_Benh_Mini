# SQL Setup Guide

Huong dan nay dung cho Chau khi chuan bi demo tren may moi.

## 1) Tao database tu script

1. Mo SQL Server Management Studio.
2. Ket noi vao SQL Server instance se dung de demo.
3. Mo `sql/Setup.sql`.
4. Chay toan bo script.
5. Xac nhan database `ClinicAppDB` duoc tao thanh cong.

Script se drop va tao lai `ClinicAppDB`, nen chi chay tren moi truong demo/dev, khong chay tren database co du lieu can giu.

## 2) Cau hinh app

Mo `src/ClinicApp.GUI/App.config` va sua connection string:

```xml
<add name="ClinicAppDB"
     connectionString="Server=.;Database=ClinicAppDB;Trusted_Connection=True;TrustServerCertificate=True;"
     providerName="Microsoft.Data.SqlClient" />
```

Neu may demo dung SQL Express, server thuong la:

```text
.\SQLEXPRESS
```

Neu dung instance ten khac, thay `Server=.` bang ten instance do.

## 3) Kiem tra tai khoan demo

Sau khi chay `Setup.sql`, app phai login duoc bang:

- Tiep nhan: `tiepnhan` / `123`
- Bac si: `bacsi` / `123`

Hash SHA256 cua password `123` phai la:

```text
a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3
```

## 4) Tao backup truoc demo

Neu SQL Server/SSMS co quyen backup:

1. Right-click database `ClinicAppDB`.
2. Chon `Tasks > Back Up...`.
3. Backup type: `Full`.
4. Luu file vao `sql/backup/ClinicAppDB.bak`.

File `.bak` trong `sql/backup/` duoc phep commit theo `.gitignore`, nhung chi commit khi file backup that su can cho demo va dung luong chap nhan duoc.

## 5) Cấu hình SQL Authentication (Nếu không dùng Trusted_Connection)

Nếu môi trường không hỗ trợ Windows Authentication (Trusted_Connection) hoặc cần sử dụng tài khoản SQL Login (ví dụ `sa`):

1. Mở SQL Server Management Studio.
2. Đảm bảo Server Authentication mode được đặt là **SQL Server and Windows Authentication mode**.
3. Sửa chuỗi kết nối trong `App.config` thành:
```xml
<add name="ClinicAppDB"

     connectionString="Server=.;Database=ClinicAppDB;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
     providerName="Microsoft.Data.SqlClient" />
```

## 6) Lỗi hay gặp & Môi trường Remote

- **Lỗi Drop Database:** Nếu chạy `Setup.sql` báo lỗi không thể drop do database đang được sử dụng (in use), hãy chạy thử lệnh `ALTER DATABASE ClinicAppDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;` thủ công, hoặc khởi động lại dịch vụ SQL Server để ngắt các kết nối đang treo.
- **App báo không tìm thấy connection string:** Kiểm tra `App.config` có thuộc tính `name="ClinicAppDB"`.
- **Login fail cả hai tài khoản demo:** Kiểm tra đã chạy đúng `Setup.sql` và password hash trong bảng `NhanVien`.
- **Không kết nối SQL Server (Local):** Kiểm tra SQL Server đang chạy, instance name đúng, và user Windows có quyền truy cập database.
- **Kết nối SQL Server từ xa (Remote):** Nếu bắt buộc cài CSDL và chạy App trên 2 máy khác nhau, bạn **phải** cấu hình SQL Server:
  1. Mở `SQL Server Configuration Manager`.
  2. Bật (Enable) giao thức **TCP/IP** trong phần *SQL Server Network Configuration*.
  3. Khởi động lại dịch vụ SQL Server.
  4. Mở Port 1433 (mặc định) trên tường lửa (Firewall) của máy chủ CSDL.
  5. Đảm bảo sử dụng *SQL Authentication* (xem mục 5) vì Windows Auth thường không hoạt động tốt qua mạng workgroup.
