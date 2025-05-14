using QLyTV.Models;
using System.Collections.Generic;
using System;
namespace QLyTV.Models
{
    public class AccountViewModel
    {
        public User UserInfo { get; set; }
        public List<BookBorrowedDetail> BorrowedBooks { get; set; }
    }

    public class BookBorrowedDetail
    {
        public int MaSach {  get; set; }
        public string TenSach { get; set; }
        public int SoLuong { get; set; }
        public DateTime NgayMuon { get; set; }
        public DateTime NgayTraDuKien { get; set; }
    }
}