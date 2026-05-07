# Task: Chau Agent Runbook

Tai lieu nay danh cho Chau khi dung AI agent khac de doc repo va code phan cua Chau.
Muc tieu la giao viec ro, tranh agent sua lan sang phan cua Du/Hung, va giu dung contract trong `docs/Master_Plan.md`.

## 0) Trang thai hien tai cua repo

- Branch lam viec cua Chau: `codex-chau-lead-work`.
- Solution: WinForms + SQL Server, .NET 10.
- Kien truc bat buoc: `GUI -> BLL -> DAL -> DataProvider`.
- `sql/Setup.sql` da co schema, constraint, demo data, `sp_TaoLuotKham`, `sp_HoanTatKham`.
- `DataProvider.cs` da co helper query/non-query/SP va log loi.
- DTO va BLL skeleton da co.
- DAL phan lon van la skeleton tra `null`, `false`, hoac `DataTable` rong.
- GUI hien dang la placeholder form; day la phan Chau can day manh nhat.

## 1) Thu tu bat buoc cho moi agent truoc khi code

Moi agent phai doc theo thu tu nay, roi moi duoc sua file:

1. `README.md`
2. `docs/Master_Plan.md`
3. `docs/Team_Workflow.md`
4. `docs/tasks/Task_Chau_Lead.md`
5. `docs/tasks/Task_Chau_Agent_Runbook.md`
6. `design/README.md`
7. `design/stitch_mini_clinic_management_system/clinical_precision/DESIGN.md`
8. Cac file code lien quan truc tiep den phase dang lam.

Neu agent thay contract thieu hoac can doi schema/DTO/ten ham, phai dung lai va bao Chau cap nhat docs truoc.

Truoc moi phase, neu worktree dang sach va phase truoc da build pass, tao checkpoint commit nho voi message `checkpoint: pre-phase-N`. Neu phase lam hong build, khong dung `git reset --hard` khi chua co Chau/lead dong y; bao ro diff va loi de quyet dinh rollback an toan.

## 2) Pham vi so huu cua Chau

Chau duoc sua:

- `src/ClinicApp.GUI/`
- `src/ClinicApp.DTO/`
- `src/ClinicApp.DAL/DataProvider.cs`
- `src/ClinicApp.DAL/AuthDAL.cs`
- `src/ClinicApp.DAL/ThongKeDAL.cs`
- `src/ClinicApp.BLL/AuthBLL.cs`
- `src/ClinicApp.BLL/ThongKeBLL.cs`
- `sql/Setup.sql`
- `docs/`

Chau chi nen sua cac file duoi day khi dang tich hop cuoi hoac can unblock demo:

- `src/ClinicApp.DAL/BenhNhanDAL.cs`
- `src/ClinicApp.DAL/LuotKhamDAL.cs`
- `src/ClinicApp.DAL/KhamDAL.cs`
- `src/ClinicApp.BLL/BenhNhanBLL.cs`
- `src/ClinicApp.BLL/LuotKhamBLL.cs`
- `src/ClinicApp.BLL/KhamBLL.cs`

Mac dinh khong sua phan cua Du/Hung neu chua can tich hop.

Trong Phase 3-5, agent khong implement BLL/DAL cua Du/Hung (`BenhNhanDAL`, `BenhNhanBLL`, `LuotKhamDAL`, `LuotKhamBLL`, `KhamDAL`, `KhamBLL`). GUI chi goi BLL theo contract va handle `null/false/DataTable` rong. Cac file do chi duoc sua trong Phase 6 khi tich hop cuoi hoac khi Chau giao ro.

## 3) Chien luoc dung model

Dung model theo viec, khong dung model manh cho viec nho:

| Viec | Model nen dung | Ly do |
| --- | --- | --- |
| Chot architecture, review contract, merge cuoi | Codex GPT-5.5 | Manh ve codebase reasoning va sua nhieu file co kiem soat |
| Doc tong the repo, lap ke hoach, doi chieu docs/design | Gemini 3.1 Pro high | Tot cho long context va tong hop nhieu tai lieu |
| Review kho, bat bug nghiep vu/transaction/concurrency | Claude Opus | Dung it quota, chi dung cho checkpoint quan trong |
| Code UI WinForms tung form | Claude Sonnet hoac Codex GPT-5.5 | Sonnet hop tac tot voi UI; Codex tot khi can edit file truc tiep |
| Code DAL/BLL SQL lap lai, query, mapping DTO | DeepSeek v4 Pro trong OpenCode | Hieu qua cho task code cu the, it ton quota cao |
| Autocomplete, sua nho trong IDE, refactor cuc bo | GitHub Copilot GPT-5.2 / GPT-Codex 5.2 | Chi dung nhu tro ly trong file, khong giao quyet dinh architecture |

Quy tac quota:

- Claude Opus chi dung cho 3 lan: review `Setup.sql`, review luong login/status atomic, review truoc demo.
- Codex GPT-5.5 lam agent chinh khi can sua nhieu file va build.
- DeepSeek lam worker cho DAL/BLL co prompt that chat va file scope ro.
- Copilot khong nen tu viet feature lon; dung de go nhanh control/event handler nho.

## 4) Phase 0 - Baseline va guardrail

Muc tieu: dam bao agent biet hien trang truoc khi code.

Viec can lam:

- Chay `git status --short --branch`.
- Chay `dotnet build src\ClinicApp.sln`.
- Verify project references: `ClinicApp.GUI.csproj` ref `BLL`, `BLL.csproj` ref `DAL` + `DTO`, `DAL.csproj` ref `DTO`.
- Doc danh sach placeholder GUI bang `rg -n "PlaceholderForm" src\ClinicApp.GUI`.
- Doc danh sach DAL skeleton bang `rg -n "return null|return false|new DataTable" src\ClinicApp.DAL src\ClinicApp.BLL`.
- Khong sua code neu build baseline fail do loi moi khong lien quan; ghi lai loi cho Chau.

Done khi:

- Biet build baseline pass/fail.
- Lap danh sach file can lam trong phase tiep theo.

Model nen dung: Gemini 3.1 Pro high de doc/tong hop, hoac Codex GPT-5.5 neu muon chay lenh va sua ngay.

## 5) Phase 1 - Database, config, va Auth

Muc tieu: login demo phai chay that voi SHA256 va role.

File chinh:

- `sql/Setup.sql`
- `src/ClinicApp.GUI/App.config`
- `src/ClinicApp.GUI/Program.cs`
- `src/ClinicApp.BLL/AuthBLL.cs`
- `src/ClinicApp.DAL/AuthDAL.cs`
- `src/ClinicApp.DTO/NhanVienDTO.cs`

Viec can lam:

- Review `Setup.sql` de dam bao DB name la `ClinicAppDB`.
- Dam bao `Program.cs` doc `ConfigurationManager.ConnectionStrings["ClinicAppDB"]` va gan vao `DataProvider.Instance.ConnectionString` truoc `Application.Run`.
- Neu thieu package `System.Configuration.ConfigurationManager`, them package do vao `ClinicApp.GUI.csproj`.
- Dam bao user demo la `tiepnhan/123` va `bacsi/123`, password hash SHA256 dung.
- Hash SHA256 phai la lowercase hex. Verify `SHA256("123") = a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3`.
- Implement `AuthDAL.DangNhap(username, passwordSha256)`.
- Mapping DB `VaiTro` sang DTO `Role`.
- Khong show SQL exception ra GUI; DAL return `null` khi fail.
- Neu connection string sai, GUI bao loi cau hinh than thien.
- Khi goi stored procedure co OUTPUT parameter, khong dua vao return value cua `ExecuteNonQuerySP` vi SP dung `SET NOCOUNT ON` co the tra `-1`. Check OUTPUT parameter: `@MaLK > 0` cho `sp_TaoLuotKham`, `@KetQua == 1` cho `sp_HoanTatKham`.
- Verify `sp_HoanTatKham` dat guard `TrangThai='DangKham'` va double-save trong transaction, co lock `WITH (UPDLOCK, HOLDLOCK)` tren `LuotKham`.

Test toi thieu:

- Login dung `tiepnhan/123` tra role `TiepNhan`.
- Login dung `bacsi/123` tra role `BacSi`.
- Login sai tra `null`, GUI khong crash.

Model nen dung:

- Codex GPT-5.5 hoac Claude Sonnet cho `AuthDAL` de establish pattern chuan cho cac DAL sau.
- Codex GPT-5.5 de wire vao GUI va build.
- Claude Opus neu can review hash/security truoc demo.

## 6) Phase 2 - UI foundation WinForms

Muc tieu: thay placeholder bang shell UI dung design, de cac form sau lap lai it code.

File chinh:

- `src/ClinicApp.GUI/Forms/FrmLogin.cs`
- `src/ClinicApp.GUI/Forms/FrmMain.cs`
- `src/ClinicApp.GUI/Forms/PlaceholderForm.cs` hoac thay bang base/helper rieng.
- Co the them helper trong `src/ClinicApp.GUI/` neu can, vi du `UiTheme`, `UiFormat`, `NavigationButton`.

Viec can lam:

- Tao theme dung `clinical_precision/DESIGN.md`: Segoe UI, mau xanh `#005596`, background sang, grid compact.
- `FrmLogin`: username/password, nut dang nhap, Enter de submit, message loi than thien.
- `FrmMain`: top bar, left navigation, vung content, logout.
- Phan quyen menu:
  - Role `TiepNhan`: Benh nhan, Tao luot kham, Lich su, Dashboard.
  - Role `BacSi`: Hang doi, Kham benh, In phieu, Lich su, Dashboard.
- Khong de menu ngoai v1 mo form chua lam; neu co thi disable hoac bao "Chuc nang nay chua trien khai trong phien ban demo."
- Dung Dock/Anchor de form scale duoc 1366x768.

Done khi:

- App mo vao `FrmLogin`.
- Dang nhap thanh cong mo `FrmMain`.
- Logout quay lai login.
- Menu role an/hien dung.

Model nen dung:

- Claude Sonnet cho layout tung form.
- Codex GPT-5.5 de tao helper UI va sua nhieu form.
- Copilot chi dung autocomplete event handler nho.

## 7) Phase 3 - GUI tiep nhan cho Chau

Muc tieu: GUI cho reception chay duoc voi BLL contract, ke ca khi DAL cua Du chua xong.

File chinh:

- `src/ClinicApp.GUI/Forms/FrmBenhNhan.cs`
- `src/ClinicApp.GUI/Forms/FrmTaoLuotKham.cs`

Viec can lam:

- `FrmBenhNhan`: search box, grid, form them/sua BN.
- Validate GUI:
  - Ho ten bat buoc.
  - SDT bat buoc.
  - CCCD neu nhap thi dung 12 chu so.
- `FrmTaoLuotKham`: chon BN, tao luot, hien `SoThuTu`, cho huy neu `DangCho`.
- GUI khong viet SQL; chi goi BLL.
- Khong implement `BenhNhanBLL`, `LuotKhamBLL`, `BenhNhanDAL`, `LuotKhamDAL` trong phase nay.
- Neu BLL tra `null/false`, show message dung trong `Master_Plan.md`.
- Grid bind theo ten cot contract, khong doan ten cot moi.

Done khi:

- Khi DAL co data, GUI search/add/edit/create visit/huy co the dung ngay.
- Khi DAL chua xong, GUI khong crash voi `DataTable` rong.

Model nen dung:

- Claude Sonnet cho UI.
- Codex GPT-5.5 de wire BLL, build, sua loi.

## 8) Phase 4 - GUI bac si va in phieu

Muc tieu: luong bac si demo duoc: hang doi -> bat dau kham -> luu -> in/preview.

File chinh:

- `src/ClinicApp.GUI/Forms/FrmHangDoiKham.cs`
- `src/ClinicApp.GUI/Forms/FrmKhamBenh.cs`
- `src/ClinicApp.GUI/Forms/FrmInPhieu.cs`

Viec can lam:

- `FrmHangDoiKham`: grid chi hien `DangCho`, nut refresh, nut bat dau kham.
- Khi refresh, giu focus theo `MaLK`, khong theo index.
- `FrmKhamBenh`: hien thong tin BN/luot, nhap trieu chung, chan doan, toa thuoc, loi dan.
- Nut luu phai disable sau click dau tien de tranh double-save.
- Neu `HoanTatKham` false, reload va bao loi than thien.
- `FrmInPhieu`: render noi dung phieu tren man hinh; demo khong phu thuoc may in that.
- `FrmInPhieu` can cac cot: `MaLK`, `SoThuTu`, `NgayKham`, `MaBN`, `HoTen`, `NgaySinh`, `GioiTinh`, `TenBacSi`, `GhiChu`, `TrieuChung`, `ChanDoan`, `ToaThuoc`, `LoiDan`.
- Khong implement `KhamBLL` hoac `KhamDAL` trong phase nay.

Done khi:

- Khong the 2 lan luu lam crash.
- Luu thanh cong xong co the xem phieu.
- Nut quay lai hang doi hoat dong.

Model nen dung:

- Claude Sonnet cho UI.
- Codex GPT-5.5 cho flow state va integration.
- Claude Opus review logic status truoc demo neu con quota.

## 9) Phase 5 - History va Dashboard cua Chau

Muc tieu: phan sau kham co du data demo de giao vien thay app hoan chinh.

File chinh:

- `src/ClinicApp.DAL/ThongKeDAL.cs`
- `src/ClinicApp.BLL/ThongKeBLL.cs`
- `src/ClinicApp.GUI/Forms/FrmLichSu.cs`
- `src/ClinicApp.GUI/Forms/FrmDashboard.cs`

Viec can lam:

- Implement `LayLichSuKham(fromDate, toDate, keyword)`.
- Implement `LayThongKe7Ngay()`.
- Query `LayThongKe7Ngay()` nen tao day 7 ngay gan nhat bang CTE/table expression, `LEFT JOIN` voi `LuotKham`, group theo `NgayKhamDate`, va tra dung cot `Ngay`, `SoLuot`, `SoDaKham`, `SoDangCho` ke ca ngay khong co luot.
- `FrmLichSu`: filter tu ngay/den ngay/keyword, grid cot dung contract.
- `FrmDashboard`: so luot 7 ngay, so da kham, so dang cho; simple la du.
- Khong lam chart phuc tap neu luong core chua on.

Done khi:

- Sau khi setup DB, history va dashboard khong bi trong.
- Filter khong crash khi keyword rong.

Model nen dung:

- DeepSeek v4 Pro cho SQL trong `ThongKeDAL`.
- Codex GPT-5.5 cho GUI va build.

## 10) Phase 6 - Tich hop DAL/BLL cua Du va Hung

Muc tieu: ghep logic cua hai ban vao GUI Chau ma khong vo contract.

Viec can lam:

- Pull/merge code cua Du/Hung vao branch tich hop rieng neu can.
- Chay build ngay sau merge.
- Test tung contract:
  - `TimBenhNhan`
  - `ThemBenhNhan`
  - `CapNhatBenhNhan`
  - `TaoLuotKham`
  - `HuyLuotKham`
  - `LayHangDoiDangCho`
  - `ChuyenSangDangKham`
  - `HoanTatKham`
  - `ChuyenVeDangCho`
  - `LayDuLieuInPhieu`
- Neu DataTable thieu cot, khong sua GUI doan cot moi; yeu cau sua DAL hoac cap nhat contract.

Done khi:

- Demo end-to-end chay du 12 buoc trong `docs/Master_Plan.md`.

Model nen dung:

- Codex GPT-5.5 lam chinh.
- Claude Opus review truoc khi nop demo neu con quota.

## 11) Phase 7 - Demo hardening

Muc tieu: app khong vo trong buoi demo.

Checklist:

- `dotnet build src\ClinicApp.sln` pass.
- Chay `sql/Setup.sql` tren SQL Server sach.
- Sua `App.config` dung server demo.
- Test login `tiepnhan/123`.
- Them hoac tim benh nhan.
- Tao luot kham va thay STT.
- Tao luot kham moi roi huy khi con `DangCho`, verify trang thai `DaHuy`.
- Login `bacsi/123`.
- Bat dau kham mot luot `DangCho`.
- Luu kham, bam luu lan 2 khong crash.
- Xem phieu.
- Kiem tra lich su co dong moi.
- Dashboard co so lieu 7 ngay.
- Tao `sql/backup/ClinicAppDB.bak` truoc ngay demo neu may co SQL Server Management Studio/backup permission.
- Doc va cap nhat `sql/README_Setup.md` neu server demo khac template.
- Mo `%LocalAppData%\ClinicApp\error.log` neu co loi va ghi lai.

Model nen dung:

- Codex GPT-5.5 cho fix cuoi.
- Claude Opus cho review nhanh cac bug co nguy co lam fail demo.

## 12) Prompt mau de giao cho agent

Dung prompt nay khi giao cho agent khac:

```text
Ban dang lam repo Quan_Ly_Kham_Benh_Mini tren branch codex-chau-lead-work.
Hay doc theo thu tu:
README.md,
docs/Master_Plan.md,
docs/Team_Workflow.md,
docs/tasks/Task_Chau_Lead.md,
docs/tasks/Task_Chau_Agent_Runbook.md,
va cac file lien quan den phase duoc giao.
Neu lam phase UI, doc them: design/stitch_mini_clinic_management_system/clinical_precision/DESIGN.md.

Pham vi cua ban chi la: <dien file/phase>.
Khong sua file ngoai pham vi neu khong can thiet.
Khong doi schema/DTO/contract neu chua bao Chau.
Khong implement BLL/DAL cua Du/Hung trong phase GUI neu khong duoc giao ro.
Kien truc bat buoc: GUI -> BLL -> DAL -> DataProvider; GUI khong viet SQL.
Truoc khi bat dau phase, neu worktree sach, commit checkpoint: git commit -m "checkpoint: pre-phase-X".
Sau khi sua, chay dotnet build src\ClinicApp.sln va bao ro file da sua, test da chay, loi con lai.
```

Prompt rieng cho UI:

```text
Hay implement phase UI WinForms theo design/stitch_mini_clinic_management_system/clinical_precision/DESIGN.md.
Dung Segoe UI, layout compact, DataGridView ro rang, Dock/Anchor de scale.
Chi goi BLL, khong viet SQL trong GUI.
Neu BLL tra null/false/DataTable rong, GUI phai hien message than thien va khong crash.
```

Prompt rieng cho DAL:

```text
Hay implement DAL theo contract trong docs/Master_Plan.md.
Tat ca SQL di qua DataProvider.
DataTable khong tra null.
DTO tra null khi khong tim thay/fail.
bool chi true khi RowsAffected == 1 hoac output SP bao thanh cong.
Khi goi SP co `SET NOCOUNT ON`, khong check return value cua `ExecuteNonQuerySP`; check OUTPUT param.
Khong throw exception ra GUI.
```

## 13) Viec khong lam trong v1

- Khong them module lich hen, kho duoc, hoa don, BHYT, ICD-10, CLS.
- Khong them bang/cot moi chi vi mockup co field minh hoa.
- Khong lam dashboard nang cao khi core flow chua on.
- Khong sua `.Designer.cs` bang tay.
- Khong commit `bin/`, `obj/`, `.vs/`, file `.user`, log, DB local.

## 14) Definition of Done cua Chau

Chau xong khi:

- `sql/Setup.sql` tao DB sach thanh cong.
- App build pass.
- Login role dung.
- GUI khong con placeholder cho luong demo chinh.
- Reception flow chay: tim/them/sua BN -> tao luot -> hien STT -> huy neu can.
- Doctor flow chay: hang doi -> bat dau kham -> luu -> in/preview.
- History va dashboard co du lieu.
- Loi SQL khong lo thang ra GUI.
- Demo end-to-end chay duoc tren may khac chi bang cach sua `App.config`.
