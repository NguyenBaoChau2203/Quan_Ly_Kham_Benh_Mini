# /src

Thư mục này chứa Visual Studio Solution và 4 projects:

- `ClinicApp.DTO`
- `ClinicApp.DAL`
- `ClinicApp.BLL`
- `ClinicApp.GUI`

Solution chính:

- `ClinicApp.sln`

Target framework hiện tại:

- Class library: `net10.0`
- WinForms GUI: `net10.0-windows`

Reference project:

- `ClinicApp.DAL` tham chiếu `ClinicApp.DTO`
- `ClinicApp.BLL` tham chiếu `ClinicApp.DAL` và `ClinicApp.DTO`
- `ClinicApp.GUI` tham chiếu `ClinicApp.BLL` và `ClinicApp.DTO`

Quy ước:

- Không commit `bin/`, `obj/`, `.vs/`.
- Không sửa file `.Designer.cs` bằng tay.
- Châu sở hữu `ClinicApp.GUI`; Dư/Hùng không chỉnh GUI nếu chưa được yêu cầu.
