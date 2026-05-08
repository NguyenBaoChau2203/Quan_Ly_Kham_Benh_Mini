# WebView2 Hybrid Migration — Implementation Plan

## Goal
Replace all problematic native WinForms UI controls (TextBox, Button, DateTimePicker, TableLayoutPanel) with **WebView2-rendered HTML/CSS** from the existing design files. Keep the WinForms shell (FrmMain top bar + navigation) and the BLL/DAL layers 100% unchanged.

## Architecture

```
┌──────────────────────────────────────────────────────┐
│  FrmMain.cs (WinForms) — KEEP AS-IS                  │
│  ┌──────────┐  ┌──────────────────────────────────┐  │
│  │ Nav Panel │  │  _contentPanel (Panel)            │  │
│  │ (WinForms)│  │  ┌──────────────────────────────┐│  │
│  │           │  │  │  FrmXxx : WebFormBase         ││  │
│  │ BenhNhan  │  │  │  ┌──────────────────────────┐││  │
│  │ TaoLuot   │  │  │  │ WebView2 (Dock=Fill)     │││  │
│  │ LichSu    │  │  │  │ HTML/CSS/JS from design  │││  │
│  │ Dashboard │  │  │  │                          │││  │
│  │ etc.      │  │  │  │ JS ←→ ClinicBridge (C#)  │││  │
│  │           │  │  │  └──────────────────────────┘││  │
│  │           │  │  └──────────────────────────────┘│  │
│  └──────────┘  └──────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

**JS → C# flow:**
```
User clicks "Tìm kiếm" in HTML
  → JS: window.chrome.webview.hostObjects.bridge.TimBenhNhan("keyword")
  → C# ClinicBridge.TimBenhNhan("keyword")
  → BenhNhanBLL.TimBenhNhan("keyword")
  → Returns JSON string
  → JS parses JSON, renders HTML table
```

---

## Files to Create/Modify

### [NEW] Files to Create

---

#### 1. `src/ClinicApp.GUI/WebFormBase.cs` — Base class for all WebView2 forms

**Purpose:** Reusable base class that each form inherits. Handles WebView2 init, bridge registration, and HTML loading.

```csharp
// Key structure:
public class WebFormBase : Form
{
    protected WebView2 _webView;
    protected ClinicBridge _bridge;
    private bool _webViewReady;

    public WebFormBase(string title)
    {
        Text = title;
        UiTheme.ApplyForm(this);
        BackColor = UiTheme.Background;
        
        _webView = new WebView2 { Dock = DockStyle.Fill };
        Controls.Add(_webView);
        
        Load += async (_, _) =>
        {
            await _webView.EnsureCoreWebView2Async(null);
            _webViewReady = true;
            
            // Register bridge
            _bridge = CreateBridge();
            _webView.CoreWebView2.AddHostObjectToScript("bridge", _bridge);
            
            // Load HTML
            _webView.CoreWebView2.NavigateToString(GetHtml());
        };
    }

    protected virtual ClinicBridge CreateBridge() => new ClinicBridge();
    protected abstract string GetHtml();
}
```

> [!IMPORTANT]
> `AddHostObjectToScript` requires the bridge class to be **COM-visible**. Must add `[ClassInterface(ClassInterfaceType.AutoDual)]` and `[ComVisible(true)]`.

---

#### 2. `src/ClinicApp.GUI/ClinicBridge.cs` — C# ↔ JS Interop Bridge

**Purpose:** Single class that exposes ALL BLL methods to JavaScript via WebView2's host object protocol.

```csharp
[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class ClinicBridge
{
    // === BenhNhanBLL ===
    public string TimBenhNhan(string keyword);           // Returns JSON array
    public string LayBenhNhanTheoMa(int maBN);          // Returns JSON object
    public string ThemBenhNhan(string jsonDto);          // Returns "true"/"false"
    public string CapNhatBenhNhan(string jsonDto);       // Returns "true"/"false"
    
    // === LuotKhamBLL ===
    public string TaoLuotKham(int maBN, string ghiChu); // Returns JSON LuotKhamDTO or "null"
    public string HuyLuotKham(int maLK);                // Returns "true"/"false"
    
    // === KhamBLL ===
    public string LayHangDoiDangCho();                  // Returns JSON array
    public string ChuyenSangDangKham(int maLK, int maBacSi); // Returns "true"/"false"
    public string HoanTatKham(string jsonChiTiet);      // Returns "true"/"false"
    public string ChuyenVeDangCho(int maLK);            // Returns "true"/"false"
    public string LayDuLieuInPhieu(int maLK);           // Returns JSON array
    
    // === ThongKeBLL ===
    public string LayLichSuKham(string fromDate, string toDate, string keyword); // Returns JSON array
    public string LayThongKe7Ngay();                    // Returns JSON array
    
    // === AuthBLL ===  
    public string DangNhap(string username, string password); // Returns JSON NhanVienDTO or "null"
}
```

**DataTable → JSON conversion:** Use `Newtonsoft.Json` or manual conversion:
```csharp
private static string DataTableToJson(DataTable dt)
{
    var rows = new List<Dictionary<string, object>>();
    foreach (DataRow row in dt.Rows)
    {
        var dict = new Dictionary<string, object>();
        foreach (DataColumn col in dt.Columns)
            dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
        rows.Add(dict);
    }
    return System.Text.Json.JsonSerializer.Serialize(rows);
}
```

> [!WARNING]  
> `System.Text.Json` is available in .NET 6+. If using `Newtonsoft.Json`, add the NuGet package. Check current `.csproj` target framework.

---

#### 3. HTML Template Files — One per form

Store in `src/ClinicApp.GUI/HtmlTemplates/` as embedded resources OR inline as C# string constants.

**Recommended approach:** Inline C# strings (simpler, no file I/O, consistent with FrmInPhieu pattern).

Each form needs ONE HTML string method that:
1. Takes the Tailwind config from the design files
2. Includes the form-specific body HTML  
3. **Removes** the sidebar and top bar (FrmMain handles those)
4. Adds JavaScript that calls `window.chrome.webview.hostObjects.bridge.XXX()`

---

### [MODIFY] Forms to Rewrite

Each form below inherits `WebFormBase` and overrides `GetHtml()`.

---

#### 4. `FrmBenhNhan.cs` — Patient Management

**Current BLL calls to preserve:**
| Method | Signature | Returns |
|--------|-----------|---------|
| `TimBenhNhan` | `BenhNhanBLL.TimBenhNhan(string?)` | `DataTable` (MaBN, HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi, TrangThaiGanNhat) |
| `ThemBenhNhan` | `BenhNhanBLL.ThemBenhNhan(BenhNhanDTO)` | `bool` |
| `CapNhatBenhNhan` | `BenhNhanBLL.CapNhatBenhNhan(BenhNhanDTO)` | `bool` |

**HTML source:** `design/3._reception_patients/code.html`

**JS interop needed:**
```javascript
// Search
async function searchPatients() {
    const keyword = document.getElementById('txtSearch').value;
    const json = await bridge.TimBenhNhan(keyword);
    const patients = JSON.parse(json);
    renderTable(patients);
}

// Save
async function savePatient() {
    const dto = JSON.stringify({ MaBN: currentMaBN, HoTen: ..., SDT: ..., ... });
    const isNew = currentMaBN <= 0;
    const result = isNew 
        ? await bridge.ThemBenhNhan(dto)
        : await bridge.CapNhatBenhNhan(dto);
    if (result === 'true') { alert('Thành công!'); searchPatients(); }
}
```

**Validation:** Keep the same rules in JS:
- HoTen: required
- SDT: required, regex `^0\d{9,10}$`
- CCCD: optional, exactly 12 digits

---

#### 5. `FrmTaoLuotKham.cs` — Create Visit

**Current BLL calls:**
| Method | Signature | Returns |
|--------|-----------|---------|
| `TimBenhNhan` | `BenhNhanBLL.TimBenhNhan(string?)` | `DataTable` |
| `TaoLuotKham` | `LuotKhamBLL.TaoLuotKham(int, int?, string?)` | `LuotKhamDTO?` |
| `HuyLuotKham` | `LuotKhamBLL.HuyLuotKham(int)` | `bool` |

**HTML source:** `design/4._reception_create_visit/code.html`

**JS state:** Maintain `selectedMaBN` and `localQueue[]` in JS memory (matching current `_dtLuotKhamLocal` DataTable).

---

#### 6. `FrmLichSu.cs` — History

**Current BLL calls:**
| Method | Signature | Returns |
|--------|-----------|---------|
| `LayLichSuKham` | `ThongKeBLL.LayLichSuKham(DateTime, DateTime, string?)` | `DataTable` (MaLK, NgayKham, MaBN, HoTen, TenBacSi, ChanDoan, TrangThai) |

**HTML source:** `design/8._history/code.html`

**JS interop:** Pass dates as ISO strings `"2024-01-01"` → bridge parses with `DateTime.Parse`.

---

#### 7. `FrmDashboard.cs` — Dashboard

**Current BLL calls:**
| Method | Signature | Returns |
|--------|-----------|---------|
| `LayThongKe7Ngay` | `ThongKeBLL.LayThongKe7Ngay()` | `DataTable` (Ngay, SoLuot, SoDaKham, SoDangCho) |

**HTML source:** `design/9._dashboard/code.html`

---

#### 8. `FrmHangDoiKham.cs` — Doctor Queue

**Current BLL calls:**
| Method | Signature | Returns |
|--------|-----------|---------|
| `LayHangDoiDangCho` | `KhamBLL.LayHangDoiDangCho()` | `DataTable` (MaLK, SoThuTu, HoTen, MaBN, NgayKham, ThoiGianChoPhut, TrangThai) |
| `ChuyenSangDangKham` | `KhamBLL.ChuyenSangDangKham(int, int)` | `bool` |

**HTML source:** `design/5._doctor_patient_queue/code.html`

**Special:** Has `ExamStarted` event. After `ChuyenSangDangKham` succeeds, JS calls `bridge.NotifyExamStarted(maLK)` → C# fires event → FrmMain navigates to FrmKhamBenh.

> [!IMPORTANT]
> `FrmHangDoiKham` has a **custom event** `ExamStarted` that `FrmMain` subscribes to for navigation. The bridge needs a way to fire this event. Use `WebView2.WebMessageReceived`:
> ```csharp
> _webView.CoreWebView2.WebMessageReceived += (s, e) => {
>     var msg = JsonSerializer.Deserialize<BridgeMessage>(e.WebMessageAsJson);
>     if (msg.Action == "ExamStarted") ExamStarted?.Invoke(this, new ExamStartedEventArgs(msg.MaLK));
> };
> ```
> JS side: `window.chrome.webview.postMessage({ action: 'ExamStarted', maLK: 123 });`

**Auto-refresh:** Use `setInterval(() => refreshQueue(), 15000)` in JS instead of WinForms Timer.

---

#### 9. `FrmKhamBenh.cs` — Examination

**Current BLL calls:**
| Method | Signature | Returns |
|--------|-----------|---------|
| `LayDuLieuInPhieu` | `KhamBLL.LayDuLieuInPhieu(int)` | `DataTable` (MaLK, SoThuTu, NgayKham, MaBN, HoTen, NgaySinh, GioiTinh, TenBacSi, GhiChu, TrieuChung, ChanDoan, ToaThuoc, LoiDan) |
| `HoanTatKham` | `KhamBLL.HoanTatKham(ChiTietKhamDTO)` | `bool` |

**HTML source:** `design/6._doctor_examination/code.html`

**Special:** Has tab switching (Chẩn đoán / Điều trị), patient info panel, and opens FrmInPhieu after save.

> [!IMPORTANT]
> `FrmKhamBenh` constructor takes `maLK` and `maBacSi` parameters. The bridge must store these. After `HoanTatKham`, JS calls `bridge.OpenPreview(maLK)` → C# opens `FrmInPhieu` dialog.

---

#### 10. `FrmInPhieu.cs` — Print Preview (MINIMAL CHANGES)

**Already uses WebView2.** Only need:
- Fix the lookup bar alignment (use HTML instead of WinForms toolbar)
- OR keep as-is if user is OK with current FrmInPhieu

---

#### 11. `FrmLogin.cs` — Login Screen

**Current BLL calls:**
| Method | Signature | Returns |
|--------|-----------|---------|
| `DangNhap` | `AuthBLL.DangNhap(string, string)` | `NhanVienDTO?` |

**HTML source:** `design/1._login_screen/code.html`

> [!WARNING]
> FrmLogin is **NOT embedded** in FrmMain. It's a standalone form. The bridge's `DangNhap` must return the user data, and C# must handle `OpenMainShell(user)`. Use `WebMessageReceived` for this:
> ```javascript
> // JS: after successful login
> window.chrome.webview.postMessage({ action: 'LoginSuccess', user: userData });
> ```

---

### [KEEP AS-IS] — No Changes Needed

| File | Reason |
|------|--------|
| `FrmMain.cs` | Shell + navigation works fine in WinForms |
| `PlaceholderForm.cs` | Base class used by FrmHangDoiKham, FrmKhamBenh, FrmInPhieu |
| `UiTheme.cs` | Still needed for FrmMain nav styling |
| `ModernUI.cs` | Still needed for FrmMain bordered panels |
| All BLL/*.cs | Zero changes |
| All DAL/*.cs | Zero changes |
| All DTO/*.cs | Zero changes |

---

## Implementation Order

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 1 | `ClinicBridge.cs` | Create bridge with ALL BLL methods | Nothing |
| 2 | `WebFormBase.cs` | Create base class | Step 1 |
| 3 | `FrmBenhNhan.cs` | Rewrite → WebView2 | Steps 1-2 |
| 4 | `FrmTaoLuotKham.cs` | Rewrite → WebView2 | Steps 1-2 |
| 5 | `FrmLichSu.cs` | Rewrite → WebView2 | Steps 1-2 |
| 6 | `FrmDashboard.cs` | Rewrite → WebView2 | Steps 1-2 |
| 7 | `FrmHangDoiKham.cs` | Rewrite → WebView2 (keep ExamStarted event) | Steps 1-2 |
| 8 | `FrmKhamBenh.cs` | Rewrite → WebView2 (keep maLK/maBacSi params) | Steps 1-2 |
| 9 | `FrmInPhieu.cs` | Minor fix or keep as-is | Steps 1-2 |
| 10 | `FrmLogin.cs` | Rewrite → WebView2 (standalone) | Steps 1-2 |
| 11 | Build + Test | Verify all flows | All steps |

---

## Key Technical Notes for Sonnet

### 1. COM Visibility for Bridge
```csharp
using System.Runtime.InteropServices;

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class ClinicBridge { ... }
```

### 2. JS async calling pattern
```javascript
// Bridge methods are sync from JS side (COM interop)
const bridge = window.chrome.webview.hostObjects.sync.bridge;
// Or async:
const bridge = window.chrome.webview.hostObjects.bridge;
const result = await bridge.TimBenhNhan("keyword");
```

### 3. Tailwind CSS — Inline Everything
The design HTML uses `https://cdn.tailwindcss.com` CDN. Since WebView2 has internet access, this works. But for offline:
- Option A: Keep CDN (simplest, requires internet) ✅
- Option B: Bundle tailwind CSS (more complex)

### 4. FrmMain ShowEmbeddedForm — Minimal Changes
Current `ShowEmbeddedForm` already handles `TopLevel=false, FormBorderStyle=None, Dock=Fill`. WebView2 forms will work the same way since they inherit `Form`.

### 5. HTML Template Pattern
Each form's `GetHtml()` should return a COMPLETE HTML document:
```csharp
protected override string GetHtml() => $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='utf-8'/>
  <script src='https://cdn.tailwindcss.com'></script>
  <script>tailwind.config = {{ /* from design */ }}</script>
  <style>/* from design */</style>
</head>
<body>
  <!-- ONLY the main content area from design, NO sidebar/topbar -->
  {GetBodyHtml()}
  <script>
    const bridge = window.chrome.webview.hostObjects.sync.bridge;
    // Form-specific JS logic here
  </script>
</body>
</html>";
```

### 6. FrmHangDoiKham.ExamStarted Event
```csharp
// In the rewritten FrmHangDoiKham:
public event EventHandler<ExamStartedEventArgs>? ExamStarted;

// In constructor or Load:
_webView.CoreWebView2.WebMessageReceived += (s, e) =>
{
    var msg = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(e.WebMessageAsJson);
    if (msg["action"].ToString() == "ExamStarted")
        ExamStarted?.Invoke(this, new ExamStartedEventArgs(int.Parse(msg["maLK"].ToString())));
};
```

### 7. FrmKhamBenh Constructor Overloads
Must preserve ALL existing constructor overloads since FrmMain uses them:
```csharp
"KhamBenh" => param is int maLK 
    ? new FrmKhamBenh(maLK, _currentUser.MaNV) 
    : new FrmKhamBenh(0, _currentUser.MaNV)
```

### 8. FrmLogin is NOT embedded
FrmLogin is standalone (TopLevel=true). After successful login via JS, use `WebMessageReceived` to get user data back to C# and call `OpenMainShell(user)`.

---

## Verification Plan

### Build
```bash
dotnet build src/ClinicApp.GUI/ClinicApp.GUI.csproj
```

### Manual Test Flows
1. **Login:** tiepnhan/123, bacsi/123
2. **BenhNhan:** Search → Click row → Edit → Save → Verify grid updates
3. **TaoLuotKham:** Search → Select patient → Add note → Create visit → Cancel visit
4. **LichSu:** Date filter + keyword → Search → Verify results
5. **Dashboard:** Verify 3 cards + grid loads
6. **HangDoiKham:** Auto-refresh → Select → Start exam → Navigates to KhamBenh
7. **KhamBenh:** Fill diagnosis → Save → Preview opens
8. **InPhieu:** Enter MaLK → Load → Print

---

## Decisions (Confirmed)

- **Q1 — Offline:** ❌ Không cần offline. Dùng Tailwind CDN (`https://cdn.tailwindcss.com`) cho đơn giản.
- **Q2 — FrmLogin:** ✅ **Convert sang WebView2** cho đồng bộ giao diện. HTML source: `design/1._login_screen/code.html`. Dùng `WebMessageReceived` để trả user data về C#.
- **Q3 — FrmInPhieu:** ✅ **Convert toàn bộ sang WebView2** (bỏ lookup bar WinForms, chuyển hết vào HTML). HTML source: `design/7._print_preview/code.html`.

### Tổng kết: ALL 8 forms convert sang WebView2
| # | Form | HTML Source |
|---|------|-------------|
| 1 | FrmLogin | `design/1._login_screen/code.html` |
| 2 | FrmBenhNhan | `design/3._reception_patients/code.html` |
| 3 | FrmTaoLuotKham | `design/4._reception_create_visit/code.html` |
| 4 | FrmHangDoiKham | `design/5._doctor_patient_queue/code.html` |
| 5 | FrmKhamBenh | `design/6._doctor_examination/code.html` |
| 6 | FrmInPhieu | `design/7._print_preview/code.html` |
| 7 | FrmLichSu | `design/8._history/code.html` |
| 8 | FrmDashboard | `design/9._dashboard/code.html` |
