﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLyTV.Models
{
    public class DocGiaDangMuonViewModel
    {
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public string TenSach { get; set; }
        public DateTime? NgayMuon { get; set; }
        public DateTime? NgayTraDuKien { get; set; }

    }
}