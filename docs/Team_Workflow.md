# Team Workflow: Đọc Gì Và Làm Gì

Tài liệu này dành cho cả nhóm trước khi bắt đầu code. Mục tiêu là để mỗi bạn biết cần đọc gì, làm phần nào, và tránh conflict khi làm song song.

## 1) Thứ Tự Đọc Bắt Buộc

Trước khi code, mỗi thành viên đọc theo thứ tự:

1. `README.md`
   - Nắm cấu trúc repo và các quy tắc không được phá.

2. `docs/Master_Plan.md`
   - Đọc kỹ các mục:
     - `3) Luồng Vận Hành Bắt Buộc`
     - `5) CSDL Chính Thức Và Ràng Buộc`
     - `6) Stored Procedure Bắt Buộc`
     - `8) Hợp Đồng Hàm (Contract)`
     - `10.1) UI Implementation Contract`
     - `12) Demo Và Definition of Done`

3. File task cá nhân:
   - Châu: `docs/tasks/Task_Chau_Lead.md`
   - Dư: `docs/tasks/Task_Du_Reception.md`
   - Hùng: `docs/tasks/Task_Hung_Doctor.md`

4. `design/README.md`
   - Chỉ dùng `design/` để tham khảo layout/màu/control.
   - Dữ liệu trong ảnh mockup không phải nghiệp vụ bắt buộc.

## 2) Khung Solution Chung

Khung solution đã được scaffold trong `src/`:

```text
src/
  ClinicApp.sln
  ClinicApp.DTO/
  ClinicApp.DAL/
  ClinicApp.BLL/
  ClinicApp.GUI/
```

Target framework hiện tại:

- `ClinicApp.DTO`, `ClinicApp.DAL`, `ClinicApp.BLL`: `net10.0`
- `ClinicApp.GUI`: `net10.0-windows`

Reference project hiện tại:

- Add reference project đúng chiều: `GUI -> BLL`, `BLL -> DAL`, `BLL/DAL/GUI -> DTO`; DAL dùng `DataProvider`.
- DTO tối thiểu đã được tạo theo `Master_Plan.md`.
- Class BLL/DAL skeleton đã có sẵn theo contract để Dư/Hùng implement.
- GUI đã có form placeholder đúng tên; Châu sẽ thay dần bằng layout thật theo `design/`.

## 3) Phân Vùng Sở Hữu File

### Châu

- Sở hữu chính:
  - `src/ClinicApp.GUI/`
  - `src/ClinicApp.DTO/`
  - `src/ClinicApp.DAL/DataProvider.cs`
  - `sql/Setup.sql`
  - `docs/Master_Plan.md`
- Chốt schema, DTO, contract, stored procedure, giao diện, tích hợp cuối.

### Dư

- Sở hữu logic tiếp nhận:
  - BLL/DAL tìm, thêm, sửa bệnh nhân.
  - BLL/DAL tạo lượt khám.
  - BLL/DAL hủy lượt khám nếu làm.
- Không chỉnh `ClinicApp.GUI` nếu Châu chưa yêu cầu.
- Không tự đổi schema/DTO/contract.

### Hùng

- Sở hữu logic bác sĩ:
  - BLL/DAL hàng đợi `DangCho`.
  - BLL/DAL bắt đầu khám.
  - BLL/DAL hoàn tất khám.
  - BLL/DAL dữ liệu in phiếu.
  - BLL/DAL quay lại `DangCho` nếu làm.
- Không chỉnh `ClinicApp.GUI` nếu Châu chưa yêu cầu.
- Không tự đổi schema/DTO/contract.

## 4) Quy Tắc Tránh Conflict

- Luôn `git pull` trước khi bắt đầu làm.
- Không chỉnh file `.Designer.cs` bằng tay.
- Không commit `bin/`, `obj/`, `.vs/`, file `.user`, database local, log.
- Không để nhiều người cùng sửa `ClinicApp.GUI` trong cùng thời điểm.
- Không để nhiều người cùng sửa `sql/Setup.sql`; mọi thay đổi DB phải qua Châu.
- Nếu cần đổi DTO, tên hàm, tên cột `DataTable`, trạng thái, stored procedure: báo Châu trước, Châu cập nhật docs rồi mới code.
- Commit nhỏ theo từng phần đã chạy được, tránh gom quá nhiều file không liên quan.

## 5) Quy Tắc Khi Gặp Lỗi Hoặc Cần Đổi Yêu Cầu

Nếu một thành viên gặp lỗi hoặc thấy contract chưa đủ:

1. Ghi rõ hàm/màn hình đang làm.
2. Ghi input đang truyền vào.
3. Ghi output mong muốn.
4. Ghi lỗi thực tế hoặc điểm thiếu.
5. Báo Châu duyệt trước khi đổi code chung.

Không tự đổi tên bảng, cột, DTO, hàm BLL/DAL, trạng thái hoặc stored procedure vì sẽ làm lệch phần của các bạn còn lại.

## 6) Checklist Trước Khi Push

Trước khi push, mỗi bạn tự kiểm:

- Code build được trên máy mình.
- Không có `bin/`, `obj/`, `.vs/`, `.user` trong commit.
- Không sửa nhầm file GUI/SQL/docs ngoài phạm vi của mình.
- Tên hàm đúng theo `Master_Plan.md`.
- GUI không gọi SQL trực tiếp.
- BLL/DAL không throw lỗi thô ra GUI; trả `null/false/DataTable rỗng` theo contract.

## 7) Khi Nào Được Xem Là Xong Phần Mình

Dư xong khi:

- Tìm bệnh nhân theo tên/SDT/CCCD chạy đúng.
- Thêm/sửa bệnh nhân validate đúng.
- Tạo lượt khám gọi đúng `sp_TaoLuotKham`, trả được `MaLK` và `SoThuTu`.
- Hủy lượt khám chỉ cho `DangCho -> DaHuy` nếu có làm.

Hùng xong khi:

- Hàng đợi chỉ load `DangCho`.
- Bắt đầu khám chỉ cho `DangCho -> DangKham`.
- Hoàn tất khám chỉ chạy khi `DangKham`, chống double-save.
- Dữ liệu in phiếu trả đủ cột cho GUI.

Châu xong khi:

- DB setup được từ `sql/Setup.sql`.
- GUI chạy đúng luồng demo.
- Login phân quyền đúng.
- Tích hợp được BLL/DAL của Dư và Hùng.
- Demo end-to-end: Login -> Tiếp nhận -> Tạo lượt -> Khám -> In -> Lịch sử -> Dashboard.
