using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLyTV.Models;

namespace QLyTV.Controllers
{
    public class ThongKeController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=m-cuogdzai;Initial Catalog=QLYTV;Integrated Security=True;TrustServerCertificate=True");
        public ActionResult ThongKeSach(string stringSearch)
        {
            var danhSach = from s in db.Saches
                           select new ThongKeSachViewModel
                           {
                               MaSach = s.MaSach,
                               TenSach = s.TenSach,
                               SoLuongBanDau = s.SoLuong ?? 0,
                               SoLuongDaChoMuon = db.PhieuMuons
                                   .Where(pm => pm.MaSach == s.MaSach && pm.isApproved == true && pm.isDelete == false)
                                   .Count(),
                               SoLuongConLai = (s.SoLuong ?? 0) - db.PhieuMuons
                                   .Where(pm => pm.MaSach == s.MaSach && pm.isApproved == true && pm.isDelete == false)
                                   .Count()
                           };


            if (!string.IsNullOrEmpty(stringSearch))
            {
                danhSach = danhSach.Where(x => x.TenSach.Contains(stringSearch) || x.MaSach.ToString().Contains(stringSearch));
            }

            var thongKe = danhSach.ToList();

            //Cập nhật sách còn hay đã mượn
            foreach (var item in thongKe)
            {
                var sach = db.Saches.FirstOrDefault(s => s.MaSach == item.MaSach);
                if (sach != null && sach.SoLuong == item.SoLuongDaChoMuon) // Cập nhật trạng thái chỉ khi số lượng còn lại bằng 0
                {
                    sach.TrangThai = "Da muon het";
                }
                else
                {
                    sach.TrangThai = "Con";
                }
            }
            db.SubmitChanges();

            return View(thongKe);
        }

        public ActionResult ThongKeDoanhThu(DateTime? fromDate, DateTime? toDate)
        {
            var doanhThu = new ThongKeDoanhThuViewModel();

            var query = db.HoaDons.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.NgayTra >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(h => h.NgayTra <= toDate.Value);
            }

            doanhThu.DoanhThuPhiPhat = query.Sum(h => (decimal?)h.PhiPhat) ?? 0;
            doanhThu.DoanhThuPhiMuon = query.Sum(h => (decimal?)h.PhiMuon) ?? 0;
            doanhThu.FromDate = fromDate;
            doanhThu.ToDate = toDate;

            return View(doanhThu);
        }

        public ActionResult DocGiaNoSach(string stringSearch)
        {
            var danhSach = (from u in db.Users
                            join ur in db.UserRoles on u.Id equals ur.UserId
                            join r in db.Roles on ur.RoleId equals r.Id
                            join pm in db.PhieuMuons on u.Id equals pm.MaDocGia
                            join s in db.Saches on pm.MaSach equals s.MaSach
                            where r.Id == 3 // Lọc độc giả có nhóm quyền là 3
                                  && pm.NgayTraDuKien < DateTime.Now
                                  && pm.isApproved == true
                                  && pm.isDelete == false
                            select new DocGiaNoSachViewModel
                            {
                                MaDocGia = u.Id,
                                TenDocGia = u.Name,
                                TenSach = s.TenSach,
                                NgayMuon = pm.NgayMuon,
                                NgayTraDuKien = pm.NgayTraDuKien,
                                SoNgayQuaHan = pm.NgayTraDuKien < DateTime.Now
                     ? (DateTime.Now - pm.NgayTraDuKien).Days : 0 // Nếu không quá hạn hoặc ngày trả dự kiến là NULL
                            });
            if (!string.IsNullOrEmpty(stringSearch))
            {
                danhSach = danhSach.Where(x => x.TenDocGia.Contains(stringSearch));
            }


            return View(danhSach.ToList());
        }

        public ActionResult DocGiaDangMuon(string stringSearch)
        {
            var result = (from pm in db.PhieuMuons
                          join u in db.Users on pm.MaDocGia equals u.Id
                          join s in db.Saches on pm.MaSach equals s.MaSach
                          where !pm.NgayTra.HasValue // Ngày trả NULL => Chưa trả sách
                             && pm.NgayTraDuKien >= DateTime.Now // Chưa quá hạn
                             && pm.isApproved == true
                             && pm.isDelete == false
                          select new DocGiaDangMuonViewModel
                          {
                              MaDocGia = u.Id,
                              TenDocGia = u.Name,
                              TenSach = s.TenSach,
                              NgayMuon = pm.NgayMuon,
                              NgayTraDuKien = pm.NgayTraDuKien
                          });

            if (!string.IsNullOrEmpty(stringSearch))
            {
                result = result.Where(x => x.TenDocGia.Contains(stringSearch));
            }


            return View(result.ToList());
        }


    }

}
