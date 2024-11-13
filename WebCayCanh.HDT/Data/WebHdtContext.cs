using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebCayCanh.HDT.Data;

public partial class WebHdtContext : DbContext
{
    public WebHdtContext()
    {
    }

    public WebHdtContext(DbContextOptions<WebHdtContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DanhGiaDh> DanhGiaDhs { get; set; }

    public virtual DbSet<Dathang> Dathangs { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<PhanLoaiSp> PhanLoaiSps { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<Thamchieu> Thamchieus { get; set; }

    public virtual DbSet<Them> Thems { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-6JRBMVT\\SQLEXPRESS;Initial Catalog=WebHDT;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DanhGiaDh>(entity =>
        {
            entity.HasKey(e => e.MaDg);

            entity.ToTable("DANH_GIA_DH");

            entity.Property(e => e.MaDg).HasColumnName("MA_DG");
            entity.Property(e => e.MaDh).HasColumnName("MA_DH");
            entity.Property(e => e.NgayDg)
                .HasColumnType("datetime")
                .HasColumnName("NGAY_DG");
            entity.Property(e => e.NoiDungDg)
                .HasColumnType("text")
                .HasColumnName("NOI_DUNG_DG");
            entity.Property(e => e.ThangDiem).HasColumnName("THANG_DIEM");
        });

        modelBuilder.Entity<Dathang>(entity =>
        {
            entity.HasKey(e => new { e.MaKh, e.MaGh });

            entity.ToTable("DATHANG");

            entity.Property(e => e.MaKh).HasColumnName("MA_KH");
            entity.Property(e => e.MaGh).HasColumnName("MA_GH");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDh);

            entity.ToTable("DON_HANG");

            entity.Property(e => e.MaDh).HasColumnName("MA_DH");
            entity.Property(e => e.MaDg).HasColumnName("MA_DG");
            entity.Property(e => e.MaKh).HasColumnName("MA_KH");
            entity.Property(e => e.NgayDat)
                .HasColumnType("datetime")
                .HasColumnName("NGAY_DAT");
            entity.Property(e => e.NgayNhan)
                .HasColumnType("datetime")
                .HasColumnName("NGAY_NHAN");
            entity.Property(e => e.TongTien).HasColumnName("TONG_TIEN");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.MaGh);

            entity.ToTable("GIO_HANG");

            entity.Property(e => e.MaGh).HasColumnName("MA_GH");
            entity.Property(e => e.DonGia).HasColumnName("DON_GIA");
            entity.Property(e => e.SoLuong).HasColumnName("SO_LUONG");
            entity.Property(e => e.TienTamTinh).HasColumnName("TIEN_TAM_TINH");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung);

            entity.ToTable("NGUOI_DUNG");

            entity.Property(e => e.MaNguoiDung).HasColumnName("MA_NGUOI_DUNG");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MAT_KHAU");
            entity.Property(e => e.TaiKhoan)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("TAI_KHOAN");
        });

        modelBuilder.Entity<PhanLoaiSp>(entity =>
        {
            entity.HasKey(e => e.MaPhanLoai);

            entity.ToTable("PHAN_LOAI_SP");

            entity.Property(e => e.MaPhanLoai).HasColumnName("MA_PHAN_LOAI");
            entity.Property(e => e.MoTaPl)
                .HasColumnType("text")
                .HasColumnName("MO_TA_PL");
            entity.Property(e => e.TenPhanLoai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TEN_PHAN_LOAI");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSp);

            entity.ToTable("SAN_PHAM");

            entity.Property(e => e.MaSp).HasColumnName("MA_SP");
            entity.Property(e => e.GiaSp).HasColumnName("GIA_SP");
            entity.Property(e => e.MaPhanLoai).HasColumnName("MA_PHAN_LOAI");
            entity.Property(e => e.MoTaSp)
                .HasColumnType("text")
                .HasColumnName("MO_TA_SP");
            entity.Property(e => e.SoLuongTonKho).HasColumnName("SO_LUONG_TON_KHO");
            entity.Property(e => e.TenSp)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("TEN_SP");
        });

        modelBuilder.Entity<Thamchieu>(entity =>
        {
            entity.HasKey(e => new { e.MaDh, e.MaSp });

            entity.ToTable("THAMCHIEU");

            entity.Property(e => e.MaDh).HasColumnName("MA_DH");
            entity.Property(e => e.MaSp).HasColumnName("MA_SP");
        });

        modelBuilder.Entity<Them>(entity =>
        {
            entity.HasKey(e => new { e.MaSp, e.MaGh });

            entity.ToTable("THEM");

            entity.Property(e => e.MaSp).HasColumnName("MA_SP");
            entity.Property(e => e.MaGh).HasColumnName("MA_GH");
            entity.Property(e => e.Soluong).HasColumnName("SOLUONG");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
