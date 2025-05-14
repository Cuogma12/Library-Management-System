using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLyTV.Models
{
    public class MuonSachViewModel
    {
        public List<Sach> SachList { get; set; }
        public int MaDocGia { get; set; }
        public List<ChiTietMuonSach> ChiTietMuon { get; set; }
    }

    public class ChiTietMuonSach
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public int SoLuongMuon { get; set; }
        public int SoLuongConLai { get; set; }
        public int NgayMuon { get; set; }
        public int NgayTra { get; set; }
    }
    public class Sạch
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        // Các thuộc tính khác
    }
}