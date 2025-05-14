using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QLyTV.Constants;
using QLyTV.Models;
using Unity.Injection;

namespace QLyTV.Controllers
{
    public class PhieuMuonController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=m-cuogdzai;Initial Catalog=QLYTV;Integrated Security=True;TrustServerCertificate=True");
        // 1. Hiển thị danh sách phiếu mượn đã duyệt
        public ActionResult Index(string keyword = "", int page = 1, int pageSize = 5)
        {
            var query = from pm in db.PhieuMuons
                        join user in db.Users on pm.MaDocGia equals user.Id
                        where user.UserRoles.Any(r => r.Role.Code == RoleConstants.DocGia) && user.IsActive && pm.isApproved == true && pm.isDelete == false
                        select new
                        {
                            pm,
                            Name = user.Name // Thêm tên độc giả vào kết quả
                        };

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(q => q.pm.MaPhieuMuon.ToString().Contains(keyword) ||
                                          q.pm.MaDocGia.ToString().Contains(keyword) ||
                                          q.pm.MaSach.ToString().Contains(keyword) ||
                                          q.Name.Contains(keyword)); // Tìm theo tên độc giả
            }

            var totalItems = query.Count();

            var items = query
                .OrderBy(q => q.pm.MaPhieuMuon)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new { q.pm, q.Name }) // Chọn cả phiếu mượn và tên độc giả
                .ToList();

            var model = new PagedResult<PhieuMuon>
            {
                Items = items.Select(q => q.pm).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems
            };

            return View(model);
        }



        // 2. Hiển thị form thêm phiếu mượn
        public ActionResult Add()
        {
            ViewBag.SachList = db.Saches.Where(s => s.TrangThai == "Con").ToList();// them cho nay
            return View(new PhieuMuon());
        }

        [HttpPost]
        public JsonResult AddPhieuMuon(int? maThuThu, int maDocGia, int maSach, DateTime ngayMuon, DateTime ngayTraDuKien)
        {
            try
            {
                // Kiểm tra ngày mượn và ngày trả
                if (ngayMuon >= ngayTraDuKien)
                {
                    return Json(new { success = false, message = "Ngày mượn phải nhỏ hơn ngày trả dự kiến!" });
                }

                // Kiểm tra trạng thái sách
                var sach = db.Saches.FirstOrDefault(s => s.MaSach == maSach);
                if (sach == null || sach.TrangThai != "Con")
                {
                    return Json(new { success = false, message = "Sách này đã hết, không thể mượn" });
                }

                // Nếu role là Độc Giả, lấy ID từ Session
                if (Session["Role"] != null && Session["Role"].ToString() == QLyTV.Constants.RoleConstants.DocGia)
                {
                    maDocGia = Convert.ToInt32(Session["UserId"]);
                }

                var newPhieuMuon = new PhieuMuon
                {
                    MaThuThu = maThuThu,
                    MaDocGia = maDocGia,
                    MaSach = maSach,
                    NgayMuon = ngayMuon,
                    isApproved = Session["Role"] != null && Session["Role"].ToString() == QLyTV.Constants.RoleConstants.DocGia ? false : true,
                    NgayTraDuKien = ngayTraDuKien
                };
                db.PhieuMuons.InsertOnSubmit(newPhieuMuon);
                db.SubmitChanges();

                return Json(new { success = true, message = "Thêm Phiếu mượn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }



        // 4. Hiển thị danh sách phiếu mượn chờ duyệt
        public ActionResult DuyetYeuCau()
        {
            var phieuMuonChoDuyet = db.PhieuMuons
                .Where(pm => pm.isApproved == false)
                .ToList();
            return View(phieuMuonChoDuyet);
        }

        // 5. Xử lý duyệt yêu cầu mượn sách
        [HttpPost]
        public ActionResult DuyetYeuCau(int maPhieuMuon, bool dongY)
        {
            var yeuCauMuon = db.PhieuMuons.FirstOrDefault(pm => pm.MaPhieuMuon == maPhieuMuon);
            if (yeuCauMuon == null) return HttpNotFound("Phiếu mượn không tồn tại!");

            if (dongY)
            {
                yeuCauMuon.MaThuThu = int.Parse(Session["UserId"].ToString());
                yeuCauMuon.isApproved = true;
            }
            else
            {
                db.PhieuMuons.DeleteOnSubmit(yeuCauMuon);
            }
            db.SubmitChanges();
            return RedirectToAction("DuyetYeuCau");
        }

        // 6. Xem chi tiết phiếu mượn
        public ActionResult ChiTiet(int? id)
        {
            if (!id.HasValue) return HttpNotFound("Không tìm thấy mã phiếu mượn.");

            var phieuMuon = db.PhieuMuons.FirstOrDefault(pm => pm.MaPhieuMuon == id);
            if (phieuMuon == null) return HttpNotFound("Không tìm thấy phiếu mượn.");
            return View(phieuMuon);
        }

        // 7. Xuất hóa đơn từ phiếu mượn
        [HttpPost]
        public ActionResult XuatHoaDon(int? id)
        {
            var phieuMuon = db.PhieuMuons.FirstOrDefault(pm => pm.MaPhieuMuon == id);
            if (phieuMuon == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn." });
            }

            var ngayTraThucTe = phieuMuon.NgayTra ?? DateTime.Now;
            var ngayTraDuKien = phieuMuon.NgayTraDuKien;
            decimal phiPhat = 0;

            // Kiểm tra chỉ tính phí phạt nếu ngày trả thực tế vượt quá ngày trả dự kiến
            if ( ngayTraThucTe > ngayTraDuKien)
            {
                var soNgayTre = (ngayTraThucTe - ngayTraDuKien).Days; // Tính số ngày trễ
                phiPhat = soNgayTre * 3000; // Mỗi ngày trễ phạt 3000 
            }


            var hoaDon = (from pm in db.PhieuMuons
                          join s in db.Saches
                          on pm.MaSach equals s.MaSach
                          where pm.MaPhieuMuon == id
                          select new HoaDonViewModel
                          {
                              MaPhieuMuon = pm.MaPhieuMuon,
                              MaThuThu = pm.MaThuThu.Value,
                              TenDocGia = pm.User != null ? pm.User.Name : "Không rõ",
                              NgayMuon = pm.NgayMuon,
                              NgayTraDuKien = pm.NgayTraDuKien,
                              TenSach = s.TenSach,  // Get TenSach from the Sach table
                              PhiMuon = 20000,
                              PhiPhat = phiPhat,
                              TongChiPhi = 20000 + phiPhat
                          }).FirstOrDefault();

            return View("HoaDon", hoaDon);
        }



        // 8. Tìm kiếm phiếu mượn
        [HttpGet]
        public ActionResult Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return View(new List<PhieuMuon>());
            }

            keyword = keyword.ToLower();

            // Truy vấn các phiếu mượn theo từ khóa (cả mã phiếu mượn và tên độc giả)
            var results = db.PhieuMuons
                .Where(pm => pm.MaPhieuMuon.ToString().ToLower().Contains(keyword) ||
                    //(pm.isApproved && pm.isApproved.ToString().ToLower().Contains(keyword)) ||
                    pm.User.Name.ToLower().Contains(keyword))
                .ToList();

            // Lấy danh sách các UserIds từ các Phiếu Mượn đã truy vấn
            var userIds = results.Select(pm => pm.MaDocGia).Distinct().ToList();

            // Truy vấn các người dùng theo danh sách UserIds
            var users = db.Users.Where(u => userIds.Contains(u.Id)).ToList();

            // Gán thông tin người dùng vào từng phiếu mượn
            foreach (var item in results)
            {
                var user = users.FirstOrDefault(u => u.Id == item.MaDocGia);
                if (user != null)
                {
                    item.User = user;
                }
            }

            return View(results);
        }


        // 9. Hiển thị hóa đơn
        public ActionResult HoaDon()
        {
            return View(new HoaDonViewModel());
        }

        // 10. Xóa phiếu mượn
        public ActionResult Delete(int? id)
        {
            //var delObj = db.PhieuMuons.FirstOrDefault(o => o.MaPhieuMuon == id);
            //if (delObj != null)
            //{
            //    db.PhieuMuons.DeleteOnSubmit(delObj);
            //    db.SubmitChanges();
            //}

            var phieuMuon = db.PhieuMuons.FirstOrDefault(pm => pm.MaPhieuMuon == id);
            if (phieuMuon != null)
            {
                phieuMuon.isDelete = true;
            }
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        // 11. Hiển thị form sửa phiếu mượn
        public ActionResult Edit(int id)
        {
            var phieuMuon = db.PhieuMuons.FirstOrDefault(pm => pm.MaPhieuMuon == id);
            if (phieuMuon == null)
            {
                return HttpNotFound("Không tìm thấy phiếu mượn.");
            }

            // Lấy danh sách sách từ cơ sở dữ liệu
            ViewBag.DanhSachSach = db.Saches.ToList(); // Chuyển thành danh sách kiểu `Sach`

            return View(phieuMuon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PhieuMuon model)
        {
            if (!ModelState.IsValid)
            {
                // Truyền lại danh sách sách nếu validation thất bại
                ViewBag.DanhSachSach = db.Saches.ToList(); // Chuyển thành danh sách kiểu `Sach`
                return View(model);
            }

            var phieuMuon = db.PhieuMuons.FirstOrDefault(pm => pm.MaPhieuMuon == model.MaPhieuMuon);
            if (phieuMuon != null)
            {
                phieuMuon.MaDocGia = model.MaDocGia;
                phieuMuon.NgayMuon = model.NgayMuon;
                phieuMuon.NgayTra = model.NgayTra;
                phieuMuon.MaSach = model.MaSach; // Cập nhật mã sách

                db.SubmitChanges();
                TempData["SuccessMessage"] = "Cập nhật phiếu mượn thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu mượn để cập nhật!";
            }

            return RedirectToAction("Index");
        }



        // 13. Tạo yêu cầu mượn sách
        [HttpPost]
        public ActionResult CreateRequest(FormCollection form)
        {
            try
            {
                var phieuMuon = new PhieuMuon
                {
                    MaDocGia = int.Parse(form["MaDocGia"]),
                    MaSach = int.Parse(form["MaSach"]),
                    NgayMuon = DateTime.Now,
                    NgayTra = DateTime.Now.AddDays(7),
                    isApproved = false,
                    isDelete = false
                };

                db.PhieuMuons.InsertOnSubmit(phieuMuon);
                db.SubmitChanges();
                return Json(new { success = true, message = "Yêu cầu mượn sách đã được gửi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 14. Hiển thị form mượn sách
        //public ActionResult MuonSach()
        //{
        //    var sachList = db.Saches.ToList();

        //    var viewModel = new MuonSachViewModel
        //    {
        //        SachList = sachList, // Danh sách sách từ cơ sở dữ liệu
        //        ChiTietMuon = sachList.Select(s => new ChiTietMuonSach
        //        {
        //            MaSach = s.MaSach,
        //            TenSach = s.TenSach,
        //            SoLuongConLai = (int)s.SoLuong,
        //            SoLuongMuon = 0
        //        }).ToList() // Tạo danh sách ChiTietMuon từ SachList
        //    };

        //    return View(viewModel);
        //}

        // 15. Xử lý mượn sách
        //[HttpPost]
        //public ActionResult MuonSach(FormCollection form)
        //{
        //    if (!ModelState.IsValid) return View();

        //    // Lấy thông tin từ form
        //    var maDocGia = int.Parse(form["MaDocGia"]);
        //    var sachMuonList = form.GetValues("MaSach").Select(int.Parse).ToList();

        //    // Tạo phiếu mượn mới
        //    var phieuMuon = new PhieuMuon
        //    {
        //        MaDocGia = maDocGia,
        //        NgayMuon = DateTime.Now,
        //        NgayTra = DateTime.Now.AddDays(7),
        //        isApproved = true
        //    };

        //    db.PhieuMuons.InsertOnSubmit(phieuMuon);
        //    db.SubmitChanges();

        //    // Xử lý từng sách trong danh sách
        //    foreach (var maSach in sachMuonList)
        //    {
        //        var sach = db.Saches.FirstOrDefault(s => s.MaSach == maSach);
        //        if (sach != null && sach.SoLuong > 0)
        //        {
        //            // Giảm số lượng sách
        //            sach.SoLuong -= 1;

        //            // Thêm chi tiết vào ChiTietPhieuMuon
        //            db.ChiTietPhieuMuons.InsertOnSubmit(new ChiTietPhieuMuon
        //            {
        //                MaPhieuMuon = phieuMuon.MaPhieuMuon,
        //                MaSach = sach.MaSach,
        //                SoLuong = 1
        //            });
        //        }
        //    }

        //    db.SubmitChanges();
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public ActionResult MuonSach(MuonSachViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Xử lý logic mượn sách từ model.ChiTietMuon
                foreach (var chiTiet in model.ChiTietMuon)
                {
                    if (chiTiet.SoLuongMuon > 0)
                    {
                        // Thực hiện lưu vào database hoặc xử lý logic khác
                        var sach = db.Saches.FirstOrDefault(s => s.MaSach == chiTiet.MaSach);
                        if (sach != null)
                        {
                            sach.SoLuong -= chiTiet.SoLuongMuon; // Cập nhật số lượng còn lại
                        }
                    }
                }
                db.SubmitChanges();
                return RedirectToAction("Index"); // Chuyển hướng sau khi lưu
            }

            // Nếu không hợp lệ, hiển thị lại view
            return View(model);
        }

        // 16. Trả sách
        [HttpPost]
        public ActionResult TraSach(int id)
        {
            var phieuMuon = db.PhieuMuons.FirstOrDefault(p => p.MaPhieuMuon == id);
            if (phieuMuon == null) return Json(new { success = false, message = "Không tìm thấy phiếu mượn." });

            phieuMuon.NgayTra = DateTime.Now;
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        // 17. Thống kê sách
        [HttpGet]
        public ActionResult ThongKe()
        {
            // Lấy danh sách sách
            var sachList = db.Saches.ToList();

            // Khởi tạo danh sách để chứa các thông tin sách đã tính toán
            var sachThongKeList = new List<object>();

            // Tính toán SoLuongBanDau cho mỗi sách
            foreach (var sach in sachList)
            {
                // Tính tổng số sách đã mượn
                var soLuongDaMuon = db.ChiTietPhieuMuons
                    .Where(ct => ct.MaSach == sach.MaSach)
                    .Sum(ct => (int?)ct.SoLuong) ?? 0;  // Sử dụng ?? để trả về 0 nếu Sum trả về null

                // Tính số lượng ban đầu = Số lượng còn lại + Số đã mượn
                int soLuongBanDau = (int)(sach.SoLuong + soLuongDaMuon);

                // Lưu thông tin sách
                sachThongKeList.Add(new
                {
                    Sach = sach,
                    SoLuongBanDau = soLuongBanDau,
                    SoLuongDaMuon = soLuongDaMuon,
                    SoLuongConLai = sach.SoLuong
                });
            }

            return View(sachThongKeList);
        }
        public ActionResult LichSuMuonSach()
        {
            // Lấy id từ session
            int userId = Convert.ToInt32(Session["UserId"]);

            // Truy vấn lịch sử mượn của độc giả theo userId
            var lichSuMuon = db.PhieuMuons
                .Where(pm => pm.MaDocGia == userId && pm.isApproved == true)
                .ToList();

            return View(lichSuMuon);
        }

    }
}
