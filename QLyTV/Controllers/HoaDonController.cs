using QLyTV.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;



namespace QLYTV.Controllers
{
    public class HoaDonController : Controller
    {

        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=m-cuogdzai;Initial Catalog=QLYTV;Integrated Security=True;TrustServerCertificate=True");

        // GET: HoaDon/Index
        public ActionResult Index()
        {
            // Lấy danh sách hóa đơn từ cơ sở dữ liệu
            var hoaDons = db.HoaDons.ToList();

            // Truyền danh sách hóa đơn vào View
            return View(hoaDons);
        }


        [HttpPost]
        public JsonResult Paging(string searchQuery = "", int pageSize = 5, int crrPage = 1)
        {
            try
            {
                var hoaDons = db.HoaDons.AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    hoaDons = hoaDons.Where(hd => hd.MaHoaDon.ToString().Contains(searchQuery) || hd.MaPhieuMuon.ToString().Contains(searchQuery));
                }

                // Tính tổng số trang
                int totalItems = hoaDons.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Phân trang
                var hoaDonPaging = hoaDons
                    .OrderBy(hd => hd.MaHoaDon)
                    .Skip((crrPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList() // Lấy dữ liệu từ cơ sở dữ liệu trước
                    .Select(hd => new
                    {
                        hd.MaHoaDon,
                        hd.MaThuThu,
                        hd.MaPhieuMuon,
                        hd.PhiPhat,
                        hd.PhiMuon,
                        NgayMuon = hd.NgayMuon.ToString("dd/MM/yyyy"),
                        NgayTra = hd.NgayTra.HasValue ? hd.NgayTra.Value.ToString("dd/MM/yyyy") : "Chưa trả"
                    })
                    .ToList();

                return Json(new
                {
                    data = hoaDonPaging,
                    success = true,
                    message = "Lấy dữ liệu thành công",
                    crrPage,
                    pageSize,
                    totalPage = totalPages
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        // GET: HoaDon/Create
        [HttpPost]
        public ActionResult Add(int MaThuThu, int MaPhieuMuon, decimal PhiPhat, decimal PhiMuon, DateTime NgayMuon)
        {
            var hoaDon = new HoaDon
            {
                MaThuThu = MaThuThu,
                MaPhieuMuon = MaPhieuMuon,
                PhiPhat = PhiPhat,
                PhiMuon = PhiMuon,
                NgayMuon = NgayMuon,
                NgayTra = DateTime.Now,
            };

            return View(hoaDon);
        }


        // POST: HoaDon/PostCreate
        [HttpPost]
      
        public ActionResult PostCreate(FormCollection form)
        {
            try
            {
                // Lấy dữ liệu từ form gửi lên
                int maThuThu = int.Parse(form["MaThuThu"]);
                int maPhieuMuon = int.Parse(form["MaPhieuMuon"]);
                decimal phiPhat = decimal.Parse(form["PhiPhat"]);
                decimal phiMuon = decimal.Parse(form["PhiMuon"]);
                DateTime ngayMuon = DateTime.Parse(form["NgayMuon"]);
                DateTime? ngayTra = string.IsNullOrEmpty(form["NgayTra"]) ? (DateTime?)null : DateTime.Parse(form["NgayTra"]);

                // Tạo đối tượng mới cho bảng HoaDon
                HoaDon newHoaDon = new HoaDon
                {
                    MaThuThu = maThuThu,
                    MaPhieuMuon = maPhieuMuon,
                    PhiPhat = phiPhat,
                    PhiMuon = phiMuon,
                    NgayMuon = ngayMuon,
                    NgayTra = ngayTra
                };

                // Thêm đối tượng vào cơ sở dữ liệu
                db.HoaDons.InsertOnSubmit(newHoaDon);

                var phieuMuon = db.PhieuMuons.FirstOrDefault(pm => pm.MaPhieuMuon == maPhieuMuon);
                if (phieuMuon != null)
                {
                    phieuMuon.isDelete = true; // Đặt isDelete = 1
                }

                db.SubmitChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                return RedirectToAction("Index", "HoaDon");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Đã xảy ra lỗi: " + ex.Message;

                return View();
            }
        }

        // GET: HoaDon/GetListHoaDon
        public JsonResult GetListHoaDon()
        {
            var hoaDons = db.HoaDons.Select(hd => new
            {
                hd.MaHoaDon,
                hd.MaThuThu,
                hd.MaPhieuMuon,
                hd.PhiPhat,
                hd.PhiMuon,
                NgayMuon = hd.NgayMuon.ToString("dd/MM/yyyy"),
                NgayTra = hd.NgayTra.HasValue ? hd.NgayTra.Value.ToString("dd/MM/yyyy") : ""
            }).ToList();

            return Json(new
            {
                data = hoaDons,
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: HoaDon/Edit/{id}
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var hoaDon = db.HoaDons.FirstOrDefault(hd => hd.MaHoaDon == id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }

            return View(hoaDon);
        }


        // POST: HoaDon/Update
        [HttpPost]
        public JsonResult Update(FormCollection form)
        {
            try
            {
                int maHoaDon = int.Parse(form["MaHoaDon"]);
                int maThuThu = int.Parse(form["MaThuThu"]);
                int maPhieuMuon = int.Parse(form["MaPhieuMuon"]);
                decimal phiPhat = decimal.Parse(form["PhiPhat"]);
                decimal phiMuon = decimal.Parse(form["PhiMuon"]);
                DateTime ngayMuon = DateTime.Parse(form["NgayMuon"]);
                DateTime? ngayTra = string.IsNullOrEmpty(form["NgayTra"]) ? (DateTime?)null : DateTime.Parse(form["NgayTra"]);

                // Tìm hóa đơn cần cập nhật
                HoaDon hoaDon = db.HoaDons.FirstOrDefault(hd => hd.MaHoaDon == maHoaDon);
                if (hoaDon != null)
                {
                    hoaDon.MaThuThu = maThuThu;
                    hoaDon.MaPhieuMuon = maPhieuMuon;
                    hoaDon.PhiPhat = phiPhat;
                    hoaDon.PhiMuon = phiMuon;
                    hoaDon.NgayMuon = ngayMuon;
                    hoaDon.NgayTra = ngayTra;

                    db.SubmitChanges();
                }

                return Json(new
                {
                    success = true,
                    message = "Cập nhật hóa đơn thành công",
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi: " + ex.Message,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult PostDelete(int id)
        {
            try
            {
                var hoaDon = db.HoaDons.FirstOrDefault(e => e.MaHoaDon == id);

                if (hoaDon == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy hóa dơn với ID " + id
                    }, JsonRequestBehavior.AllowGet);
                }

                db.HoaDons.DeleteOnSubmit(hoaDon);
                db.SubmitChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                return Json(new
                {
                    success = true,
                    message = "Xóa hóa đơn thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
