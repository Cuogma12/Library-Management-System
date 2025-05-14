using QLyTV.Constants;
using QLyTV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLyTV.Controllers
{
    public class SachController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=m-cuogdzai;Initial Catalog=QLYTV;Integrated Security=True;TrustServerCertificate=True");        // GET: Sach
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Edit(int id)
        {
                var sach = db.Saches.FirstOrDefault(o => o.MaSach == id);
                return View(sach);
        }


        [HttpPost]
        public JsonResult Paging(string searchQuery = "", int pageSize = 5, int crrPage = 1)
        {
            try
            {
                var books = db.Saches.AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    books = books.Where(b => b.TenSach.Contains(searchQuery) || b.TacGia.Contains(searchQuery));
                }

                // Tính tổng số trang
                int totalItems = books.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Phân trang
                var bookPaging = books
                    .OrderBy(b => b.MaSach)
                    .Skip((crrPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList() // Lấy dữ liệu từ cơ sở dữ liệu trước
                    .Select(b => new
                    {
                        b.MaSach,
                        b.TenSach,
                        b.TacGia,
                        b.NhaXuatBan,
                        b.TheLoai,
                        b.SoLuong,
                        b.TrangThai,
                        NgayTao = b.NgayTao.HasValue ? b.NgayTao.Value.ToString("dd/MM/yyyy") : "Không có",
                        NgaySua = b.NgaySua.HasValue ? b.NgaySua.Value.ToString("dd/MM/yyyy") : "Không có"
                    })
                    .ToList();

                return Json(new
                {
                    data = bookPaging,
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


        [HttpPost]
        public JsonResult PostDelete(int id)
        {

            try
            {
                var sach = db.Saches.FirstOrDefault(e => e.MaSach == id);

                if (sach == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy sách với ID " + id
                    }, JsonRequestBehavior.AllowGet);
                }

                var isBorrowed = db.PhieuMuons.Any(pm => pm.MaSach == id);

                if (isBorrowed)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa sách vì sách đã được mượn."
                    }, JsonRequestBehavior.AllowGet);
                }

                db.Saches.DeleteOnSubmit(sach);
                db.SubmitChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                return Json(new
                {
                    success = true,
                    message = "Xóa sách thành công"
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

        public ActionResult Add()
        {
            return View();
        }


        public ActionResult Create(FormCollection form)
        {
            string tensach = form["TenSach"];
            string tacgia = form["TacGia"];
            string nhaxuatban = form["NhaXuatBan"];
            string theloai = form["TheLoai"];
            int soluong = int.Parse(form["SoLuong"]);
            string trangthai = form["TrangThai"];
            DateTime ngaytao = DateTime.Parse(form["NgayTao"]);
            DateTime ngaysua = DateTime.Parse(form["NgaySua"]);

            Sach s = new Sach
            {
                TenSach = tensach,
                TacGia = tacgia,
                NhaXuatBan = nhaxuatban,
                TheLoai = theloai,
                SoLuong = soluong,
                TrangThai = trangthai,
                NgayTao = ngaytao,
                NgaySua = ngaysua
            };

            db.Saches.InsertOnSubmit(s);
            db.SubmitChanges();

            Console.WriteLine("Thêm mới thành công!");

            return RedirectToAction("Index");
        }


        [HttpPost]
        public JsonResult Update(int maSach, string tenSach, string tacGia, string nhaXuatBan, string theLoai,
        int soLuong, string trangThai)
        {
            try
            {
                var sach = db.Saches.FirstOrDefault(s => s.MaSach == maSach);
                if (sach == null)
                {
                    return Json(new { success = false, message = "Sách không tồn tại!" });
                }

                // Cập nhật các thuộc tính của sách
                sach.TenSach = tenSach;
                sach.TacGia = tacGia;
                sach.NhaXuatBan = nhaXuatBan;
                sach.TheLoai = theLoai;
                sach.SoLuong = soLuong;
                sach.TrangThai = trangThai;

                // Cập nhật Ngày sửa với giá trị hiện tại
                sach.NgaySua = DateTime.Now;

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

    }
}
