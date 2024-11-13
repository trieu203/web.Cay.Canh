using System;
using System.Collections.Generic;

namespace WebCayCanh.HDT.Data;

public partial class DonHang
{
    public int MaDh { get; set; }

    public int MaKh { get; set; }

    public int? MaDg { get; set; }

    public DateTime? NgayDat { get; set; }

    public DateTime? NgayNhan { get; set; }

    public double? TongTien { get; set; }
}
