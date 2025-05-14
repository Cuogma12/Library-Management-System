using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLyTV.Models
{
    public class ThongKeDoanhThuViewModel
    {
        public decimal DoanhThuPhiPhat { get; set; }
        public decimal DoanhThuPhiMuon { get; set; }
        public decimal TongDoanhThu
        {
            get
            {
                return DoanhThuPhiPhat + DoanhThuPhiMuon;
            }
        }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
}