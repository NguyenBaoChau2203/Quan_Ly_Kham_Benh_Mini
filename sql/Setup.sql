-- =============================================
-- Setup.sql - Quản Lý Khám Bệnh Mini
-- Chạy trên SQL Server sạch để tạo toàn bộ DB.
-- Database: ClinicAppDB
-- =============================================

-- 1. Tạo Database
-- =============================================
IF DB_ID('ClinicAppDB') IS NOT NULL
BEGIN
    ALTER DATABASE ClinicAppDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ClinicAppDB;
END
GO

CREATE DATABASE ClinicAppDB;
GO

USE ClinicAppDB;
GO

-- 2. Tạo Bảng
-- =============================================

CREATE TABLE BenhNhan (
    MaBN        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100)   NOT NULL,
    NgaySinh    DATE            NULL,
    GioiTinh    NVARCHAR(10)    NOT NULL DEFAULT N'Nam',
    SDT         VARCHAR(15)     NOT NULL,
    CCCD        VARCHAR(12)     NULL,
    DiaChi      NVARCHAR(200)   NULL
);

CREATE TABLE NhanVien (
    MaNV            INT IDENTITY(1,1) PRIMARY KEY,
    Username        VARCHAR(50)     NOT NULL,
    PasswordHash    VARCHAR(64)     NOT NULL,
    VaiTro          VARCHAR(20)     NOT NULL,
    HoTen           NVARCHAR(100)   NOT NULL,

    CONSTRAINT UQ_NhanVien_Username UNIQUE (Username),
    CONSTRAINT CK_NhanVien_VaiTro   CHECK (VaiTro IN ('TiepNhan', 'BacSi'))
);

CREATE TABLE LuotKham (
    MaLK            INT IDENTITY(1,1) PRIMARY KEY,
    MaBN            INT             NOT NULL,
    SoThuTu         INT             NOT NULL,
    NgayKham        DATETIME        NOT NULL,
    NgayKhamDate    DATE            NOT NULL,
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT 'DangCho',
    MaBacSi         INT             NULL,
    GhiChu          NVARCHAR(500)   NULL,

    CONSTRAINT FK_LuotKham_BenhNhan FOREIGN KEY (MaBN) REFERENCES BenhNhan(MaBN),
    CONSTRAINT FK_LuotKham_BacSi    FOREIGN KEY (MaBacSi) REFERENCES NhanVien(MaNV),
    CONSTRAINT CK_LuotKham_TrangThai CHECK (TrangThai IN ('DangCho', 'DangKham', 'DaKham', 'DaHuy')),
    CONSTRAINT UQ_LuotKham_NgaySTT  UNIQUE (NgayKhamDate, SoThuTu)
);

CREATE TABLE ChiTietKham (
    MaCTK       INT IDENTITY(1,1) PRIMARY KEY,
    MaLK        INT             NOT NULL,
    TrieuChung  NVARCHAR(1000)  NULL,
    ChanDoan    NVARCHAR(1000)  NOT NULL,
    ToaThuoc    NVARCHAR(1000)  NULL,
    LoiDan      NVARCHAR(1000)  NULL,

    CONSTRAINT FK_ChiTietKham_LuotKham FOREIGN KEY (MaLK) REFERENCES LuotKham(MaLK),
    CONSTRAINT UQ_ChiTietKham_MaLK     UNIQUE (MaLK)
);

CREATE TABLE BoDemSoThuTu (
    Ngay    DATE PRIMARY KEY,
    SoCuoi  INT NOT NULL DEFAULT 0
);
GO

-- 3. Stored Procedures
-- =============================================

-- 3.1) sp_TaoLuotKham
--   - Transaction + UPDLOCK/HOLDLOCK khi tăng bộ đếm
--   - NgayKham/NgayKhamDate tự sinh từ GETDATE()
--   - OUTPUT: @MaLK, @SoThuTu
-- =============================================
CREATE PROCEDURE sp_TaoLuotKham
    @MaBN       INT,
    @MaBacSi    INT         = NULL,
    @GhiChu     NVARCHAR(500) = NULL,
    @MaLK       INT         OUTPUT,
    @SoThuTu    INT         OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @MaLK = 0;
    SET @SoThuTu = 0;

    DECLARE @Now  DATETIME = GETDATE();
    DECLARE @Ngay DATE     = CAST(@Now AS DATE);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Upsert BoDemSoThuTu với lock
        IF EXISTS (SELECT 1 FROM BoDemSoThuTu WITH (UPDLOCK, HOLDLOCK) WHERE Ngay = @Ngay)
        BEGIN
            UPDATE BoDemSoThuTu
            SET    SoCuoi = SoCuoi + 1
            WHERE  Ngay = @Ngay;

            SELECT @SoThuTu = SoCuoi FROM BoDemSoThuTu WHERE Ngay = @Ngay;
        END
        ELSE
        BEGIN
            SET @SoThuTu = 1;
            INSERT INTO BoDemSoThuTu (Ngay, SoCuoi) VALUES (@Ngay, 1);
        END

        -- Insert LuotKham
        INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi, GhiChu)
        VALUES (@MaBN, @SoThuTu, @Now, @Ngay, 'DangCho', @MaBacSi, @GhiChu);

        SET @MaLK = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @MaLK = 0;
        SET @SoThuTu = 0;
        -- Lỗi sẽ được DAL bắt qua return value (@MaLK = 0)
    END CATCH
END
GO

-- 3.2) sp_HoanTatKham
--   - Guard: chỉ chạy khi TrangThai = 'DangKham'
--   - Guard: chưa có ChiTietKham cho MaLK này
--   - Transaction: Insert ChiTietKham + Update LuotKham → DaKham
--   - Double-save → fail êm
-- =============================================
CREATE PROCEDURE sp_HoanTatKham
    @MaLK       INT,
    @TrieuChung NVARCHAR(1000) = NULL,
    @ChanDoan   NVARCHAR(1000),
    @ToaThuoc   NVARCHAR(1000) = NULL,
    @LoiDan     NVARCHAR(1000) = NULL,
    @KetQua     INT OUTPUT      -- 1 = thành công, 0 = thất bại
AS
BEGIN
    SET NOCOUNT ON;
    SET @KetQua = 0;

    -- Guard: kiểm tra trạng thái hiện tại
    IF NOT EXISTS (SELECT 1 FROM LuotKham WHERE MaLK = @MaLK AND TrangThai = 'DangKham')
    BEGIN
        RETURN;
    END

    -- Guard: chống double-save
    IF EXISTS (SELECT 1 FROM ChiTietKham WHERE MaLK = @MaLK)
    BEGIN
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO ChiTietKham (MaLK, TrieuChung, ChanDoan, ToaThuoc, LoiDan)
        VALUES (@MaLK, @TrieuChung, @ChanDoan, @ToaThuoc, @LoiDan);

        UPDATE LuotKham
        SET    TrangThai = 'DaKham'
        WHERE  MaLK = @MaLK AND TrangThai = 'DangKham';

        IF @@ROWCOUNT = 1
            SET @KetQua = 1;
        ELSE
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @KetQua = 0;
    END CATCH
END
GO

-- 4. Dữ Liệu Demo
-- =============================================

-- 4.1) Nhân viên (SHA256 của '123' = a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3)
INSERT INTO NhanVien (Username, PasswordHash, VaiTro, HoTen) VALUES
('tiepnhan', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'TiepNhan', N'Nguyễn Thị Lan'),
('bacsi',    'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'BacSi',    N'Trần Văn Minh');

-- 4.2) Bệnh nhân (12 bệnh nhân)
INSERT INTO BenhNhan (HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi) VALUES
(N'Nguyễn Văn An',     '1988-05-12', N'Nam', '0901234567', '079088012345', N'Quận 1, TP. Hồ Chí Minh'),
(N'Trần Thị Bích',     '1995-11-24', N'Nữ',  '0912888999', '079095023456', N'Bình Thạnh, TP. Hồ Chí Minh'),
(N'Lê Minh Cường',     '1975-01-05', N'Nam', '0844555666', NULL,            N'Dĩ An, Bình Dương'),
(N'Phạm Thu Dung',     '2002-09-18', N'Nữ',  '0933111222', '079002034567', N'Quận 7, TP. Hồ Chí Minh'),
(N'Hoàng Hải Đăng',    '1990-03-30', N'Nam', '0967000111', NULL,            N'Biên Hòa, Đồng Nai'),
(N'Võ Thanh Hà',       '1982-07-15', N'Nữ',  '0978222333', '079082045678', N'Thủ Đức, TP. Hồ Chí Minh'),
(N'Đặng Quốc Hưng',    '2000-12-01', N'Nam', '0356444555', '079000056789', N'Quận 3, TP. Hồ Chí Minh'),
(N'Bùi Thị Kim',       '1998-04-22', N'Nữ',  '0388777888', NULL,            N'Tân Bình, TP. Hồ Chí Minh'),
(N'Ngô Đức Long',      '1970-08-10', N'Nam', '0909111222', '079070067890', N'Quận 5, TP. Hồ Chí Minh'),
(N'Phan Thị Mai',      '1985-06-28', N'Nữ',  '0917333444', '079085078901', N'Gò Vấp, TP. Hồ Chí Minh'),
(N'Lý Văn Nam',        '1993-02-14', N'Nam', '0945666777', NULL,            N'Quận 10, TP. Hồ Chí Minh'),
(N'Huỳnh Thị Oanh',    '2005-10-03', N'Nữ',  '0374888999', '079005089012', N'Phú Nhuận, TP. Hồ Chí Minh');

-- 4.3) Lượt khám cũ (6 lượt DaKham trong 7 ngày gần đây) + 2 lượt DangCho hôm nay
-- Dùng ngày tương đối để demo luôn có dữ liệu dù chạy lúc nào.

DECLARE @Today DATE = CAST(GETDATE() AS DATE);

-- Ngày -6: 1 lượt
INSERT INTO BoDemSoThuTu (Ngay, SoCuoi) VALUES (DATEADD(DAY, -6, @Today), 1);
INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi)
VALUES (1, 1, DATEADD(DAY, -6, CAST(GETDATE() AS DATETIME)), DATEADD(DAY, -6, @Today), 'DaKham', 2);
INSERT INTO ChiTietKham (MaLK, TrieuChung, ChanDoan, ToaThuoc, LoiDan)
VALUES (SCOPE_IDENTITY(), N'Đau đầu, mệt mỏi', N'Cảm cúm thông thường', N'Paracetamol 500mg x 10 viên, uống 2 lần/ngày', N'Nghỉ ngơi, uống nhiều nước');

-- Ngày -5: 2 lượt
INSERT INTO BoDemSoThuTu (Ngay, SoCuoi) VALUES (DATEADD(DAY, -5, @Today), 2);
INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi)
VALUES (2, 1, DATEADD(DAY, -5, CAST(GETDATE() AS DATETIME)), DATEADD(DAY, -5, @Today), 'DaKham', 2);
INSERT INTO ChiTietKham (MaLK, TrieuChung, ChanDoan, ToaThuoc, LoiDan)
VALUES (SCOPE_IDENTITY(), N'Ho khan, sốt nhẹ', N'Viêm họng cấp tính', N'Amoxicillin 500mg x 14 viên, uống 2 lần/ngày', N'Kiêng đồ lạnh, tái khám sau 5 ngày');

INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi)
VALUES (5, 2, DATEADD(DAY, -5, CAST(GETDATE() AS DATETIME)), DATEADD(DAY, -5, @Today), 'DaKham', 2);
INSERT INTO ChiTietKham (MaLK, TrieuChung, ChanDoan, ToaThuoc, LoiDan)
VALUES (SCOPE_IDENTITY(), N'Đau bụng, buồn nôn', N'Rối loạn tiêu hóa', N'Smecta x 10 gói, uống 3 lần/ngày', N'Ăn nhẹ, kiêng đồ cay nóng');

-- Ngày -3: 1 lượt
INSERT INTO BoDemSoThuTu (Ngay, SoCuoi) VALUES (DATEADD(DAY, -3, @Today), 1);
INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi)
VALUES (9, 1, DATEADD(DAY, -3, CAST(GETDATE() AS DATETIME)), DATEADD(DAY, -3, @Today), 'DaKham', 2);
INSERT INTO ChiTietKham (MaLK, TrieuChung, ChanDoan, ToaThuoc, LoiDan)
VALUES (SCOPE_IDENTITY(), N'Tăng huyết áp, chóng mặt', N'Tăng huyết áp vô căn', N'Amlodipine 5mg x 30 viên, uống 1 lần/ngày', N'Đo huyết áp hàng ngày, giảm muối');

-- Ngày -1: 2 lượt (1 DaKham, 1 DaHuy)
INSERT INTO BoDemSoThuTu (Ngay, SoCuoi) VALUES (DATEADD(DAY, -1, @Today), 2);
INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi)
VALUES (3, 1, DATEADD(DAY, -1, CAST(GETDATE() AS DATETIME)), DATEADD(DAY, -1, @Today), 'DaKham', 2);
INSERT INTO ChiTietKham (MaLK, TrieuChung, ChanDoan, ToaThuoc, LoiDan)
VALUES (SCOPE_IDENTITY(), N'Đau lưng kéo dài', N'Thoái hóa cột sống thắt lưng', N'Meloxicam 7.5mg x 10 viên, uống 1 lần/ngày', N'Tập vật lý trị liệu, tránh mang vác nặng');

INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi)
VALUES (4, 2, DATEADD(DAY, -1, CAST(GETDATE() AS DATETIME)), DATEADD(DAY, -1, @Today), 'DaHuy', NULL);

-- Hôm nay: 2 lượt DangCho (để hàng đợi bác sĩ có dữ liệu test)
INSERT INTO BoDemSoThuTu (Ngay, SoCuoi) VALUES (@Today, 2);
INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi, GhiChu)
VALUES (6, 1, GETDATE(), @Today, 'DangCho', NULL, N'Khám định kỳ');
INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi, GhiChu)
VALUES (10, 2, GETDATE(), @Today, 'DangCho', NULL, N'Đau đầu, chóng mặt');

PRINT N'=== Setup hoàn tất: ClinicAppDB ===';
PRINT N'Tài khoản demo: tiepnhan/123, bacsi/123';
GO

-- =============================================
-- Reception module procedures
-- =============================================

DROP PROCEDURE IF EXISTS sp_ThemBenhNhan;
GO

CREATE PROCEDURE sp_ThemBenhNhan
    @HoTen       NVARCHAR(100),
    @NgaySinh    DATE          = NULL,
    @GioiTinh    NVARCHAR(10),
    @SDT         VARCHAR(15),
    @CCCD        VARCHAR(12)   = NULL,
    @DiaChi      NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT OFF;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @CCCD IS NOT NULL
           AND EXISTS (
                SELECT 1
                FROM BenhNhan WITH (UPDLOCK, HOLDLOCK)
                WHERE CCCD = @CCCD
           )
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO BenhNhan (HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi)
        VALUES (@HoTen, @NgaySinh, @GioiTinh, @SDT, @CCCD, @DiaChi);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
    END CATCH
END
GO

DROP PROCEDURE IF EXISTS sp_TaoLuotKham;
GO

CREATE PROCEDURE sp_TaoLuotKham
    @MaBN       INT,
    @MaBacSi    INT           = NULL,
    @GhiChu     NVARCHAR(500) = NULL,
    @SoThuTu    INT           OUTPUT
AS
BEGIN
    SET NOCOUNT OFF;
    SET XACT_ABORT ON;

    SET @SoThuTu = NULL;

    DECLARE @Now DATETIME = GETDATE();
    DECLARE @Ngay DATE = CAST(@Now AS DATE);

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @SoThuTu = ISNULL(MAX(SoThuTu), 0) + 1
        FROM LuotKham WITH (UPDLOCK, HOLDLOCK)
        WHERE NgayKhamDate = @Ngay;

        INSERT INTO LuotKham (MaBN, SoThuTu, NgayKham, NgayKhamDate, TrangThai, MaBacSi, GhiChu)
        VALUES (@MaBN, @SoThuTu, @Now, @Ngay, 'DangCho', @MaBacSi, @GhiChu);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @SoThuTu = NULL;
    END CATCH
END
GO
