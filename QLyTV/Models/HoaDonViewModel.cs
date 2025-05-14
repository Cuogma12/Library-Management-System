using System;
using System.Collections.Generic;

namespace QLyTV.Models
{
    public class HoaDonViewModel
    {
        public int MaPhieuMuon { get; set; }
        public int MaThuThu { get; set; }
        public string TenDocGia { get; set; }
        public DateTime NgayMuon { get; set; }
        public DateTime? NgayTraDuKien { get; set; }
        public DateTime? NgayTraThucTe { get; set; }
        //public List<Sach> SachMuon { get; set; }
        public string TenSach { get; set; }
        public decimal PhiMuon { get; set; }
        public decimal PhiPhat { get; set; }
        public decimal TongChiPhi { get; set; }
    }

}
