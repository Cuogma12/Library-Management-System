using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLyTV.Models
{
    public class ThongKeSachViewModel
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public int SoLuongBanDau { get; set; }
        public int SoLuongDaChoMuon { get; set; }
        public int SoLuongConLai { get; set; }

    }
}